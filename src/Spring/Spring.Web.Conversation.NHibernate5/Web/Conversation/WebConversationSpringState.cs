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

using System.Collections;
using System.Web;
using Common.Logging;
using NHibernate;
using Spring.Context;
using Spring.Context.Support;
using Spring.Data.Common;
using Spring.Objects.Factory;

namespace Spring.Web.Conversation
{
    /// <summary>
    /// Implementation of conversation in the infrastructure of Spring.
    /// It avoid Circular Dependencies. 
    /// </summary>
    /// <author>Hailton de Castro</author>
    [Serializable]
    public class WebConversationSpringState : IConversationState, IObjectNameAware, IApplicationContextAware
    {
        private const string SPRING_SESSSION_SCOPE_KEY = "spring.objects";
        private static readonly ILog LOG = LogManager.GetLogger(typeof(WebConversationSpringState));

        /// <summary>
        /// Default message for "CONVERSATION ALREADY HAS A PARENT" error.
        /// </summary>
        public static readonly String MSG_CONVERSATION_ALREADY_HAS_PARENT =
            "This conversation already has a different parent." +
            " Current: '{0}'. Tried: '{1}'";

        private IDictionary<string, object> state = new Dictionary<string, object>();

        #region IConversationState Members

        private String id;
        /// <summary>
        /// <see cref="IConversationState"/>
        /// </summary>
        public string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        /// <summary>
        /// <see cref="IConversationState"/>
        /// </summary>
        /// <returns></returns>
        public void EndConversation()
        {
            if (LOG.IsDebugEnabled) LOG.Debug(String.Format("End of Conversation '{0}'", this.id));

            IDictionary springSessionScope = (IDictionary)HttpContext.Current.Session[SPRING_SESSSION_SCOPE_KEY];
            if (springSessionScope == null)
            {
                throw new InvalidOperationException(string.Format("The 'spring session scope' is not located in the key {0} of HttpSessionState.", SPRING_SESSSION_SCOPE_KEY));
            }

            if (springSessionScope.Contains(this.Id))
            {
                if (LOG.IsDebugEnabled) LOG.Debug(String.Format("EndConversation: Id='{0}' Was Found on 'spring session scope'!", this.id));
                springSessionScope.Remove(this.id);
            }
            else
            {
                if (LOG.IsDebugEnabled) LOG.Debug(String.Format("EndConversation: Id='{0}' Was NOT Found on 'spring session scope!", this.id));
            }

            List<IConversationState> innerConversationsListTemp = new List<IConversationState>(this.InnerConversations);
            foreach (IConversationState innerConversationItem in innerConversationsListTemp)
            {
                innerConversationItem.EndConversation();
            }

            if (this.parentConversation != null)
            {
                this.parentConversation.InnerConversations.Remove(this);
            }

            this.ended = true;
        }

        private bool ended = false;
        /// <summary>
        /// <see cref="IConversationState"/>
        /// </summary>
        public bool Ended
        {
            get { return ended; }
        }

        private int timeOut = 180000;
        /// <summary>
        /// Default 180000.
        /// </summary>
        public int TimeOut
        {
          get { return timeOut; }
          set { timeOut = value; }
        }

        private IList<IConversationState> innerConversations;
        /// <summary>
        /// <see cref="IConversationState"/>
        /// </summary>
        public IList<IConversationState> InnerConversations
        {
            get
            {
                if (this.innerConversations == null)
                {
                    this.innerConversations = new InnerConversationList(this);
                }
                return this.innerConversations;
            }
        }

        private IConversationState parentConversation;
        /// <summary>
        /// <see cref="IConversationState"/>
        /// </summary>
        public IConversationState ParentConversation
        {
            get
            {
                return this.parentConversation;
            }
            set
            {
                if (this.parentConversation != null && this.parentConversation != value)
                {
                    throw new InvalidOperationException(
                        String.Format(MSG_CONVERSATION_ALREADY_HAS_PARENT, this.parentConversation.Id, value.Id));
                }
                if (!this.IsNew)
                {
                    throw new InvalidOperationException(
                        String.Format("This Conversation is not new." +
                        " Conversation.Id: '{0}'. Parent Tried: '{1}'", this.Id, value.Id));
                }
                //Perhaps the father need not be new, only the child needs to be.
                //if (!value.IsNew)
                //{
                //    throw new InvalidOperationException(
                //        String.Format("The Parent conversation is not new." +
                //        " Conversation.Id: '{0}'. Parent Tried: '{1}'", this.Id, value.Id));
                //}

                this.parentConversation = value;
                if (!this.parentConversation.InnerConversations.Contains(this))
                {
                    this.parentConversation.InnerConversations.Add(this);
                }
            }
        }

