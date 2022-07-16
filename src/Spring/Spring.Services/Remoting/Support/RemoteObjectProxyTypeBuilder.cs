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
using System.Reflection.Emit;

using Spring.Proxy;

namespace Spring.Remoting.Support
{
    /// <summary>
    /// Builds a proxy type based on <see cref="BaseRemoteObject"/> to wrap a target object
    /// that is intended to be remotable.
    /// </summary>
    /// <author>Bruno Baia</author>
    internal class RemoteObjectProxyTypeBuilder : CompositionProxyTypeBuilder
    {
        #region Fields

        private static readonly MethodInfo TimeSpan_FromTicks =
            typeof(TimeSpan).GetMethod("FromTicks", BindingFlags.Public | BindingFlags.Static);

        private static readonly MethodInfo BaseRemoteObject_IsInfinite =
            typeof(BaseRemoteObject).GetMethod("set_IsInfinite", BindingFlags.Public | BindingFlags.Instance);

        private static readonly MethodInfo BaseRemoteObject_InitialLeaseTime =
            typeof(BaseRemoteObject).GetMethod("set_InitialLeaseTime", BindingFlags.Public | BindingFlags.Instance);

        private static readonly MethodInfo BaseRemoteObject_RenewOnCallTime =
            typeof(BaseRemoteObject).GetMethod("set_RenewOnCallTime", BindingFlags.Public | BindingFlags.Instance);

        private static readonly MethodInfo BaseRemoteObject_SponsorshipTimeout =
            typeof(BaseRemoteObject).GetMethod("set_SponsorshipTimeout", BindingFlags.Public | BindingFlags.Instance);

        private ILifetime lifetime;

        #endregion

        #region Constructor(s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="RemoteObjectProxyTypeBuilder"/> class.
        /// </summary>
        /// <param name="lifetime">
        /// The lifetime properties to be applied to the target object.
        /// </param>
        public RemoteObjectProxyTypeBuilder(ILifetime lifetime)
        {
            this.lifetime = lifetime;

            Name = "RemoteObjectProxy";
        }

        #endregion

        #region IProxyTypeBuilder Members

        /// <summary>
        /// Creates a remotable proxy type based on <see cref="BaseRemoteObject"/>.
        /// </summary>
        /// <returns>The generated proxy class.</returns>
        /// <exception cref="System.ArgumentException">
        /// If the <see cref="IProxyTypeBuilder.BaseType"/> is not
        /// an instance of <see cref="BaseRemoteObject"/>.
        /// </exception>
        public override Type BuildProxyType()
        {
            if (!typeof(BaseRemoteObject).IsAssignableFrom(this.BaseType))
            {
                throw new ArgumentException(
                    "BaseRemoteObject cannot be assigned from BaseType.", "BaseType");
            }

            return base.BuildProxyType();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Implements constructors for the proxy class.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="System.Reflection.Emit.TypeBuilder"/> to use.
        /// </param>
        protected override void ImplementConstructors(TypeBuilder builder)
        {
            MethodAttributes attributes = MethodAttributes.Public |
                MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName;
            ConstructorBuilder cb = builder.DefineConstructor(attributes,
                CallingConventions.Standard,
                new Type[1] { TargetType });

            ILGenerator il = cb.GetILGenerator();

            GenerateRemoteObjectLifetimeInitialization(il);

            PushProxy(il);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, targetInstance);

            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Generate initialization code for <see cref="BaseRemoteObject"/>'s lifetime properties.
        /// </summary>
        /// <param name="il">ILGenerator</param>
        protected void GenerateRemoteObjectLifetimeInitialization(ILGenerator il)
        {
            // Set IsInfinite Property
            if (this.lifetime.Infinite)
            {
                PushProxy(il);
                il.Emit(OpCodes.Ldc_I4_1);	// true
                il.Emit(OpCodes.Call, BaseRemoteObject_IsInfinite);
            }
            else
            {
                // Set InitialLeaseTime Property
                if (this.lifetime.InitialLeaseTime != TimeSpan.Zero)
                {
                    PushProxy(il);
                    il.Emit(OpCodes.Ldc_I8, this.lifetime.InitialLeaseTime.Ticks);
                    il.Emit(OpCodes.Call, TimeSpan_FromTicks);
                    il.Emit(OpCodes.Call, BaseRemoteObject_InitialLeaseTime);
                }

                // Set RenewOnCallTime Property
                if (this.lifetime.RenewOnCallTime != TimeSpan.Zero)
                {
                    PushProxy(il);
                    il.Emit(OpCodes.Ldc_I8, this.lifetime.RenewOnCallTime.Ticks);
                    il.Emit(OpCodes.Call, TimeSpan_FromTicks);
                    il.Emit(OpCodes.Call, BaseRemoteObject_RenewOnCallTime);
                }

                // Set SponsorshipTimeout Property
                if (this.lifetime.SponsorshipTimeout != TimeSpan.Zero)
                {
                    PushProxy(il);
                    il.Emit(OpCodes.Ldc_I8, this.lifetime.SponsorshipTimeout.Ticks);
                    il.Emit(OpCodes.Call, TimeSpan_FromTicks);
                    il.Emit(OpCodes.Call, BaseRemoteObject_SponsorshipTimeout);
                }
            }
        }

        #endregion
    }
}
