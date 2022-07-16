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
using System.Web.UI;
using Spring.Collections;
using Spring.Validation;
using IValidator = Spring.Validation.IValidator;

using Spring.Context;
using Spring.Context.Support;
using Spring.Globalization;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using Spring.Web.Support;

#endregion Imports

namespace Spring.Web.UI
{
    #region ASP.NET 2.0 Spring Master Page Implementation

    /// <summary>
    /// Spring.NET Master Page implementation for ASP.NET 2.0
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class MasterPage : System.Web.UI.MasterPage, IApplicationContextAware, ISupportsWebDependencyInjection, IWebNavigable
    {
        #region Instance Fields

        private ILocalizer localizer;
        private IValidationErrors validationErrors = new ValidationErrors();
        private IMessageSource messageSource;
        private IApplicationContext applicationContext;
        private IApplicationContext defaultApplicationContext;
        private IWebNavigator webNavigator;
        private IDictionary args;

        #endregion

        #region Lifecycle methods

        /// <summary>
        /// Initialize a new MasterPage instance.
        /// </summary>
        public MasterPage()
        {
            InitializeNavigationSupport();
        }

        /// <summary>
        /// Initializes user control.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            InitializeMessageSource();

            base.OnInit(e);

            // initialize controls
            OnInitializeControls(EventArgs.Empty);
        }

        /// <summary>
        /// Binds data from the data model into controls and raises
        /// PreRender event afterwards.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
            if (localizer != null)
            {
                localizer.ApplyResources(this, messageSource, UserCulture);
            }
            else if (Page.Localizer != null)
            {
                Page.Localizer.ApplyResources(this, messageSource, UserCulture);
            }

