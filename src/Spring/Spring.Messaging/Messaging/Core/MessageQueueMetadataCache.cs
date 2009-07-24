using System;
using System.Collections;
using Common.Logging;
using Spring.Context;
using Spring.Messaging.Support;
using Spring.Objects.Factory;

namespace Spring.Messaging.Core
{
    public class MessageQueueMetadataCache : IApplicationContextAware, IInitializingObject
    {
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof(MessageQueueMetadataCache));

        #endregion

        private readonly IDictionary itemStore = new Hashtable();

        private IConfigurableApplicationContext configurableApplicationContext;
        private IApplicationContext applicationContext;


        private bool isInitialized;
       
        public MessageQueueMetadataCache()
        {
        }

        public MessageQueueMetadataCache(IConfigurableApplicationContext configurableApplicationContext)
        {
            this.configurableApplicationContext = configurableApplicationContext;
        }

        public IApplicationContext ApplicationContext
        {
            set { applicationContext = value; }
        }

        public void Initialize()
        {
            IDictionary messageQueueDictionary = configurableApplicationContext.GetObjectsOfType(typeof(MessageQueueFactoryObject));
            lock (itemStore.SyncRoot)
            {
                foreach (DictionaryEntry entry in messageQueueDictionary)
                {
                    MessageQueueFactoryObject mqfo = entry.Value as MessageQueueFactoryObject;
                    if (mqfo != null)
                    {
                        if (mqfo.Path != null)
                        {
                            Insert(mqfo.Path,
                                   new MessageQueueMetadata(mqfo.RemoteQueue, mqfo.RemoteQueueIsTransactional));
                        } else
                        {
                            #region Logging
                            if (LOG.IsWarnEnabled)
                            {
                                LOG.Warn(
                                    "Path for MessageQueueFactoryObject named [" +
                                    mqfo.ObjectName + "] is null, so can't cache its metadata.");
                            }
                            #endregion
                        }
                    } else
                    {
                        // This would indicate some bug in GetObjectsOfType
                        LOG.Error("Unexpected type of " + entry.Value.GetType() + " was given as candidate for caching MessageQueueMetadata.");
                    }
                }
                isInitialized = true;
            }
        }

        public void AfterPropertiesSet()
        {
            IConfigurableApplicationContext ctx = applicationContext as IConfigurableApplicationContext;
            if (ctx == null)
            {
                throw new InvalidOperationException(
                    "Implementations of IApplicationContext must also implement IConfigurableApplicationContext");
            }
            configurableApplicationContext = ctx;
        }

        /// <summary>
        /// Gets the number of items in the cache.
        /// </summary>
        public int Count
        {
            get
            {
                lock (itemStore.SyncRoot)
                {
                    return itemStore.Count;
                }
            }
        }

        public bool Initalized
        {
            get
            {
                lock (itemStore.SyncRoot)
                {
                    return isInitialized;
                }
            }    
        }
        /// <summary>
        /// Gets a collection of all cache queue paths.
        /// </summary>
        public string[] Paths
        {
            get
            {
                lock (itemStore.SyncRoot)
                {
                    string[] paths = new string[itemStore.Count];
                    int count = 0;                    
                    foreach (object path in itemStore.Keys)
                    {
                        paths[count] = (string) path;
                        count++;
                    }
                    return paths;
                }
            }
        }

        /// <summary>
        /// Retrieves MessageQueueMetadata from the cache.
        /// </summary>
        /// <param name="queuePath">The queue path.</param>
        /// <returns>
        /// Item for the specified <paramref name="queuePath"/>, or <c>null</c>.
        /// </returns>
        public MessageQueueMetadata Get(string queuePath)
        {
            lock (itemStore.SyncRoot)
            {
                return (MessageQueueMetadata) itemStore[queuePath];
            }
        }

        /// <summary>
        /// Removes the specified queue path from the cache
        /// </summary>
        /// <param name="queuePath">The queue path.</param>
        public void Remove(string queuePath)
        {
            lock (itemStore.SyncRoot)
            {
                itemStore.Remove(queuePath);
            }
        }

        /// <summary>
        /// Removes collection of MessageQueueMetaCache from the cache.
        /// </summary>
        /// <param name="queuePaths">
        /// Array of MessageQueue paths to remove.
        /// </param>
        public void RemoveAll(string[] queuePaths)
        {
            lock (itemStore.SyncRoot)
            {
                foreach (string queuePath in queuePaths)
                {
                    itemStore.Remove(queuePath);
                }
            }
        }

        /// <summary>
        /// Removes all MessageQueueMetadata from the cache.
        /// </summary>
        public void Clear()
        {
            lock (itemStore.SyncRoot)
            {
                itemStore.Clear();
            }
        }

        public void Insert(string path, MessageQueueMetadata messageQueueMetadata)
        {
            lock (itemStore.SyncRoot)
            {
                itemStore[path] = messageQueueMetadata;
            }
        }
    }
}