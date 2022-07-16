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

using System.ComponentModel;
using System.Reflection;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Common.Logging;

using Spring.Core.TypeResolution;
using Spring.DataBinding;
using Spring.Globalization;
using Spring.Util;
using AttributeCollection=System.Web.UI.AttributeCollection;
using BindingDirection=Spring.DataBinding.BindingDirection;

#endregion

namespace Spring.Web.UI.Controls
{
    /// <summary>
    /// Any WebControl placed on a DataBindingPanel may be bound
    /// to a model by adding an attribute "BindingTarget"
    /// </summary>
    /// <author>Erich Eichinger</author>
    [PersistChildren(true),
    ToolboxData("<{0}:DataBindingPanel runat=\"server\" Width=\"125px\" Height=\"50px\"> </{0}:DataBindingPanel>"),
    ParseChildren(false),
    Designer("System.Web.UI.Design.WebControls.PanelContainerDesigner, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
    AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal),
    AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class DataBindingPanel : Panel
    {
        private const string ATTR_BINDINGTARGET = "BindingTarget";
        private const string ATTR_BINDINGSOURCE = "BindingSource";
        private const string ATTR_BINDINGDIRECTION = "BindingDirection";
        private const string ATTR_BINDINGFORMATTER = "BindingFormatter";
        private const string ATTR_BINDINGTYPE = "BindingType";
        private const string ATTR_MESSAGEID = "MessageId";
        private const string ATTR_ERRORPROVIDERS = "ErrorProviders";

        private static readonly ILog Log = LogManager.GetLogger(typeof(DataBindingPanel));

        private delegate void TraversalAction(IWebDataBound bindingContainer, WebControl wc);

        /// <summary>
        /// Registers this control for the <see cref="UserControl.DataBindingsInitialized"/> event of it's container.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            RegisterDataControl(this, new EventHandler(BindingOwner_DataBindingsInitialized));
            base.OnInit(e);
        }

        /// <summary>
        /// Overridden to remove custom binding attributes before rendering.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            // remove custom attributes
            if (this.HasControls())
            {
                this.TraverseControls(null, this.Controls, new TraversalAction(this.RemoveBindingAttributes));
            }

            base.OnPreRender(e);
        }