            base.OnPreRender(e);
        }

        /// <summary>
        /// This event is raised before Load event and should be used to initialize
        /// controls as necessary.
        /// </summary>
        public event EventHandler InitializeControls;

        /// <summary>
        /// Raises InitializeControls event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnInitializeControls(EventArgs e)
        {
            if (InitializeControls != null)
            {
                InitializeControls(this, e);
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
        protected virtual new Control LoadControl(string virtualPath)
        {
            Control control = base.LoadControl(virtualPath);
            control = WebDependencyInjectionUtils.InjectDependenciesRecursive(defaultApplicationContext,control);
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
        protected virtual new Control LoadControl( Type t, params object[] parameters)
        {
            Control control = base.LoadControl( t, parameters );
            control = WebDependencyInjectionUtils.InjectDependenciesRecursive(defaultApplicationContext,control);
            return control;
        }

        #endregion Control lifecycle methods

        #region Data binding events

        /// <summary>
        /// This event is raised after all controls have been populated with values
        /// from the data model.
        /// </summary>
        public event EventHandler DataBound;

        /// <summary>
        /// Raises DataBound event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnDataBound(EventArgs e)
        {
            if (DataBound != null)
            {
                DataBound(this, e);
            }
        }

        /// <summary>
        /// This event is raised after data model has been populated with values from
        /// web controls.
        /// </summary>
        public event EventHandler DataUnbound;

        /// <summary>
        /// Raises DataBound event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnDataUnbound(EventArgs e)
        {
            if (DataUnbound != null)
            {
                DataUnbound(this, e);
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
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ILocalizer Localizer
        {
            get { return localizer; }
            set
            {
                localizer = value;
                if (localizer.ResourceCache is NullResourceCache)
                {
                    localizer.ResourceCache = new AspNetResourceCache();
                }
            }
        }

        /// <summary>
        /// Gets or sets the local message source.
        /// </summary>
        /// <value>The local message source.</value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IMessageSource MessageSource
        {
            get { return messageSource; }
            set
            {
                messageSource = value;
                if (messageSource != null && messageSource is AbstractMessageSource)
                {
                    ((AbstractMessageSource) messageSource).ParentMessageSource = applicationContext;
                }
            }
        }

        /// <summary>
        /// Initializes local message source
        /// </summary>
        protected void InitializeMessageSource()
        {
            if (MessageSource == null)
            {
                string key = GetType().FullName + ".MessageSource";
                MessageSource = (IMessageSource) Context.Cache.Get(key);

                if (MessageSource == null)
                {
                    ResourceSetMessageSource defaultMessageSource = new ResourceSetMessageSource();
                    ResourceManager rm = GetLocalResourceManager();
                    if (rm != null)
                    {
                        defaultMessageSource.ResourceManagers.Add(rm);
                    }
                    Context.Cache.Insert(key, defaultMessageSource);
                    MessageSource = defaultMessageSource;
                }
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
        /// However, in ASP.NET 2.0, local resources are compiled into the dynamic assembly,
        /// so we need to find that assembly instead and load the resources from it.
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
        public string GetMessage(string name)
        {
            return messageSource.GetMessage(name, UserCulture);
        }

        /// <summary>
        /// Returns message for the specified resource name.
        /// </summary>
        /// <param name="name">Resource name.</param>
        /// <param name="args">Message arguments that will be used to format return value.</param>
        /// <returns>Formatted message text.</returns>
        public string GetMessage(string name, params object[] args)
        {
            return messageSource.GetMessage(name, UserCulture, args);
        }

        /// <summary>
        /// Returns resource object for the specified resource name.
        /// </summary>
        /// <param name="name">Resource name.</param>
        /// <returns>Resource object.</returns>
        public object GetResourceObject(string name)
        {
            return messageSource.GetResourceObject(name, UserCulture);
        }

        /// <summary>
        /// Gets or sets user's culture
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual CultureInfo UserCulture
        {
            get { return Page.UserCulture; }
            set { Page.UserCulture = value; }
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
            webNavigator = new WebFormsResultWebNavigator(this, null, null, true);
        }

        /// <summary>
        /// Gets/Sets the navigator to be used for handling <see cref="SetResult(string, object)"/> calls.
        /// </summary>
        public IWebNavigator WebNavigator
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
                throw new NotSupportedException("WebNavigator must be of type IResultWebNavigator to support Results");
            }
        }

        /// <summary>
        /// A convenience, case-insensitive table that may be used to e.g. pass data into SpEL expressions"/>.
        /// </summary>
        /// <remarks>
        /// By default, e.g. <see cref="SetResult(string)"/> passes the control instance into an expression. Using
        /// <see cref="Args"/> is an easy way to pass additional parameters into the expression
        /// <example>
        /// // config:
        ///
        /// &lt;property Name=&quot;Results&quot;&gt;
        ///   &lt;dictionary&gt;
        ///   		&lt;entry key=&quot;ok_clicked&quot; value=&quot;redirect:~/ShowResult.aspx?result=%{Args['result']}&quot; /&gt;
        ///   &lt;/dictionary&gt;
        /// &lt;/property&gt;
        ///
        /// // code:
        ///
        /// void OnOkClicked(object sender, EventArgs e)
        /// {
        ///   Args[&quot;result&quot;] = txtUserInput.Text;
        ///   SetResult(&quot;ok_clicked&quot;);
        /// }
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
        public virtual bool Validate(object validationContext, params IValidator[] validators)
        {
            IDictionary<string, object> contextParams = CreateValidatorParameters();
            bool result = true;
            foreach (IValidator validator in validators)
            {
                if (validator == null)
                {
                    throw new ArgumentException("Validator is not defined.");
                }
                result = validator.Validate(validationContext, contextParams, this.validationErrors) && result;
            }

            return result;
        }

        /// <summary>
        /// Gets the validation errors container.
        /// </summary>
        /// <value>The validation errors container.</value>
        public virtual IValidationErrors ValidationErrors
        {
            get { return validationErrors; }
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
            IDictionary<string, object> parameters = new Dictionary<string, object>(8);
            parameters["page"] = this.Page;
            parameters["usercontrol"] = this;
            parameters["session"] = this.Session;
            parameters["application"] = this.Application;
            parameters["request"] = this.Request;
            parameters["response"] = this.Response;
            parameters["context"] = this.Context;

            return parameters;
        }

        #endregion

        #region Spring Page support

        /// <summary>
        /// Overrides Page property to return <see cref="Spring.Web.UI.Page"/>
        /// instead of <see cref="System.Web.UI.Page"/>.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Page Page
        {
            get { return (Page) base.Page; }
        }

        #endregion

        #region Dependency Injection Support

        /// <summary>
        /// Holds the default ApplicationContext to be used during DI.
        /// </summary>
        IApplicationContext ISupportsWebDependencyInjection.DefaultApplicationContext
        {
            get { return defaultApplicationContext; }
            set { defaultApplicationContext = value; }
        }

        /// <summary>
        /// Injects dependencies before adding the control.
        /// </summary>
        protected override void AddedControl(Control control,int index)
        {
            control = WebDependencyInjectionUtils.InjectDependenciesRecursive(defaultApplicationContext,control);
            base.AddedControl(control,index);
        }

		#endregion Dependency Injection Support
    }

    #endregion
}
