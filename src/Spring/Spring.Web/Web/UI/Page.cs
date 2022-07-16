#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using Spring.Collections;
using Spring.Context;
using Spring.Context.Support;
using Spring.DataBinding;
using Spring.Globalization;
using Spring.Globalization.Resolvers;
using Spring.Objects;
using Spring.Util;
using Spring.Validation;
using Spring.Web.Support;
using IValidator = Spring.Validation.IValidator;

#endregion

namespace Spring.Web.UI
{
    #region Result support

    #endregion

    /// <summary>
    /// Represents an .aspx file, also known as a Web Forms page, requested from a
    /// server that hosts an ASP.NET Web application.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The <b>Page</b> class is associated with files that have an .aspx extension.
    /// These files are compiled at run time as Page objects and cached in server memory.
    /// </p>
    /// <p>
    /// This class extends <see cref="System.Web.UI.Page"/> and adds support for master
    /// pages similar to upcoming ASP.Net 2.0 master pages feature.
    /// </p>
    /// <p>
    /// It also adds support for automatic localization using local page resource file, and
    /// simplifies access to global resources (resources from the message source for the
    /// application context).
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    [AspNetHostingPermission( SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal )]
    [AspNetHostingPermission( SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal )]
    public class Page : System.Web.UI.Page, IHttpHandler, IApplicationContextAware, ISharedStateAware,
                        ISupportsWebDependencyInjection, IWebDataBound, IValidationContainer, IWebNavigable
    {
        #region Constants

        private static readonly object EventInitializeControls = new object();
        private static readonly object EventPreLoadViewState = new object();
        private static readonly object EventDataBindingsInitialized = new object();
        private static readonly object EventDataBound = new object();
        private static readonly object EventDataUnbound = new object();

        #endregion

        #region Instance Fields

        private object controller;
        private IDictionary sharedState;

        private ILocalizer localizer;
        private ICultureResolver cultureResolver = new DefaultWebCultureResolver();
        private IMessageSource messageSource;
        private IBindingContainer bindingManager;
        private IValidationErrors validationErrors = new ValidationErrors();
        private IWebNavigator webNavigator;
        private IDictionary args;
        private IApplicationContext applicationContext;
        private IApplicationContext defaultApplicationContext;
        private static readonly string traceCategory = "Spring.Page";

        private IDictionary styles = new ListDictionary();
        private IDictionary styleFiles = new ListDictionary();
        private IDictionary headScripts = new ListDictionary();
        private string cssRoot = "CSS";
        private string scriptsRoot = "Scripts";
        private string imagesRoot = "Images";

        #endregion

        #region Page lifecycle methods

        /// <summary>
        /// Creates and initializes the new page instance.
        /// </summary>
        /// <remarks>
        /// Calls <see cref="InitializeNavigationSupport"/>.
        /// </remarks>
        public Page()
        {
            InitializeNavigationSupport();
        }

        /// <summary>
        /// Initializes Spring.NET page internals and raises the PreInit event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnPreInit( EventArgs e )
        {
            if (SharedState == null)
            {
                SharedState = new CaseInsensitiveHashtable();
            }
            InitializeCulture();
            InitializeMessageSource();

            base.OnPreInit( e );
        }

        /// <summary>
        /// Initializes the culture.
        /// </summary>
        protected override void InitializeCulture()
        {
            CultureInfo userCulture = this.UserCulture;
            Thread.CurrentThread.CurrentUICulture = userCulture;
            if (userCulture.IsNeutralCulture)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture( userCulture.Name );
            }
            else
            {
                Thread.CurrentThread.CurrentCulture = userCulture;
            }
        }

        /// <summary>
        /// Initializes data model and controls.
        /// </summary>
        protected override void OnInit( EventArgs e )
        {
            InitializeBindingManager();

            if (!IsPostBack)
            {
                InitializeModel();
            }
            else
            {
                LoadModel( LoadModelFromPersistenceMedium() );
            }

            base.OnInit( e );

            // initialize controls
            Trace.Write( traceCategory, "Initialize Controls" );
            OnInitializeControls( EventArgs.Empty );
        }

        /// <summary>
        /// Raises the <see cref="PreLoadViewState"/> event after page initialization.
        /// </summary>
        protected virtual void OnPreLoadViewState( EventArgs e )
        {
            EventHandler handler = (EventHandler)base.Events[EventPreLoadViewState];
            if (handler != null)
            {
                handler( this, e );
            }
        }

        /// <summary>
        /// PreLoadViewState event.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event is raised if <see cref="System.Web.UI.Page.IsPostBack"/> is true
        /// immediately before state is restored from ViewState.
        /// </para>
        /// <para>
        /// NOTE: Different from <see cref="System.Web.UI.Control.LoadViewState(object)"/>, this event
        /// will also be raised if the control has no ViewState or ViewState is disabled.
        /// </para>
        /// </remarks>
        public event EventHandler PreLoadViewState
        {
            add { base.Events.AddHandler( EventPreLoadViewState, value ); }
            remove { base.Events.RemoveHandler( EventPreLoadViewState, value ); }
        }

