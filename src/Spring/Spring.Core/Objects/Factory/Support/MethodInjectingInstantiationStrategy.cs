#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;

using Spring.Util;

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// An <see cref="Spring.Objects.Factory.Support.IInstantiationStrategy"/>
	/// implementation that supports method injection.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Classes that want to take advantage of method injection must meet some
	/// stringent criteria. Every method that is to be method injected
	/// <b>must</b> be defined as either <see lang="virtual"/> or
	/// <see lang="abstract"/>. An <see cref="Spring.Objects.ObjectsException"/>
	/// will be thrown if these criteria are not met.
	/// </p>
	/// </remarks>
	/// <author>Rick Evans</author>
    [Serializable]
    public class MethodInjectingInstantiationStrategy : SimpleInstantiationStrategy
	{
	    /// <summary>
	    /// The name of the dynamic assembly that holds dynamically created code
	    /// </summary>
        private const string DYNAMIC_ASSEMBLY_NAME = "Spring.MethodInjected";

		/// <summary>
		/// A cache of generated <see cref="System.Type"/> instances, keyed on
		/// the object name for which the <see cref="System.Type"/> was generated.
		/// </summary>
		private IDictionary typeCache = new Hashtable();

		/// <summary>
		/// Instantiate an instance of the object described by the supplied
		/// <paramref name="definition"/> from the supplied <paramref name="factory"/>,
		/// injecting methods as appropriate.
		/// </summary>
		/// <param name="definition">
		/// The definition of the object that is to be instantiated.
		/// </param>
		/// <param name="objectName">
		/// The name associated with the object definition. The name can be the
		/// <see lang="null"/> or zero length string if we're autowiring an
		/// object that doesn't belong to the supplied
		/// <paramref name="factory"/>.
		/// </param>
		/// <param name="factory">
		/// The owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// </param>
		/// <returns>
		/// An instance of the object described by the supplied
		/// <paramref name="definition"/> from the supplied <paramref name="factory"/>.
		/// </returns>
		/// <seealso cref="Spring.Objects.Factory.Support.SimpleInstantiationStrategy.InstantiateWithMethodInjection(RootObjectDefinition, string, IObjectFactory)"/>
		protected override object InstantiateWithMethodInjection(
			RootObjectDefinition definition, string objectName, IObjectFactory factory)
		{
			return DoInstantiate(definition, objectName, factory, Type.EmptyTypes, ObjectUtils.EmptyObjects);
		}

		/// <summary>
		/// Instantiate an instance of the object described by the supplied
		/// <paramref name="definition"/> from the supplied <paramref name="factory"/>,
		/// injecting methods as appropriate.
		/// </summary>
		/// <param name="definition">
		/// The definition of the object that is to be instantiated.
		/// </param>
		/// <param name="objectName">
		/// The name associated with the object definition. The name can be the
		/// <see lang="null"/> or zero length string if we're autowiring an
		/// object that doesn't belong to the supplied
		/// <paramref name="factory"/>.
		/// </param>
		/// <param name="factory">
		/// The owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// </param>
		/// <param name="constructor">
		/// The <see cref="System.Reflection.ConstructorInfo"/> to be used to instantiate
		/// the object.
		/// </param>
		/// <param name="arguments">
		/// Any arguments to the supplied <paramref name="constructor"/>. May be null.
		/// </param>
		/// <returns>
		/// An instance of the object described by the supplied
		/// <paramref name="definition"/> from the supplied <paramref name="factory"/>.
		/// </returns>
		/// <seealso cref="Spring.Objects.Factory.Support.SimpleInstantiationStrategy.InstantiateWithMethodInjection(RootObjectDefinition, string, IObjectFactory, ConstructorInfo, object[])"/>
		protected override object InstantiateWithMethodInjection(
			RootObjectDefinition definition, string objectName, IObjectFactory factory, ConstructorInfo constructor, object[] arguments)
		{
			return DoInstantiate(definition, objectName, factory, ReflectionUtils.GetParameterTypes(constructor), arguments);
		}

		/// <summary>
		/// Instantiate an instance of the object described by the supplied
		/// <paramref name="definition"/> from the supplied <paramref name="factory"/>,
		/// injecting methods as appropriate.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This method dynamically generates a subclass that supports method
		/// injection for the supplied <paramref name="definition"/>. It then
		/// instantiates an new instance of said type using the constructor
		/// identified by the supplied <paramref name="ctorParameterTypes"/>,
		/// passing the supplied <paramref name="arguments"/> to said
		/// constructor. It then manually injects (generic) method replacement
		/// and method lookup instances (of <see cref="System.Type"/>
		/// <see cref="Spring.Objects.Factory.Support.IMethodReplacer"/>) into
		/// the new instance: those methods that are 'method-injected' will
		/// then delegate to the approriate
		/// <see cref="Spring.Objects.Factory.Support.IMethodReplacer"/>
		/// instance to effect the actual method injection.
		/// </p>
		/// </remarks>
		/// <param name="definition">
		/// The definition of the object that is to be instantiated.
		/// </param>
		/// <param name="objectName">
		/// The name associated with the object definition. The name can be the
		/// <see lang="null"/> or zero length string if we're autowiring an
		/// object that doesn't belong to the supplied
		/// <paramref name="factory"/>.
		/// </param>
		/// <param name="factory">
		/// The owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// </param>
		/// <param name="ctorParameterTypes">
		/// The parameter <see cref="System.Type"/>s to use to find the
		/// appropriate constructor to invoke.
		/// </param>
		/// <param name="arguments">
		/// The aguments that are to be passed to the appropriate constructor
		/// when the object is being instantiated.
		/// </param>
		/// <returns>
		/// A new instance of the <see cref="System.Type"/> defined by the
		/// supplied <paramref name="definition"/>.
		/// </returns>
		private object DoInstantiate(
			RootObjectDefinition definition, string objectName, IObjectFactory factory, Type[] ctorParameterTypes, object[] arguments)
		{
			Type type = GetGeneratedType(objectName, definition);
			object instance = type.GetConstructor(ctorParameterTypes).Invoke(arguments);
			IObjectWrapper wrapper = new ObjectWrapper(instance);
			wrapper.SetPropertyValue(
				MethodInjectingTypeBuilder.MethodReplacementPropertyName,
				new DelegatingMethodReplacer(definition, factory));
			wrapper.SetPropertyValue(
				MethodInjectingTypeBuilder.MethodLookupPropertyName,
				new LookupMethodReplacer(definition, factory));
			return instance;
		}

		private Type GetGeneratedType(string objectName, RootObjectDefinition definition)
		{
			lock (typeCache.SyncRoot)
			{
                Type generatedType = (Type) typeCache[objectName];
				if (generatedType == null)
				{
					#region Instrumentation

					if (log.IsDebugEnabled)
					{
						log.Debug(string.Format(CultureInfo.InvariantCulture,
						                        "Generating a subclass of the [{0}] class for the '{1}' " +
						                        	"object definition for the purposes of method injection.",
						                        definition.ObjectType, objectName));
					}

					#endregion

                    ModuleBuilder module = DynamicCodeManager.GetModuleBuilder(DYNAMIC_ASSEMBLY_NAME);
                    generatedType = new MethodInjectingTypeBuilder(module, definition).BuildType();
					typeCache[objectName] = generatedType;
				}
                return generatedType;
            }
		}

		#region Inner Class : MethodInjectingTypeBuilder

		/// <summary>
		/// A <see cref="System.Type"/> factory that generates subclasses of those
		/// classes that have been configured for the Method-Injection form of
		/// Dependency Injection.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This class is designed as for <c>one-shot</c> usage; i.e. it must
		/// be used to generate <i>exactly</i> one method injected subclass and
		/// then discarded (it maintains state in instance fields).
		/// </p>
		/// </remarks>
		private sealed class MethodInjectingTypeBuilder
		{
			/// <summary>
			/// The name of the generated <see cref="IMethodReplacer"/>
			/// property (for method replacement).
			/// </summary>
			/// <remarks>
			/// <p>
			/// Exists so that clients of this class can use this name to set properties reflectively
			/// on the dynamically generated subclass.
			/// </p>
			/// </remarks>
			internal const string MethodReplacementPropertyName = "MethodReplacement";

			/// <summary>
			/// The name of the generated <see cref="IMethodReplacer"/>
			/// property (for method lookup).
			/// </summary>
			/// <remarks>
			/// <p>
			/// Exists so that clients of this class can use this name to set properties reflectively
			/// on the dynamically generated subclass.
			/// </p>
			/// </remarks>
			internal const string MethodLookupPropertyName = "MethodLookup";

			private RootObjectDefinition objectDefinition;
			private FieldBuilder methodReplacementField;
			private FieldBuilder methodLookupField;
			private ModuleBuilder module;

			private readonly MethodInfo MethodReplacerImplementMethod
				= typeof (IMethodReplacer).GetMethod("Implement", new Type[] {typeof (object), typeof (MethodInfo), typeof (object[])});

			/// <summary>
			/// Creates a new instance of the
			/// <see cref="MethodInjectingTypeBuilder"/> class.
			/// </summary>
			/// <param name="module">
			/// The <see cref="System.Reflection.Emit.ModuleBuilder"/> in which
			/// the generated <see cref="System.Type"/> is to be defined.
			/// </param>
			/// <param name="objectDefinition">
			/// The object definition that is the target of the method injection.
			/// </param>
			/// <exception cref="System.ArgumentNullException">
			/// If either of the supplied arguments is <see langword="null"/>.
			/// </exception>
			public MethodInjectingTypeBuilder(ModuleBuilder module, RootObjectDefinition objectDefinition)
			{
				AssertUtils.ArgumentNotNull(module, "module");
				AssertUtils.ArgumentNotNull(objectDefinition, "objectDefinition");
				this.module = module;
				this.objectDefinition = objectDefinition;
			}

			/// <summary>
			/// Builds a <see cref="System.Type"/> suitable for Method-Injection.
			/// </summary>
			/// <returns>
			/// A <see cref="System.Type"/> suitable for Method-Injection.
			/// </returns>
			public Type BuildType()
			{
				TypeBuilder typeBuilder = DefineType();
				DefineFields(typeBuilder);
				DefineConstructors(typeBuilder);
				DefineProperties(typeBuilder);
				DefineMethods(typeBuilder);
				return typeBuilder.CreateTypeInfo();
			}

			private Type BaseType
			{
				get { return this.objectDefinition.ObjectType; }
			}

			private TypeBuilder DefineProperties(TypeBuilder typeBuilder)
			{
				DefineWritePropertyForMethodReplacement(typeBuilder, MethodReplacementPropertyName, this.methodReplacementField);
				DefineWritePropertyForMethodReplacement(typeBuilder, MethodLookupPropertyName, this.methodLookupField);
				return typeBuilder;
			}

			private TypeBuilder DefineType()
			{
                // Generates unique type name
                string generatedSubclassName = String.Format("{0}_{1}",
                    BaseType.FullName, Guid.NewGuid().ToString("N"));
				return this.module.DefineType(
					generatedSubclassName, TypeAttributes.BeforeFieldInit | TypeAttributes.Public, BaseType);
			}

			private TypeBuilder DefineFields(TypeBuilder typeBuilder)
			{
				methodReplacementField = typeBuilder.DefineField("methodReplacement", typeof (IMethodReplacer), FieldAttributes.Private);
				methodLookupField = typeBuilder.DefineField("methodLookup", typeof (IMethodReplacer), FieldAttributes.Private);
				return typeBuilder;
			}

			private TypeBuilder DefineConstructors(TypeBuilder typeBuilder)
			{
				ConstructorInfo[] constructors = BaseType.GetConstructors(
					BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				for (int i = 0; i < constructors.Length; ++i)
				{
					ConstructorInfo constructor = constructors[i];
					if (constructor.IsPublic || constructor.IsFamily)
					{
						MethodAttributes attributes = MethodAttributes.Public |
							MethodAttributes.HideBySig | MethodAttributes.SpecialName |
							MethodAttributes.RTSpecialName;
						ConstructorBuilder cb = typeBuilder.DefineConstructor(attributes,
						                                                      constructor.CallingConvention,
						                                                      ReflectionUtils.GetParameterTypes(constructor.GetParameters()));
						ILGenerator il = cb.GetILGenerator();
						int paramCount = constructor.GetParameters().Length;
						il.Emit(OpCodes.Ldarg_0);
						for (int j = 1; j <= paramCount; ++j)
						{
							il.Emit(OpCodes.Ldarg_S, j);
						}
						il.Emit(OpCodes.Call, constructor);
						il.Emit(OpCodes.Ret);
					}
				}
				return typeBuilder;
			}

			/// <summary>
			/// Defines overrides for those methods that are configured with an appropriate
			/// <see cref="Spring.Objects.Factory.Support.MethodOverride"/>.
			/// </summary>
			/// <param name="typeBuilder">
			/// The overarching <see cref="System.Reflection.Emit.TypeBuilder"/> that is defining
			/// the generated <see cref="System.Type"/>.
			/// </param>
			private TypeBuilder DefineMethods(TypeBuilder typeBuilder)
			{
				MethodInfo[] methods = BaseType.GetMethods(
					BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
				for (int i = 0; i < methods.Length; ++i)
				{
					MethodInfo method = methods[i];
					MethodOverride methodOverride
						= this.objectDefinition.MethodOverrides.GetOverride(method);
					if (methodOverride != null)
					{
						if (!method.IsVirtual || method.IsFinal)
						{
							throw new ObjectCreationException(
								"A replaced method must be marked as either abstract or virtual.");
						}
						FieldBuilder field = null;
						if (methodOverride is ReplacedMethodOverride)
						{
							field = this.methodReplacementField;
						}
						else
						{
							// lookup methods cannot have any arguments...
							if (method.GetParameters().Length > 0)
							{
								throw new ObjectCreationException(
									"The signature of a lookup method cannot have any arguments.");
							}
							// lookup methods cannot return void...
							if (method.ReturnType == typeof (void))
							{
								throw new ObjectCreationException(
									"A lookup method cannot be declared with a void return type.");
							}
							field = this.methodLookupField;
						}
						DefineReplacedMethod(typeBuilder, method, field);
					}
				}
				return typeBuilder;
			}

			/// <summary>
			/// Override the supplied <paramref name="method"/> with the logic
			/// encapsulated by the
			/// <see cref="Spring.Objects.Factory.Support.IMethodReplacer"/>
			/// defined by the supplied <paramref name="field"/>.
			/// </summary>
			/// <param name="typeBuilder">
			/// The builder for the subclass that is being generated.
			/// </param>
			/// <param name="method">
			/// The method on the superclass that is to be overridden.
			/// </param>
			/// <param name="field">
			/// The field defining the
			/// <see cref="Spring.Objects.Factory.Support.IMethodReplacer"/>
			/// that the overridden method will delegate to to do the 'actual'
			/// method injection logic.
			/// </param>
			private void DefineReplacedMethod(TypeBuilder typeBuilder, MethodInfo method, FieldBuilder field)
			{
				ParameterInfo[] methodParameters = method.GetParameters();
				MethodBuilder methodBuilder
					= typeBuilder.DefineMethod(method.Name,
					                           CalculateMethodAttributes(method),
					                           method.CallingConvention,
					                           method.ReturnType,
					                           ReflectionUtils.GetParameterTypes(methodParameters));
                //DefineOverrideMethodParameters(methodParameters, methodBuilder);
				ILGenerator il = methodBuilder.GetILGenerator();
				LocalBuilder returnValue = DefineReturnValueIfAny(method, il);
				// prepare the invocation of the 'Implement' method for the 'field' (an IMethodReplacer)...
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldfld, field);
				PushArguments(methodParameters, il);
				// invoke the 'Implement' method of the IMethodReplacer in the 'field'...
				il.Emit(OpCodes.Callvirt, MethodReplacerImplementMethod);
				SetupTheReturnValueIfAny(returnValue, il);
				il.Emit(OpCodes.Ret);
			}

            /*
			/// <summary>
			/// Defines the parameters to the method that is being overridden.
			/// </summary>
			/// <remarks>
			/// <p>
			/// Since we are simply overridding a method (in this method
			/// injection context), all we do here is simply copy the
			/// parameters (since we want a method with the exact same parameters).
			/// </p>
			/// </remarks>
			/// <param name="methodParameters">
			/// The parameters to the original method that is being overridden.
			/// </param>
			/// <param name="methodBuilder">
			/// The builder we are using to define the new overridden method.
			/// </param>
			private static void DefineOverrideMethodParameters(
				ParameterInfo[] methodParameters, MethodBuilder methodBuilder)
			{
				for (int i = 0; i < methodParameters.Length; ++i)
				{
					ParameterInfo parameter = methodParameters[i];
					methodBuilder.DefineParameter(i + 1, parameter.Attributes, parameter.Name);
				}
			}
            */

			/// <summary>
			/// Generates the MSIL for actually returning a return value if the
			/// supplied <paramref name="returnValue"/> is not
			/// <see lang="null"/>.
			/// </summary>
			/// <param name="returnValue">
			/// The definition of the return value; if <see lang="null"/>, it
			/// means that no return value is to required (a <c>void</c>
			/// return type).
			/// </param>
			/// <param name="il">
			/// The <see cref="System.Reflection.Emit.ILGenerator"/> to emit
			/// the MSIL to.
			/// </param>
			private static void SetupTheReturnValueIfAny(LocalBuilder returnValue, ILGenerator il)
			{
				if (returnValue != null)
				{
					il.Emit(OpCodes.Castclass, returnValue.LocalType);
					il.Emit(OpCodes.Stloc, returnValue);
					il.Emit(OpCodes.Ldloc, returnValue);
				}
				else
				{
					il.Emit(OpCodes.Pop);
				}
			}

			/// <summary>
			/// Generates the MSIL for a return value if the supplied
			/// <paramref name="method"/> returns a value.
			/// </summary>
			/// <param name="method">
			/// The method to be checked.
			/// </param>
			/// <param name="il">
			/// The <see cref="System.Reflection.Emit.ILGenerator"/> to emit
			/// the MSIL to.
			/// </param>
			/// <returns>
			/// The return value, or <see lang="null"/> if the method does not
			/// return a value (has a <c>void</c> return type).
			/// </returns>
			private static LocalBuilder DefineReturnValueIfAny(MethodInfo method, ILGenerator il)
			{
				LocalBuilder returnValue = null;
				if (method.ReturnType != typeof (void))
				{
					returnValue = il.DeclareLocal(method.ReturnType);
				}
				return returnValue;
			}

			/// <summary>
			/// Pushes (sets up) the arguments for a call to the
			/// <see cref="Spring.Objects.Factory.Support.IMethodReplacer.Implement(object, MethodInfo, object[])"/>
			/// method of an appropriate
			/// <see cref="Spring.Objects.Factory.Support.IMethodReplacer"/>.
			/// </summary>
			/// <param name="methodParameters">
			/// The parameters to the <i>original</i> method (will be bundled
			/// up into a generic <c>object[]</c> and passed as the third
			/// argument to the
			/// <see cref="Spring.Objects.Factory.Support.IMethodReplacer.Implement(object, MethodInfo, object[])"/>
			/// invocation.
			/// </param>
			/// <param name="il">
			/// The <see cref="System.Reflection.Emit.ILGenerator"/> to emit
			/// the MSIL to.
			/// </param>
			private static void PushArguments(ParameterInfo[] methodParameters, ILGenerator il)
			{
				// push 'this' (1st arg)...
				il.Emit(OpCodes.Ldarg_0);
				// push the currently executing method (2nd arg)...
				il.Emit(OpCodes.Call, typeof (MethodBase).GetMethod("GetCurrentMethod", BindingFlags.Static | BindingFlags.Public));
				il.Emit(OpCodes.Castclass, typeof (MethodInfo));
				// push the arguments to the currently executing method as an object [] (3rd arg)...
				il.Emit(OpCodes.Ldc_I4, methodParameters.Length);
				LocalBuilder args = il.DeclareLocal(typeof (object[]));
				il.Emit(OpCodes.Newarr, typeof (object));
				il.Emit(OpCodes.Stloc, args);
				for (int i = 0; i < methodParameters.Length; ++i)
				{
					il.Emit(OpCodes.Ldloc, args);
					il.Emit(OpCodes.Ldc_I4, i);
					il.Emit(OpCodes.Ldarg_S, i + 1);
					ParameterInfo parameter = methodParameters[i];
					if (parameter.ParameterType.IsEnum || parameter.ParameterType.IsValueType)
					{
						il.Emit(OpCodes.Box, parameter.ParameterType);
					}
					il.Emit(OpCodes.Stelem_Ref);
				}
				il.Emit(OpCodes.Ldloc, args);
			}

			/// <summary>
			/// Simply generates the IL for a write only property for the
			/// <see cref="IMethodReplacer"/> <see cref="System.Type"/>.
			/// </summary>
			/// <param name="typeBuilder">
			/// The <see cref="System.Type"/> in which the property is defined.
			/// </param>
			/// <param name="propertyName">
			/// The name of the (to be) generated property.
			/// </param>
			/// <param name="field">
			/// The (instance) field that the property is to 'set'.
			/// </param>
			private void DefineWritePropertyForMethodReplacement(
				TypeBuilder typeBuilder, string propertyName, FieldBuilder field)
			{
				MethodBuilder setMethodBuilder = typeBuilder.DefineMethod(
					"set_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName,
					typeof (void), new Type[] {typeof (IMethodReplacer)});
				ILGenerator il = setMethodBuilder.GetILGenerator();
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Stfld, field);
				il.Emit(OpCodes.Ret);
				PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(
					propertyName, PropertyAttributes.None, typeof (IMethodReplacer), Type.EmptyTypes);
				propertyBuilder.SetSetMethod(setMethodBuilder);
			}

			private MethodAttributes CalculateMethodAttributes(MethodInfo method)
			{
                MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.ReuseSlot
                    | MethodAttributes.HideBySig | MethodAttributes.Virtual;
				if (method.IsSpecialName)
				{
					return attributes | MethodAttributes.SpecialName;
				}
				return attributes;
			}
		}

		#endregion
	}
}
