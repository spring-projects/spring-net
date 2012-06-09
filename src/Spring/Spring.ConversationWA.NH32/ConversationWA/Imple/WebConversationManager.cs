using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web;
using Common.Logging;
using Spring.Data.NHibernate.Support;
using NHibernate;

namespace Spring.ConversationWA.Imple
{
    /// <summary>
    /// This was made to stay under session scope.
    /// </summary>
    /// <author>Hailton de Castro</author>
    public class WebConversationManager : SessionPerConversationScope, IConversationManager
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(WebConversationManager));

        private static readonly String CONVERSATION_COOKIE_ID = "WebConversationManager.activeConversationId";

        /// <summary>
        /// Semaphore to synchronize writes to the dictionary.
        /// </summary>
        private Mutex mutexEditDic = new Mutex();
        private IDictionary<String, IConversationState> conversations = new Dictionary<String, IConversationState>();
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
                this.mutexEditDic.WaitOne(5000);
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
                this.mutexEditDic.ReleaseMutex();
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
                this.mutexEditDic.WaitOne(5000);
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
                    this.Close(this.sessionFactory, removeList);
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
                this.mutexEditDic.ReleaseMutex();
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
                this.mutexEditDic.WaitOne(5000);
                this.conversations.Add(conversation.Id, conversation);
            }
            finally
            {
                this.mutexEditDic.ReleaseMutex();
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
        [Obsolete("Not used, the active conversation is defined by call 'IConversationManager.SetActiveConversation' on 'IConversationState.StartResumeConversation'")]
        public void LoadActiveConversation()
        {
            //reset this.activeConversation
            this.activeConversation = null;

            if (LOG.IsDebugEnabled) LOG.Debug("LoadActiveConversation");

            HttpCookie activeConveCookie = HttpContext.Current.Request.Cookies[CONVERSATION_COOKIE_ID];
            if (activeConveCookie != null && !String.IsNullOrEmpty(activeConveCookie.Value))
            {
                if (LOG.IsDebugEnabled) LOG.Debug(String.Format("LoadActiveConversation: cooking found for current active conversation: [{0}]", activeConveCookie.ToString()));
                if (this.conversations.ContainsKey(activeConveCookie.Value))
                {
                    if (LOG.IsDebugEnabled) LOG.Debug(String.Format("LoadActiveConversation: active conversation found for id: '{0}'", activeConveCookie.Value));
                    IConversationState conversation = this.conversations[activeConveCookie.Value];
                    if (conversation != null)
                    {
                        if (LOG.IsDebugEnabled) LOG.Debug(String.Format("LoadActiveConversation: conversation found: '{0}'", conversation.Id));
                        //find root conversation.
                        IConversationState rootConversation = conversation;
                        while (rootConversation.ParenteConversation != null)
                        {
                            rootConversation = rootConversation.ParenteConversation;
                        }
                        rootConversation.StartResumeConversation();
                        this.SetActiveConversation(rootConversation);
                    }
                }
                else
                {
                    if (LOG.IsDebugEnabled) LOG.Debug(String.Format("LoadActiveConversation: conversation NOT found for id on the cookie: '{0}'", activeConveCookie.Value));
                    HttpContext.Current.Response.Cookies.Remove(CONVERSATION_COOKIE_ID);
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

        private ISessionFactory sessionFactory;
        /// <summary>
        /// <see cref="IConversationManager"/>
        /// </summary>
        public ISessionFactory SessionFactory
        {
            get { return sessionFactory; }
            set { sessionFactory = value; }
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
                this.mutexEditDic.WaitOne(5000);
                if (!this.conversations.Remove(conversation.Id))
                {
                    throw new InvalidOperationException(String.Format("Conversation '{0}' not exists on this manager", conversation.Id));
                }
            }
            finally
            {
                this.mutexEditDic.ReleaseMutex();
            }
        }
    }
}
