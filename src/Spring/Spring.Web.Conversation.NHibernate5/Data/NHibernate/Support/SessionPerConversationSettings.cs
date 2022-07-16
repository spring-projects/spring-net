#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

using NHibernate;

namespace Spring.Data.NHibernate.Support
{
    /// <summary>
    /// Setting for <see cref="SessionPerConversationScope"/>
    /// </summary>
    /// <author>Hailton de Castro</author>
    [Serializable]
    public class SessionPerConversationScopeSettings
    {
        /// <summary>
        /// Default value for <see cref="DefaultFlushMode"/> property.
        /// </summary>
        public static readonly FlushMode FLUSHMODE_DEFAULT = FlushMode.Manual;

        private IInterceptor entityInterceptor;
        private FlushMode defaultFlushMode;

        /// <summary>
        /// Initialize a new instance of <see cref="SessionScopeSettings"/> with default values.
        /// </summary>
        /// <remarks>
        /// Calling this constructor from your derived class leaves <see cref="EntityInterceptor"/>
        /// uninitialized. See <see cref="ResolveEntityInterceptor"/> for more.
        /// </remarks>
        public SessionPerConversationScopeSettings()
        {
            this.entityInterceptor = null;
            this.defaultFlushMode = FLUSHMODE_DEFAULT;
        }

        /// <summary>
        /// Initialize a new instance of <see cref="SessionPerConversationScopeSettings"/> with the given values and references.
        /// </summary>
        /// <param name="entityInterceptor">
        /// Specify the <see cref="IInterceptor"/> to be set on each session provided by the <see cref="SessionPerConversationScope"/> instance.
        /// </param>
        /// <param name="defaultFlushMode">
        /// Specify the flushmode to be applied on each session provided by the <see cref="SessionScope"/> instance.
        /// </param>
        /// <remarks>
        /// Calling this constructor marks all properties initialized.
        /// </remarks>
        public SessionPerConversationScopeSettings(IInterceptor entityInterceptor, FlushMode defaultFlushMode)
        {
            this.entityInterceptor = entityInterceptor;
            this.defaultFlushMode = defaultFlushMode;
        }

        #region Properties

        /// <summary>
        /// Gets the configured <see cref="IInterceptor"/> instance to be used.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public virtual IInterceptor EntityInterceptor
        {
            get
            {
                return entityInterceptor;
            }
            set
            {
                entityInterceptor = value;
            }
        }

        /// <summary>
        /// Gets or Sets the flushmode to be applied on each newly created session.
        /// </summary>
        /// <remarks>
        /// This property defaults to <see cref="FlushMode.Never"/> to ensure that modifying objects outside the boundaries 
        /// of a transaction will not be persisted. It is recommended to not change this value but wrap any modifying operation
        /// within a transaction.
        /// </remarks>
        public FlushMode DefaultFlushMode
        {
            get { return defaultFlushMode; }
            set { defaultFlushMode = value; }
        }

        #endregion

        /// <summary>
        /// Override this method to resolve an <see cref="IInterceptor"/> instance according to your chosen strategy.
        /// </summary>
        protected virtual IInterceptor ResolveEntityInterceptor()
        {
            return null;
        }
    }
}