        /// <summary>
        /// Raises the PreLoadViewState event for
        /// this page and all contained controls.
        /// </summary>
        private void RaisePreLoadViewStateEvent()
        {
            this.OnPreLoadViewState( EventArgs.Empty );
            if (this.HasControls())
            {
                PreLoadViewStateRecursive( this.Controls );
            }
        }

        /// <summary>
        /// Recursively raises PreLoadViewState event.
        /// </summary>
        private void PreLoadViewStateRecursive( ControlCollection controls )
        {
            for (int i = 0; i < controls.Count; i++)
            {
                Control control = controls[i];

                if (control is UserControl)
                {
                    ((UserControl)control).OnPreLoadViewState( EventArgs.Empty );
                }

                if (control.HasControls())
                {
                    PreLoadViewStateRecursive( control.Controls );
                }
            }
        }

        /// <summary>
        /// Overridden to add support for <see cref="PreLoadViewState"/>
        /// </summary>
        /// <remarks>
        /// If necessary override <see cref="LoadPageStateFromPersistenceMediumBase"/> instead of this method.
        /// </remarks>
        protected override object LoadPageStateFromPersistenceMedium()
        {
            RaisePreLoadViewStateEvent();

            // If ViewState is disabled, use BindFormData() to populate controls.
            BindFormDataIfNecessary();

            // continue with default behaviour
            return LoadPageStateFromPersistenceMediumBase();
        }

        /// <summary>
        /// If ViewState is disabled, calls <see cref="BindFormData"/> recursively for all controls.
        /// </summary>
        /// <remarks>
        /// If ViewState is disabled, DropDownLists etc. might not fire "Changed" events.
        /// </remarks>
        protected virtual void BindFormDataIfNecessary()
        {
            if (!IsViewStateEnabled)
            {
                BindFormDataRecursive();
            }
        }

        /// <summary>
        /// Calls <see cref="BindFormData"/> recursively for all controls.
        /// </summary>
        protected void BindFormDataRecursive()
        {
            this.BindFormData();
            if (this.HasControls())
            {
                BindFormDataRecursive( this.Controls );
            }
        }

        /// <summary>
        /// Recursively calls <see cref="BindFormData"/> for all controls.
        /// </summary>
        private void BindFormDataRecursive( ControlCollection controls )
        {
            for (int i = 0; i < controls.Count; i++)
            {
                Control control = controls[i];

                if (control is UserControl)
                {
                    ((UserControl)control).BindFormData();
                }

                if (control.HasControls())
                {
                    BindFormDataRecursive( control.Controls );
                }
            }
        }

        /// <summary>
        /// If necessary override this method instead of <see cref="LoadPageStateFromPersistenceMedium"/>
        /// </summary>
        protected virtual object LoadPageStateFromPersistenceMediumBase()
        {
            return base.LoadPageStateFromPersistenceMedium();
        }

        /// <summary>
        /// Initializes dialog result and unbinds data from the controls
        /// into a data model.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnLoad( EventArgs e )
        {
            // create dialog result if necessary
            //            if (GetType().IsDefined(typeof(DialogAttribute), true))
            //            {
            //                if (!IsPostBack)
            //                {
            //                    ViewState["__dialogResult"] = "redirect:" + Request.UrlReferrer.AbsoluteUri;
            //                }
            //                Results["close"] = new Result((string) ViewState["__dialogResult"]);
            //            }

            if (IsPostBack)
            {
                // unbind form data
                UnbindFormData();
            }


            Trace.Write( traceCategory, "Execute Handlers for Load Event" );
            base.OnLoad( e );
        }

        /// <summary>
        /// Binds data from the data model into controls and raises
        /// PreRender event afterwards.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnPreRender( EventArgs e )
        {
            // bind data from model to form
            BindFormData();

            if (localizer != null)
            {
                Trace.Write( traceCategory, "Apply Localized Resources" );
                localizer.ApplyResources( this, messageSource, UserCulture );
            }

            base.OnPreRender( e );

            object modelToSave = SaveModel();
            if (modelToSave != null)
            {
                SaveModelToPersistenceMedium( modelToSave );
            }
        }

        /// <summary>
        /// This event is raised before Init event and should be used to initialize
        /// controls as necessary.
        /// </summary>
        public event EventHandler InitializeControls
        {
            add { base.Events.AddHandler( EventInitializeControls, value ); }
            remove { base.Events.RemoveHandler( EventInitializeControls, value ); }
        }

        /// <summary>
        /// Raises InitializeControls event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnInitializeControls( EventArgs e )
        {
            EventHandler handler = (EventHandler)base.Events[EventInitializeControls];
            if (handler != null)
            {
                handler( this, e );
            }
        }

