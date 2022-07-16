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

using System.Reflection;
using System.Reflection.Emit;

namespace Spring.Proxy
{
	/// <summary>
	/// Builds a proxy type using inheritance.
	/// </summary>
	/// <remarks>
	/// <note>
	/// In order for this builder to work, target methods have to be either
	/// <see langword="virtual"/>, or belong to an interface.
	/// </note>
	/// </remarks>
	/// <author>Aleksandar Seovic</author>
    /// <author>Bruno Baia</author>
	public class InheritanceProxyTypeBuilder : AbstractProxyTypeBuilder
	{
        #region Fields

        private bool declaredMembersOnly = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether inherited members should be proxied.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if they should be; otherwise, <see langword="false"/>.
        /// </value>
        public bool DeclaredMembersOnly
        {
            get { return declaredMembersOnly; }
            set { declaredMembersOnly = value; }
        }

        #endregion

        #region Constructor(s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="InheritanceProxyTypeBuilder"/> class.
        /// </summary>
        public InheritanceProxyTypeBuilder()
        {
            Name = "InheritanceProxy";
            //ProxyTargetAttributes = false;
        }

        #endregion

        #region IProxyTypeBuilder Members

        /// <summary>
		/// Creates a proxy that inherits the proxied object's class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Only <see langword="virtual"/> (non-final) methods can be proxied,
		/// unless they are members of one of the interfaces that target class
		/// implements. In that case, methods will be proxied using explicit
		/// interface implementation, which means that client code will have
		/// to cast the proxy to a specific interface in order to invoke the
		/// methods.
		/// </p>
		/// </remarks>
		/// <returns>The generated proxy class.</returns>
		public override Type BuildProxyType()
		{
			BaseType = TargetType;
            if (BaseType.IsSealed)
            {
                throw new ArgumentException("Inheritance proxy cannot be created for a sealed class [" + BaseType.FullName + "]");
            }

            TypeBuilder typeBuilder = CreateTypeBuilder(Name, BaseType);

            // apply custom attributes to the proxy type.
            ApplyTypeAttributes(typeBuilder, BaseType);

            // create constructors
            ImplementConstructors(typeBuilder);

            // proxy only final methods that are members of one of the interfaces
            foreach (Type intf in Interfaces)
            {
                ImplementInterface(typeBuilder,
                    new BaseProxyMethodBuilder(typeBuilder, this, true),
                    intf, TargetType, false);
            }

            // proxy base virtual methods
            InheritType(typeBuilder,
                new BaseProxyMethodBuilder(typeBuilder, this, false),
                BaseType, declaredMembersOnly);

            return typeBuilder.CreateTypeInfo();
        }

        #endregion

        #region IProxyTypeGenerator Members

        /// <summary>
        /// Generates the IL instructions that pushes
        /// the target instance on which calls should be delegated to.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        public override void PushTarget(ILGenerator il)
        {
            PushProxy(il);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Generates the proxy constructor.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This implementation delegates the call to a base class constructor.
        /// </p>
        /// </remarks>
        /// <param name="builder">The constructor builder to use.</param>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="constructor">
        /// The base class constructor to delegate the call to.
        /// </param>
        protected override void GenerateConstructor(
            ConstructorBuilder builder, ILGenerator il, ConstructorInfo constructor)
        {
            int paramCount = constructor.GetParameters().Length;
            il.Emit(OpCodes.Ldarg_0);
            for (int i = 1; i <= paramCount; i++)
            {
                il.Emit(OpCodes.Ldarg_S, i);
            }
            il.Emit(OpCodes.Call, constructor);
        }

        #endregion
    }
}
