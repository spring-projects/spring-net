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

using Common.Logging;
using NHibernate;
using Spring.Context;
using Spring.Context.Support;
using Spring.Data.NHibernate.Support;

namespace Spring.Web.Conversation
{
    /// <summary>
    /// This was made to stay under session scope.
    /// </summary>
    /// <author>Hailton de Castro</author>
    [Serializable]
    public class WebConversationManager : SessionPerConversationScope, IConversationManager, IApplicationContextAware
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(WebConversationManager));

        /// <summary>
        /// Semaphore to synchronize writes to the dictionary.
        /// </summary>
        [NonSerialized]
        private Mutex mutexEditDic = new Mutex();

        private Mutex MutexEditDic
        {
            get
            {
                if (this.mutexEditDic == null)
                    this.mutexEditDic = new Mutex();
                return this.mutexEditDic;
            }
        }

        private readonly IDictionary<String, IConversationState> conversations = new Dictionary<String, IConversationState>();
        private IConversationState activeConversation = null;

        #region IConversationManager Members

        /// <summary>
        /// <see cref="IConversationManager"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IConversationState GetConversationById(string id)
        {
            if (this.conversations.ContainsKey(id))
            {
                return this.conversations[id];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// <see cref="IConversationManager"/>
        /// </summary>
        public void EndOnTimeOut()
        {
            try
            {
                if (LOG.IsDebugEnabled) LOG.Debug("EndOnTimeOut");
                this.MutexEditDic.WaitOne(5000);
                foreach (String keyItem in this.conversations.Keys)
                {
                    IConversationState conversationItem = this.conversations[keyItem];
                    if (conversationItem.TimeOut > 0)
                    {
                        if (DateTime.Now.Subtract(conversationItem.LastAccess).TotalMilliseconds > conversationItem.TimeOut)
                        {
                            if (LOG.IsDebugEnabled) LOG.Debug(String.Format("Timeout for conversation '{0}'", conversationItem.Id));
                            conversationItem.EndConversation();
                        }
                    }
                }
            }
            finally
            {
                this.MutexEditDic.ReleaseMutex();
            }
        }

        /// <summary>
        /// <see cref="IConversationManager"/>
        /// </summary>
        public void PauseConversations()
        {
            foreach (IConversationState convItem in this.conversations.Values)
            {
                convItem.PauseConversation();
            }
            this.Close(this.SessionFactory, this.conversations.Values);
        }

        /// <summary>
        /// <see cref="IConversationManager"/>
        /// </summary>
        public void FreeEnded()
        {
            try
            {
                if (LOG.IsDebugEnabled) LOG.Debug("EndOnTimeOut");
                this.MutexEditDic.WaitOne(5000);
                List<IConversationState> removeList = new List<IConversationState>();
                foreach (String keyItem in this.conversations.Keys)
                {
                    IConversationState conversationItem = this.conversations[keyItem];
                    if (conversationItem.Ended)
                    {
                        if (LOG.IsDebugEnabled) LOG.Debug(String.Format("FreeEnded: Release conversation '{0}'", conversationItem.Id));
                        removeList.Add(conversationItem);
                    }
                }

                if (removeList.Count > 0)
                {
                    this.Close(this.SessionFactory, removeList);
                }

                foreach (IConversationState conversationItem in removeList)
                {
                    if (LOG.IsDebugEnabled) LOG.Debug(String.Format("FreeEnded: Remove conversation '{0}'", conversationItem.Id));
                    conversationItem.EndConversation();
                    this.RemoveConversation(conversationItem);
                }
            }
            finally
            {
                this.MutexEditDic.ReleaseMutex();
            }
        }

        /// <summary>
        /// <see cref="IConversationManager"/>
        /// </summary>
        /// <param name="conversation"></param>
        public void AddConversation(IConversationState conversation)
        {
            try
            {
                this.MutexEditDic.WaitOne(5000);
                this.conversations.Add(conversation.Id, conversation);
            }
            finally
            {
                this.MutexEditDic.ReleaseMutex();
            }

            if (conversation.ConversationManager != null && conversation.ConversationManager != this)
            {
                throw new InvalidOperationException(String.Format("Conversation already has another manager. conversation='{0}'", conversation.Id));
            }

            if (conversation.ConversationManager == null)
            {
                conversation.ConversationManager = this;
            }
        }

        /// <summary>
        /// <see cref="IConversationManager"/>
        /// </summary>
        /// <param name="conversation"></param>
        public void SetActiveConversation(IConversationState conversation)
        {
            if (LOG.IsDebugEnabled) LOG.Debug(String.Format("SetActiveConversation('{0}')", conversation.Id));

            //Close connection for the last conversation, if it is open.
            if (this.activeConversation != null && this.activeConversation != conversation)
            {
                this.Close(this.SessionFactory, this.conversations.Values);
            }
            this.Open(conversation, this.conversations.Values);

            this.activeConversation = conversation;

            //Ending the paused conversations.
            if (this.EndPaused)
            {
                foreach (IConversationState convItem in this.conversations.Values)
                {
                    if (convItem.IsPaused)
                    {
                        convItem.EndConversation();
                    }
                }
            }
        }

        /// <summary>
        /// <see cref="IConversationManager"/>
        /// </summary>
        public IConversationState ActiveConversation
        {
            get
            {
                return this.activeConversation;
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
        /// <see cref="IConversationManager"/>
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

        private bool endPaused = false;

        /// <summary>
        /// <see cref="IConversationManager"/>
        /// </summary>
        public bool EndPaused
        {
            get { return endPaused; }
            set { endPaused = value; }
        }
        #endregion

        #region SessionPerConversationScope Members
        /// <summary>
        /// Ends all conversations and Closes all their Session.
        /// </summary>
        public override void Dispose()
        {
            if (LOG.IsDebugEnabled) LOG.Debug("Dispose. End all Conversations");
            foreach (String conversationId in this.conversations.Keys)
            {
                if (LOG.IsDebugEnabled) LOG.Debug(string.Format("Ending Conversation for Conversation Id: {0}", conversationId));
                this.conversations[conversationId].EndConversation();
            }

            this.Close(this.SessionFactory, this.conversations.Values);

            this.conversations.Clear();
            this.sessionFactory = null;
        }
        #endregion

        /// <summary>
        /// Remove conversation.
        /// </summary>
        /// <param name="conversation"></param>
        private void RemoveConversation(IConversationState conversation)
        {
            try
            {
                this.MutexEditDic.WaitOne(5000);
                if (!this.conversations.Remove(conversation.Id))
                {
                    throw new InvalidOperationException(String.Format("Conversation '{0}' does not exist on this manager.", conversation.Id));
                }
            }
            finally
            {
                this.MutexEditDic.ReleaseMutex();
            }
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
