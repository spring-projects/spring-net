using Spring.Globalization;
using Spring.Validation;

namespace Spring.DataBinding
{
    /// <summary>
    /// Base implementation of the <see cref="IBindingContainer"/>.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class BaseBindingContainer : IBindingContainer
    {
        #region Fields

        private IList<IBinding> bindings = new List<IBinding>();

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Creates a new instance of <see cref="BaseBindingContainer"/>.
        /// </summary>
        public BaseBindingContainer()
        { }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a list of bindings for this container.
        /// </summary>
        /// <value>
        /// A list of bindings for this container.
        /// </value>
        protected IList<IBinding> Bindings
        {
            get { return bindings; }
        }

        #endregion

        #region IBindingContainer Implementation

        /// <summary>
        /// Gets a value indicating whether this instance has bindings.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has bindings; otherwise, <c>false</c>.
        /// </value>
        public bool HasBindings
        {
            get { return bindings.Count > 0; }
        }

        /// <summary>
        /// Adds the binding.
        /// </summary>
        /// <param name="binding">
        /// Binding definition to add.
        /// </param>
        /// <returns>
        /// Added <see cref="IBinding"/> instance.
        /// </returns>
        public IBinding AddBinding(IBinding binding)
        {
            bindings.Add(binding);
            return binding;
        }

        /// <summary>
        /// Adds the <see cref="SimpleExpressionBinding"/> binding with a default
        /// binding direction of <see cref="BindingDirection.Bidirectional"/>.
        /// </summary>
        /// <param name="sourceExpression">
        /// The source expression.
        /// </param>
        /// <param name="targetExpression">
        /// The target expression.
        /// </param>
        /// <returns>
        /// Added <see cref="SimpleExpressionBinding"/> instance.
        /// </returns>
        public IBinding AddBinding(string sourceExpression, string targetExpression)
        {
            return AddBinding(sourceExpression, targetExpression, BindingDirection.Bidirectional, null);
        }

        /// <summary>
        /// Adds the <see cref="SimpleExpressionBinding"/> binding.
        /// </summary>
        /// <param name="sourceExpression">
        /// The source expression.
        /// </param>
        /// <param name="targetExpression">
        /// The target expression.
        /// </param>
        /// <param name="direction">
        /// Binding direction.
        /// </param>
        /// <returns>
        /// Added <see cref="SimpleExpressionBinding"/> instance.
        /// </returns>
        public IBinding AddBinding(string sourceExpression, string targetExpression, BindingDirection direction)
        {
            return AddBinding(sourceExpression, targetExpression, direction, null);
        }

        /// <summary>
        /// Adds the <see cref="SimpleExpressionBinding"/> binding with a default
        /// binding direction of <see cref="BindingDirection.Bidirectional"/>.
        /// </summary>
        /// <param name="sourceExpression">
        /// The source expression.
        /// </param>
        /// <param name="targetExpression">
        /// The target expression.
        /// </param>
        /// <param name="formatter">
        /// <see cref="IFormatter"/> to use for value formatting and parsing.
        /// </param>
        /// <returns>
        /// Added <see cref="SimpleExpressionBinding"/> instance.
        /// </returns>
        public IBinding AddBinding(string sourceExpression, string targetExpression, IFormatter formatter)
        {
            return AddBinding(sourceExpression, targetExpression, BindingDirection.Bidirectional, formatter);
        }

        /// <summary>
        /// Adds the <see cref="SimpleExpressionBinding"/> binding.
        /// </summary>
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
        public virtual IBinding AddBinding(string sourceExpression, string targetExpression,
                                           BindingDirection direction, IFormatter formatter)
        {
            SimpleExpressionBinding binding = new SimpleExpressionBinding(sourceExpression, targetExpression);
            binding.Direction = direction;
            binding.Formatter = formatter;
            bindings.Add(binding);

            return binding;
        }

        #endregion

        #region IBinding Implementation

        /// <summary>
        /// Binds source object to target object.
        /// </summary>
        /// <param name="source">
        /// The source object.
        /// </param>
        /// <param name="target">
        /// The target object.
        /// </param>
        /// <param name="validationErrors">
        /// Validation errors collection that type conversion errors should be added to.
        /// </param>
        public virtual void BindSourceToTarget(object source, object target, IValidationErrors validationErrors)
        {
            BindSourceToTarget(source, target, validationErrors, null);
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
        public virtual void BindSourceToTarget(object source, object target, IValidationErrors validationErrors, IDictionary<string, object> variables)
        {
            foreach (IBinding binding in bindings)
            {
                binding.BindSourceToTarget(source, target, validationErrors, variables);
            }
        }

        /// <summary>
        /// Binds target object to source object.
        /// </summary>
        /// <param name="source">
        /// The source object.
        /// </param>
        /// <param name="target">
        /// The target object.
        /// </param>
        /// <param name="validationErrors">
        /// Validation errors collection that type conversion errors should be added to.
        /// </param>
        public virtual void BindTargetToSource(object source, object target, IValidationErrors validationErrors)
        {
            BindTargetToSource(source, target, validationErrors, null);
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
        public virtual void BindTargetToSource(object source, object target, IValidationErrors validationErrors, IDictionary<string, object> variables)
        {
            foreach (IBinding binding in bindings)
            {
                binding.BindTargetToSource(source, target, validationErrors, variables);
            }
        }

        /// <summary>
        /// Implemented as a NOOP for containers.
        /// of a non-fatal binding error.
        /// </summary>
        /// <param name="messageId">
        /// Resource ID of the error message.
        /// </param>
        /// <param name="errorProviders">
        /// List of error providers message should be added to.
        /// </param>
        public virtual void SetErrorMessage(string messageId, params string[] errorProviders)
        { }

        #endregion
    }
}
