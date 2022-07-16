using Spring.Collections;
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

        /// <summary>
        /// The name of the always filled error provider
        /// </summary>
        public static readonly string ALL_BINDINGERRORS_PROVIDER = "__all_bindingerrors";

        // each Binding instance needs its own ID
        private readonly string BINDING_ID = Guid.NewGuid().ToString("N");

        private BindingDirection direction = BindingDirection.Bidirectional;
        private BindingErrorMessage errorMessage;
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
        public bool IsValid(IValidationErrors errors)
        {
            if (errors == null) return true;

            IList<ErrorMessage> errorList = errors.GetErrors(ALL_BINDINGERRORS_PROVIDER);
            return (errorList == null) || (!errorList.Contains(this.ErrorMessage));
        }

        /// <summary>
        /// Marks this binding's state as invalid for this validationErrors collection.
        /// Returns false if <paramref name="validationErrors"/> is null.
        /// </summary>
        /// <param name="validationErrors"></param>
        /// <returns>false, if validationErrors is null</returns>
        protected bool SetInvalid(IValidationErrors validationErrors)
        {
            if (validationErrors != null)
            {
                foreach (string provider in this.ErrorProviders)
                {
                    validationErrors.AddError(provider, this.ErrorMessage);
                }
                return true;
            }
            return false;
        }

        ///<summary>
        /// Gets the unique ID of this binding instance.
        ///</summary>
        public string Id
        {
            get { return BINDING_ID; }
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
        public BindingErrorMessage ErrorMessage
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

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"></see> class.
        /// </summary>
        protected AbstractBinding()
        {
            this.errorMessage = new BindingErrorMessage( this.Id, "Binding-Error");
            this.errorProviders = new string[] { ALL_BINDINGERRORS_PROVIDER };
        }

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
        public abstract void BindSourceToTarget(object source, object target, IValidationErrors validationErrors, IDictionary<string, object> variables);

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
        public abstract void BindTargetToSource(object source, object target, IValidationErrors validationErrors, IDictionary<string, object> variables);

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

            this.errorMessage = new BindingErrorMessage(this.BINDING_ID, messageId, null);
            Set providers = new HashedSet();
            providers.Add(ALL_BINDINGERRORS_PROVIDER);
            providers.AddAll(errorProviders);
            errorProviders = new string[providers.Count];
            providers.CopyTo(errorProviders, 0);
            this.errorProviders = errorProviders;
        }

        #endregion

        ///<summary>
        ///Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        ///</summary>
        ///<returns>
        ///true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        ///</returns>
        ///<param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            AbstractBinding other = obj as AbstractBinding;
            return (other != null) && (this.Id == other.Id);
        }

        ///<summary>
        ///Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        ///</summary>
        ///<returns>
        ///A hash code for the current <see cref="T:System.Object"></see>.
        ///</returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
