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

using Apache.NMS;
using Common.Logging;
using Spring.Messaging.Nms.Support;
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
        private readonly SemaphoreSlim semaphoreSessionList = new SemaphoreSlim(1,1); 
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
            return CreateProducerAsync().GetAsyncResult();
        }

        public async Task<IMessageProducer> CreateProducerAsync()
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

                    cachedUnspecifiedDestinationMessageProducer = await target.CreateProducerAsync().Awaiter();

                }
                transactionOpen = true;
                return new CachedMessageProducer(cachedUnspecifiedDestinationMessageProducer);
            }
            else
            {
                return await target.CreateProducerAsync().Awaiter();
            }
        }

        /// <summary>
        /// Creates the producer, potentially returning a cached instance.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>A message producer.</returns>
        public IMessageProducer CreateProducer(IDestination destination)
        {
            return CreateProducerAsync(destination).GetAsyncResult();
        }

        public async Task<IMessageProducer> CreateProducerAsync(IDestination destination)
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
                    producer = await target.CreateProducerAsync(destination).Awaiter();

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
                return await target.CreateProducerAsync(destination).Awaiter();
            }
        }


        /// <summary>
        /// If have not yet reached session cache size, cache the session, otherwise
        /// dispose of all cached message producers and close the session.
        /// </summary>
        public void Close()
        {
            CloseAsync().GetAsyncResult();
        }

        public async Task CloseAsync()
        {
            if (ccf.IsActive)
            {
                //don't pass the call to the underlying target.
                await semaphoreSessionList.WaitAsync().Awaiter();
                try
                {
                    if (sessionList.Count < sessionCacheSize)
                    {
                        await LogicalClose().Awaiter();
                        // Remain open in the session list.
                        return;
                    }
                }
                finally
                {
                    semaphoreSessionList.Release();
                }
            }
            // If we get here, we're supposed to shut down.
            await PhysicalClose().Awaiter();
        }

        private async Task LogicalClose()
        {
            // Preserve rollback-on-close semantics.
            if (transactionOpen && target.Transacted)
            {
                transactionOpen = false;
                await target.RollbackAsync().Awaiter();
            }

            // Physically close durable subscribers at time of Session close call.
            var toRemove = new List<ConsumerCacheKey>();
            foreach (var dictionaryEntry in cachedConsumers)
            {
                ConsumerCacheKey key = dictionaryEntry.Key;
                if (key.Subscription != null)
                {
                    await dictionaryEntry.Value.CloseAsync().Awaiter();
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

        private async Task PhysicalClose()
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
                    await entry.Value.CloseAsync().Awaiter();
                }
                foreach (var entry in cachedConsumers)
                {
                    await entry.Value.CloseAsync().Awaiter();
                }
            }
            finally
            {
                // Now actually close the Session.
                await target.CloseAsync().Awaiter();
            }
        }

        /// <summary>
        /// Creates the consumer, potentially returning a cached instance. 
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>A message consumer</returns>
        public IMessageConsumer CreateConsumer(IDestination destination)
        {
            return CreateConsumerInternalAsync(destination, null, false, null, false, false).GetAsyncResult();
        }

        public Task<IMessageConsumer> CreateConsumerAsync(IDestination destination)
        {
            return CreateConsumerInternalAsync(destination, null, false, null, false, false);
        }

        /// <summary>
        /// Creates the consumer, potentially returning a cached instance. 
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>A message consumer</returns>
        public IMessageConsumer CreateConsumer(IDestination destination, string selector)
        {
            return CreateConsumerInternalAsync(destination, selector, false, null, false, false).GetAsyncResult();
        }

        public Task<IMessageConsumer> CreateConsumerAsync(IDestination destination, string selector)
        {
            return CreateConsumerInternalAsync(destination, selector, false, null, false, false);
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
            return CreateConsumerInternalAsync(destination, selector, noLocal, null, false, false).GetAsyncResult();
        }

        public Task<IMessageConsumer> CreateConsumerAsync(IDestination destination, string selector, bool noLocal)
        {
            return CreateConsumerInternalAsync(destination, selector, noLocal, null, false, false);
        }

        public IMessageConsumer CreateDurableConsumer(ITopic destination, string name)
        {
            return CreateConsumerInternalAsync(destination, null, false, name, false, true).GetAsyncResult();
        }

        public Task<IMessageConsumer> CreateDurableConsumerAsync(ITopic destination, string name)
        {
            return CreateConsumerInternalAsync(destination, null, false, name, false, true);
        }

        public IMessageConsumer CreateDurableConsumer(ITopic destination, string name, string selector)
        {
            return CreateConsumerInternalAsync(destination, selector, false, name, false, true).GetAsyncResult();
        }

        public Task<IMessageConsumer> CreateDurableConsumerAsync(ITopic destination, string name, string selector)
        {
            return CreateConsumerInternalAsync(destination, selector, false, name, false, true);
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
            return CreateConsumerInternalAsync(destination, selector, noLocal, subscription, false, true).GetAsyncResult();
        }
        
        public Task<IMessageConsumer> CreateDurableConsumerAsync(ITopic destination, string name, string selector, bool noLocal)
        {
            return CreateConsumerInternalAsync(destination, selector, noLocal, name, false, true);
        }

        public IMessageConsumer CreateSharedConsumer(ITopic destination, string name)
        {
            return CreateConsumerInternalAsync(destination, null, false, name, true, false).GetAsyncResult();
        }

        public Task<IMessageConsumer> CreateSharedConsumerAsync(ITopic destination, string name)
        {
            return CreateConsumerInternalAsync(destination, null, false, name, true, false);
        }

        public IMessageConsumer CreateSharedConsumer(ITopic destination, string name, string selector)
        {
            return CreateConsumerInternalAsync(destination, selector, false, name, true, false).GetAsyncResult();
        }

        public Task<IMessageConsumer> CreateSharedConsumerAsync(ITopic destination, string name, string selector)
        {
            return CreateConsumerInternalAsync(destination, selector, false, name, true, false);
        }

        public IMessageConsumer CreateSharedDurableConsumer(ITopic destination, string name)
        {
            return CreateConsumerInternalAsync(destination, null, false, name, true, true).GetAsyncResult();
        }

        public Task<IMessageConsumer> CreateSharedDurableConsumerAsync(ITopic destination, string name)
        {
            return CreateConsumerInternalAsync(destination, null, false, name, true, true);
        }

        public IMessageConsumer CreateSharedDurableConsumer(ITopic destination, string name, string selector)
        {
            return CreateConsumerInternalAsync(destination, selector, false, name, true, true).GetAsyncResult();
        }

        public Task<IMessageConsumer> CreateSharedDurableConsumerAsync(ITopic destination, string name, string selector)
        {
            return CreateConsumerInternalAsync(destination, selector, false, name, true, true);
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
            
            target.Unsubscribe(durableSubscriptionName); //DeleteDurableConsumer(durableSubscriptionName);            
        }

        public void Unsubscribe(string name)
        {
            target.Unsubscribe(name);
        }

        public Task UnsubscribeAsync(string name)
        {
            return target.UnsubscribeAsync(name);
        }

        /// <summary>
        /// Creates the consumer.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="noLocal">if set to <c>true</c> [no local].</param>
        /// <param name="subscriptionName">The durable or shared subscription name.</param>
        /// <returns></returns>
        protected async Task<IMessageConsumer> CreateConsumerInternalAsync(IDestination destination, string selector, bool noLocal, string subscriptionName, bool shared, bool durable)
        {
            transactionOpen = true;
            if (shouldCacheConsumers)
            {
                return await GetCachedConsumerAsync(destination, selector, noLocal, subscriptionName, shared, durable).Awaiter();
            }
            else
            {
                if (shared && durable)
                {
                    return await target.CreateSharedDurableConsumerAsync((ITopic) destination, subscriptionName, selector).Awaiter();
                }
                else if (shared)
                {
                    return await target.CreateSharedConsumerAsync((ITopic) destination, subscriptionName, selector).Awaiter();
                }
                else if (durable)
                {
                    return await target.CreateDurableConsumerAsync((ITopic) destination, subscriptionName, selector, noLocal).Awaiter();
                }
                else
                {
                    return await target.CreateConsumerAsync(destination, selector, noLocal).Awaiter();
                }
            }
        }

        private async Task<IMessageConsumer> GetCachedConsumerAsync(IDestination destination, string selector, bool noLocal, string subscriptionName, bool durable, bool shared)
        {
            if ((durable || shared) && subscriptionName == null)
            {
                throw new ArgumentException("Durable or shared subscriptions must have a name");
            }

            var cacheKey = new ConsumerCacheKey(destination, selector, noLocal, subscriptionName, durable, shared);
            if (cachedConsumers.TryGetValue(cacheKey, out var consumer))
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Found cached NMS MessageConsumer for destination [" + destination + "]: " + consumer);
                }
            }
            else
            {
                if (shared && durable)
                {
                    consumer = await target.CreateSharedDurableConsumerAsync((ITopic) destination, subscriptionName, selector).Awaiter();
                }
                else if (shared)
                {
                    consumer = await target.CreateSharedConsumerAsync((ITopic) destination, subscriptionName, selector).Awaiter();
                }
                else if (durable)
                {
                    consumer = await target.CreateDurableConsumerAsync((ITopic) destination, subscriptionName, selector, noLocal).Awaiter();
                }
                else
                {
                    consumer = await target.CreateConsumerAsync(destination, selector, noLocal).Awaiter();
                }
            }

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Creating cached NMS MessageConsumer for destination [" + destination + "]: " + consumer);
            }

            cachedConsumers[cacheKey] = consumer;

            return new CachedMessageConsumer(consumer);
        }

        public Task<IQueueBrowser> CreateBrowserAsync(IQueue queue, string selector)
        {
            transactionOpen = true;
            return target.CreateBrowserAsync(queue, selector);
        }

        /// <summary>
        /// Gets the queue.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IQueue GetQueue(string name)
        {
            return GetQueueAsync(name).GetAsyncResult();
        }

        public async Task<IQueue> GetQueueAsync(string name)
        {
            transactionOpen = true;
            return await target.GetQueueAsync(name).Awaiter();
        }

        /// <summary>
        /// Gets the topic.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public ITopic GetTopic(string name)
        {
            return GetTopicAsync(name).GetAsyncResult();
        }

        public async Task<ITopic> GetTopicAsync(string name)
        {
            transactionOpen = true;
            return await target.GetTopicAsync(name).Awaiter();
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

        public async Task<ITemporaryQueue> CreateTemporaryQueueAsync()
        {
            transactionOpen = true;
            return await target.CreateTemporaryQueueAsync().Awaiter();
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

        public async Task<ITemporaryTopic> CreateTemporaryTopicAsync()
        {
            transactionOpen = true;
            return await target.CreateTemporaryTopicAsync().Awaiter();
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

        public async Task DeleteDestinationAsync(IDestination destination)
        {
            transactionOpen = true;
            await target.DeleteDestinationAsync(destination).Awaiter();
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

        public async Task<IMessage> CreateMessageAsync()
        {
            transactionOpen = true;
            return await target.CreateMessageAsync().Awaiter();
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

        public async Task<ITextMessage> CreateTextMessageAsync()
        {
            transactionOpen = true;
            return await target.CreateTextMessageAsync().Awaiter();
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

        public async Task<ITextMessage> CreateTextMessageAsync(string text)
        {
            transactionOpen = true;
            return await target.CreateTextMessageAsync(text).Awaiter();
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

        public async Task<IMapMessage> CreateMapMessageAsync()
        {
            transactionOpen = true;
            return await target.CreateMapMessageAsync().Awaiter();
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

        public async Task<IObjectMessage> CreateObjectMessageAsync(object body)
        {
            transactionOpen = true;
            return await target.CreateObjectMessageAsync(body).Awaiter();
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

        public async Task<IBytesMessage> CreateBytesMessageAsync()
        {
            transactionOpen = true;
            return await target.CreateBytesMessageAsync().Awaiter();
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

        public async Task<IBytesMessage> CreateBytesMessageAsync(byte[] body)
        {
            transactionOpen = true;
            return await target.CreateBytesMessageAsync(body).Awaiter();
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

        public async Task<IStreamMessage> CreateStreamMessageAsync()
        {
            transactionOpen = true;
            return await target.CreateStreamMessageAsync().Awaiter();
        }
       
        public void Acknowledge()
        {
            transactionOpen = true;
            target.Acknowledge();
        }

        public async Task AcknowledgeAsync()
        {
            transactionOpen = true;
            await target.AcknowledgeAsync().Awaiter();
        }

        /// <summary>
        /// Commits this instance.
        /// </summary>
        public void Commit()
        {
            transactionOpen = false;
            target.Commit();
        }

        public async Task CommitAsync()
        {
            transactionOpen = false;
            await target.CommitAsync().Awaiter();
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
        
        public async Task RecoverAsync()
        {
            transactionOpen = true;
            await target.RecoverAsync().Awaiter();
        }

        /// <summary>
        /// Rollbacks this instance.
        /// </summary>
        public void Rollback()
        {
            transactionOpen = false;
            target.Rollback();
        }

        public async Task RollbackAsync()
        {
            transactionOpen = false;
            await target.RollbackAsync().Awaiter();
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

        public async Task<IQueueBrowser> CreateBrowserAsync(IQueue queue)
        {
            transactionOpen = true;
            return await target.CreateBrowserAsync(queue).Awaiter();
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
        private readonly bool shared;
        private readonly bool durable;

        public ConsumerCacheKey(IDestination destination, string selector, bool noLocal, string subscription, bool durable, bool shared)
        {
            this.destination = destination;
            this.selector = selector;
            this.noLocal = noLocal;
            this.subscription = subscription;
            this.shared = shared;
            this.durable = durable;
        }

        public string Subscription => subscription;

        protected bool Equals(ConsumerCacheKey consumerCacheKey)
        {
            if (consumerCacheKey == null) return false;
            if (!Equals(destination, consumerCacheKey.destination)) return false;
            if (!ObjectUtils.NullSafeEquals(selector, consumerCacheKey.selector)) return false;
            if (!Equals(noLocal, consumerCacheKey.noLocal)) return false;
            if (!ObjectUtils.NullSafeEquals(subscription, consumerCacheKey.subscription)) return false;
            if (shared != consumerCacheKey.shared) return false;
            if (durable != consumerCacheKey.durable) return false;
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