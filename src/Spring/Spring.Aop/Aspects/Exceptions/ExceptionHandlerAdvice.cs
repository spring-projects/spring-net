#region License

/*
 * Copyright 2002-2007 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Collections;
using System.Reflection;
using AopAlliance.Intercept;
using Common.Logging;

namespace Spring.Aspects.Exceptions
{
    /// <summary>
    /// Exception advice to perform exception translation, conversion of exceptions to default return values, and
    /// exception swallowing.  Configuration is via a DSL like string for ease of use in common cases as well as
    /// allowing for custom translation logic by leveraging the Spring expression language.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The exception handler collection can be filled with either instances of objects that implement the interface
    /// <see cref="IExceptionHandler"/> or a string that follows a simple syntax for most common exception management
    /// needs.  The source exceptions to perform processing on are listed immediately after the keyword 'on' and can
    /// be comma delmited.  Following that is the action to perform, either log, translate, wrap, replace, return, or
    /// swallow.  Following the action is a Spring expression language (SpEL) fragment that is used to either create the
    /// translated/wrapped/replaced exception or specify an alternative return value.  The variables available to be
    /// used in the expression language fragment are, #method, #args, #target, and #e which are 1) the method that
    /// threw the exception, the arguments to the method, the target object itself, and the exception that was thrown.
    /// Using SpEL gives you great flexibility in creating a translation of an exception that has access to the calling context.
    /// </para>
    /// <para>Common translation cases, wrap and rethrow, are supported with a shorter syntax where you can specify only
    /// the exception text for the new translated exception.  If you ommit the exception text a default value will be
    /// used.</para>
    /// <para>The exceptionsHandlers are compared to the thrown exception in the order they are listed.  logging
    /// an exception will continue the evaluation process, in all other cases exceution stops at that point and the
    /// appropriate exceptions handler is executed.</para>
    /// <code escaped="true">
    /// <property name="exceptionHandlers">
    ///   <list>
    ///     <value>on FooException1 log 'My Message, Method Name ' + #method.Name</value>
	///     <value>on FooException1 translate new BarException('My Message, Method Called = ' + #method.Name", #e)</value>
	///     <value>on FooException2,Foo3Exception wrap BarException 'My Bar Message'</value>
	///     <value>on FooException4 replace BarException 'My Bar Message'</value>
	///     <value>on FooException5 return 32</value>
	///     <value>on FooException6 swallow</value>
	///     <ref object="exceptionExpression"/>
	///   </list>
	/// </property>
	/// </code>
	/// 
    /// </remarks>
    /// <author>Mark Pollack</author>
    public class ExceptionHandlerAdvice : AbstractExceptionHandlerAdvice
    {
        #region Fields

        /// <summary>
        /// Log instance available to subclasses
        /// </summary>
        protected static readonly ILog log = LogManager.GetLogger(typeof(ExceptionHandlerAdvice));

        private IList exceptionHandlers = new ArrayList();

        private string onExceptionNameRegex = @"^(on\s+exception\s+name)\s+(.*?)\s+(log|translate|wrap|replace|return|swallow)\s*(.*?)$";

        private string onExceptionRegex = @"^(on\s+exception\s+)(\(.*?\))\s+(log|translate|wrap|replace|return|swallow)\s*(.*?)$";
           
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Regex string used to parse advice expressions starting with 'on exception name' and exception handling actions.
        /// </summary>
        /// <value>The regex string to parse advice expressions starting with 'on exception name' and exception handling actions.</value>
        public override string OnExceptionNameRegex
        {
            get { return onExceptionNameRegex; }
            set { onExceptionNameRegex = value; }
        }

        /// <summary>
        /// Gets or sets the Regex string used to parse advice expressions starting with 'on exception (constraint)' and exception handling actions.
        /// </summary>
        /// <value>The regex string to parse advice expressions starting with 'on exception (constraint)' and exception handling actions.</value>
        public override string OnExceptionRegex
        {
            get { return onExceptionRegex; }
            set { onExceptionRegex = value; }
        }

        /// <summary>
        /// Gets or sets the exception handler.
        /// </summary>
        /// <value>The exception handler.</value>
        public IList ExceptionHandlers
        {
            get { return exceptionHandlers; }
            set { exceptionHandlers = value; }
        }

        #endregion

        #region IMethodInterceptor implementation

        /// <summary>
        /// Implement this method to perform extra treatments before and after
        /// the call to the supplied <paramref name="invocation"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Polite implementations would certainly like to invoke
        /// <see cref="AopAlliance.Intercept.IJoinpoint.Proceed"/>. 
        /// </p>
        /// </remarks>
        /// <param name="invocation">
        /// The method invocation that is being intercepted.
        /// </param>
        /// <returns>
        /// The result of the call to the
        /// <see cref="AopAlliance.Intercept.IJoinpoint.Proceed"/> method of
        /// the supplied <paramref name="invocation"/>; this return value may
        /// well have been intercepted by the interceptor.
        /// </returns>
        /// <exception cref="System.Exception">
        /// If any of the interceptors in the chain or the target object itself
        /// throws an exception.
        /// </exception>
        public override object Invoke(IMethodInvocation invocation)
        {
            try
            {
                return invocation.Proceed();
            }
            catch (TargetInvocationException ex)
            {
                Exception realException = ex.InnerException;
                InvokeHandlers(realException, invocation);
                throw realException;
            }
            catch (Exception ex)
            {
                object returnVal = InvokeHandlers(ex, invocation);

                if (returnVal == null)
                {
                    return null;
                }

                // if only logged
                if (returnVal.Equals("logged"))
                {
                    throw;
                }

                //only here if we only are swallowing, returning alternative value, no matching handler was found.
                
                // no matching handler.
                if (returnVal.Equals("nomatch"))
                {
                    throw;
                }
                else
                {
                    //TODO make spring specific value.
                    if (!returnVal.Equals("swallow"))
                    {
                        return returnVal;
                    }
                    else
                    {                        
                        Type returnType = invocation.Method.ReturnType;
                        return returnType.IsValueType && !returnType.Equals(typeof(void))? Activator.CreateInstance(returnType) : null;
                    }
                }
            }
        }

        #endregion

        #region IInitializingObject implementation

        /// <summary>
        /// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// after it has injected all of an object's dependencies.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method allows the object instance to perform the kind of
        /// initialization only possible when all of it's dependencies have
        /// been injected (set), and to throw an appropriate exception in the
        /// event of misconfiguration.
        /// </p>
        /// <p>
        /// Please do consult the class level documentation for the
        /// <see cref="Spring.Objects.Factory.IObjectFactory"/> interface for a
        /// description of exactly <i>when</i> this method is invoked. In
        /// particular, it is worth noting that the
        /// <see cref="Spring.Objects.Factory.IObjectFactoryAware"/>
        /// and <see cref="Spring.Context.IApplicationContextAware"/>
        /// callbacks will have been invoked <i>prior</i> to this method being
        /// called.
        /// </p>
        /// </remarks>
        /// <exception cref="System.Exception">
        /// In the event of misconfiguration (such as the failure to set a
        /// required property) or if initialization fails.
        /// </exception>
        public override void AfterPropertiesSet()
        {
            if (exceptionHandlers.Count == 0)
            {
                throw new ArgumentException("At least one handler is required");
            }
            IList newExceptionHandlers = new ArrayList();
            foreach (object o in exceptionHandlers)
            {
                string handlerString = o as string;
                if (handlerString != null)
                {
                    IExceptionHandler handler = Parse(handlerString);
                    if (handler == null)
                    {
                        throw new ArgumentException("Was not able to parse exception handler string [" + handlerString +
                                                    "]");
                    }
                    newExceptionHandlers.Add(handler);
                }
                IExceptionHandler handlerObject = o as IExceptionHandler;
                if (handlerObject != null)
                {
                    newExceptionHandlers.Add(handlerObject);
                }

            }
            //TODO sync.
            exceptionHandlers = newExceptionHandlers;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invokes handlers registered for the passed exception and <see cref="IMethodInvocation"/>
        /// </summary>
        /// <param name="ex">The exception to be handled</param>
        /// <param name="invocation">The <see cref="IMethodInvocation"/> that raised this exception.</param>
        /// <returns>The output of <see cref="IExceptionHandler.HandleException"/> </returns>
        protected virtual object InvokeHandlers(Exception ex, IMethodInvocation invocation)
        {
            IDictionary callContextDictionary = new Hashtable();
            callContextDictionary.Add("method", invocation.Method);
            callContextDictionary.Add("args", invocation.Arguments);
            callContextDictionary.Add("target", invocation.Target);
            callContextDictionary.Add("e", ex);
            object retValue = "nomatch";
            foreach (IExceptionHandler handler in exceptionHandlers)
            {
                if (handler != null)
                {
                    if (handler.CanHandleException(ex, callContextDictionary))
                    {
                        retValue = handler.HandleException(callContextDictionary);
                        if (!handler.ContinueProcessing)
                        {
                            return retValue;
                        }
                    }
                }
            }
            return retValue;
        }

        /// <summary>
        /// Parses the specified handler string, creating an instance of IExceptionHander.
        /// </summary>
        /// <param name="handlerString">The handler string.</param>
        /// <returns>an instance of an exception handler or null if was not able to correctly parse
        /// handler string.</returns>
        protected virtual IExceptionHandler Parse(string handlerString)
        {
            ParsedAdviceExpression parsedAdviceExpression = ParseAdviceExpression(handlerString);

            if (!parsedAdviceExpression.Success)
            {
                log.Warn("Could not parse exception hander statement " + handlerString);
                return null;
            }

            return CreateExceptionHandler(parsedAdviceExpression);
        }

        /// <summary>
        /// Creates the exception handler.
        /// </summary>
        /// <param name="parsedAdviceExpression">The parsed advice expression.</param>
        /// <returns>The exception handler instance</returns>
        protected virtual IExceptionHandler CreateExceptionHandler(ParsedAdviceExpression parsedAdviceExpression)           
        {
            if (parsedAdviceExpression.ActionText.IndexOf("log") >= 0)
            {
                //TODO support user selection of level, log.Debug , log.Info etc.
                LogExceptionHandler handler = new LogExceptionHandler(parsedAdviceExpression.ExceptionNames);
                handler.ConstraintExpressionText = parsedAdviceExpression.ConstraintExpression;
                handler.ActionExpressionText = "#log.Trace(" + parsedAdviceExpression.ActionExpressionText + ")";
                return handler;
            }
            else if (parsedAdviceExpression.ActionText.IndexOf("translate") >= 0)
            {
                TranslationExceptionHandler handler = new TranslationExceptionHandler(parsedAdviceExpression.ExceptionNames);
                handler.ConstraintExpressionText = parsedAdviceExpression.ConstraintExpression;
                handler.ActionExpressionText = parsedAdviceExpression.ActionExpressionText;
                return handler;
            }
            else if (parsedAdviceExpression.ActionText.IndexOf("wrap") >= 0)
            {
                TranslationExceptionHandler handler = new TranslationExceptionHandler(parsedAdviceExpression.ExceptionNames);
                handler.ConstraintExpressionText = parsedAdviceExpression.ConstraintExpression;
                handler.ActionExpressionText = ParseWrappedExceptionExpression("wrap", parsedAdviceExpression.AdviceExpression);
                return handler;                
            }
            else if (parsedAdviceExpression.ActionText.IndexOf("replace") >= 0)
            {
                TranslationExceptionHandler handler = new TranslationExceptionHandler(parsedAdviceExpression.ExceptionNames);
                handler.ConstraintExpressionText = parsedAdviceExpression.ConstraintExpression;
                handler.ActionExpressionText = ParseWrappedExceptionExpression("replace", parsedAdviceExpression.AdviceExpression);
                return handler;
            }
            else if (parsedAdviceExpression.ActionText.IndexOf("swallow") >= 0)
            {
                SwallowExceptionHandler handler = new SwallowExceptionHandler(parsedAdviceExpression.ExceptionNames);
                handler.ConstraintExpressionText = parsedAdviceExpression.ConstraintExpression;
                return handler;
            }
            else if (parsedAdviceExpression.ActionText.IndexOf("return") >= 0)
            {
                ReturnValueExceptionHandler handler = new ReturnValueExceptionHandler(parsedAdviceExpression.ExceptionNames);
                handler.ConstraintExpressionText = parsedAdviceExpression.ConstraintExpression;
                handler.ActionExpressionText = parsedAdviceExpression.ActionExpressionText;
                return handler;
            }
            else
            {
                log.Warn("Could not parse exception hander statement " + parsedAdviceExpression.AdviceExpression);
            }
            return null;
        }

        /// <summary>
        /// Parses the wrapped exception expression.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="handlerString">The handler string.</param>
        /// <returns></returns>
        protected string ParseWrappedExceptionExpression(string action, string handlerString)
        {
            int endOfActionIndex = handlerString.IndexOf(action) + action.Length;
            string exceptionAndMessage = handlerString.Substring(endOfActionIndex).Trim();
            int endOfExceptionTypeIndex = exceptionAndMessage.IndexOf(" ");

            string rawExpressionTextPart;
            string exception;
            //Has two pieces.
            if (endOfExceptionTypeIndex > 0)
            {
                exception = exceptionAndMessage.Substring(0, endOfExceptionTypeIndex).Trim();
                rawExpressionTextPart = exceptionAndMessage.Substring(endOfExceptionTypeIndex).Trim();
            }
            else
            {
                exception = exceptionAndMessage;
                if (action.Equals("wrap"))
                {
                    rawExpressionTextPart = "'Wrapped ' + #e.GetType().Name";
                } else
                {
                    rawExpressionTextPart = "'Replaced ' + #e.GetType().Name";
                }
            }

            if (action.Equals("wrap"))
            {
                return string.Format("new {0}({1}, #e)", exception, rawExpressionTextPart);
            } else
            {
                return string.Format("new {0}({1})", exception, rawExpressionTextPart);
            }
        }



        #endregion
    }
}