        /// <summary>
        /// Obtains a <see cref="T:System.Web.UI.UserControl"/> object from a user control file
        /// and injects dependencies according to Spring config file.
        /// </summary>
        /// <param name="virtualPath">The virtual path to a user control file.</param>
        /// <returns>
        /// Returns the specified <see langword="UserControl"/> object, with dependencies injected.
        /// </returns>
        protected virtual new Control LoadControl( string virtualPath )
        {
            Control control = base.LoadControl( virtualPath );
            control = WebDependencyInjectionUtils.InjectDependenciesRecursive( defaultApplicationContext, control );
            return control;
        }

        /// <summary>
        /// Obtains a <see cref="T:System.Web.UI.UserControl"/> object by type
        /// and injects dependencies according to Spring config file.
        /// </summary>
        /// <param name="t">The type of a user control.</param>
        /// <param name="parameters">parameters to pass to the control</param>
        /// <returns>
        /// Returns the specified <see langword="UserControl"/> object, with dependencies injected.
        /// </returns>
        protected virtual new Control LoadControl( Type t, params object[] parameters )
        {
            Control control = base.LoadControl( t, parameters );
            control = WebDependencyInjectionUtils.InjectDependenciesRecursive( defaultApplicationContext, control );
            return control;
        }

        #endregion

        #region Model Management Support

        private IModelPersistenceMedium modelPersistenceMedium = new SessionModelPersistenceMedium();

        /// <summary>
        /// Set the <see cref="IModelPersistenceMedium"/> strategy for storing model
        /// instances between requests.
        /// </summary>
        /// <remarks>
        /// By default the <see cref="SessionModelPersistenceMedium"/> strategy is used.
        /// </remarks>
        public IModelPersistenceMedium ModelPersistenceMedium
        {
            set
            {
                AssertUtils.ArgumentNotNull(value, "ModelPersistenceMedium");
                modelPersistenceMedium = value;
            }
        }

        /// <summary>
        /// Retrieves data model from a persistence store.
        /// </summary>
        /// <remarks>
        /// The default implementation uses <see cref="System.Web.UI.Page.Session"/> to store and retrieve
        /// the model for the current <see cref="System.Web.HttpRequest.CurrentExecutionFilePath" />
        /// </remarks>
        protected virtual object LoadModelFromPersistenceMedium()
        {
            //return Session[Request.CurrentExecutionFilePath + ".Model"];
            return modelPersistenceMedium.LoadFromMedium(this);
        }

        /// <summary>
        /// Saves data model to a persistence store.
        /// </summary>
        /// <remarks>
        /// The default implementation uses <see cref="System.Web.UI.Page.Session"/> to store and retrieve
        /// the model for the current <see cref="System.Web.HttpRequest.CurrentExecutionFilePath" />
        /// </remarks>
        protected virtual void SaveModelToPersistenceMedium( object modelToSave )
        {
            //Session[Request.CurrentExecutionFilePath + ".Model"] = modelToSave;
            modelPersistenceMedium.SaveToMedium(this, modelToSave);
        }

        /// <summary>
        /// Initializes data model when the page is first loaded.
        /// </summary>
        /// <remarks>
        /// This method should be overriden by the developer
        /// in order to initialize data model for the page.
        /// </remarks>
        protected virtual void InitializeModel()
        {
        }

        /// <summary>
        /// Loads the saved data model on postback.
        /// </summary>
        /// <remarks>
        /// This method should be overriden by the developer
        /// in order to load data model for the page.
        /// </remarks>
        protected virtual void LoadModel( object savedModel )
        {
        }

        /// <summary>
        /// Returns a model object that should be saved.
        /// </summary>
        /// <remarks>
        /// This method should be overriden by the developer
        /// in order to save data model for the page.
        /// </remarks>
        /// <returns>
        /// A model object that should be saved.
        /// </returns>
        protected virtual object SaveModel()
        {
            return null;
        }

        #endregion

        #region Process and Controller support

        /// <summary>
        /// Gets or sets controller for the page.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Application pages should shadow this property and change its type
        /// in order to make calls to controller within the page as simple as possible.
        /// </para>
        /// <para>
        /// If external controller is not specified, page will serve as its own controller,
        /// which will allow data binding to work properly.
        /// </para>
        /// </remarks>
        /// <value>Controller for the page. Defaults to the page itself.</value>
        public object Controller
        {
            get { return GetController(); }
            set { SetController( value ); }
        }

        /// <summary>
        /// <para>Stores the controller to be returned by <see cref="Controller"/> property.</para>
        /// </summary>
        /// <remarks>
        /// The default implementation uses a field to store the reference. Derived classes may override this behaviour
        /// but must ensure to also change the behaviour of <see cref="GetController"/> accordingly.
        /// </remarks>
        /// <param name="controller">Controller for the page.</param>
        protected virtual void SetController( object controller )
        {
            this.controller = controller;
        }

        /// <summary>
        /// <para>Returns the controller stored by <see cref="SetController"/>.</para>
        /// </summary>
        /// <remarks>
        /// The default implementation uses a field to retrieve the reference. Derived classes may override this behaviour
        /// but must ensure to also change the behaviour of <see cref="SetController"/> accordingly.
        /// </remarks>
        /// <returns>
        /// The controller for this page.
        /// <list type="bullet">
        /// <item>If no controller is set, a reference to the page itself is returned.</item>
        /// </list>
        /// </returns>
        protected virtual object GetController()
        {
            if (controller == null)
            {
                return this;
            }
            return controller;
        }

