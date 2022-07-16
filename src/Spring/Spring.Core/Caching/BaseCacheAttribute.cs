using Spring.Core.TypeConversion;
using Spring.Expressions;

namespace Spring.Caching
{
    /// <summary>
    /// Abstract base class containing shared properties for all cache attributes.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public abstract class BaseCacheAttribute : Attribute
    {
        #region Fields

        /// <summary>
        /// The <see cref="TimeSpanConverter"/> instance used to parse <see cref="TimeSpan"/> values.
        /// </summary>
        /// <see cref="TimeToLive"/>
        /// <see cref="TimeToLiveTimeSpan"/>
        protected static readonly TimeSpanConverter TimeSpanConverter = new TimeSpanConverter();

        private string cacheName;
        private string key;
        private IExpression keyExpression;
        private string condition;
        private IExpression conditionExpression;
        private string timeToLive = null;
        private TimeSpan timeToLiveTimeSpan = TimeSpan.MinValue;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an attribute instance.
        /// </summary>
        public BaseCacheAttribute()
        {
        }

        /// <summary>
        /// Creates an attribute instance.
        /// </summary>
        /// <param name="cacheName">
        /// The name of the cache to use.
        /// </param>
        /// <param name="key">
        /// An expression string that should be evaluated in order to determine
        /// the cache key for the item.
        /// </param>
        /// <remarks>The cache key cannot evaluate be null or an empty string.</remarks>
        public BaseCacheAttribute(string cacheName, string key)
        {
            if (null == key)
            {
                throw new ArgumentNullException("key", "The expression for the Cache Key cannot be null.");
            }

            if (key.Trim() == string.Empty)
            {
                throw new ArgumentOutOfRangeException("key", "The expression for the Cache Key cannot be an empty string.");
            }

            this.CacheName = cacheName;
            this.Key = key;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the cache to use.
        /// </summary>
        /// <value>
        /// The name of the cache to use.
        /// </value>
        public string CacheName
        {
            get { return cacheName; }
            set { cacheName = value; }
        }

        /// <summary>
        /// Gets or sets a SpEL expression that should be evaluated in order
        /// to determine the cache key for the item.
        /// </summary>
        /// <value>
        /// An expression string that should be evaluated in order to determine
        /// the cache key for the item.
        /// </value>
        public string Key
        {
            get { return key; }
            set
            {
                key = value;
                keyExpression = Expression.Parse(value);
            }
        }

        /// <summary>
        /// Gets an expression instance that should be evaluated in order
        /// to determine the cache key for the item.
        /// </summary>
        /// <value>
        /// An expression instance that should be evaluated in order to determine
        /// the cache key for the item.
        /// </value>
        public IExpression KeyExpression
        {
            get { return keyExpression; }
        }

        /// <summary>
        /// Gets or sets a SpEL expression that should be evaluated in order
        /// to determine whether the item should be cached.
        /// </summary>
        /// <value>
        /// An expression string that should be evaluated in order to determine
        /// whether the item should be cached.
        /// </value>
        public string Condition
        {
            get { return condition; }
            set
            {
                condition = value;
                conditionExpression = Expression.Parse(value);
            }
        }

        /// <summary>
        /// Gets an expression instance that should be evaluated in order
        /// to determine whether the item should be cached.
        /// </summary>
        /// <value>
        /// An expression instance that should be evaluated in order to determine
        /// whether the item should be cached.
        /// </value>
        public IExpression ConditionExpression
        {
            get { return conditionExpression; }
        }

        /// <summary>
        /// The amount of time an object should remain in the cache.
        /// </summary>
        /// <remarks>
        /// If no TTL is specified, the default TTL defined by the
        /// cache's policy will be applied.
        /// </remarks>
        /// <value>
        /// The amount of time object should remain in the cache
        /// formatted to be recognizable by <see cref="TimeSpan.Parse(string)"/>.
        /// </value>
        public string TimeToLive
        {
            get { return timeToLive; }
            set
            {
                timeToLive = value;
                timeToLiveTimeSpan = (timeToLive == null) ? TimeSpan.MinValue : (TimeSpan)TimeSpanConverter.ConvertFrom(timeToLive);
            }
        }


        /// <summary>
        /// The amount of time an object should remain in the cache (in seconds).
        /// </summary>
        /// <remarks>
        /// If no TTL is specified, the default TTL defined by the
        /// cache's policy will be applied.
        /// </remarks>
        /// <value>
        /// The amount of time object should remain in the cache (in seconds).
        /// </value>
        public TimeSpan TimeToLiveTimeSpan
        {
            get { return timeToLiveTimeSpan; }
        }

        #endregion
    }
}
