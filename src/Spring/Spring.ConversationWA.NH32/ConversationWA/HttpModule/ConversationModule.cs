using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Spring.Context.Support;
using Common.Logging;
using Spring.Data.NHibernate.Support;
using System.Web.UI;
using Spring.Context;

namespace Spring.ConversationWA.HttpModule
{
    /// <summary>
    /// HttpModule for end Conversation with Timeout exceeded.
    /// </summary>
    /// <author>Hailton de Castro</author>
    public class ConversationModule : IHttpModule, IApplicationContextAware
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ConversationModule));

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ConversationModule(){ }

        private IList<String> conversationManagerName;
        /// <summary>
        /// Name for the IConversationManager on the spring context.
        /// </summary>
        public IList<String> ConversationManagerNameList
        {
            get { return conversationManagerName; }
            set { conversationManagerName = value; }
        }

        #region IHttpModule Members

        /// <summary>
        /// Add PostRequestHandlerExecute event to clear conversations with timeout exceeded.
        /// </summary>
        /// <param name="context"></param>
        public void Init(HttpApplication context)
        {
            context.PreRequestHandlerExecute += new EventHandler(context_PreRequestHandlerExecute);
            context.PostRequestHandlerExecute += new EventHandler(context_PostRequestHandlerExecute);
            context.EndRequest += new EventHandler(context_EndRequest);
        }

        /// <summary>
        /// NOOP.
        /// </summary>
        public void Dispose()
        {
            //noop
        }

        void context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            if (HttpContext.Current.Handler is Page)
            {
                Page page = (Page)HttpContext.Current.Handler;
                page.Unload += new EventHandler(page_Unload);

                if (HttpContext.Current.Session != null)
                {
                    if (LOG.IsDebugEnabled) LOG.Debug("context_PreRequestHandlerExecute: HttpContext.Current.Session is NOT null");
                    foreach (String convMngName in this.ConversationManagerNameList)
                    {
                        IConversationManager convMng = (IConversationManager)this.applicationContext.GetObject(convMngName);
                        convMng.EndOnTimeOut();
                        convMng.FreeEnded();
                    }
                }
                else
                {
                    if (LOG.IsDebugEnabled) LOG.Debug("context_PreRequestHandlerExecute: HttpContext.Current.Session IS null");
                }
            }
        }

        /// <summary>
        /// Necessary for Redirect or Abort for some reason.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void page_Unload(object sender, EventArgs e)
        {
            if (LOG.IsDebugEnabled) LOG.Debug("page_Unload HttpContext.Current.Session is null: " + (HttpContext.Current.Session == null));
            foreach (String convMngName in this.ConversationManagerNameList)
            {
                IConversationManager convMng = (IConversationManager)this.applicationContext.GetObject(convMngName);
                convMng.EndOnTimeOut();
                convMng.FreeEnded();
                convMng.PauseConversations();
            }
        }

        void context_EndRequest(object sender, EventArgs e)
        {
            if (LOG.IsDebugEnabled) LOG.Debug("context_EndRequest HttpContext.Current.Session is null: " + (HttpContext.Current.Session == null));
        }

        void context_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            if (LOG.IsDebugEnabled) LOG.Debug("context_PostRequestHandlerExecute HttpContext.Current.Session is null: " + (HttpContext.Current.Session == null));
            if (HttpContext.Current.Session != null)
            {
            }
        }

        #endregion

        #region IApplicationContextAware Members
        private IApplicationContext applicationContext;
        /// <summary>
        /// Used to obtain the instances of "IConversationManager".
        /// </summary>
        public IApplicationContext ApplicationContext
        {
            set { this.applicationContext = value; }
        }

        #endregion
    }
}
