#region License

/*
 * Copyright 2002-2007 the original author or authors.
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

#region Imports

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.Web.SessionState;
using Spring.Collections;
using Spring.Context;
using Spring.Util;
using Spring.Web.Process;
using Spring.Web.Support;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// An abstract base class that defines common behavior for different process implementations.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public abstract class AbstractProcessHandler : IProcess, ISharedStateAware, IApplicationContextAware, IHttpHandler, IRequiresSessionState
    {
        /// <summary>
        /// Parameter name that is used for process ID.
        /// </summary>
        protected internal const string ProcessIdParamName = "pid";

        #region Fields

        private string id = Guid.NewGuid().ToString("N");
        private IProcess parent;
        private object controller;
        private string defaultView;
        private string currentView;
        private IDictionary views = new CaseInsensitiveHashtable(); //CollectionsUtil.CreateCaseInsensitiveHashtable();
        private IDictionary sharedState;
        private IApplicationContext applicationContext;
        private string processUrl;
        private bool viewChanged;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates instance of the process and registers it with the <see cref="Spring.Web.Process.ProcessManager"/>.
        /// </summary>
        public AbstractProcessHandler()
        {
            ProcessManager.RegisterProcess(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Unique ID of this component instance.
        /// </summary>
        public string Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// Gets or sets the parent process.
        /// </summary>
        internal IProcess Parent
        {
            get { return this.parent; }
            set { this.parent = value; }
        }

        /// <summary>
        /// Returns a thread-safe dictionary that contains state that is shared by 
        /// all views of this component.
        /// </summary>
        public IDictionary SharedState
        {
            get { return this.sharedState; }
            set { this.sharedState = value; }
        }

        /// <summary>
        /// Controller for the component.
        /// </summary>
        /// <remarks>
        /// Process controller will be shared by all the views 
        /// that belong to this component.
        /// </remarks>
        public object Controller
        {
            get { return this.controller; }
            set { this.controller = value; }
        }

        /// <summary>
        /// Default view for the component.
        /// </summary>
        public string DefaultView
        {
            get { return this.defaultView; }
            set { this.defaultView = value; }
        }

        /// <summary>
        /// Gets the name of the current view.
        /// </summary>
        public string CurrentView
        {
            get
            {
                if (this.currentView == null)
                {
                    this.CurrentView = this.defaultView;
                }
                return this.currentView;
            }
            set
            {
                string oldView = this.currentView;
                if (this.views.Contains(value))
                {
                    this.currentView = (string) this.views[value];
                }
                else
                {
                    this.currentView = value;
                }
                this.viewChanged = (oldView != this.currentView);
            }
        }

        /// <summary>
        /// Gets the the flag that indicates if selected view 
        /// has changed during the current request.
        /// </summary>
        public bool ViewChanged
        {
            get { return this.viewChanged; }
        }

        /// <summary>
        /// Gets a map of process views.
        /// </summary>
        public IDictionary Views
        {
            get { return this.views; }
        }

        /// <summary>
        /// Gets the process URL.
        /// </summary>
        protected string ProcessUrl
        {
            get { return this.processUrl; }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Starts the process.
        /// </summary>
        /// <param name="url">Process URL.</param>
        public void Start(string url)
        {
            this.processUrl = url;
            this.NavigateToStartView();
        }

        /// <summary>
        /// Resolves and sets the view for the specified view name.
        /// </summary>
        /// <param name="viewName">Name of the view to go to.</param>
        public virtual void SetView(string viewName)
        {
            this.CurrentView = viewName;
            this.NavigateToCurrentView();
        }

        /// <summary>
        /// Ends the process by unregistering it from the <see cref="ProcessManager"/>. 
        /// </summary>
        public virtual void End()
        {
            ProcessManager.UnregisterProcess(this.id);
            if (this.parent != null)
            {
                this.parent.SetView(this.parent.CurrentView);
            }
        }

        #endregion

        #region Abstract methods

        /// <summary>
        /// Method that needs to be implemented by specific process implementations
        /// in order to navigate to the first view in the process.
        /// </summary>
        protected abstract void NavigateToStartView();

        /// <summary>
        /// Method that needs to be implemented by specific process implementations
        /// in order to navigate to the current view.
        /// </summary>
        protected abstract void NavigateToCurrentView();

        #endregion

        #region IHttpHandler implementation

        /// <summary>
        /// Processes the request by delegating to appropriate view, which could be
        /// another process.
        /// </summary>
        /// <param name="context"></param>
        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            IHttpHandler handler = (IHttpHandler) this.applicationContext.GetObject(WebUtils.GetPageName(this.CurrentView));
            this.viewChanged = false;

            if (handler is AbstractProcessHandler)
            {
                ((AbstractProcessHandler) handler).Parent = this;
                // TODO: start child process
            }

            if (handler is IProcessAware)
            {
                ((IProcessAware) handler).Process = this;
            }
            if (handler is ISharedStateAware)
            {
                ((ISharedStateAware) handler).SharedState = this.sharedState;
            }

            context.Handler = handler;
            handler.ProcessRequest(context);
        }

        /// <summary>
        /// Returns true because this wrapper handler can be reused.
        /// Actual page is instantiated at the beginning of the ProcessRequest method.
        /// </summary>
        bool IHttpHandler.IsReusable
        {
            get { return false; }
        }

        #endregion

        #region IApplicationContextAware implementation

        /// <summary>
        /// Gets or sets the application context.
        /// </summary>
        public IApplicationContext ApplicationContext
        {
            get { return this.applicationContext; }
            set { this.applicationContext = value; }
        }

        #endregion
    }
}