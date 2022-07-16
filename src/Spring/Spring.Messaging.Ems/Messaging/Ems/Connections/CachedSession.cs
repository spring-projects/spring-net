#region License

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

#endregion

using System.Collections;
using Spring.Messaging.Ems.Common;
using Common.Logging;
using Spring.Collections;
using Spring.Util;
using Queue=TIBCO.EMS.Queue;

namespace Spring.Messaging.Ems.Connections
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
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof(CachedSession));

        #endregion

        private readonly ISession target;
        private readonly LinkedList sessionList;
        private readonly int sessionCacheSize;
        private readonly IDictionary cachedProducers = new Hashtable();
        private readonly IDictionary cachedConsumers = new Hashtable();
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
        public CachedSession(ISession targetSession, LinkedList sessionList, CachingConnectionFactory ccf)
        {
            target = targetSession;
            this.sessionList = sessionList;
            this.sessionCacheSize = ccf.SessionCacheSize;
            shouldCacheProducers = ccf.CacheProducers;
            shouldCacheConsumers = ccf.CacheConsumers;
            this.ccf = ccf;
        }


        /// <summary>
        /// Gets the target, for testing purposes.
        /// </summary>
        /// <value>The target.</value>
        public ISession TargetSession
        {
            get { return target; }
        }

        /// <summary>
        /// Creates the producer, potentially returning a cached instance.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>A message producer.</returns>
        public IMessageProducer CreateProducer(Destination destination)
        {
            if (shouldCacheProducers)
            {
                IMessageProducer producer = (IMessageProducer)cachedProducers[destination];
                if (producer != null)
                {
                    #region Logging

                    if (LOG.IsDebugEnabled)
                    {
                        LOG.Debug("Found cached MessageProducer for destination [" + destination + "]");
                    }

                    #endregion
                }
                else
                {
                    producer = target.CreateProducer(destination);
                    #region Logging

                    if (LOG.IsDebugEnabled)
                    {
                        LOG.Debug("Creating cached MessageProducer for destination [" + destination + "]");
                    }

                    #endregion
                    cachedProducers.Add(destination, producer);

                }
                this.transactionOpen = true;
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
            if (this.transactionOpen && this.target.Transacted)
            {
                this.transactionOpen = false;
                this.target.Rollback();
            }

            // Physically close durable subscribers at time of Session close call.
            IList ToRemove = new ArrayList();
            foreach (DictionaryEntry dictionaryEntry in cachedConsumers)
            {
                ConsumerCacheKey key = (ConsumerCacheKey) dictionaryEntry.Key;
                if (key.Subscription != null)
                {
                    ((IMessageConsumer) dictionaryEntry.Value).Close();
                    ToRemove.Add(key);
                }
            }
            foreach (ConsumerCacheKey key in ToRemove)
            {
                cachedConsumers.Remove(key);
            }

            // Allow for multiple close calls...
            if (!sessionList.Contains(this))
            {
                #region Logging

                if (LOG.IsDebugEnabled)
                {
                    LOG.Debug("Returning cached Session: " + target);
                }

                #endregion

                sessionList.Add(this); //add to end of linked list.
            }
        }

        private void PhysicalClose()
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Closing cached Session: " + this.target);
            }
            // Explicitly close all MessageProducers and MessageConsumers that
            // this Session happens to cache...
            try
            {
                foreach (DictionaryEntry entry in cachedProducers)
                {
                    ((IMessageProducer)entry.Value).Close();
                }
                foreach (DictionaryEntry entry in cachedConsumers)
                {
                    ((IMessageConsumer)entry.Value).Close();
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
        public IMessageConsumer CreateConsumer(Destination destination)
        {
            return CreateConsumer(destination, null, false, null);
        }


        /// <summary>
        /// Creates the consumer, potentially returning a cached instance.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>A message consumer</returns>
        public IMessageConsumer CreateConsumer(Destination destination, string selector)
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
        public IMessageConsumer CreateConsumer(Destination destination, string selector, bool noLocal)
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
        public ITopicSubscriber CreateDurableSubscriber(Topic destination, string subscription, string selector, bool noLocal)
        {
            this.transactionOpen = true;
            if (shouldCacheConsumers)
            {
                return (ITopicSubscriber)GetCachedConsumer(destination, selector, noLocal, subscription);
            }
            else
            {
                return target.CreateDurableSubscriber(destination, subscription, selector, noLocal);
            }
        }

        /// <summary>
        /// Creates the durable consumer, potentially returning a cached instance.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="subscription">The name of the durable subscription.</param>
        /// <returns>A message consumer</returns>
        public ITopicSubscriber CreateDurableSubscriber(Topic destination, string subscription)
        {
            return CreateDurableSubscriber(destination, subscription, null, false);
        }

        /// <summary>
        /// Creates the consumer.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="noLocal">if set to <c>true</c> [no local].</param>
        /// <param name="subscription">The subscription.</param>
        /// <returns></returns>
        protected IMessageConsumer CreateConsumer(Destination destination, string selector, bool noLocal, string subscription)
        {
            this.transactionOpen = true;
            if (shouldCacheConsumers)
            {
                return GetCachedConsumer(destination, selector, noLocal, subscription);
            }
            else
            {
                return target.CreateConsumer(destination, selector, noLocal);
            }
        }

        private IMessageConsumer GetCachedConsumer(Destination destination, string selector, bool noLocal, string subscription)
        {
            object cacheKey = new ConsumerCacheKey(destination, selector, noLocal, null);
            IMessageConsumer consumer = (IMessageConsumer)cachedConsumers[cacheKey];
            if (consumer != null)
            {
                if (LOG.IsDebugEnabled)
                {
                    LOG.Debug("Found cached EMS MessageConsumer for destination [" + destination + "]: " + consumer);
                }
            }
            else
            {
                if (destination is Topic)
                {
                    consumer = (subscription != null
                                    ? target.CreateDurableSubscriber((Topic)destination, subscription, selector, noLocal)
                                    : target.CreateConsumer(destination, selector, noLocal));
                }
                else
                {
                    consumer = target.CreateConsumer(destination, selector);
                }
                if (LOG.IsDebugEnabled)
                {
                    LOG.Debug("Creating cached EMS MessageConsumer for destination [" + destination + "]: " + consumer);
                }
                cachedConsumers[cacheKey] = consumer;
            }
            return new CachedMessageConsumer(consumer);
        }

        #region Pass through implementations

        /// <summary>
        /// Gets the queue.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public TIBCO.EMS.Queue CreateQueue(string name)
        {
            this.transactionOpen = true;
            return target.CreateQueue(name);
        }

        /// <summary>
        /// Gets the topic.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Topic CreateTopic(string name)
        {
            this.transactionOpen = true;
            return target.CreateTopic(name);
        }

        /// <summary>
        /// Creates the temporary queue.
        /// </summary>
        /// <returns></returns>
        public TemporaryQueue CreateTemporaryQueue()
        {
            this.transactionOpen = true;
            return target.CreateTemporaryQueue();
        }

        /// <summary>
        /// Creates the temporary topic.
        /// </summary>
        /// <returns></returns>
        public TemporaryTopic CreateTemporaryTopic()
        {
            this.transactionOpen = true;
            return target.CreateTemporaryTopic();
        }

        /// <summary>
        /// Creates the message.
        /// </summary>
        /// <returns></returns>
        public Message CreateMessage()
        {
            this.transactionOpen = true;
            return target.CreateMessage();
        }

        /// <summary>
        /// Creates the text message.
        /// </summary>
        /// <returns></returns>
        public TextMessage CreateTextMessage()
        {
            this.transactionOpen = true;
            return target.CreateTextMessage();
        }

        /// <summary>
        /// Creates the text message.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public TextMessage CreateTextMessage(string text)
        {
            this.transactionOpen = true;
            return target.CreateTextMessage(text);
        }

        /// <summary>
        /// Creates the map message.
        /// </summary>
        /// <returns></returns>
        public MapMessage CreateMapMessage()
        {
            this.transactionOpen = true;
            return target.CreateMapMessage();
        }

        /// <summary>
        /// Creates the bytes message.
        /// </summary>
        /// <returns></returns>
        public BytesMessage CreateBytesMessage()
        {
            this.transactionOpen = true;
            return target.CreateBytesMessage();
        }

        /// <summary>
        /// Creates the object message.
        /// </summary>
        /// <returns></returns>
        public ObjectMessage CreateObjectMessage()
        {
            this.transactionOpen = true;
            return target.CreateObjectMessage();
        }

        /// <summary>
        /// Creates the object message.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        public ObjectMessage CreateObjectMessage(object body)
        {
            this.transactionOpen = true;
            return target.CreateObjectMessage(body);
        }

        /// <summary>
        /// Creates the stream message.
        /// </summary>
        /// <returns></returns>
        public StreamMessage CreateStreamMessage()
        {
            this.transactionOpen = true;
            return target.CreateStreamMessage();
        }


        /// <summary>
        /// Commits this instance.
        /// </summary>
        public void Commit()
        {
            this.transactionOpen = false;
            target.Commit();
        }

        /// <summary>
        /// Rollbacks this instance.
        /// </summary>
        public void Rollback()
        {
            this.transactionOpen = false;
            target.Rollback();
        }


        public QueueBrowser CreateBrowser(Queue queue)
        {
            this.transactionOpen = true;
            return target.CreateBrowser(queue);
        }

        public QueueBrowser CreateBrowser(Queue queue, string messageSelector)
        {
            this.transactionOpen = true;
            return target.CreateBrowser(queue, messageSelector);
        }


        public void Recover()
        {
            this.transactionOpen = true;
            target.Recover();
        }

        public void Run()
        {
            this.transactionOpen = true;
            target.Run();
        }

        public void Unsubscribe(string name)
        {
            this.transactionOpen = true;
            target.Unsubscribe(name);
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="CachedSession"/> is transacted.
        /// </summary>
        /// <value><c>true</c> if transacted; otherwise, <c>false</c>.</value>
        public bool Transacted
        {
            get
            {
                this.transactionOpen = true;
                return target.Transacted;
            }
        }

        /// <summary>
        /// Gets the acknowledgement mode.
        /// </summary>
        /// <value>The acknowledgement mode.</value>
        public SessionMode SessionAcknowledgeMode
        {
            get
            {
                this.transactionOpen = true;
                return target.SessionAcknowledgeMode;
            }
        }

        public long SessID
        {
            get
            {
                this.transactionOpen = true;
                return target.SessID;
            }
        }

        public Session NativeSession
        {
            get
            {
                this.transactionOpen = true;
                return target.NativeSession;
            }
        }

        public int AcknowledgeMode
        {
            get
            {
                this.transactionOpen = true;
                return target.AcknowledgeMode;
            }
        }

        public Connection Connection
        {
            get
            {
                this.transactionOpen = true;
                return target.Connection;
            }
        }

        public bool IsClosed
        {
            get {
                this.transactionOpen = true;
                return target.IsClosed;
            }
        }

        public bool IsTransacted
        {
            get
            {
                this.transactionOpen = true;
                return target.IsTransacted;
            }
        }

        #endregion

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return "Cached EMS Session: " + this.target;
        }
    }

    internal class ConsumerCacheKey
    {
        private readonly Destination destination;
        private readonly string selector;
        private readonly bool noLocal;
        private readonly string subscription;

        public ConsumerCacheKey(Destination destination, string selector, bool noLocal, string subscription)
        {
            this.destination = destination;
            this.selector = selector;
            this.noLocal = noLocal;
            this.subscription = subscription;
        }

        public string Subscription
        {
            get { return subscription; }
        }

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
