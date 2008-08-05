using System.Reflection;
using Spring.Aop;
using Spring.Context;
using Spring.Validation;

namespace Spring.Aspects.Validation
{
    /// <summary>
    /// This advice is typically applied to service-layer methods in order to validate
    /// method arguments.
    /// </summary>
    /// <remarks>
    /// <para>Each argument that should be validated has to be marked with one or more
    /// <see cref="ValidatedAttribute"/>s.</para>
    /// <para>If the validation fails, this advice will throw <see cref="ValidationException"/>,
    /// thus preventing target method invocation.
    /// </para>
    /// </remarks>
    /// <author>Damjan Tomic</author>
    /// <author>Aleksandar Seovic</author>
    public class ParameterValidationAdvice : IMethodBeforeAdvice, IApplicationContextAware
    {
        private IApplicationContext applicationContext; 

        /// <summary>
        /// Intercepts method invocation and validates arguments.
        /// </summary>
        /// <param name="method">
        /// Method invocation.
        /// </param>
        /// <param name="args">
        /// Method arguments.
        /// </param>
        /// <param name="target">
        /// Target object.
        /// </param>
        /// <exception cref="ValidationException">
        /// If one or more method arguments fail validation.
        /// </exception>
        public void Before(MethodInfo method, object[] args, object target)
        {
            ValidationErrors errors = new ValidationErrors();
            ParameterInfo[] parameters = method.GetParameters();
            
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo info = parameters[i];
                ValidatedAttribute[] attributes = (ValidatedAttribute[]) info.GetCustomAttributes(typeof(ValidatedAttribute), true);
                
                foreach (ValidatedAttribute attribute in attributes)
                {
                    // throws NoSuchObjectDefinitionException if validator cannot be found
                    IValidator validator = (IValidator) applicationContext.GetObject(attribute.ValidatorName);
                    validator.Validate(args[i], errors);                        
                }
            }            
            if (!errors.IsEmpty)
            {
                throw new ValidationException(errors);
            }
        }

        /// <summary>
        /// Sets the application context to search for validators in.
        /// </summary>
        /// <value>
        /// The application context to search for validators in.
        /// </value>
        public IApplicationContext ApplicationContext
        {
            set { this.applicationContext = value; }
        }
    }
}