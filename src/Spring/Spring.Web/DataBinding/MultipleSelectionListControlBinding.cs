using System.Collections;
using System.Web.UI.WebControls;

using Spring.Globalization;

namespace Spring.DataBinding
{
    ///<summary>
    /// Handles binding of a multiple selection ListControl to a target's IList property.
    ///</summary>
    public class MultipleSelectionListControlBinding : SimpleExpressionBinding
    {
        ///<summary>
        /// Creates a new instance of this binding.
        ///</summary>
        ///<param name="sourceExpression">The expression that will evaluate to the <see cref="ListControl"/> acting as the source</param>
        ///<param name="targetExpression">The expression that will evaluate to an <see cref="IList"/> property acting as the target</param>
        /// <param name="direction">Binding's direction</param>
        ///<param name="itemFormatter">An optional formatter converting target items into <see cref="ListItem.Value"/> and back</param>
        /// <remarks>
        /// If no formatter is specified, a <see cref="DataSourceItemFormatter"/> will be used.
        /// </remarks>
        public MultipleSelectionListControlBinding(string sourceExpression, string targetExpression, BindingDirection direction,
                                                   IFormatter itemFormatter)
            :base(sourceExpression, targetExpression, itemFormatter)
        {
            this.Direction = direction;
            if (itemFormatter == null)
            {
                this.Formatter = new DataSourceItemFormatter("DataSource", "DataValueField");
            }
        }

        /// <summary>
        /// Actually performs unbinding the ListControl's selected values into the target's <see cref="IList"/>
        /// </summary>
        protected override void DoBindSourceToTarget(object source, object target, IDictionary<string, object> variables)
        {
            // retrieve targetlist
            IList targetList = this.GetTargetValue(target,variables) as IList;
            if(targetList == null)
            {
                throw new ArgumentException(string.Format("Target property must evaluate to an instance of {0}.", typeof(IList).FullName));
            }

            // retrieve ListControl to be unbound
            source = this.GetSourceValue(source, variables);
            ListControl listControl = source as ListControl;
            if(listControl == null)
            {
                throw new ArgumentException(string.Format("Source property must evaluate to an instance of {0}.", typeof(ListControl).FullName));
            }

            // reset target list
            targetList.Clear();

            // set binding context if applicable
            IBindingAwareFormatter bindingAwareFormatter = this.Formatter as IBindingAwareFormatter;
            if (bindingAwareFormatter != null)
            {
                bindingAwareFormatter.SetBindingContext(source, target, variables, BindingDirection.SourceToTarget);
            }

            // (re-)add selected items to the target list
            try
            {
                IFormatter formatter = this.Formatter;
                ListItemCollection listItems = listControl.Items;
                foreach(ListItem listItem in listControl.Items)
                {
                    if (listItem.Selected)
                    {
                        // formatter must return an object applicable to the targetlist.
                        object item = formatter.Parse(listItem.Value);
                        targetList.Add(item);
                    }
                }
            }
            finally
            {
                if(bindingAwareFormatter != null)
                {
                    bindingAwareFormatter.ClearBindingContext();
                }
            }
        }

        /// <summary>
        /// Actually performs binding the targetlist to the ListControl.
        /// </summary>
        protected override void DoBindTargetToSource(object source, object target, IDictionary<string, object> variables)
        {
            // retrieve targetlist
            IList targetList = this.GetTargetValue(target, variables) as IList;
            if (targetList == null)
            {
                throw new ArgumentException("Target property has to be initialized to an instance of IList interface.");
            }

            // retrieve ListControl to be bound
            source = this.GetSourceValue(source, variables);
            ListControl listControl = source as ListControl;
            if(listControl == null)
            {
                throw new ArgumentException(string.Format("Source property has to be initialized to an instance of {0}.", typeof(ListControl).FullName));
            }

            // clear control's selection
            listControl.ClearSelection();
            ListItemCollection listItems = listControl.Items;

            // set binding context if applicable
            IBindingAwareFormatter bindingAwareFormatter = this.Formatter as IBindingAwareFormatter;
            if(bindingAwareFormatter != null)
            {
                bindingAwareFormatter.SetBindingContext(source, target, variables, BindingDirection.TargetToSource);
            }

            // select all items in ListControl matching keys in targetlist
            try
            {
                IFormatter formatter = this.Formatter;
                foreach(object objectItem in targetList)
                {
                    string key = formatter.Format(objectItem);
                    ListItem item = listItems.FindByValue(key);
                    if (item != null)
                    {
                        item.Selected = true;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("value", key, string.Format("Value '{0}' does not exist", key ));
                    }
                }
            }
            finally
            {
                if(bindingAwareFormatter != null)
                {
                    bindingAwareFormatter.ClearBindingContext();
                }
            }
        }

        /// <summary>
        /// Setting source value is not allowed in this bindingType.
        /// </summary>
        protected override void SetSourceValue(object source, object value, IDictionary<string, object> variables)
        {
            throw new InvalidOperationException(string.Format("Setting source value in '{0}' is not allowed.",this.GetType().FullName));
        }

        /// <summary>
        /// Setting target value is not allowed in this bindingType.
        /// </summary>
        protected override void SetTargetValue(object target, object value, IDictionary<string, object> variables)
        {
            throw new InvalidOperationException(string.Format("Setting target value in '{0}' is not allowed.", this.GetType().FullName));
        }
    }
}
