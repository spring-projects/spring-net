using Spring.Expressions;

namespace Spring.Aspects.Exceptions
{
    /// <summary>
    /// Executes an abribtrary Spring Expression Language (SpEL) expression as an action when handling an exception.
    /// </summary>
    public class ExecuteSpelExceptionHandler : AbstractExceptionHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteSpelExceptionHandler"/> class.
        /// </summary>
        public ExecuteSpelExceptionHandler()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteSpelExceptionHandler"/> class.
        /// </summary>
        /// <param name="exceptionNames">The exception names.</param>
        public ExecuteSpelExceptionHandler(string[] exceptionNames)
            : base(exceptionNames)
        {
        }

        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <returns>The return value from handling the exception, if not rethrown or a new exception is thrown.</returns>
        public override object HandleException(IDictionary<string, object> callContextDictionary)
        {
            try
            {
                IExpression expression = Expression.Parse(ActionExpressionText);
                expression.GetValue(null, callContextDictionary);
            }
            catch (Exception e)
            {
                log.Warn("Was not able to evaluate action expression [" + ActionExpressionText + "]", e);
            }
            return null;
        }
    }
}
