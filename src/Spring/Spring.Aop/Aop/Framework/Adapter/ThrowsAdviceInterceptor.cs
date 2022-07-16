#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

#region Imports

using System.Collections;
using System.Reflection;
using AopAlliance.Intercept;
using Common.Logging;
using Spring.Util;

#endregion

namespace Spring.Aop.Framework.Adapter
{
    /// <summary>Interceptor to wrap an after throwing advice.</summary>
    /// <remarks>
    /// <p>
    /// Implementations of the <see cref="Spring.Aop.IThrowsAdvice"/> interface
    /// <b>must</b> define methods of the form...
    /// <code lang="C#">
    /// AfterThrowing([MethodInfo method, Object[] args, Object target], Exception subclass);
    /// </code>
    /// The method name is fixed (i.e. your methods <b>must</b> be named
    /// <c>AfterThrowing</c>. The first three arguments (<i>as a whole</i>) are
    /// optional, and only useful if futher information about the joinpoint is
    /// required. The return type <i>can</i> be anything, but is almost always
    /// <see langword="void"/> by convention.
    /// </p>
    /// <p>
    /// Please note that the object encapsulating the throws advice does not
    /// need to implement the <see cref="Spring.Aop.IThrowsAdvice"/> interface.
    /// Throws advice methods are discovered via reflection... the
    /// <see cref="Spring.Aop.IThrowsAdvice"/> interface serves merely to
    /// <i>discover</i> objects that are to be considered as throws advice.
    /// Other mechanisms for discovering throws advice such as attributes are
    /// also equally valid... all that this class cares about is that a throws
    /// advice object implement one or more methods with a valid throws advice
    /// signature (see above, and the examples below).
    /// </p>
    /// <p>
    /// This is a framework class that should not normally need to be used
    /// directly by Spring.NET users.
    /// </p>
    /// </remarks>
    /// <example>
    /// <p>
    /// Find below some examples of valid <see cref="Spring.Aop.IThrowsAdvice"/>
    /// method signatures...
    /// </p>
    /// <code language="C#">
    /// public class GlobalExceptionHandlingAdvice : IThrowsAdvice
    /// {
    ///     public void AfterThrowing(Exception ex) {
    ///         // handles absolutely any and every Exception...
    ///     }
    /// }
    /// </code>
    /// <code language="C#">
    /// public class RemotingExceptionHandlingAdvice : IThrowsAdvice
    /// {
    ///     public void AfterThrowing(RemotingException ex) {
    ///         // handles any and every RemotingException (and subclasses of RemotingException)...
    ///     }
    /// }
    /// </code>
    /// <code language="C#">
    /// using System.Data;
    ///
    /// public class DataExceptionHandlingAdvice
    /// {
    ///     public void AfterThrowing(ConstraintException ex) {
    ///         // specialised handling of ConstraintExceptions
    ///     }
    ///
    ///     public void AfterThrowing(NoNullAllowedException ex) {
    ///         // specialised handling of NoNullAllowedExceptions
    ///     }
    ///
    ///     public void AfterThrowing(DataException ex) {
    ///         // handles all other DataExceptions...
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <author>Rod Johnson</author>
    /// <author>Aleksandar Seovic (.NET)</author>
    /// <seealso cref="Spring.Aop.IThrowsAdvice"/>
    [Serializable]
    public sealed class ThrowsAdviceInterceptor : IMethodInterceptor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ThrowsAdviceInterceptor));

        private const string SpecialThrowingMethodName = "AfterThrowing";

        private readonly object throwsAdvice;

        /// <summary>
        /// The mapping of exception Types to MethodInfo handlers.
        /// </summary>
        private readonly IDictionary exceptionHandlers;

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Aop.Framework.Adapter.ThrowsAdviceInterceptor"/> class.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="advice">
        /// The throws advice to check for exception handler methods.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="advice"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If no (0) handler methods were discovered on the supplied <paramref name="advice"/>;
        /// or if more than one handler method suitable for a particular
        /// <see cref="System.Exception"/> type was discovered on the supplied
        /// <paramref name="advice"/>.
        /// </exception>
        public ThrowsAdviceInterceptor(object advice)
        {
            AssertUtils.ArgumentNotNull(advice, "advice");
            this.exceptionHandlers = new Hashtable();
            this.throwsAdvice = advice;
            MapAllExceptionHandlingMethods(advice);
            if (exceptionHandlers.Count == 0)
            {
                throw new ArgumentException(
                    "At least one handler method must be found in class ["
                    + advice.GetType().FullName + "].");
            }
        }

        private void MapAllExceptionHandlingMethods(object advice)
        {
            MethodInfo[] methods = advice.GetType().GetMethods();
            foreach (MethodInfo method in methods)
            {
                int numParams = method.GetParameters().Length;
                if (method.Name.Equals(SpecialThrowingMethodName)
                    && (numParams == 1 || numParams == 4))
                {
                    Type lastParametersType = method.GetParameters()[numParams - 1].ParameterType;
                    if (typeof (Exception).IsAssignableFrom(lastParametersType))
                    {
                        #region Instrumentation

                        if(log.IsDebugEnabled)
                        {
                            log.Debug("Found exception handler method: " + method);
                        }

                        #endregion

                        if(this.exceptionHandlers.Contains(lastParametersType))
                        {
                            throw new ArgumentException(
                                "Throws advice handler method for the [" +
                                lastParametersType + "] type already exists; don't define " +
                                "both single and multiple argument methods for the same " +
                                "Exception type in the same class.");
                        }
                        this.exceptionHandlers[lastParametersType] = method;
                    }
                }
            }
        }

        /// <summary>
        /// Convenience property that returns the number of exception handler
        /// methods managed by this interceptor.
        /// </summary>
        /// <value>
        /// The number of exception handler methods managed by this interceptor.
        /// </value>
        public int HandlerMethodCount
        {
            get { return exceptionHandlers.Count; }
        }

        /// <summary>
        /// Executes interceptor if (and only if) the supplied
        /// <paramref name="invocation"/> throws an exception that is mapped to
        /// an appropriate exception handler.
        /// </summary>
        /// <param name="invocation">
        /// The method invocation that is being intercepted.
        /// </param>
        /// <returns>
        /// The result of the call to the
        /// <see cref="AopAlliance.Intercept.IJoinpoint.Proceed"/> method of
        /// the supplied <paramref name="invocation"/> (this assumes no
        /// exception was thrown by the call to the supplied <paramref name="invocation"/>.
        /// </returns>
        /// <exception cref="System.Exception">
        /// If any of the interceptors in the chain or the target object itself
        /// throws an exception.
        /// </exception>
        /// <seealso cref="AopAlliance.Intercept.IMethodInterceptor.Invoke"/>
        public object Invoke(IMethodInvocation invocation)
        {
            try
            {
                return invocation.Proceed();
            }
            catch (TargetInvocationException ex)
            {
                // bah, this is a tad gross...
                Exception realException = ex.InnerException;
                LookupAndInvokeAnyHandler(realException, invocation);
                throw realException;
            }
            catch (Exception ex)
            {
                LookupAndInvokeAnyHandler(ex, invocation);
                throw;
            }
        }

        private void LookupAndInvokeAnyHandler(Exception ex, IMethodInvocation invocation)
        {
            MethodInfo handlerMethod = GetExceptionHandler(ex);
            if (handlerMethod != null)
            {
                InvokeHandlerMethod(invocation, ex, handlerMethod);
            }
        }

        /// <summary>
        /// Gets the exception handler (if any) that has been mapped to the
        /// supplied <paramref name="exception"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Will return <cref lang="null"/> if not found.
        /// </p>
        /// </remarks>
        /// <returns>
        /// The exception handler for the <see cref="System.Type"/> of the
        /// supplied <paramref name="exception"/> given exception.
        /// </returns>
        /// <param name="exception">exception that was thrown</param>
        private MethodInfo GetExceptionHandler(Exception exception)
        {
            Type exceptionClass = exception.GetType();

            #region Instrumentation

            if(log.IsDebugEnabled)
            {
                log.Debug("Trying to find handler for exception of type [" + exception.GetType().Name + "].");
            }

            #endregion

            MethodInfo handler = (MethodInfo) this.exceptionHandlers[exceptionClass];
            while (handler == null && !exceptionClass.Equals(typeof(Exception)))
            {
                exceptionClass = exceptionClass.BaseType;
                handler = (MethodInfo) this.exceptionHandlers[exceptionClass];
            }
            return handler;
        }

        /// <summary>
        /// Invokes handler method with appropriate number of parameters
        /// </summary>
        /// <param name="invocation">
        /// The original method invocation that was intercepted.
        /// </param>
        /// <param name="triggeringException">
        /// The exception that triggered this interceptor.
        /// </param>
        /// <param name="handlerMethod">
        /// The exception handler method to invoke.
        /// </param>
        private void InvokeHandlerMethod(
            IMethodInvocation invocation, Exception triggeringException, MethodInfo handlerMethod)
        {
            object[] handlerArgs;
            if (handlerMethod.GetParameters().Length == 1)
            {
                handlerArgs = new object[] {triggeringException};
            }
            else
            {
                handlerArgs = new object[] {invocation.Method, invocation.Arguments, invocation.This, triggeringException};
            }
            try
            {
                handlerMethod.Invoke(this.throwsAdvice, handlerArgs);
            }
            catch (TargetInvocationException ex)
            {
                throw ReflectionUtils.UnwrapTargetInvocationException(ex);
            }
        }
    }
}
