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
using System.Reflection.Emit;

using Spring.Proxy;


#endregion

namespace Spring.Aop.Framework.DynamicProxy
{
	/// <summary>
	/// Base class for proxy builders that can be used
    /// to create an AOP proxy for any object.
	/// </summary>
    /// <author>Bruno Baia</author>
    public abstract class AbstractAopProxyTypeBuilder :
        AbstractProxyTypeBuilder, IAopProxyTypeGenerator
    {
        #region IProxyTypeGenerator Members

        /// <summary>
        /// Generates the IL instructions that pushes
        /// the target instance on which calls should be delegated to.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        public override void PushTarget(ILGenerator il)
        {
            PushAdvisedProxy(il);
            il.Emit(OpCodes.Ldfld, References.TargetSourceField);
            il.EmitCall(OpCodes.Callvirt, References.GetTargetMethod, null);
        }

        #endregion

        #region IAopProxyTypeGenerator Members

        /// <summary>
        /// Generates the IL instructions that pushes
        /// the current <see cref="Spring.Aop.Framework.DynamicProxy.AdvisedProxy"/>
        /// instance on stack.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        public abstract void PushAdvisedProxy(ILGenerator il);

        #endregion

        #region Protected Methods

        /// <inheritdoc />
        protected override IList GetTypeAttributes(Type type)
        {
            IList attrs = base.GetTypeAttributes(type);
            int i = 0;
            while (i < attrs.Count)
            {
                if (IsAttributeMatchingType(attrs[i], typeof(SerializableAttribute)))
                {
                    attrs.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            return attrs;
        }


        #endregion
    }
}