        #endregion

        #region Shared State support

        /// <summary>
        /// Returns a thread-safe dictionary that contains state that is shared by
        /// all instances of this page.
        /// </summary>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public IDictionary SharedState
        {
            get { return this.sharedState; }
            set { this.sharedState = value; }
        }

        #endregion

        /// <summary>
        /// Overrides the default PreviousPage property to return an instance of <see cref="Spring.Web.UI.Page"/>,
        /// and to work properly during server-side transfers and executes.
        /// </summary>
        public new Page PreviousPage
        {
            get { return this.Context.PreviousHandler as Page; }
        }

        ///<summary>
        /// Publish <see cref="HttpContext"/> associated with this page for convenient usage in Binding Expressions
        ///</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new virtual HttpContext Context
        {
            get {
                return base.Context; }
        }

        #region Master Page support

        /// <summary>
        /// Gets the master page that determines the overall look of the page.
        /// </summary>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public new MasterPage Master
        {
            get { return (MasterPage)base.Master; }
        }

        /// <summary>
        /// Returns true if page uses master page, false otherwise.
        /// </summary>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public bool HasMaster
        {
            get { return Master != null || MasterPageFile != null; }
        }

        #endregion

        #region CSS support

        /// <summary>
        /// Gets a dictionary of registered styles.
        /// </summary>
        /// <value>Registered styles.</value>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public IDictionary Styles
        {
            get { return styles; }
        }

        /// <summary>
        /// Gets a dictionary of registered style files.
        /// </summary>
        /// <value>Registered style files.</value>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public IDictionary StyleFiles
        {
            get { return styleFiles; }
        }

        /// <summary>
        /// Registers single CSS style with the page.
        /// </summary>
        /// <param name="name">Style name.</param>
        /// <param name="style">Style definition.</param>
        public void RegisterStyle( string name, string style )
        {
            styles[name] = style;
        }

        /// <summary>
        /// Returns <c>True</c> if specified style is registered, <c>False</c> otherwise.
        /// </summary>
        /// <param name="name">Style name.</param>
        /// <returns><c>True</c> if specified style is registered, <c>False</c> otherwise.</returns>
        public bool IsStyleRegistered( string name )
        {
            return styles.Contains( name );
        }

        /// <summary>
        /// Registers CSS file with the page.
        /// </summary>
        /// <param name="key">Style file key.</param>
        /// <param name="fileName">Style file name.</param>
        public void RegisterStyleFile( string key, string fileName )
        {
            styleFiles[key] = fileName;
        }

        /// <summary>
        /// Returns <c>True</c> if specified style file is registered, <c>False</c> otherwise.
        /// </summary>
        /// <param name="key">Style file key.</param>
        /// <returns><c>True</c> if specified style file is registered, <c>False</c> otherwise.</returns>
        public bool IsStyleFileRegistered( string key )
        {
            return styleFiles.Contains( key );
        }

        #endregion

        #region Client script support

        /// <summary>
        /// Gets a dictionary of registered head scripts.
        /// </summary>
        /// <value>Registered head scripts.</value>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public IDictionary HeadScripts
        {
            get { return headScripts; }
        }

        /// <summary>
        /// Registers script block that should be rendered within the <c>head</c> HTML element.
        /// </summary>
        /// <param name="key">Script key.</param>
        /// <param name="script">Script text.</param>
        public void RegisterHeadScriptBlock( string key, string script )
        {
            RegisterHeadScriptBlock( key, Script.DefaultType, script );
        }

        /// <summary>
        /// Registers script block that should be rendered within the <c>head</c> HTML element.
        /// </summary>
        /// <param name="key">Script key.</param>
        /// <param name="type">Script language MIME type.</param>
        /// <param name="script">Script text.</param>
        public void RegisterHeadScriptBlock( string key, MimeMediaType type, string script )
        {
            headScripts[key] = new ScriptBlock( type, script );
        }

        /// <summary>
        /// Registers script file that should be referenced within the <c>head</c> HTML element.
        /// </summary>
        /// <param name="key">Script key.</param>
        /// <param name="fileName">Script file name.</param>
        public void RegisterHeadScriptFile( string key, string fileName )
        {
            RegisterHeadScriptFile( key, Script.DefaultType, fileName );
        }

        /// <summary>
        /// Registers script file that should be referenced within the <c>head</c> HTML element.
        /// </summary>
        /// <param name="key">Script key.</param>
        /// <param name="type">Script language MIME type.</param>
        /// <param name="fileName">Script file name.</param>
        public void RegisterHeadScriptFile( string key, MimeMediaType type, string fileName )
        {
            headScripts[key] = new ScriptFile( type, fileName );
        }