        private DateTime lastAccess = DateTime.Now;
        /// <summary>
        /// <see cref="LastAccess"/>.
        /// </summary>
        public DateTime LastAccess
        {
            get { return lastAccess; }
            set { lastAccess = value; }
        }

        private IConversationManager conversationManager;
        /// <summary>
        /// <see cref="IConversationState"/>
        /// </summary>
        public IConversationManager ConversationManager
        {
            get
            {
                return conversationManager;
            }
            set 
            { 
                conversationManager = value;
                if (this.conversationManager.GetConversationById(this.Id) == null)
                {
                    this.conversationManager.AddConversation(this);
                }
            }
        }

        /// <summary>
        /// <see cref="IConversationState"/>
        /// </summary>
        public void StartResumeConversation()
        {
            this.isNew = false;
            this.isPaused = false;
            this.lastAccess = DateTime.Now;

            if (this.Ended)
            {
                throw new InvalidOperationException(
                    String.Format(
                        "StartResumeConversation: this conversation is ended." + 
                        " Conversation.Id '{0}'", this.Id));
            }
            if (this.ConversationManager != null)
            {
                if (LOG.IsDebugEnabled) LOG.Debug(String.Format("StartResumeConversation('{0}'): ConversationManager is not null.", this.Id));
                //if this is the root conversation.
                if (this.ParentConversation == null)
                {
                    if (LOG.IsDebugEnabled) LOG.Debug(String.Format("SetActiveConversation('{0}'): ConversationManager is not null.", this.Id));
                    //if this is the root conversation.
                    this.ConversationManager.SetActiveConversation(this);

                    //makes SessionHolder to reopen this session
                    if (this.RootSessionPerConversation != null)
                    {
                        ISession session = this.SessionFactory.GetCurrentSession();
                        if (this.RootSessionPerConversation != session)
                        {
                            //How it is implemented it will never happen, because of the sequence of previous calls: 
                            // ConversationManager.SetActiveConversation -> SessionPerConversationScope.Open
                            // 'new InvalidOperationException("Participating in existing Hibernate SessionFactory IS NOT ALOWED.")' happen first.
                            throw new InvalidOperationException(
                                String.Format(
                                    "StartResumeConversation: this.SessionFactory.GetCurrentSession()" + 
                                    " have a different instance than 'RootSessionPerConversation'" + 
                                    " from conversation '{0}'", this.Id));
                        }
                    }
                }
                else
                {
                    this.ParentConversation.StartResumeConversation();
                }
            }
        }

        private ISession rootSessionPerConversation;
        /// <summary>
        /// <see cref="IConversationState"/>
        /// </summary>
        public ISession RootSessionPerConversation
        {
            get
            {
                if (this.ParentConversation != null)
                {
                    return this.ParentConversation.RootSessionPerConversation;
                }
                else
                {
                    return rootSessionPerConversation;
                }
            }
            set
            {
                if (this.ParentConversation != null)
                {
                    this.ParentConversation.RootSessionPerConversation = value;
                }
                else
                {
                    rootSessionPerConversation = value;
                }
            }
        }

        private String sessionFactoryName;
        /// <summary>
        /// "SessionFactory" name in the current context. 
        /// This approach is required to support serialization.
        /// </summary>
        public String SessionFactoryName
        {
            get { return sessionFactoryName; }
            set { sessionFactoryName = value; }
        }

        [NonSerialized]
        private ISessionFactory sessionFactory;
        /// <summary>
        /// <see cref="IConversationState"/>
        /// </summary>
        public ISessionFactory SessionFactory
        {
            get
            {
                if (this.sessionFactory == null && this.sessionFactoryName != null)
                {
                    this.sessionFactory = this.ApplicationContext.GetObject<ISessionFactory>(this.sessionFactoryName);
                }
                return sessionFactory;
            }
        }

        private String dbProviderName;
        /// <summary>
        /// "DbProvider" name in the current context. 
        /// This approach is required to support serialization.
        /// </summary>
        public String DbProviderName
        {
            get { return dbProviderName; }
            set { dbProviderName = value; }
        }

        [NonSerialized]
        private IDbProvider dbProvider;
        /// <summary>
        /// <see cref="IConversationState"/>
        /// </summary>
        public IDbProvider DbProvider
        {
            get
            {
                if (this.dbProvider == null && this.dbProviderName != null)
                {
                    this.dbProvider = this.ApplicationContext.GetObject<IDbProvider>(this.dbProviderName);
                }
                return dbProvider;
            }
        }

        private bool isNew = true;
        /// <summary>
        /// <see cref="IConversationState"/>
        /// </summary>
        public bool IsNew
        {
            get { return this.isNew; }
        }

