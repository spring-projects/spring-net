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

#region Imports

using System.ComponentModel;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

using Spring.Context;
using Spring.Context.Support;
using Spring.Web.Support;

#endregion

namespace Spring.Web.UI.Controls
{
    /// <summary>
    /// This panel provides the same features as <see cref="System.Web.UI.WebControls.Panel"/>, but provides additional means with regards to Spring.
    /// </summary>
    /// <remarks>
    /// In some cases, automatic dependency injection can cause performance problems. In this case,you can use a Panel for finer-grained control
    /// over which controls are to be injected or not.
    /// </remarks>
    /// <author>Erich Eichinger</author>
    [PersistChildren(true),
    ToolboxData("<{0}:Panel runat=\"server\" Width=\"125px\" Height=\"50px\"> </{0}:Panel>"),
    ParseChildren(false),
    Designer("System.Web.UI.Design.WebControls.PanelContainerDesigner, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
    AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal),
    AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class Panel : System.Web.UI.WebControls.Panel, ISupportsWebDependencyInjection
    {
        // this helper class intercepts the first call to the child controls
        // collection from within InitRecursive().
        // InitRecursive() is called right after the control has been added to it's parent's children.
        private class InitRecursiveInterceptingControlsCollection : ControlCollection
        {
            private bool _passedInitRecursiveCheck = false;

            public InitRecursiveInterceptingControlsCollection(Control owner) : base(owner)
            {
            }

            public override Control this[int index]
            {
                get
                {
                    // the following check relies on the fact, that ControlCollection is set readonly
                    // right before InitRecursive() is called on each child control
                    if (!_passedInitRecursiveCheck
                        && this.IsReadOnly)
                    {
                            _passedInitRecursiveCheck = true;
                            ((Panel)this.Owner).OnPreInitRecursive(EventArgs.Empty);
                    }
                    return base[index];
                }
            }
        }

        private bool _suppressDependencyInjection;
        private bool _renderContainerTag;
        private string _visibleConditionExpression;

        /// <summary>
        /// An optional SpEL expression to control this Panel's <see cref="Control.Visible"/> state.
        /// </summary>
        /// <remarks>
        /// This panel instance is the context for evaluating the given expression. If no expression is specified, visibility behavior
        /// reverts to standard behavior.
        /// </remarks>
        public string VisibleIf
        {
            get { return _visibleConditionExpression; }
            set { _visibleConditionExpression = value; }
        }

        /// <summary>
        /// This flag controls, whether DI on child controls will be done or not
        /// </summary>
        /// <remarks>By default, DI will be done</remarks>
        public bool SuppressDependencyInjection
        {
            get { return _suppressDependencyInjection; }
            set { _suppressDependencyInjection = value; }
        }

        /// <summary>
        /// This flag controls, whether this Panel's tag will be rendered.
        /// </summary>
        public bool RenderContainerTag
        {
            get { return _renderContainerTag; }
            set { _renderContainerTag = value; }
        }

        /// <summary>
        /// Overridden to set <see cref="Control.Visible"/> according to <see cref="VisibleIf"/> if necessary.
        /// </summary>
        protected override void OnPreRender( EventArgs e )
        {
            this.Visible = (_visibleConditionExpression != null)
                ? Spring.Expressions.ExpressionEvaluator.GetValue(this, _visibleConditionExpression).Equals(true)
                : this.Visible;

            base.OnPreRender( e );
        }

        ///<summary>
        ///Renders the HTML opening tag of the <see cref="Panel"></see> control to the specified writer.
        ///</summary>
        ///<param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter"></see> that represents the output stream to render HTML content on the client.</param>
        public override void RenderBeginTag( HtmlTextWriter writer )
        {
            if (this.RenderContainerTag)
            {
                base.RenderBeginTag( writer );
            }
        }

        ///<summary>
        ///Renders the HTML closing tag of the <see cref="Panel"></see> control to the specified writer.
        ///</summary>
        ///<param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter"></see> that represents the output stream to render HTML content on the client.</param>
        public override void RenderEndTag( HtmlTextWriter writer )
        {
            if (this.RenderContainerTag)
            {
                base.RenderEndTag( writer );
            }
        }

        /// <summary>
        /// This method is invoked right before InitRecursive() is called for this Panel and
        /// ensures, that dependencies get injected on child controls before their Init event is raised.
        /// </summary>
        protected virtual void OnPreInitRecursive(EventArgs e)
        {
            if (!_suppressDependencyInjection
                && _defaultApplicationContext == null)
            {
                // obtain appContext of the closed parent template (.ascx/.aspx) control
                IApplicationContext appCtx = WebApplicationContext.GetContext(this.TemplateSourceDirectory.TrimEnd('/') + "/");
                // NOTE: _defaultApplicationContext will be set during DI!
                WebDependencyInjectionUtils.InjectDependenciesRecursive(appCtx, this);
            }
        }

        /// <summary>
        /// Expose the context of this panel for medium trust safe access in e.g. <see cref="VisibleIf"/>.
        /// </summary>
        public new virtual HttpContext Context
        {
            get { return base.Context; }
        }
        #region Dependency Injection Support

        private IApplicationContext _defaultApplicationContext;

        /// <summary>
        /// Sets / Gets the ApplicationContext instance used to configure this control
        /// </summary>
        IApplicationContext ISupportsWebDependencyInjection.DefaultApplicationContext
        {
            get { return _defaultApplicationContext; }
            set { _defaultApplicationContext = value; }
        }

        /// <summary>
        /// Injects dependencies into control before adding it.
        /// </summary>
        protected override void AddedControl(Control control, int index)
        {
            if (!_suppressDependencyInjection
                && _defaultApplicationContext != null)
            {
                control = WebDependencyInjectionUtils.InjectDependenciesRecursive(_defaultApplicationContext, control);
            }
            base.AddedControl(control, index);
        }

        /// <summary>
        /// Overridden to automatically aquire a reference to
        /// root application context to use for DI.
        /// </summary>
        protected override ControlCollection CreateControlCollection()
        {
            // set root application context to signal, that we care
            // for our children ourselves
            if (_suppressDependencyInjection)
            {
                _defaultApplicationContext = WebApplicationContext.GetRootContext();
                return base.CreateControlCollection();
            }
            else
            {
                return new InitRecursiveInterceptingControlsCollection(this);
            }
        }

        #endregion Dependency Injection Support
    }
}