        /// <summary>
        /// Registers script block that should be rendered within the <c>head</c> HTML element.
        /// </summary>
        /// <param name="key">Script key.</param>
        /// <param name="element">Element ID of the event source.</param>
        /// <param name="eventName">Name of the event to handle.</param>
        /// <param name="script">Script text.</param>
        public void RegisterHeadScriptEvent( string key, string element, string eventName, string script )
        {
            RegisterHeadScriptEvent( key, Script.DefaultType, element, eventName, script );
        }

        /// <summary>
        /// Registers script block that should be rendered within the <c>head</c> HTML element.
        /// </summary>
        /// <param name="key">Script key.</param>
        /// <param name="mimeType">The scripting language's MIME type.</param>
        /// <param name="element">Element ID of the event source.</param>
        /// <param name="eventName">Name of the event to handle.</param>
        /// <param name="script">Script text.</param>
        public void RegisterHeadScriptEvent( string key, MimeMediaType mimeType, string element, string eventName, string script )
        {
            headScripts[key] = new ScriptEvent( mimeType, element, eventName, script );
        }

        /// <summary>
        /// Returns <c>True</c> if specified head script is registered, <c>False</c> otherwise.
        /// </summary>
        /// <param name="key">Script key.</param>
        /// <returns><c>True</c> if specified head script is registered, <c>False</c> otherwise.</returns>
        public bool IsHeadScriptRegistered( string key )
        {
            return headScripts.Contains( key );
        }

        #endregion

        #region Well-known folders support

        /// <summary>
        /// Gets or sets the CSS root.
        /// </summary>
        /// <value>The CSS root.</value>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public string CssRoot
        {
            get { return WebUtils.CreateAbsolutePath( Request.ApplicationPath, cssRoot ); }
            set { cssRoot = value; }
        }

        /// <summary>
        /// Gets or sets the scripts root.
        /// </summary>
        /// <value>The scripts root.</value>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public string ScriptsRoot
        {
            get { return WebUtils.CreateAbsolutePath( Request.ApplicationPath, scriptsRoot ); }
            set { scriptsRoot = value; }
        }

        /// <summary>
        /// Gets or sets the images root.
        /// </summary>
        /// <value>The images root.</value>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public string ImagesRoot
        {
            get { return WebUtils.CreateAbsolutePath( Request.ApplicationPath, imagesRoot ); }
            set { imagesRoot = value; }
        }

        #endregion

        #region Result support

        /// <summary>
        /// Ensure, that <see cref="WebNavigator"/> is set to a valid instance.
        /// </summary>
        /// <remarks>
        /// If <see cref="WebNavigator"/> is not already set, creates and sets a new <see cref="WebFormsResultWebNavigator"/> instance.<br/>
        /// Override this method if you don't want to inject a navigator, but need a different default.
        /// </remarks>
        protected virtual void InitializeNavigationSupport()
        {
            webNavigator = new WebFormsResultWebNavigator( this, null, null, true );
        }

        /// <summary>
        /// Gets/Sets the navigator to be used for handling <see cref="SetResult(string, object)"/> calls.
        /// </summary>
        public virtual IWebNavigator WebNavigator
        {
            get
            {
                return webNavigator;
            }
            set
            {
                webNavigator = value;
            }
        }

        /// <summary>
        /// Gets or sets map of result names to target URLs
        /// </summary>
        /// <remarks>
        /// Using <see cref="Results"/> requires <see cref="WebNavigator"/> to implement <see cref="IResultWebNavigator"/>.
        /// </remarks>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public virtual IDictionary Results
        {
            get
            {
                if (WebNavigator is IResultWebNavigator)
                {
                    return ((IResultWebNavigator)WebNavigator).Results;
                }
                return null;
            }
            set
            {
                if (WebNavigator is IResultWebNavigator)
                {
                    ((IResultWebNavigator)WebNavigator).Results = value;
                    return;
                }
                throw new NotSupportedException( "WebNavigator must be of type IResultWebNavigator to support Results" );
            }
        }

        /// <summary>
        /// A convenience, case-insensitive table that may be used to e.g. pass data into SpEL expressions"/>.
        /// </summary>
        /// <remarks>
        /// By default, e.g. <see cref="SetResult(string)"/> passes the control instance into an expression. Using
        /// <see cref="Args"/> is an easy way to pass additional parameters into the expression
        /// <example>
        /// This example shows how to pass an arbitrary value 'age' into a result expression.
        /// <code>
        /// // config:
        ///
        /// &lt;property Name=&quot;Results&quot;&gt;
        ///   &lt;dictionary&gt;
        ///   		&lt;entry key=&quot;ok_clicked&quot; value=&quot;redirect:~/ShowResult.aspx?age=%{Args['age']}&quot; /&gt;
        ///   &lt;/dictionary&gt;
        /// &lt;/property&gt;
        ///
        /// // code:
        ///
        /// void OnOkClicked(object sender, EventArgs e)
        /// {
        ///   Args[&quot;result&quot;] = txtAge.Text;
        ///   SetResult(&quot;ok_clicked&quot;);
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        public IDictionary Args
        {
            get
            {
                if (args == null)
                {
                    args = new CaseInsensitiveHashtable();
                }
                return args;
            }
        }