        private bool isPaused = true;
        /// <summary>
        /// <see cref="IConversationState"/>
        /// </summary>
        public bool IsPaused
        {
            get { return isPaused; }
        }

        /// <summary>
        /// <see cref="IConversationState"/>
        /// </summary>
        public void PauseConversation()
        {
            this.isPaused = true;
            foreach (IConversationState conversation in this.InnerConversations)
            {
                conversation.PauseConversation();
            }
        }
        #endregion

        #region IObjectNameAware Members
        /// <summary>
        /// <see cref="IObjectNameAware"/>. It is used to valddate <see cref="Id"/>
        /// </summary>
        public string ObjectName
        {
            set
            {
                if (this.id != value)
                {
                    throw new InvalidOperationException(String.Format("Id is different from spring name for this instance.. Currents='{0}', springName='{1}'", this.id, value));
                }
                if (LOG.IsDebugEnabled) LOG.Debug(String.Format("Begin of Conversation '{0}'", this.id));
            }
        }

        #endregion

        #region IDictionary<string,object> Members
        /// <summary>
        /// <see cref="T:IDictionary`2"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, object value)
        {
            this.state.Add(key, value);
        }

        /// <summary>
        /// <see cref="T:IDictionary`2"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            if (this.state.ContainsKey(key))
            {
                return this.state.ContainsKey(key);
            }
            else if (this.parentConversation != null)
            {
                return this.parentConversation.ContainsKey(key);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// <see cref="T:IDictionary`2"/>
        /// </summary>
        public ICollection<string> Keys
        {
            get
            {
                return this.state.Keys;
            }
        }

        /// <summary>
        /// <see cref="T:IDictionary`2"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            return this.state.Remove(key);
        }

        /// <summary>
        /// <see cref="T:IDictionary`2"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string key, out object value)
        {
            if (this.state.TryGetValue(key, out value))
            {
                return true;
            }
            else if (this.parentConversation != null && this.parentConversation.TryGetValue(key, out value))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// <see cref="T:IDictionary`2"/>
        /// </summary>
        public ICollection<object> Values
        {
            get
            {
                return this.state.Values;
            }
        }

        /// <summary>
        /// <see cref="T:IDictionary`2"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get
            {
                if (this.state.ContainsKey(key))
                {
                    return this.state[key];
                }
                else if (this.parentConversation != null)
                {
                    return this.parentConversation[key];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                this.state[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,object>> Members
        /// <summary>
        /// <see cref="T:ICollection`1"/>
        /// </summary>
        /// <param name="item"></param>
        public void Add(KeyValuePair<string, object> item)
        {
            this.state.Add(item);
        }

        /// <summary>
        /// <see cref="T:ICollection`1"/>
        /// </summary>
        public void Clear()
        {
            this.state.Clear();
        }

        /// <summary>
        /// <see cref="T:ICollection`1"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<string, object> item)
        {
            if (this.state.Contains(item))
            {
                return this.state.Contains(item);
            }
            else if (this.parentConversation != null)
            {
                return this.parentConversation.Contains(item);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// <see cref="T:ICollection`1"/>
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            this.state.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// <see cref="T:ICollection`1"/>
        /// </summary>
        public int Count
        {
            get
            {
                return this.state.Count;
            }
        }

        /// <summary>
        /// <see cref="T:ICollection`1"/>
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return this.state.IsReadOnly;
            }
        }

        /// <summary>
        /// <see cref="T:ICollection`1"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(KeyValuePair<string, object> item)
        {
            return this.state.Remove(item);
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,object>> Members
        /// <summary>
        /// <see cref="T:IEnumerable`1"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.state.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members
        /// <summary>
        /// <see cref="IEnumerable"/>
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.state.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// A String representation from conversation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            String innerConsversationsStr = "";

            String conector = "";
            foreach (IConversationState convItem in this.InnerConversations)
            {
                innerConsversationsStr += conector + convItem;
            }

            return String.Format("{{Id='{0}'; this.ParentConversation.Id={1}; InnerConversations=[{2}]}}",
                this.id,
                this.ParentConversation != null ? this.ParentConversation.Id : "<no_parent>",
                innerConsversationsStr
                );
        }

        #region IApplicationContextAware Members
        private String applicationContextName;
        [NonSerialized]
        private IApplicationContext applicationContext = null;
        /// <summary>
        /// Returns the current context. Supports serialization and deserialization.
        /// </summary>
        public IApplicationContext ApplicationContext
        {
            set 
            {
                this.applicationContext = value;
                this.applicationContextName = this.applicationContext.Name;
            }
            get 
            {
                if (this.applicationContext == null)
                {
                    this.applicationContext = ContextRegistry.GetContext(this.applicationContextName);
                }
                return this.applicationContext;
            }
        }

        #endregion
    }
}
