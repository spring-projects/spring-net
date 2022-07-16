using Spring.Validation;

namespace Spring.DataBinding
{
    /// <summary>
    /// An interface that defines the methods that have to be implemented by all data bindings.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public interface IBinding
    {
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
        void BindSourceToTarget(object source, object target, IValidationErrors validationErrors);

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
        void BindSourceToTarget(object source, object target, IValidationErrors validationErrors, IDictionary<string, object> variables);

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
        void BindTargetToSource(object source, object target, IValidationErrors validationErrors);

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
        void BindTargetToSource(object source, object target, IValidationErrors validationErrors, IDictionary<string, object> variables);

        /// <summary>
        /// Sets error message that should be displayed in the case
        /// of a non-fatal binding error.
        /// </summary>
        /// <param name="messageId">
        /// Resource ID of the error message.
        /// </param>
        /// <param name="errorProviders">
        /// List of error providers message should be added to.
        /// </param>
        void SetErrorMessage(string messageId, params string[] errorProviders);

    }
}