        /// <summary>
        /// Redirects user to a URL mapped to specified result name.
        /// </summary>
        /// <param name="resultName">Result name.</param>
        protected void SetResult( string resultName )
        {
            WebNavigator.NavigateTo( resultName, this, null );
        }

        /// <summary>
        /// Redirects user to a URL mapped to specified result name.
        /// </summary>
        /// <param name="resultName">Name of the result.</param>
        /// <param name="context">The context to use for evaluating the SpEL expression in the Result.</param>
        protected void SetResult( string resultName, object context )
        {
            WebNavigator.NavigateTo( resultName, this, context );
        }

        /// <summary>
        /// Returns a redirect url string that points to the
        /// <see cref="Spring.Web.Support.Result.TargetPage"/> defined by this
        /// result evaluated using this Page for expression
        /// </summary>
        /// <param name="resultName">Name of the result.</param>
        /// <returns>A redirect url string.</returns>
        protected string GetResultUrl( string resultName )
        {
            return ResolveUrl( WebNavigator.GetResultUri( resultName, this, null ) );
        }

        /// <summary>
        /// Returns a redirect url string that points to the
        /// <see cref="Spring.Web.Support.Result.TargetPage"/> defined by this
        /// result evaluated using this Page for expression
        /// </summary>
        /// <param name="resultName">Name of the result.</param>
        /// <param name="context">The context to use for evaluating the SpEL expression in the Result</param>
        /// <returns>A redirect url string.</returns>
        protected string GetResultUrl( string resultName, object context )
        {
            return ResolveUrl( WebNavigator.GetResultUri( resultName, this, context ) );
        }

        #endregion

        #region Validation support

        ///<summary>
        ///Instructs any validation controls included on the page to validate their assigned information.
        ///</summary>
        ///
        public new virtual void Validate()
        {
            base.Validate();
        }

        ///<summary>
        ///Instructs the validation controls in the specified validation group to validate their assigned information.
        ///</summary>
        ///
        ///<param name="validationGroup">The validation group name of the controls to validate.</param>
        public new virtual void Validate(string validationGroup)
        {
            base.Validate(validationGroup);
        }

        /// <summary>
        /// Evaluates specified validators and returns <c>True</c> if all of them are valid.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Each validator can itself represent a collection of other validators if it is
        /// an instance of <see cref="ValidatorGroup"/> or one of its derived types.
        /// </p>
        /// <p>
        /// Please see the Validation Framework section in the documentation for more info.
        /// </p>
        /// </remarks>
        /// <param name="validationContext">Object to validate.</param>
        /// <param name="validators">Validators to evaluate.</param>
        /// <returns>
        /// <c>True</c> if all of the specified validators are valid, <c>False</c> otherwise.
        /// </returns>
        public bool Validate( object validationContext, params IValidator[] validators )
        {
            IDictionary<string, object> contextParams = CreateValidatorParameters();
            bool result = true;
            foreach (IValidator validator in validators)
            {
                if (validator == null)
                {
                    throw new ArgumentException( "Validator is not defined." );
                }
                result = validator.Validate( validationContext, contextParams, this.ValidationErrors ) && result;
            }

            return result;
        }

        /// <summary>
        /// Gets or sets the validation errors container.
        /// </summary>
        /// <value>The validation errors container.</value>
        public virtual IValidationErrors ValidationErrors
        {
            get { return validationErrors; }
            set
            {
                AssertUtils.ArgumentNotNull(value, "ValidationErrors");
                validationErrors = value;
            }
        }

        /// <summary>
        /// Creates the validator parameters.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method can be overriden if you want to pass additional parameters
        /// to the validation framework, but you should make sure that you call
        /// this base implementation in order to add page, session, application,
        /// request, response and context to the variables collection.
        /// </para>
        /// </remarks>
        /// <returns>
        /// Dictionary containing parameters that should be passed to
        /// the data validation framework.
        /// </returns>
        protected virtual IDictionary<string, object> CreateValidatorParameters()
        {
            IDictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["page"] = this;
            parameters["session"] = this.Session;
            parameters["application"] = this.Application;
            parameters["request"] = this.Request;
            parameters["response"] = this.Response;
            parameters["context"] = this.Context;

            return parameters;
        }

        #endregion

        #region Data binding support

        /// <summary>
        /// Initializes the data bindings.
        /// </summary>
        protected virtual void InitializeDataBindings()
        {
        }

        /// <summary>
        /// Returns the key to be used for looking up a cached
        /// BindingManager instance in <see cref="SharedState"/>.
        /// </summary>
        /// <returns>a unique key identifying the <see cref="IBindingContainer"/> instance in the <see cref="SharedState"/> dictionary.</returns>
        protected virtual string GetBindingManagerKey()
        {
            return CreateSharedStateKey( "DataBindingManager" );
        }

