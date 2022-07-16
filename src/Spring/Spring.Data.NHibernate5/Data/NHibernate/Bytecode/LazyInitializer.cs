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

using System.Reflection;
using AopAlliance.Intercept;
using NHibernate.Engine;
using NHibernate.Proxy.Poco;
using NHibernate.Type;
using Spring.Aop;
using Spring.Reflection.Dynamic;

namespace Spring.Data.NHibernate.Bytecode
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Fabio Maulo</author>
    [Serializable]
    public class LazyInitializer : BasicLazyInitializer, IMethodInterceptor, ITargetSource
    {
        private static readonly MethodInfo exceptionInternalPreserveStackTrace;

        static LazyInitializer()
        {
            exceptionInternalPreserveStackTrace = typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        ///<summary>
        ///</summary>
        ///<param name="entityName"></param>
        ///<param name="persistentClass"></param>
        ///<param name="id"></param>
        ///<param name="getIdentifierMethod"></param>
        ///<param name="setIdentifierMethod"></param>
        ///<param name="componentIdType"></param>
        ///<param name="session"></param>
        public LazyInitializer(string entityName, Type persistentClass, object id, MethodInfo getIdentifierMethod,
            MethodInfo setIdentifierMethod, IAbstractComponentType componentIdType,
            ISessionImplementor session)
            : base(
                entityName,
                persistentClass,
                id,
                getIdentifierMethod,
                setIdentifierMethod,
                componentIdType,
                session,
                overridesEquals: false
            )
        {
        }

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
        public object Invoke(IMethodInvocation invocation)
        {
            try
            {
                MethodInfo methodInfo = invocation.Method;
                object returnValue = base.Invoke(methodInfo, invocation.Arguments, invocation.Proxy);

                if (returnValue != InvokeImplementation)
                {
                    return returnValue;
                }

                SafeMethod method = new SafeMethod(methodInfo);
                return method.Invoke(GetImplementation(), invocation.Arguments);
            }
            catch (TargetInvocationException ex)
            {
                exceptionInternalPreserveStackTrace.Invoke(ex.InnerException, new Object[] { });
                throw ex.InnerException;
            }
        }

        object ITargetSource.GetTarget() => Target;

        void ITargetSource.ReleaseTarget(object target) { }

        Type ITargetSource.TargetType => PersistentClass;

        bool ITargetSource.IsStatic => false;
    }
}
