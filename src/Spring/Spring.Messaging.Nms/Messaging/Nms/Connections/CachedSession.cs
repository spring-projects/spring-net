#region License

/*
 * Copyright 2002-2008 the original author or authors.
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
using Apache.NMS;
using Common.Logging;
using Spring.Collections;
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
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof(CachedSession));

        #endregion

        private ISession target;
        private LinkedList sessionList;
        private int sessionCacheSize;
        private IDictionary cachedProducers = new Hashtable();
        private IMessageProducer cachedUnspecifiedDestinationMessageProducer;
        private bool shouldCacheProducers;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedSession"/> class.
        /// </summary>
        /// <param name="targetSession">The target session.</param>
        /// <param name="sessionList">The session list.</param>
        /// <param name="sessionCacheSize">Size of the session cache.</param>
        /// <param name="cacheProducers">if set to <c>true</c> to cache message producers.</param>
        public CachedSession(ISession targetSession, LinkedList sessionList, int sessionCacheSize, bool cacheProducers)
        {
            target = targetSession;
            this.sessionList = sessionList;
            this.sessionCacheSize = sessionCacheSize;
            shouldCacheProducers = cacheProducers;
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
        /// <returns>A message producer, potentially cached.</returns>
        public IMessageProducer CreateProducer()
        {
            if (shouldCacheProducers)
            {
                if (cachedUnspecifiedDestinationMessageProducer != null)
                {
                    #region Logging

                    if (LOG.IsDebugEnabled)
                    {
                        LOG.Debug("Found cached MessageProducer for unspecified destination");
                    }

                    #endregion
                }
                else
                {
                    cachedUnspecifiedDestinationMessageProducer = target.CreateProducer();
                    #region Logging

                    if (LOG.IsDebugEnabled)
                    {
                        LOG.Debug("Created cached MessageProducer for unspecified destination");
                    }

                    #endregion
                }
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
        /// <returns></returns>
        public IMessageProducer CreateProducer(IDestination destination)
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
                    cachedProducers.Add(destination, producer);
                    #region Logging

                    if (LOG.IsDebugEnabled)
                    {
                        LOG.Debug("Created cached MessageProducer for destination [" + destination + "]");
                    }

                    #endregion
                }
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
            lock (sessionList)
            {
                if (sessionList.Count < sessionCacheSize)
                {   //don't pass the call to the underlying target.
                    if (!sessionList.Contains(this))
                    {
                        sessionList.Add(this); //add to end of linked list.
                        #region Logging
                        if (LOG.IsDebugEnabled)
                        {
                            LOG.Debug("Returned cached Session: " + target);
                        }
                        #endregion
                    }
                }
                else
                {
                    foreach (DictionaryEntry entry in cachedProducers)
                    {
                        ((IMessageProducer)entry.Value).Dispose();
                    }
                    target.Close();
                    if (LOG.IsDebugEnabled)
                    {
                        LOG.Debug("Closed cached Session: " + target);
                    }
                }
            }
        }

        #region Pass through implementations
        /// <summary>
        /// Creates the consumer.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns></returns>
        public IMessageConsumer CreateConsumer(IDestination destination)
        {
            return target.CreateConsumer(destination);
        }

        /// <summary>
        /// Creates the consumer.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public IMessageConsumer CreateConsumer(IDestination destination, string selector)
        {
            return target.CreateConsumer(destination, selector);
        }

        /// <summary>
        /// Creates the consumer.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="noLocal">if set to <c>true</c> [no local].</param>
        /// <returns></returns>
        public IMessageConsumer CreateConsumer(IDestination destination, string selector, bool noLocal)
        {
            return target.CreateConsumer(destination, selector, noLocal);
        }


        /// <summary>
        /// Creates the durable consumer.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="name">The name.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="noLocal">if set to <c>true</c> [no local].</param>
        /// <returns></returns>
        public IMessageConsumer CreateDurableConsumer(ITopic destination, string name, string selector, bool noLocal)
        {
            return target.CreateDurableConsumer(destination, name, selector, noLocal);
        }

        /// <summary>
        /// Gets the queue.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IQueue GetQueue(string name)
        {
            return target.GetQueue(name);
        }

        /// <summary>
        /// Gets the topic.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public ITopic GetTopic(string name)
        {
            return target.GetTopic(name);
        }

        /// <summary>
        /// Creates the temporary queue.
        /// </summary>
        /// <returns></returns>
        public ITemporaryQueue CreateTemporaryQueue()
        {
            return target.CreateTemporaryQueue();
        }

        /// <summary>
        /// Creates the temporary topic.
        /// </summary>
        /// <returns></returns>
        public ITemporaryTopic CreateTemporaryTopic()
        {
            return target.CreateTemporaryTopic();
        }

        /// <summary>
        /// Creates the message.
        /// </summary>
        /// <returns></returns>
        public IMessage CreateMessage()
        {
            return target.CreateMessage();
        }

        /// <summary>
        /// Creates the text message.
        /// </summary>
        /// <returns></returns>
        public ITextMessage CreateTextMessage()
        {
            return target.CreateTextMessage();
        }

        /// <summary>
        /// Creates the text message.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public ITextMessage CreateTextMessage(string text)
        {
            return target.CreateTextMessage(text);
        }

        /// <summary>
        /// Creates the map message.
        /// </summary>
        /// <returns></returns>
        public IMapMessage CreateMapMessage()
        {
            return target.CreateMapMessage();
        }

        /// <summary>
        /// Creates the object message.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        public IObjectMessage CreateObjectMessage(object body)
        {
            return target.CreateObjectMessage(body);
        }

        /// <summary>
        /// Creates the bytes message.
        /// </summary>
        /// <returns></returns>
        public IBytesMessage CreateBytesMessage()
        {
            return target.CreateBytesMessage();
        }

        /// <summary>
        /// Creates the bytes message.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        public IBytesMessage CreateBytesMessage(byte[] body)
        {
            return target.CreateBytesMessage(body);
        }

        /// <summary>
        /// Commits this instance.
        /// </summary>
        public void Commit()
        {
            target.Commit();
        }

        /// <summary>
        /// Rollbacks this instance.
        /// </summary>
        public void Rollback()
        {
            target.Rollback();
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="CachedSession"/> is transacted.
        /// </summary>
        /// <value><c>true</c> if transacted; otherwise, <c>false</c>.</value>
        public bool Transacted
        {
            get { return target.Transacted; }
        }

        /// <summary>
        /// Gets the acknowledgement mode.
        /// </summary>
        /// <value>The acknowledgement mode.</value>
        public AcknowledgementMode AcknowledgementMode
        {
            get { return target.AcknowledgementMode; }
        }

        /// <summary>
        /// Call dispose on the target.
        /// </summary>
        public void Dispose()
        {
            target.Dispose();
        }
        #endregion
    }
}