        /// <summary>
        /// Creates a new <see cref="IBindingContainer"/> instance.
        /// </summary>
        /// <remarks>
        /// This factory method is called if no <see cref="IBindingContainer"/> could be found in <see cref="SharedState"/>
        /// using the key returned by <see cref="GetBindingManagerKey"/>.<br/>
        /// <br/>
        ///
        /// </remarks>
        /// <returns>a <see cref="IBindingContainer"/> instance to be used for DataBinding</returns>
        protected virtual IBindingContainer CreateBindingManager()
        {
            return new BaseBindingManager();
        }

        /// <summary>
        /// Expose BindingManager via IDataBound interface
        /// </summary>
        IBindingContainer IDataBound.BindingManager
        {
            get { return this.BindingManager; }
        }

        /// <summary>
        /// Gets the binding manager.
        /// </summary>
        /// <value>The binding manager.</value>
        protected IBindingContainer BindingManager
        {
            get { return this.bindingManager; }
        }

        /// <summary>
        /// Initializes binding manager and data bindings if necessary.
        /// </summary>
        private void InitializeBindingManager()
        {
            IDictionary sharedState = this.SharedState;

            string key = GetBindingManagerKey();
            this.bindingManager = sharedState[key] as BaseBindingManager;
            if (this.bindingManager == null)
            {
                // access to shared state must be synchronized
                lock (this.SharedState.SyncRoot)
                {
                    this.bindingManager = sharedState[key] as BaseBindingManager;
                    if (this.bindingManager == null)
                    {
                        Trace.Write( traceCategory, "Initialize Data Bindings" );
                        this.bindingManager = CreateBindingManager();
                        if (this.bindingManager == null)
                        {
                            throw new ArgumentNullException( "bindingManager",
                                                            "CreateBindingManager() must not return null" );
                        }
                        InitializeDataBindings();
                        sharedState[key] = this.bindingManager;
                        OnDataBindingsInitialized( EventArgs.Empty );
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="DataBindingsInitialized"/> event.
        /// </summary>
        protected virtual void OnDataBindingsInitialized( EventArgs e )
        {
            EventHandler handler = (EventHandler)base.Events[EventDataBindingsInitialized];

            if (handler != null)
            {
                handler( this, e );
            }
        }

        /// <summary>
        /// This event is raised after <see cref="BindingManager"/> as been initialized.
        /// </summary>
        public event EventHandler DataBindingsInitialized
        {
            add
            {
                base.Events.AddHandler( EventDataBindingsInitialized, value );
            }
            remove
            {
                base.Events.RemoveHandler( EventDataBindingsInitialized, value );
            }
        }

        /// <summary>
        /// Bind data from model to form.
        /// </summary>
        protected virtual void BindFormData()
        {
            if (BindingManager.HasBindings)
            {
                Trace.Write( traceCategory, "Bind Data Model onto Controls" );

                BindingManager.BindTargetToSource( this, Controller, ValidationErrors );
            }
            OnDataBound( EventArgs.Empty );
        }

        /// <summary>
        /// Unbind data from form to model.
        /// </summary>
        protected virtual void UnbindFormData()
        {
            if (BindingManager.HasBindings)
            {
                Trace.Write( traceCategory, "Unbind Controls into Data Model" );

                BindingManager.BindSourceToTarget( this, Controller, ValidationErrors );
            }
            OnDataUnbound( EventArgs.Empty );
        }

        /// <summary>
        /// This event is raised after all controls have been populated with values
        /// from the data model.
        /// </summary>
        public event EventHandler DataBound
        {
            add { base.Events.AddHandler( EventDataBound, value ); }
            remove { base.Events.RemoveHandler( EventDataBound, value ); }
        }

        /// <summary>
        /// Raises DataBound event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnDataBound( EventArgs e )
        {
            EventHandler handler = (EventHandler)base.Events[EventDataBound];
            if (handler != null)
            {
                handler( this, e );
            }
        }

        /// <summary>
        /// This event is raised after data model has been populated with values from
        /// web controls.
        /// </summary>
        public event EventHandler DataUnbound
        {
            add { base.Events.AddHandler( EventDataUnbound, value ); }
            remove { base.Events.RemoveHandler( EventDataUnbound, value ); }
        }

        /// <summary>
        /// Raises DataBound event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnDataUnbound( EventArgs e )
        {
            EventHandler handler = (EventHandler)base.Events[EventDataUnbound];
            if (handler != null)
            {
                handler( this, e );
            }
        }

        #endregion

        #region Application context support

        /// <summary>
        /// Gets or sets the <see cref="Spring.Context.IApplicationContext"/> that this
        /// object runs in.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// <p>
        /// Normally this call will be used to initialize the object.
        /// </p>
        /// <p>
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
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public virtual IApplicationContext ApplicationContext
        {
            get { return applicationContext; }
            set { applicationContext = value; }
        }

        #endregion

        #region Message source and localization support

        /// <summary>
        /// Gets or sets the localizer.
        /// </summary>
        /// <value>The localizer.</value>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public ILocalizer Localizer
        {
            get { return this.localizer; }
            set
            {
                this.localizer = value;
                if (this.localizer.ResourceCache is NullResourceCache)
                {
                    this.localizer.ResourceCache = new SharedStateResourceCache( this );
                }
            }
        }

        /// <summary>
        /// Gets or sets the culture resolver.
        /// </summary>
        /// <value>The culture resolver.</value>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public ICultureResolver CultureResolver
        {
            get
            {
                //                if (cultureResolver == null)
                //                {
                //                    cultureResolver = new DefaultWebCultureResolver();
                //                }
                return cultureResolver;
            }
            set { cultureResolver = value; }
        }

        /// <summary>
        /// Gets or sets the local message source.
        /// </summary>
        /// <value>The local message source.</value>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public IMessageSource MessageSource
        {
            get { return messageSource; }
            set
            {
                messageSource = value;
                if (messageSource != null && messageSource is AbstractMessageSource)
                {
                    ((AbstractMessageSource)messageSource).ParentMessageSource = applicationContext;
                }
            }
        }

        /// <summary>
        /// Initializes local message source
        /// </summary>
        private void InitializeMessageSource()
        {
            if (MessageSource == null)
            {
                string MessageSourceKey = CreateSharedStateKey( "MessageSource" );
                if (this.SharedState[MessageSourceKey] == null)
                {
                    lock (this.SharedState.SyncRoot)
                    {
                        if (this.SharedState[MessageSourceKey] == null)
                        {
                            ResourceSetMessageSource defaultMessageSource = new ResourceSetMessageSource();
                            defaultMessageSource.UseCodeAsDefaultMessage = true;
                            ResourceManager rm = GetLocalResourceManager();
                            if (rm != null)
                            {
                                defaultMessageSource.ResourceManagers.Add( rm );
                            }
                            this.SharedState[MessageSourceKey] = defaultMessageSource;
                        }
                    }
                }

                MessageSource = (IMessageSource)this.SharedState[MessageSourceKey];
            }
        }

        /// <summary>
        /// Creates and returns local ResourceManager for this page.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In ASP.NET 1.1, this method loads local resources from the web application assembly.
        /// </para>
        /// <para>
        /// However, in ASP.NET 2.0, local resources are compiled into a dynamic assembly,
        /// so we need to find that assembly and load the resources from it.
        /// </para>
        /// </remarks>
        /// <returns>Local ResourceManager instance.</returns>
        private ResourceManager GetLocalResourceManager()
        {
            return LocalResourceManager.GetLocalResourceManager(this);
        }

        /// <summary>
        /// Returns message for the specified resource name.
        /// </summary>
        /// <param name="name">Resource name.</param>
        /// <returns>Message text.</returns>
        public string GetMessage( string name )
        {
            return messageSource.GetMessage( name, UserCulture );
        }

        /// <summary>
        /// Returns message for the specified resource name.
        /// </summary>
        /// <param name="name">Resource name.</param>
        /// <param name="args">Message arguments that will be used to format return value.</param>
        /// <returns>Formatted message text.</returns>
        public string GetMessage( string name, params object[] args )
        {
            return messageSource.GetMessage( name, UserCulture, args );
        }

        /// <summary>
        /// Returns resource object for the specified resource name.
        /// </summary>
        /// <param name="name">Resource name.</param>
        /// <returns>Resource object.</returns>
        public object GetResourceObject( string name )
        {
            return messageSource.GetResourceObject( name, UserCulture );
        }

        /// <summary>
        /// Gets or sets user's culture
        /// </summary>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public virtual CultureInfo UserCulture
        {
            get { return CultureResolver.ResolveCulture(); }
            set
            {
                CultureResolver.SetCulture( value );
                Thread.CurrentThread.CurrentUICulture = value;
                if (value.IsNeutralCulture)
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture( value.Name );
                }
                else
                {
                    Thread.CurrentThread.CurrentCulture = value;
                }
                OnUserCultureChanged( EventArgs.Empty );
            }
        }

        /// <summary>
        /// This event is raised when the value of UserLocale property changes.
        /// </summary>
        public event EventHandler UserCultureChanged;

        /// <summary>
        /// Raises UserLocaleChanged event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnUserCultureChanged( EventArgs e )
        {
            if (UserCultureChanged != null)
            {
                UserCultureChanged( this, e );
            }
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Creates a key for shared state, taking into account whether
        /// this page belongs to a process or not.
        /// </summary>
        /// <param name="key">Key suffix</param>
        /// <returns>Generated unique shared state key.</returns>
        protected virtual string CreateSharedStateKey( string key )
        {
            return key;
        }

        #endregion

        #region Dependency Injection Support

        /// <summary>
        /// Holds default ApplicationContext instance to be used during DI.
        /// </summary>
        IApplicationContext ISupportsWebDependencyInjection.DefaultApplicationContext
        {
            get { return defaultApplicationContext; }
            set { defaultApplicationContext = value; }
        }

        /// <summary>
        /// Injects dependencies into control before adding it.
        /// </summary>
        protected override void AddedControl( Control control, int index )
        {
            control = WebDependencyInjectionUtils.InjectDependenciesRecursive( defaultApplicationContext, control );
            base.AddedControl( control, index );
        }

        #endregion Dependency Injection Support

    }
}
