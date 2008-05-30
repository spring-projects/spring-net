using System;

namespace Spring.Validation
{
    /// <summary>
    /// Allows developers to specify which validator should be used
    /// to validate method argument.
    /// </summary>
    /// <author>Damjan Tomic</author>
    /// <author>Aleksandar Seovic</author>
    /// <version>$Id: ValidatedAttribute.cs,v 1.1 2008/04/02 23:02:36 markpollack Exp $</version>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    [Serializable]
    public class ValidatedAttribute : Attribute
    {
        private readonly string validatorName;

        /// <summary>
        /// Creates an attribute instance.
        /// </summary>
        /// <param name="validatorName">
        /// The name of the validator to use (must be defined within
        /// Spring application context).
        /// </param>
        public ValidatedAttribute(string validatorName)
        {
            this.validatorName = validatorName;
        }
        
        /// <summary>
        /// Gets the name of the validator to use.
        /// </summary>
        /// <value>The name of the validator to use.</value>
        public string ValidatorName
        {
            get { return validatorName; }
        }
    }
}