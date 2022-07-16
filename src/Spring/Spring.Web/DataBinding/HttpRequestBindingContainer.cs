using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

using Spring.Expressions;
using Spring.Globalization;
using Spring.Util;
using Spring.Validation;

namespace Spring.DataBinding
{
    /// <summary>
    /// Binds agroup of HTTP request multi-valued items to the data model.
    /// </summary>
    /// <remarks>
    /// Due to the fact, that browsers don't send the values of unchecked checkboxes, you
    /// can't use <see cref="HttpRequestListBindingContainer"/> for binding to checkboxes.
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class HttpRequestListBindingContainer : BaseBindingContainer
    {
        private string[] requestParams;
        private ConstructorInfo itemCtor;
        private IExpression targetExpression;

        /// <summary>
        /// Creates a new instance of <see cref="HttpRequestListBindingContainer"/>.
        /// </summary>
        /// <param name="requestParams">
        /// Comma separated list of multi-valued request parameters that
        /// should be used to create a target item.
        /// </param>
        /// <param name="targetList">
        /// Expression that identifies a target list items should be added to.
        /// </param>
        /// <param name="itemType">
        /// The type of the target item.
        /// </param>
        public HttpRequestListBindingContainer(string requestParams, string targetList, Type itemType)
        {
            this.requestParams = requestParams.Split(',');
            this.targetExpression = Expression.Parse(targetList);
            this.itemCtor = itemType.GetConstructor(Type.EmptyTypes);
            if (this.itemCtor == null)
            {
                throw new ArgumentException("Type [{0}] does not have a default constructor. " +
                                            "A default constructor for the item type is required for collection data binding.",
                                            itemType.FullName);
            }
        }

        /// <summary>
        /// Adds the <see cref="SimpleExpressionBinding"/> binding.
        /// </summary>
        /// <remarks>
        /// This is a convinience method for adding <b>SimpleExpressionBinding</b>,
        /// one of the most often used binding types, to the bindings list.
        /// </remarks>
        /// <param name="sourceExpression">
        /// The source expression.
        /// </param>
        /// <param name="targetExpression">
        /// The target expression.
        /// </param>
        /// <param name="direction">
        /// Binding direction.
        /// </param>
        /// <param name="formatter">
        /// <see cref="IFormatter"/> to use for value formatting and parsing.
        /// </param>
        /// <returns>
        /// Added <see cref="SimpleExpressionBinding"/> instance.
        /// </returns>
        public override IBinding AddBinding(string sourceExpression, string targetExpression, BindingDirection direction,
                                            IFormatter formatter)
        {
            // setup variable for each HTTP request parameter
            return base.AddBinding("#" + sourceExpression, targetExpression, direction, formatter);
        }

        /// <summary>
        /// Binds source object to target object.
        /// </summary>
        /// <param name="source">
        ///   The source object.
        /// </param>
        /// <param name="target">
        ///   The target object.
        /// </param>
        /// <param name="validationErrors">
        ///   Validation errors collection that type conversion errors should be added to.
        /// </param>
        /// <param name="variables">
        ///   Variables that should be used during expression evaluation.
        /// </param>
        public override void BindSourceToTarget(object source, object target, IValidationErrors validationErrors, IDictionary<string, object> variables)
        {
            NameValueCollection parameters = VirtualEnvironment.RequestParams;
            IList targetList = targetExpression.GetValue(target) as IList;
            if (targetList == null)
            {
                throw new ArgumentException("Target property has to be initialized to an instance of IList interface.");
            }

            if (parameters.GetValues(requestParams[0]) != null)
            {
                int valueCount = parameters.GetValues(requestParams[0]).Length;
                for (int i = 0; i < valueCount; i++)
                {
                    IDictionary<string, object> vars = new Dictionary<string, object>();
                    foreach (string paramName in requestParams)
                    {
                        vars[paramName] = parameters.GetValues(paramName)[i];
                    }

                    object targetItem;
                    bool addToList;
                    if (i < targetList.Count)
                    {
                        targetItem = targetList[i];
                        addToList = false;
                    }
                    else
                    {
                        targetItem = itemCtor.Invoke(ObjectUtils.EmptyObjects);
                        addToList = true;
                    }

                    foreach (IBinding binding in Bindings)
                    {
                        binding.BindSourceToTarget(null, targetItem, validationErrors, vars);
                    }

                    if (addToList)
                    {
                        targetList.Add(targetItem);
                    }
                }
            }
        }

        /// <summary>
        /// Binds target object to source object.
        /// </summary>
        /// <param name="source">
        ///   The source object.
        /// </param>
        /// <param name="target">
        ///   The target object.
        /// </param>
        /// <param name="validationErrors">
        ///   Validation errors collection that type conversion errors should be added to.
        /// </param>
        /// <param name="variables">
        ///   Variables that should be used during expression evaluation.
        /// </param>
        public override void BindTargetToSource(object source, object target, IValidationErrors validationErrors, IDictionary<string, object> variables)
        {
            // can't bind to a read-only Request object...
        }
    }
}
