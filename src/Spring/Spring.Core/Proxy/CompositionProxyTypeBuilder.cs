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

using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace Spring.Proxy
{
    /// <summary>
    /// Builds a proxy type using composition.
    /// </summary>
    /// <remarks>
    /// <note>
    /// In order for this builder to work, the target <b>must</b> implement
    /// one or more interfaces.
    /// </note>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    /// <author>Bruno Baia</author>
    public class CompositionProxyTypeBuilder : AbstractProxyTypeBuilder
    {
        #region Fields

        private bool explicitInterfaceImplementation = false;

        /// <summary>
        /// Target instance calls should be delegated to.
        /// </summary>
        protected FieldBuilder targetInstance;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether interfaces should be implemented explicitly.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if they should be; otherwise, <see langword="false"/>.
        /// </value>
        public bool ExplicitInterfaceImplementation
        {
            get { return explicitInterfaceImplementation; }
            set { explicitInterfaceImplementation = value; }
        }

        #endregion

        #region Constructor(s) / Destructor

        /// <summary>
        /// Creates a new instance of the 
        /// <see cref="CompositionProxyTypeBuilder"/> class.
        /// </summary>
        public CompositionProxyTypeBuilder()
        {
            Name = "CompositionProxy";
        }

        #endregion

        #region IProxyTypeBuilder Members

        /// <summary>
        /// Creates a proxy that delegates calls to an instance of the
        /// target object.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Only interfaces can be proxied using composition, so the target
        /// <b>must</b> implement one or more interfaces.
        /// </p>
        /// </remarks>
        /// <returns>The generated proxy class.</returns>
        /// <exception cref="System.ArgumentException">
        /// If the <see cref="IProxyTypeBuilder.TargetType"/>
        /// does not implement any interfaces.
        /// </exception>
        public override Type BuildProxyType()
        {
            if (Interfaces == null || Interfaces.Count == 0)
            {
                throw new ArgumentException(
                    "Composition proxy target must implement at least one interface.");
            }

            TypeBuilder typeBuilder = CreateTypeBuilder(Name, BaseType);

            // apply custom attributes to the proxy type.
            ApplyTypeAttributes(typeBuilder, TargetType);

            // declare fields
            DeclareTargetInstanceField(typeBuilder);

            // create constructors
            ImplementConstructors(typeBuilder);

            // implement interfaces
            foreach (Type intf in Interfaces)
            {
                ImplementInterface(typeBuilder,
                    CreateTargetProxyMethodBuilder(typeBuilder),
                    intf, TargetType);
            }

            ImplementCustom(typeBuilder);

            return typeBuilder.CreateTypeInfo();
        }

        /// <summary>
        /// Create an <see cref="IProxyMethodBuilder"/> to create interface implementations
        /// </summary>
        protected virtual IProxyMethodBuilder CreateTargetProxyMethodBuilder(TypeBuilder typeBuilder)
        {
            return new TargetProxyMethodBuilder(typeBuilder, this, explicitInterfaceImplementation);
        }

        #endregion

        /// <summary>
        /// Allows subclasses to generate additional code
        /// </summary>
        protected virtual void ImplementCustom(TypeBuilder builder)
        { }

        #region IProxyTypeGenerator Members

        /// <summary>
        /// Generates the IL instructions that pushes 
        /// the target instance on which calls should be delegated to.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        public override void PushTarget(ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, targetInstance);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Deaclares a field that holds the target object instance.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="System.Type"/> builder to use for code generation.
        /// </param>
        protected virtual void DeclareTargetInstanceField(TypeBuilder builder)
        {
            targetInstance = builder.DefineField("__proxyTarget", TargetType, FieldAttributes.Private);
        }

        /// <summary>
        /// Generates the proxy constructor.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This implementation creates instance of the target object for delegation
        /// using constructor arguments.
        /// </p>
        /// </remarks>
        /// <param name="builder">The constructor builder to use.</param>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="constructor">The constructor to delegate the creation to.</param>
        protected override void GenerateConstructor(
            ConstructorBuilder builder, ILGenerator il, ConstructorInfo constructor)
        {
            int paramCount = constructor.GetParameters().Length;
            il.Emit(OpCodes.Ldarg_0);
            for (int i = 1; i <= paramCount; i++)
            {
                il.Emit(OpCodes.Ldarg_S, i);
            }

            il.Emit(OpCodes.Newobj, constructor);
            il.Emit(OpCodes.Stfld, targetInstance);
        }

        #endregion
    }
}
