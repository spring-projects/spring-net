using System.Collections;
using Spring.Expressions;
using Spring.Validation;

namespace Spring.DataBinding
{
    /// <summary>
    /// <see cref="IBindingContainer"/> implementation that allows
    /// data binding between collections that implement <see cref="IList"/>
    /// interface.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class ListBinding : AbstractBinding
    {
        private IExpression sourceExpression = Expression.Parse("#source = #target");
        private IExpression targetExpression = Expression.Parse("#target = #source");

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
            if (variables == null)
            {
                variables = new Dictionary<string, object>();
            }
            variables["source"] = source;
            variables["target"] = target;

            targetExpression.GetValue(null, variables);
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
            if (variables == null)
            {
                variables = new Dictionary<string, object>();
            }
            variables["source"] = source;
            variables["target"] = target;

            sourceExpression.GetValue(null, variables);
        }
    }
}
