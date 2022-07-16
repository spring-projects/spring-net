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

using System.Web;
using System.Web.UI;
using Common.Logging;
using Spring.Context;

namespace Spring.Web.Conversation.HttpModule
{
    /// <summary>
    /// HttpModule for ending Conversations with Timeout exceeded.
    /// </summary>
    /// <author>Hailton de Castro</author>
    public class ConversationModule : IHttpModule, IApplicationContextAware
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ConversationModule));

        private IList<string> conversationManagerName;

        /// <summary>
        /// The Names of the <see cref="IConversationManager"/>s in the <see cref="IApplicationContext"/>
        /// </summary>
        public IList<string> ConversationManagerNameList
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
            context.PreRequestHandlerExecute += context_PreRequestHandlerExecute;
            context.PostRequestHandlerExecute += context_PostRequestHandlerExecute;
            context.EndRequest += context_EndRequest;
        }


        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
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
                page.Unload += page_Unload;

                if (HttpContext.Current.Session != null)
                {
                    if (LOG.IsDebugEnabled) LOG.Debug("context_PreRequestHandlerExecute: Processing HttpContext.Current.Session");
                    foreach (String convMngName in this.ConversationManagerNameList)
                    {
                        if (LOG.IsDebugEnabled) LOG.Debug(string.Format("context_PreRequestHandlerExecute: Processing ConversationManager: {0}", convMngName));
                        IConversationManager convMng = (IConversationManager)this.applicationContext.GetObject(convMngName);
                        convMng.EndOnTimeOut();
                        convMng.FreeEnded();
                    }
                }
                else
                {
                    if (LOG.IsDebugEnabled) LOG.Debug("context_PreRequestHandlerExecute: no HttpContext.Current.Session found.");
                }
            }
        }


        /// <summary>
        /// Handles the Unload event of the page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Necessary for Redirect or Abort for any reason.
        /// </remarks>
        void page_Unload(object sender, EventArgs e)
        {
            if (LOG.IsDebugEnabled) LOG.Debug("page_Unload HttpContext.Current.Session is null: " + (HttpContext.Current.Session == null));
            foreach (String convMngName in this.ConversationManagerNameList)
            {
                if (LOG.IsDebugEnabled) LOG.Debug(string.Format("page_Unload: Processing ConversationManager: {0}", convMngName));
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
        /// Sets the <see cref="Spring.Context.IApplicationContext"/> that this
        /// object runs in.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// 	<p>
        /// Used to obtain the instances of <see cref="IConversationManager"/>
        /// </p>
        /// 	<p>
        /// Invoked after population of normal object properties but before an
        /// init callback such as
        /// <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
        /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
        /// or a custom init-method. Invoked after the setting of any
        /// <see cref="Spring.Context.IResourceLoaderAware"/>'s
        /// <see cref="Spring.Context.IResourceLoaderAware.ResourceLoader"/>
        /// property.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Context.ApplicationContextException">
        /// In the case of application context initialization errors.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If thrown by any application context methods.
        /// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectInitializationException"/>
        public IApplicationContext ApplicationContext
        {
            set { this.applicationContext = value; }
        }

        #endregion
    }
}
