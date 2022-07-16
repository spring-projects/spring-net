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

using Spring.Objects.Support;

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// An <see cref="Spring.Objects.Factory.IFactoryObject"/> that returns a value
	/// that is the result of a <see langword="static"/> or instance method invocation.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Note that this class generally is expected to be used for accessing factory methods,
	/// and as such defaults to operating in singleton mode. The first request to
	/// <see cref="Spring.Objects.Factory.Config.MethodInvokingFactoryObject.GetObject()"/>
	/// by the owning object factory will cause a method invocation, the return
	/// value of which will be cached for all subsequent requests. The
	/// <see cref="MethodInvokingFactoryObject.IsSingleton"/> property may be set to
	/// <see langword="false"/>, to cause this factory to invoke the target method each
	/// time it is asked for an object.
	/// </p>
	/// <p>
	/// A <see langword="static"/> target method may be specified by setting the
	/// <see cref="Spring.Objects.Support.MethodInvoker.TargetMethod"/> property to a string representing
	/// the <see langword="static"/> method name, with <see cref="Spring.Objects.Support.MethodInvoker.TargetType"/> specifying
	/// the <see cref="System.Type"/> that the <see langword="static"/> method is defined on.
	/// Alternatively, a target instance method may be specified, by setting the
	/// <see cref="Spring.Objects.Support.MethodInvoker.TargetObject"/> property as the target object, and
	/// the <see cref="Spring.Objects.Support.MethodInvoker.TargetMethod"/> property as the name of the
	/// method to call on that target object. Arguments for the method invocation may be
	/// specified by setting the <see cref="Spring.Objects.Support.MethodInvoker.Arguments"/> property.
	/// </p>
	/// <p>
	/// Another (esoteric) use case for this factory object is when one needs to call a method
	/// that doesn't return any value (for example, a <see langword="static"/> class method to
	/// force some sort of initialization to happen)... this use case is not supported by
	/// factory-methods, since a return value is needed to become the object.
	/// </p>
	/// <p>
	/// <note>
	/// This class depends on the
	/// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet()"/>
	/// method being called after all properties have been set, as per the
	/// <see cref="Spring.Objects.Factory.IInitializingObject"/> contract. If you are
	/// using this class outside of a Spring.NET IoC container, you must call one of either
	/// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet()"/> or
	/// <see cref="Spring.Objects.Support.MethodInvoker.Prepare()"/> yourself to ready the object's internal
	/// state, or you will get a nasty <see cref="System.NullReferenceException"/>.
	/// </note>
	/// </p>
	/// </remarks>
	/// <example>
	/// <p>
	/// The following example uses an instance of this class to call a <see langword="static"/>
	/// factory method...
	/// </p>
	/// <code escaped="true">
	/// <object id="myObject" type="Spring.Objects.Factory.Config.MethodInvokingFactoryObject, Spring.Core">
	///   <property name="TargetType" value="Whatever.MyClassFactory, MyAssembly"/>
	///   <property name="TargetMethod" value="Instance"/>
	///   <!-- the ordering of arguments is significant -->
	///   <property name="Arguments">
	///		<list>
	///			<value>1st</value>
	///			<value>2nd</value>
	///			<value>and 3rd arguments</value>
	///		</list>
	///   </property>
	/// </object>
	/// </code>
	/// <p>
	/// The following example is similar to the preceding example; the only pertinent difference is the fact that
	/// a number of different objects are passed as arguments, demonstrating that not only simple value types
	/// are valid as elements of the argument list...
	/// </p>
	/// <code language="C#">
	/// </code>
	/// <code escaped="true">
	/// <object id="myObject" type="Spring.Objects.Factory.Config.MethodInvokingFactoryObject, Spring.Core">
	///   <property name="TargetType" value="Whatever.MyClassFactory, MyAssembly"/>
	///   <property name="TargetMethod" value="Instance"/>
	///   <!-- the ordering of arguments is significant -->
	///   <property name="Arguments">
	///		<list>
	///			<!-- a primitive type (a string) -->
	///			<value>1st</value>
	///			<!-- an inner object definition is passed as the second argument -->
	///			<object type="Whatever.SomeClass, MyAssembly"/>
	///			<!-- a reference to another objects is passed as the third argument -->
	///			<ref object="someOtherObject"/>
	///			<!-- another list is passed as the fourth argument -->
	///			<list>
	///				<value>http://www.springframework.net/</value>
	///			</list>
	///		</list>
	///   </property>
	/// </object>
	/// </code>
	/// <p>
	/// Named parameters are also supported... this next example yields the same results as
	/// the preceding example (that did not use named arguments).
	/// </p>
	/// <code escaped="true">
	/// <object id="myObject" type="Spring.Objects.Factory.Config.MethodInvokingFactoryObject, Spring.Core">
	///   <property name="TargetObject">
	///     <object type="Whatever.MyClassFactory, MyAssembly"/>
	///   </property>
	///   <property name="TargetMethod" value="Execute"/>
	///   <!-- the ordering of named arguments is not significant -->
	///   <property name="NamedArguments">
	///		<dictionary>
	///			<entry key="argumentName"><value>1st</value></entry>
	///			<entry key="finalArgumentName"><value>and 3rd arguments</value></entry>
	///			<entry key="anotherArgumentName"><value>2nd</value></entry>
	///		</dictionary>
	///   </property>
	/// </object>
	/// </code>
	/// <p>
	/// Similarly, the following example uses an instance of this class to call an instance method...
	/// </p>
	/// <code escaped="true">
	/// <object id="myMethodObject" type="Whatever.MyClassFactory, MyAssembly" />
	/// <object id="myObject" type="Spring.Objects.Factory.Config.MethodInvokingFactoryObject, Spring.Core">
	///   <property name="TargetObject">
	///     <ref local="myMethodObject"/>
	///   </property>
	///   <property name="TargetMethod" value="Execute"/>
	/// </object>
	/// </code>
	/// <p>
	/// The above example could also have been written using an anonymous inner object definition... if the
	/// object on which the method is to be invoked is not going to be used outside of the factory object
	/// definition, then this is the preferred idiom because it limits the scope of the object on which the
	/// method is to be invoked to the surrounding factory object.
	/// </p>
	/// <code escaped="true">
	/// <object id="myObject" type="Spring.Objects.Factory.Config.MethodInvokingFactoryObject, Spring.Core">
	///   <property name="TargetObject">
	///     <object type="Whatever.MyClassFactory, MyAssembly"/>
	///   </property>
	///   <property name="TargetMethod" value="Execute"/>
	/// </object>
	/// </code>
	/// </example>
	/// <author>Colin Sampaleanu</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	/// <author>Simon White (.NET)</author>
	/// <seealso cref="Spring.Objects.Support.MethodInvoker"/>
	/// <seealso cref="Spring.Objects.Support.ArgumentConvertingMethodInvoker"/>
    [Serializable]
    public class MethodInvokingFactoryObject : ArgumentConvertingMethodInvoker, IFactoryObject, IInitializingObject
	{
		private bool singleton = true;
		private object singletonObject;

		/// <summary>
		/// If a singleton should be created, or a new object on each request.
		/// Defaults to <see langword="true"/>.
		/// </summary>
		public bool IsSingleton
		{
			get { return singleton; }
			set { singleton = value; }
		}

		/// <summary>
		/// Return the return value <see cref="System.Type"/> of the method
		/// that this factory invokes, or <see langword="null"/> if not
		/// known in advance.
		/// </summary>
		/// <remarks>
		/// <p>
		/// If the return value of the method that this factory is to invoke is
		/// <see langword="void"/>, then the <see cref="System.Reflection.Missing"/>
		/// <see cref="System.Type"/> will be returned (in accordance with the
		/// <see cref="Spring.Objects.Factory.IFactoryObject"/> contract that
		/// treats a <see langword="null"/> value as a configuration error).
		/// </p>
		/// </remarks>
		/// <seealso cref="Spring.Objects.Factory.IFactoryObject.ObjectType"/>
		public Type ObjectType
		{
			get
			{
				Type objectType = null;
				if (GetPreparedMethod() != null)
				{
					objectType = GetPreparedMethod().ReturnType;
					if (objectType.Equals(typeof (void)))
					{
						objectType = Void.GetType();
					}
				}
				return objectType;
			}
		}

		/// <summary>
		/// Return an instance (possibly shared or independent) of the object
		/// managed by this factory.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Returns the return value of the method that is to be invoked.
		/// </p>
		/// <p>
		/// Will return the same value each time if the
		/// <see cref="Spring.Objects.Factory.IFactoryObject.IsSingleton"/>
		/// property value is <see langword="true"/>.
		/// </p>
		/// </remarks>
		/// <returns>
		/// An instance (possibly shared or independent) of the object managed by
		/// this factory.
		/// </returns>
		/// <seealso cref="Spring.Objects.Factory.IFactoryObject.GetObject"/>
		public object GetObject()
		{
			if (singleton)
			{
				if (singletonObject == null)
				{
					singletonObject = Invoke();
				}
				return singletonObject;
			}
			return Invoke();
		}

		/// <summary>
		/// Prepares this method invoker.
		/// </summary>
		/// <exception cref="System.ArgumentException">
		/// If all required properties are not set.
		/// </exception>
		/// <exception cref="System.MissingMethodException">
		/// If the specified method could not be found.
		/// </exception>
		/// <seealso cref="Spring.Objects.Support.ArgumentConvertingMethodInvoker.Prepare()"/>
		public void AfterPropertiesSet()
		{
			Prepare();
		}
	}
}
