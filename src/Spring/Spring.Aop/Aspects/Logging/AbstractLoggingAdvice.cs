#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using System.Reflection;
using System.Runtime.Serialization;
using AopAlliance.Intercept;
using Common.Logging;
using Spring.Aop.Framework;

namespace Spring.Aspects.Logging
{
    /// <summary>
    /// Abstract base class for logging advice
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    /// <author>Mark Pollack</author>
    [Serializable]
    public abstract class AbstractLoggingAdvice : IMethodInterceptor, IDeserializationCallback
    {
        #region Fields

        /// <summary>
        /// The default <code>ILog</code> instance used to write logging messages.
        /// </summary>
        [NonSerialized]
        protected ILog defaultLogger;

        /// <summary>
        /// The name of the logger instance to use for obtaining from <see cref="LogManager.GetLogger(string)"/>.
        /// </summary>
        private string defaultLoggerName;

        /// <summary>
        /// Indicates whether or not proxy type names should be hidden when using dynamic loggers.
        /// </summary>
        private bool hideProxyTypeNames = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new advice instance using this advice type's name for logging by default.
        /// </summary>
        protected AbstractLoggingAdvice()
        {
            SetDefaultLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);
        }

        /// <summary>
        /// Creates a new advice instance using the given logger by default.
        /// </summary>
        protected AbstractLoggingAdvice(ILog defaultLogger)
        {
            this.defaultLogger = defaultLogger;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Sets a value indicating whether to use a dynamic logger or static logger
        /// </summary>
        /// <remarks>Default is to use a static logger.
        /// <para>
        /// Used to determine which ILog instance should be used to write log messages for
        /// a particular method invocation: a dynamic one for the Type getting called,
        /// or a static one for the Type of the trace interceptor.
        /// </para>
        /// <para>
        /// Specify either this property or LoggerName, not both.
        /// </para>
        /// </remarks>
        /// <value><c>true</c> if use dynamic logger; otherwise, <c>false</c>.</value>
        public bool UseDynamicLogger
        {
            set
            {
                SetDefaultLogger(value ? null : this.GetType().FullName);
            }
        }

        /// <summary>
        /// Sets the name of the logger to use.
        /// </summary>
        /// <remarks>
        /// The name will be passed to the underlying logging implementation through Common.Logging,
        /// getting interpreted as the log category according to the loggers configuration.
        /// <para>
        /// This can be specified to not log into the category of a Type (whether this
        /// interceptor's class or the class getting called) but rather to a specific named category.
        /// </para>
        /// <para>
        /// Specify either this property or UseDynamicLogger, but not both.
        /// </para>
        /// </remarks>
        /// <value>The name of the logger.</value>
        public string LoggerName
        {
            set
            {
                SetDefaultLogger(value);
            }
        }


        /// <summary>
        /// Sets a value indicating whether hide proxy type names (whenever possible)
        /// when using dynamic loggers, i.e. property UseDynamicLogger is set to true.
        /// </summary>
        /// <value><c>true</c> if [hide proxy type names]; otherwise, <c>false</c>.</value>
        public bool HideProxyTypeNames
        {
            set { hideProxyTypeNames = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds logging to the method invocation.
        /// </summary>
        /// <remarks>
        /// The method IsInterceptorEnabled is called
        /// as an optimization to determine if logging should be applied.  If logging should be
        /// applied, the method invocation is passed to the InvokeUnderLog method for handling.
        /// If not, the method proceeds as normal.
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
        public object Invoke(IMethodInvocation invocation)
        {
            object o = invocation.This;
            ILog log = GetLoggerForInvocation(invocation);
            if (IsInterceptorEnabled(invocation, log))
            {
                return InvokeUnderLog(invocation, log);
            }
            else
            {
                return invocation.Proceed();
            }

        }

        /// <summary>
        /// Determines whether the interceptor is enabled for the specified invocation, that
        /// is, whether the method InvokeUnderLog is called.
        /// </summary>
        /// <remarks>The default behavior is to check whether the given ILog instance
        /// is enabled by calling IsLogEnabled, whose default behavior is to check if
        /// the TRACE level of logging is enabled.  Subclasses</remarks>
        /// <param name="invocation">The invocation.</param>
        /// <param name="log">The log to write messages to</param>
        /// <returns>
        /// 	<c>true</c> if [is interceptor enabled] [the specified invocation]; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsInterceptorEnabled(IMethodInvocation invocation, ILog log)
        {
            return IsLogEnabled(log);
        }

        /// <summary>
        /// Determines whether the given log is enabled.
        /// </summary>
        /// <remarks>
        /// Default is true when the trace level is enabled.  Subclasses may override this
        /// to change the level at which logging occurs, or return true to ignore level
        /// checks.</remarks>
        /// <param name="log">The log instance to check.</param>
        /// <returns>
        /// 	<c>true</c> if log is for a given log level; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsLogEnabled(ILog log)
        {
            return log.IsTraceEnabled;
        }

        /// <summary>
        /// Subclasses must override this method to perform any tracing around the supplied
        /// IMethodInvocation.
        /// </summary>
        /// <remarks>
        /// Subclasses are resonsible for ensuring that the IMethodInvocation actually executes
        /// by calling IMethodInvocation.Proceed().
        /// <para>
        /// By default, the passed-in ILog instance will have log level
        /// "trace" enabled. Subclasses do not have to check for this again, unless
        /// they overwrite the IsInterceptorEnabled method to modify
        /// the default behavior.
        /// </para>
        /// </remarks>
        /// <param name="invocation">The method invocation to log</param>
        /// <param name="log">The log to write messages to</param>
        /// <returns>The result of the call to IMethodInvocation.Proceed()
        /// </returns>
        /// <exception cref="System.Exception">
        /// If any of the interceptors in the chain or the target object itself
        /// throws an exception.
        /// </exception>
        protected abstract object InvokeUnderLog(IMethodInvocation invocation, ILog log);


        /// <summary>
        /// Gets the appropriate log instance to use for the given IMethodInvocation.
        /// </summary>
        /// <remarks>
        /// If the UseDynamicLogger property is set to true, the ILog instance will be
        /// for the target class of the IMethodInvocation, otherwise the log will be the
        /// default static logger.
        /// </remarks>
        /// <param name="invocation">The method invocation being logged.</param>
        /// <returns>The ILog instance to use.</returns>
        protected virtual ILog GetLoggerForInvocation(IMethodInvocation invocation)
        {
            if (defaultLogger != null)
            {
                return defaultLogger;
            }
            else
            {
                object target = invocation.This;
                Type logCategoryType = target.GetType();
                if (hideProxyTypeNames)
                {
                    logCategoryType = AopUtils.GetTargetType(target);
                }
                return LogManager.GetLogger(logCategoryType);
            }
        }

        #endregion

        /// <summary>
        /// Sets the default logger to the given name.
        /// </summary>
        /// <param name="name">if <c>null</c>, the default logger is removed.</param>
        protected void SetDefaultLogger(string name)
        {
            defaultLogger = (name == null ? null : LogManager.GetLogger(name));
            defaultLoggerName = name;
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            OnDeserialization(sender);
        }

        /// <summary>
        /// Override in case you need to initialized non-serialized fields on deserialization.
        /// </summary>
        protected virtual void OnDeserialization(object sender)
        {
            SetDefaultLogger(this.defaultLoggerName);
        }
    }
}
