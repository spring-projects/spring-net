/*
 * Copyright 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections;
using System.Collections.Generic;

using Apache.NMS;
using Common.Logging;
using Spring.Collections;
using Spring.Util;
using IQueue=Apache.NMS.IQueue;

namespace Spring.Messaging.Nms.Connections
{
    /// <summary>
    /// Wrapper for Session that caches producers and registers itself as available
    /// to the session cache when being closed.  Generally used for testing purposes or
    /// if need to get at the wrapped Session object via the TargetSession property (for
    /// vendor specific methods).
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack</author>
    public class CachedSession : IDecoratorSession
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CachedSession));

        private readonly ISession target;
        private readonly List<ISession> sessionList;
        private readonly int sessionCacheSize;
        private readonly Dictionary<IDestination, IMessageProducer> cachedProducers = new Dictionary<IDestination, IMessageProducer>();
        private readonly Dictionary<ConsumerCacheKey, IMessageConsumer> cachedConsumers = new Dictionary<ConsumerCacheKey, IMessageConsumer>();
        private IMessageProducer cachedUnspecifiedDestinationMessageProducer;
        private readonly bool shouldCacheProducers;
        private readonly bool shouldCacheConsumers;
        private bool transactionOpen = false;
        private readonly CachingConnectionFactory ccf;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedSession"/> class.
        /// </summary>
        /// <param name="targetSession">The target session.</param>
        /// <param name="sessionList">The session list.</param>
        /// <param name="ccf">The CachingConnectionFactory.</param>
        public CachedSession(
            ISession targetSession,
            List<ISession> sessionList, 
            CachingConnectionFactory ccf)
        {
            target = targetSession;
            this.sessionList = sessionList;
            sessionCacheSize = ccf.SessionCacheSize;
            shouldCacheProducers = ccf.CacheProducers;
            shouldCacheConsumers = ccf.CacheConsumers;
            this.ccf = ccf;
        }


        /// <summary>
        /// Gets the target, for testing purposes.
        /// </summary>
        /// <value>The target.</value>
        public ISession TargetSession => target;

        /// <summary>
        /// Creates the producer, potentially returning a cached instance.
        /// </summary>
        /// <returns>A message producer, potentially cached.</returns>
        public IMessageProducer CreateProducer()
        {
            if (shouldCacheProducers)
            {
                if (cachedUnspecifiedDestinationMessageProducer != null)
                {
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("Found cached MessageProducer for unspecified destination");
                    }
                }
                else
                {
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("Creating cached MessageProducer for unspecified destination");
                    }

                    cachedUnspecifiedDestinationMessageProducer = target.CreateProducer();

                }
                transactionOpen = true;
                return new CachedMessageProducer(cachedUnspecifiedDestinationMessageProducer);
            }
            else
            {
                return target.CreateProducer();
            }
        }

        /// <summary>
        /// Creates the producer, potentially returning a cached instance.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>A message producer.</returns>
        public IMessageProducer CreateProducer(IDestination destination)
        {
            AssertUtils.ArgumentNotNull(destination,"destination");

            if (shouldCacheProducers)
            {
                if (cachedProducers.TryGetValue(destination, out var producer))
                {
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("Found cached MessageProducer for destination [" + destination + "]");
                    }
                }
                else
                {
                    producer = target.CreateProducer(destination);

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("Creating cached MessageProducer for destination [" + destination + "]");
                    }

                    cachedProducers[destination] = producer;

                }
                transactionOpen = true;
                return new CachedMessageProducer(producer);
            }
            else
            {
                return target.CreateProducer(destination);
            }
        }


        /// <summary>
        /// If have not yet reached session cache size, cache the session, otherwise
        /// dispose of all cached message producers and close the session.
        /// </summary>
        public void Close()
        {
            if (ccf.IsActive)
            {
                //don't pass the call to the underlying target.
                lock (sessionList)
                {
                    if (sessionList.Count < sessionCacheSize)
                    {
                        LogicalClose();
                        // Remain open in the session list.
                        return;
                    }
                }
            }
            // If we get here, we're supposed to shut down.
            PhysicalClose();
        }

        private void LogicalClose()
        {
            // Preserve rollback-on-close semantics.
            if (transactionOpen && target.Transacted)
            {
                transactionOpen = false;
                target.Rollback();
            }

            // Physically close durable subscribers at time of Session close call.
            var toRemove = new List<ConsumerCacheKey>();
            foreach (var dictionaryEntry in cachedConsumers)
            {
                ConsumerCacheKey key = dictionaryEntry.Key;
                if (key.Subscription != null)
                {
                    dictionaryEntry.Value.Close();
                    toRemove.Add(key);
                }                
            }
            foreach (ConsumerCacheKey key in toRemove)
            {
                cachedConsumers.Remove(key);
            }

            // Allow for multiple close calls...
            if (!sessionList.Contains(this))
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Returning cached Session: " + target);
                }

                sessionList.Add(this); //add to end of linked list.
            }
        }

        private void PhysicalClose()
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("Closing cached Session: " + target);
            }
            // Explicitly close all MessageProducers and MessageConsumers that
            // this Session happens to cache...
            try
            {
                foreach (var entry in cachedProducers)
                {
                    entry.Value.Close();
                }
                foreach (var entry in cachedConsumers)
                {
                    entry.Value.Close();
                }
            }
            finally
            {
                // Now actually close the Session.
                target.Close();
            }
        }

        /// <summary>
        /// Creates the consumer, potentially returning a cached instance. 
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>A message consumer</returns>
        public IMessageConsumer CreateConsumer(IDestination destination)
        {
            return CreateConsumer(destination, null, false, null);
        }


        /// <summary>
        /// Creates the consumer, potentially returning a cached instance. 
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>A message consumer</returns>
        public IMessageConsumer CreateConsumer(IDestination destination, string selector)
        {
            return CreateConsumer(destination, selector, false, null);
        }

        /// <summary>
        /// Creates the consumer, potentially returning a cached instance.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="noLocal">if set to <c>true</c> [no local].</param>
        /// <returns>A message consumer.</returns>
        public IMessageConsumer CreateConsumer(IDestination destination, string selector, bool noLocal)
        {
            return CreateConsumer(destination, selector, noLocal, null);
        }

        /// <summary>
        /// Creates the durable consumer, potentially returning a cached instance.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="subscription">The name of the durable subscription.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="noLocal">if set to <c>true</c> [no local].</param>
        /// <returns>A message consumer</returns>
        public IMessageConsumer CreateDurableConsumer(ITopic destination, string subscription, string selector, bool noLocal)
        {
            transactionOpen = true;
            if (shouldCacheConsumers)
            {
                return GetCachedConsumer(destination, selector, noLocal, subscription);
            }
            else
            {
                return target.CreateDurableConsumer(destination, subscription, selector, noLocal);
            }
        }

        /// <summary>
        /// Deletes the durable consumer.
        /// </summary>
        /// <param name="durableSubscriptionName">The name of the durable subscription.</param>
        public void DeleteDurableConsumer(string durableSubscriptionName)
        {
            if (shouldCacheConsumers)
            {
                throw new InvalidOperationException("Deleting of durable consumers is not supported when caching of consumers is enabled");
            } 
            target.DeleteDurableConsumer(durableSubscriptionName);            
        }


        /// <summary>
        /// Creates the consumer.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="noLocal">if set to <c>true</c> [no local].</param>
        /// <param name="durableSubscriptionName">The durable subscription name.</param>
        /// <returns></returns>
        protected IMessageConsumer CreateConsumer(IDestination destination, string selector, bool noLocal, string durableSubscriptionName)
        {
            transactionOpen = true;
            if (shouldCacheConsumers)
            {
                return GetCachedConsumer(destination, selector, noLocal, durableSubscriptionName);
            }
            else
            {
                return target.CreateConsumer(destination, selector, noLocal);
            }
        }

        private IMessageConsumer GetCachedConsumer(IDestination destination, string selector, bool noLocal, string durableSubscriptionName)
        {
            var cacheKey = new ConsumerCacheKey(destination, selector, noLocal, durableSubscriptionName);
            if (cachedConsumers.TryGetValue(cacheKey, out var consumer))
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Found cached NMS MessageConsumer for destination [" + destination + "]: " + consumer);
                }
            }
            else
            {
                if (destination is ITopic topic)
                {
                    consumer = (durableSubscriptionName != null
                                    ? target.CreateDurableConsumer(topic, durableSubscriptionName, selector, noLocal)
                                    : target.CreateConsumer(topic, selector, noLocal));
                }
                else
                {
                    consumer = target.CreateConsumer(destination, selector);
                }
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Creating cached NMS MessageConsumer for destination [" + destination + "]: " + consumer);
                }
                cachedConsumers[cacheKey] = consumer;
            }
            return new CachedMessageConsumer(consumer);
        }

        /// <summary>
        /// Gets the queue.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IQueue GetQueue(string name)
        {
            transactionOpen = true;
            return target.GetQueue(name);
        }

        /// <summary>
        /// Gets the topic.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public ITopic GetTopic(string name)
        {
            transactionOpen = true;
            return target.GetTopic(name);
        }

        /// <summary>
        /// Creates the temporary queue.
        /// </summary>
        /// <returns></returns>
        public ITemporaryQueue CreateTemporaryQueue()
        {
            transactionOpen = true;
            return target.CreateTemporaryQueue();
        }

        /// <summary>
        /// Creates the temporary topic.
        /// </summary>
        /// <returns></returns>
        public ITemporaryTopic CreateTemporaryTopic()
        {
            transactionOpen = true;
            return target.CreateTemporaryTopic();
        }

        /// <summary>
        /// Deletes the destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        public void DeleteDestination(IDestination destination)
        {
            transactionOpen = true;
            target.DeleteDestination(destination);
        }

        /// <summary>
        /// Creates the message.
        /// </summary>
        /// <returns></returns>
        public IMessage CreateMessage()
        {
            transactionOpen = true;
            return target.CreateMessage();
        }

        /// <summary>
        /// Creates the text message.
        /// </summary>
        /// <returns></returns>
        public ITextMessage CreateTextMessage()
        {
            transactionOpen = true;
            return target.CreateTextMessage();
        }

        /// <summary>
        /// Creates the text message.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public ITextMessage CreateTextMessage(string text)
        {
            transactionOpen = true;
            return target.CreateTextMessage(text);
        }

        /// <summary>
        /// Creates the map message.
        /// </summary>
        /// <returns></returns>
        public IMapMessage CreateMapMessage()
        {
            transactionOpen = true;
            return target.CreateMapMessage();
        }

        /// <summary>
        /// Creates the object message.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        public IObjectMessage CreateObjectMessage(object body)
        {
            transactionOpen = true;
            return target.CreateObjectMessage(body);
        }

        /// <summary>
        /// Creates the bytes message.
        /// </summary>
        /// <returns></returns>
        public IBytesMessage CreateBytesMessage()
        {
            transactionOpen = true;
            return target.CreateBytesMessage();
        }

        /// <summary>
        /// Creates the bytes message.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        public IBytesMessage CreateBytesMessage(byte[] body)
        {
            transactionOpen = true;
            return target.CreateBytesMessage(body);
        }

        /// <summary>
        /// Creates the stream message.
        /// </summary>
        /// <returns></returns>
        public IStreamMessage CreateStreamMessage()
        {
            transactionOpen = true;
            return target.CreateStreamMessage();
        }

        /// <summary>
        /// Commits this instance.
        /// </summary>
        public void Commit()
        {
            transactionOpen = false;
            target.Commit();
        }

        /// <summary>
        /// Stops all Message delivery in this session and restarts it again with the oldest unacknowledged message. Messages that were delivered
        /// but not acknowledged should have their redelivered property set. This is an optional method that may not by implemented by all NMS
        /// providers, if not implemented an Exception will be thrown. Message redelivery is not requried to be performed in the original
        /// order. It is not valid to call this method on a Transacted Session.
        /// </summary>
        public void Recover()
        {
            transactionOpen = true;
            target.Recover();
        }

        /// <summary>
        /// Rollbacks this instance.
        /// </summary>
        public void Rollback()
        {
            transactionOpen = false;
            target.Rollback();
        }

        /// <summary>
        /// A Delegate that is called each time a Message is dispatched to allow the client to do
        /// any necessary transformations on the received message before it is delivered.
        /// The Session instance sets the delegate on each Consumer it creates.
        /// </summary>
        /// <value></value>
        public ConsumerTransformerDelegate ConsumerTransformer
        {
            get => target.ConsumerTransformer;
            set => target.ConsumerTransformer = value;
        }

        /// <summary>
        /// A delegate that is called each time a Message is sent from this Producer which allows
        /// the application to perform any needed transformations on the Message before it is sent.
        /// The Session instance sets the delegate on each Producer it creates.
        /// </summary>
        /// <value></value>
        public ProducerTransformerDelegate ProducerTransformer
        {
            get => target.ProducerTransformer;
            set => target.ProducerTransformer = value;
        }
        /// <summary>
        /// Gets or sets the request timeout.
        /// </summary>
        /// <value>The request timeout.</value>
        public TimeSpan RequestTimeout
        {
            get => target.RequestTimeout;
            set => target.RequestTimeout = value;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="CachedSession"/> is transacted.
        /// </summary>
        /// <value><c>true</c> if transacted; otherwise, <c>false</c>.</value>
        public bool Transacted
        {
            get
            {
                transactionOpen = true;
                return target.Transacted;
            }
        }

        /// <summary>
        /// Gets the acknowledgement mode.
        /// </summary>
        /// <value>The acknowledgement mode.</value>
        public AcknowledgementMode AcknowledgementMode
        {
            get
            {
                transactionOpen = true;
                return target.AcknowledgementMode;
            }
        }

        /// <summary>
        /// Occurs, when a transaction is started.
        /// </summary>
        public event SessionTxEventDelegate TransactionStartedListener
        {
            add => target.TransactionStartedListener += value;
            remove => target.TransactionStartedListener -= value;
        }

        /// <summary>
        /// Occurs, when a transaction is commited.
        /// </summary>
        public event SessionTxEventDelegate TransactionCommittedListener
        {
            add => target.TransactionCommittedListener += value;
            remove => target.TransactionCommittedListener -= value;
        }

        /// <summary>
        /// Occurs, when a transaction is rolled back.
        /// </summary>
        public event SessionTxEventDelegate TransactionRolledBackListener
        {
            add => target.TransactionRolledBackListener += value;
            remove => target.TransactionRolledBackListener -= value;
        }

        /// <summary>
        /// Call dispose on the target.
        /// </summary>
        public void Dispose()
        {
            transactionOpen = true;
            target.Dispose();
        }

        /// <summary>
        /// Creates the queue browser with a specified selector
        /// </summary>
        /// <param name="queue">The queue.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The Queue browser</returns>
        public IQueueBrowser CreateBrowser(IQueue queue, string selector)
        {
            transactionOpen = true;
            return target.CreateBrowser(queue, selector);
        }

        /// <summary>
        /// Creates the queue browser.
        /// </summary>
        /// <param name="queue">The queue.</param>
        /// <returns>The Queue browser</returns>
        public IQueueBrowser CreateBrowser(IQueue queue)
        {
            transactionOpen = true;
            return target.CreateBrowser(queue);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return "Cached NMS Session: " + target;
        }
    }

    internal class ConsumerCacheKey
    {
        private readonly IDestination destination;
        private readonly string selector;
        private readonly bool noLocal;
        private readonly string subscription;

        public ConsumerCacheKey(IDestination destination, string selector, bool noLocal, string subscription)
        {
            this.destination = destination;
            this.selector = selector;
            this.noLocal = noLocal;
            this.subscription = subscription;
        }

        public string Subscription => subscription;

        protected bool Equals(ConsumerCacheKey consumerCacheKey)
        {
            if (consumerCacheKey == null) return false;
            if (!Equals(destination, consumerCacheKey.destination)) return false;
            if (!ObjectUtils.NullSafeEquals(selector, consumerCacheKey.selector)) return false;
            if (!Equals(noLocal, consumerCacheKey.noLocal)) return false;
            if (!ObjectUtils.NullSafeEquals(subscription, consumerCacheKey.subscription)) return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as ConsumerCacheKey);
        }

        public override int GetHashCode()
        {
            return destination.GetHashCode();
        }
    }
}