        /// <summary>
        /// Overriden to suppress rendering this control's tag
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            base.RenderContents(writer);
        }

        /// <summary>
        /// Called by the containing <see cref="UserControl"/> if bindings must be initialized.
        /// </summary>
        private void BindingOwner_DataBindingsInitialized(object sender, EventArgs e)
        {
            IWebDataBound bindingContainer = (IWebDataBound) sender;
            if (this.HasControls())
            {
                this.TraverseControls(bindingContainer, this.Controls, new TraversalAction(this.BindControl));
            }
        }

        /// <summary>
        /// Adds all controls on this panel to the containing <see cref="UserControl.BindingManager"/>'s binding collection.
        /// </summary>
        /// <param name="bindingContainer">the usercontrol containing this panel</param>
        /// <param name="controls">the <see cref="ControlCollection"/> of controls to be bound</param>
        /// <param name="action">action to be performed on matching controls</param>
        private void TraverseControls(IWebDataBound bindingContainer, ControlCollection controls, TraversalAction action)
        {
            foreach (Control control in controls)
            {
                // DataBindingPanels must not be nested
                if (control is DataBindingPanel)
                {
                    throw new HttpException("Controls of type DataBindingPanel must not be nested");
                }

                // abort recursion on LiteralControl or nested UserControl
                if (control is LiteralControl
                    || control is UserControl)
                {
                    continue;
                }

                // if it's a WebControl, check for binding-related attributes
                WebControl wc = control as WebControl;
                if (wc != null)
                {
                    try
                    {
                        action(bindingContainer, wc);
                    }
                    catch (Exception ex)
                    {
                        string msg =
                            string.Format("Error executing action on control '{0}' of type '{1}'", wc.UniqueID,
                                          wc.GetType().FullName);
                        Log.Error(msg, ex);
                        throw new HttpException(msg, ex);
                    }
                }

                if (control.HasControls())
                {
                    this.TraverseControls(bindingContainer, control.Controls, action);
                }
            }
        }

        /// <summary>
        /// Retrieves custom binding attributes from a webcontrol and adds a new binding
        /// instance to the container's <see cref="UserControl.BindingManager"/> if necessary.
        /// </summary>
        private void BindControl(IWebDataBound dataBound, WebControl theControl)
        {
            // special handling of adapted controls
            DataBindingAdapter adapterControl = theControl as DataBindingAdapter;

            Control wc = (adapterControl != null && adapterControl.WrappedControl != null) ? adapterControl.WrappedControl : theControl;

            AttributeCollection attributeCollection = theControl.Attributes;
            string bindingTarget = attributeCollection[ATTR_BINDINGTARGET];

            // at least a BindingTarget must be specified
            if (bindingTarget == null)
            {
                return;
            }
            attributeCollection.Remove(ATTR_BINDINGTARGET);

            // determine direction
            BindingDirection bindingDirection = BindingDirection.Bidirectional;
            string strBindingDirection = attributeCollection[ATTR_BINDINGDIRECTION];
            if (strBindingDirection != null)
            {
                bindingDirection = (BindingDirection) Enum.Parse(typeof(BindingDirection), strBindingDirection);
            }

            // determine BindingSource
            string bindingSource = attributeCollection[ATTR_BINDINGSOURCE];
            if (bindingSource == null)
            {
                bindingSource = AutoProbeSourceProperty(wc);
            }
            attributeCollection.Remove(ATTR_BINDINGSOURCE);

            // get formatter if any
            IFormatter bindingFormatter = null;
            string bindingFormatterName = attributeCollection[ATTR_BINDINGFORMATTER];
            if (bindingFormatterName != null)
            {
                bindingFormatter = (IFormatter) dataBound.ApplicationContext.GetObject(bindingFormatterName);
                attributeCollection.Remove(ATTR_BINDINGFORMATTER);
            }

            // determine source expression
            string containerName = dataBound.UniqueID;
            string controlName = wc.UniqueID;
            string relativeControlName = null;
            if ( dataBound is System.Web.UI.Page )
            {
                relativeControlName = string.Format("FindControl('{0}')", controlName);
            }
            else if ( (Control)dataBound != this.NamingContainer)
            {
                relativeControlName = (controlName.StartsWith(containerName)) ? controlName.Substring(containerName.Length + 1) : controlName;
                relativeControlName = string.Format("FindControl('{0}')", relativeControlName);
            }
            else
            {
                relativeControlName = wc.ID;
            }
            // if no bindingSource, expression evaluates to the bound control
            bindingSource = (StringUtils.HasLength(bindingSource))
                                ? relativeControlName + "." + bindingSource
                                : relativeControlName;

            Log.Debug(
                string.Format("binding control '{0}' relative to '{1}' using expression '{2}'", controlName,
                              containerName, bindingSource));

            //get bindingType if any
            IBinding binding = null;
            string bindingTypeName = attributeCollection[ATTR_BINDINGTYPE];
            if (bindingTypeName == null)
            {
                bindingTypeName = AutoProbeBindingType(wc);
            }

            // get messageId and errorProviders list
            string messageId = attributeCollection[ATTR_MESSAGEID];
            string errorProvidersText = attributeCollection[ATTR_ERRORPROVIDERS];
            string[] errorProviders = null;
            if (StringUtils.HasLength(errorProvidersText))
            {
                errorProviders = (string[])new Spring.Core.TypeConversion.StringArrayConverter().ConvertFrom(errorProvidersText);
            }

            // add binding to BindingManager
            if (bindingTypeName != null)
            {
                binding = CreateBindingInstance(bindingTypeName, bindingSource, bindingTarget, bindingDirection, bindingFormatter);
                binding = dataBound.BindingManager.AddBinding(binding);
            }
            else
            {
                binding = dataBound.BindingManager.AddBinding(bindingSource, bindingTarget, bindingDirection, bindingFormatter);
            }

            // set error message
            if (StringUtils.HasLength(messageId))
            {
                binding.SetErrorMessage( messageId, errorProviders );
            }
        }

        /// <summary>
        /// Removes custom binding attributes from a webcontrol to avoid them being rendered.
        /// </summary>
        private void RemoveBindingAttributes(IWebDataBound dataBound, WebControl wc)
        {
            AttributeCollection attributeCollection = wc.Attributes;
            attributeCollection.Remove(ATTR_BINDINGTARGET);
            attributeCollection.Remove(ATTR_BINDINGSOURCE);
            attributeCollection.Remove(ATTR_BINDINGTYPE);
            attributeCollection.Remove(ATTR_BINDINGDIRECTION);
            attributeCollection.Remove(ATTR_BINDINGFORMATTER);
        }

        /// <summary>
        /// Probe for bindingType of know controls
        /// </summary>
        /// <param name="wc">the control, who's bindingType is to be determined</param>
        /// <returns>null, if standard binding is to be used or the fully qualified typename of the binding implementation</returns>
        protected virtual string AutoProbeBindingType(Control wc)
        {
            if (wc is ListBox && ((ListBox) wc).SelectionMode == ListSelectionMode.Multiple)
            {
                return typeof(MultipleSelectionListControlBinding).FullName;
            }
            return null;
        }

        /// <summary>
        /// Probes for a few know controls and their properties.
        /// </summary>
        /// <returns>
        /// The 'BindingSource' expression to be used for binding this control.
        /// </returns>
        protected virtual string AutoProbeSourceProperty(Control wc)
        {
            if (wc is ListBox && ((ListBox) wc).SelectionMode == ListSelectionMode.Multiple)
            {
                return null; // force evaluate to control itself
            }
            else if (wc is CheckBoxList)
            {
                return "SelectedValues";
            }
            else if (wc is ListControl)
            {
                return "SelectedValue";
            }
            else if (wc is CheckBox)
            {
                return "Checked";
            }
            else if (wc is TextBox)
            {
                return "Text";
            }
            else if (wc is HiddenField)
            {
                return "Value";
            }
            else if (wc is RadioButtonGroup)
            {
                return "Value";
            }
            else if (wc is DataBindingAdapter)
            {
                throw new ArgumentNullException("Attribute 'BindingSource' is mandatory when using DataBindingAdapter");
            }

            throw new ArgumentNullException("Attribute 'BindingSource' is missing and control is of unknown type");
        }

        private static IBinding CreateBindingInstance(string bindingTypeName, string bindingSource, string bindingTarget, BindingDirection bindingDirection, IFormatter bindingFormatter)
        {
            IBinding binding;
            Type bindingType = TypeResolutionUtils.ResolveType(bindingTypeName);
            ConstructorInfo ctor =
                bindingType.GetConstructor(new Type[] {typeof(string), typeof(string), typeof(BindingDirection), typeof(IFormatter)});

            if (ctor == null)
            {
                throw new ArgumentException(string.Format("Specified BindingType '{0}' does not implement constructor (string,string,BindingDirection,IFormatter)",bindingTypeName));
            }
            binding =
                (IBinding)
                ObjectUtils.InstantiateType(ctor, new object[] {bindingSource, bindingTarget, bindingDirection, bindingFormatter});
            return binding;
        }

        private static void RegisterDataControl(Control control, EventHandler initializeBindingHandler)
        {
            GetBindingContainerControl(control).DataBindingsInitialized += initializeBindingHandler;
        }

        private static IWebDataBound GetBindingContainerControl(Control control)
        {
            Control parent = control;
            while (parent != null)
            {
                if (parent is IWebDataBound)
                {
                    return (IWebDataBound) parent;
                }
                parent = parent.Parent;
            }
            return null;
        }
    }
}
