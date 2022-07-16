using System.Reflection;
using Common.Logging;
using Spring.Globalization;
using Spring.Validation;

namespace Spring.DataBinding
{
    /// <summary>
    /// Abstract base class for simple, one-to-one <see cref="IBinding"/> implementations.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public abstract class AbstractSimpleBinding : AbstractBinding
    {
        #region Fields

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IFormatter formatter;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Initialize a new instance of <see cref="AbstractSimpleBinding"/> without any <see cref="IFormatter"/>
        /// </summary>
        protected AbstractSimpleBinding()
        {
        }

        /// <summary>
        /// Initialize a new instance of <see cref="AbstractSimpleBinding"/> with the
        /// specified <see cref="IFormatter"/>.
        /// </summary>
        protected AbstractSimpleBinding(IFormatter formatter)
        {
            this.formatter = formatter;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="IFormatter"/> to use.
        /// </summary>
        /// <value>The formatter to use.</value>
        public IFormatter Formatter
        {
            get { return formatter; }
            set { formatter = value; }
        }

        #endregion

        #region IBinding Implementation

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
            if (this.IsValid(validationErrors)
                && (this.Direction == BindingDirection.Bidirectional || this.Direction == BindingDirection.SourceToTarget))
            {
                try
                {
                    DoBindSourceToTarget(source, target, variables);
                }
                catch (Exception)
                {
                    if (!SetInvalid(validationErrors)) throw;
                }
            }
        }

        /// <summary>
        /// Concrete implementation if source to target binding.
        /// </summary>
        /// <param name="source">
        ///   The source object.
        /// </param>
        /// <param name="target">
        ///   The target object.
        /// </param>
        /// <param name="variables">
        ///   Variables that should be used during expression evaluation.
        /// </param>
        protected virtual void DoBindSourceToTarget(object source, object target, IDictionary<string, object> variables)
        {
            object value = this.GetSourceValue(source, variables);
            if (this.Formatter != null && value is string)
            {
                value = this.Formatter.Parse((string)value);
            }
            this.SetTargetValue(target, value, variables);
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
            if (this.IsValid(validationErrors)
                && (this.Direction == BindingDirection.Bidirectional || this.Direction == BindingDirection.TargetToSource))
            {
                try
                {
                    DoBindTargetToSource(source, target, variables);
                }
                catch (Exception ex)
                {
                    log.Warn(string.Format("Failed binding[{0}]:{1}", this.Id, ex));
                    if (!SetInvalid(validationErrors)) throw;
                }
            }
        }

        /// <summary>
        /// Concrete implementation of target to source binding.
        /// </summary>
        /// <param name="source">
        ///   The source object.
        /// </param>
        /// <param name="target">
        ///   The target object.
        /// </param>
        /// <param name="variables">
        ///   Variables that should be used during expression evaluation.
        /// </param>
        protected virtual void DoBindTargetToSource(object source, object target, IDictionary<string, object> variables)
        {
            object value = this.GetTargetValue(target, variables);
            if (this.Formatter != null)
            {
                value = this.Formatter.Format(value);
            }
            this.SetSourceValue(source, value, variables);
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Gets the source value for the binding.
        /// </summary>
        /// <param name="source">
        ///   Source object to extract value from.
        /// </param>
        /// <param name="variables">
        ///   Variables for expression evaluation.
        /// </param>
        /// <returns>
        /// The source value for the binding.
        /// </returns>
        protected abstract object GetSourceValue(object source, IDictionary<string, object> variables);

        /// <summary>
        /// Sets the source value for the binding.
        /// </summary>
        /// <param name="source">
        ///   The source object to set the value on.
        /// </param>
        /// <param name="value">
        ///   The value to set.
        /// </param>
        /// <param name="variables">
        ///   Variables for expression evaluation.
        /// </param>
        protected abstract void SetSourceValue(object source, object value, IDictionary<string, object> variables);

        /// <summary>
        /// Gets the target value for the binding.
        /// </summary>
        /// <param name="target">
        ///   Source object to extract value from.
        /// </param>
        /// <param name="variables">
        ///   Variables for expression evaluation.
        /// </param>
        /// <returns>
        /// The target value for the binding.
        /// </returns>
        protected abstract object GetTargetValue(object target, IDictionary<string, object> variables);

        /// <summary>
        /// Sets the target value for the binding.
        /// </summary>
        /// <param name="target">
        ///   The target object to set the value on.
        /// </param>
        /// <param name="value">
        ///   The value to set.
        /// </param>
        /// <param name="variables">
        ///   Variables for expression evaluation.
        /// </param>
        protected abstract void SetTargetValue(object target, object value, IDictionary<string, object> variables);

        #endregion
    }
}
