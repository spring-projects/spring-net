#region License

/*
 * Copyright 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace Spring.Objects
{
    /// <summary>
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class TestTransparentProxyFactory : RealProxy, IRemotingTypeInfo
    {
        public delegate object InvokeCallback(object proxy, object targetInstance, MethodInfo targetMethod, object[] arguments);

        private object targetInstance;
        private Type targetType;
        private InvokeCallback invokeHandler;

        public object TargetInstance
        {
            get { return targetInstance; }
            set { targetInstance = value; }
        }

        public Type TargetType
        {
            get { return targetType; }
            set { targetType = value; }
        }

        public InvokeCallback InvokeHandler
        {
            get { return invokeHandler; }
            set { invokeHandler = value; }
        }

        public TestTransparentProxyFactory(object targetInstance, Type targetType, InvokeCallback invokeHandler)
            : base(typeof(MarshalByRefObject))
        {
            this.targetInstance = targetInstance;
            this.targetType = targetType;
            this.invokeHandler = invokeHandler;
        }

        public override IMessage Invoke(IMessage msg)
        {
            if (msg is IMethodCallMessage)
            {
                IMethodCallMessage callMsg = (IMethodCallMessage)msg;

                // obtain method with same name & signature from target instance
                MethodInfo targetMethod = targetInstance.GetType().GetMethod(callMsg.MethodName, (Type[])callMsg.MethodSignature);

                // invoke
                object result;
                if (invokeHandler != null)
                {
                    result = invokeHandler(this, targetInstance, targetMethod, callMsg.Args);
                }
                else
                {
                    result = targetMethod.Invoke(targetInstance, callMsg.Args);
                }

                // create result msg
                return new ReturnMessage(result, null, 0, null, callMsg);
            }
            throw new NotSupportedException();
        }

        public virtual bool CanCastTo(Type fromType, object o)
        {
            // we accept ALL messages...
            return fromType.IsAssignableFrom(fromType);
        }

        public virtual string TypeName
        {
            get { return targetInstance.GetType().AssemblyQualifiedName; }
            set { throw new System.NotSupportedException(); }
        }

        public static TestTransparentProxyFactory GetProxy(object transparentProxy )
        {
            return (TestTransparentProxyFactory) RemotingServices.GetRealProxy(transparentProxy);
        }

        public static object GetTargetInstance( object transparentProxy )
        {
            return GetProxy(transparentProxy).targetInstance;
        }
    }
}