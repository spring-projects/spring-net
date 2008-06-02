using System;
using System.Collections;
using Spring.Threading;
using Spring.Util;
using Spring.Validation;

namespace Spring.DataBinding
{
    /// <summary>
    /// Abstract base class for <see cref="IBinding"/> implementations.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public abstract class AbstractBinding : IBinding
    {
        #region Fields

        // each Binding instance needs its own ID
        private readonly string BINDING_ID = Guid.NewGuid().ToString("N");

        private BindingDirection direction = BindingDirection.Bidirectional;
        private ErrorMessage errorMessage;
        private string[] errorProviders;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a flag specifying whether this binding is valid.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this binding evaluated without errors;
        ///     <c>false</c> otherwise.
        /// </value>
        public bool IsValid
        {
            get
            {
                object val = LogicalThreadContext.GetData(GetIsValidKey());
                return val == null || (bool)val;
            }
            set
            {
                LogicalThreadContext.SetData(GetIsValidKey(), value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="BindingDirection"/>.
        /// </summary>
        /// <value>The binding direction.</value>
        public BindingDirection Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <value>The error message.</value>
        public ErrorMessage ErrorMessage
        {
            get { return errorMessage; }
        }

        /// <summary>
        /// Gets the error providers.
        /// </summary>
        public string[] ErrorProviders
        {
            get { return errorProviders; }
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
        public void BindSourceToTarget(object source, object target, IValidationErrors validationErrors)
        {
            BindSourceToTarget(source, target, validationErrors, null);
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
        public void BindTargetToSource(object source, object target, IValidationErrors validationErrors)
        {
            BindTargetToSource(source, target, validationErrors, null);
        }

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
        /// <param name="variables">
        /// Variables that should be used during expression evaluation.
        /// </param>
        public abstract void BindSourceToTarget(object source, object target, IValidationErrors validationErrors, IDictionary variables);

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
        /// <param name="variables">
        /// Variables that should be used during expression evaluation.
        /// </param>
        public abstract void BindTargetToSource(object source, object target, IValidationErrors validationErrors, IDictionary variables);

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
        public void SetErrorMessage(string messageId, params string[] errorProviders)
        {
            AssertUtils.ArgumentHasText(messageId, "messageId");
            if (errorProviders == null || errorProviders.Length == 0)
            {
                throw new ArgumentException("At least one error provider has to be specified.", "providers");
            }

            this.errorMessage = new ErrorMessage(messageId, null);
            this.errorProviders = errorProviders;
        }

        #endregion

        #region Private Methods

        private string GetIsValidKey()
        {
            return "Binding." + BINDING_ID + ".IsValid";
        }

        #endregion
    }
}