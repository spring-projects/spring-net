/*
 * Copyright  2002-2005 the original author or authors.
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

using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.Remoting;
#if !NETSTANDARD
using System.Runtime.Remoting.Proxies;
#endif
using Common.Logging;

using Spring.Reflection.Dynamic;

namespace Spring.Util
{
    /// <summary>
    /// Helper methods with regard to objects, types, properties, etc.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Not intended to be used directly by applications.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    public sealed class ObjectUtils
    {
        /// <summary>
        /// The <see cref="Common.Logging.ILog"/> instance for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(ObjectUtils));

        /// <summary>
        /// An empty object array.
        /// </summary>
        public static readonly object[] EmptyObjects = { };

        private static readonly MethodInfo GetHashCodeMethodInfo;

        static ObjectUtils()
		{
			Type type = typeof(object);
			GetHashCodeMethodInfo = type.GetMethod("GetHashCode");
		}

        // CLOVER:OFF

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.Util.ObjectUtils"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is a utility class, and as such exposes no public constructors.
        /// </p>
        /// </remarks>
        private ObjectUtils()
        {
        }

        // CLOVER:ON

        /// <summary>
        /// Instantiates the type using the assembly specified to load the type.
        /// </summary>
        /// <remarks>This is a convenience in the case of needing to instantiate a type but not
        /// wanting to specify in the string the version, culture and public key token.</remarks>
        /// <param name="assembly">The assembly.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"> 
        /// If the <paramref name="assembly"/> or <paramref name="typeName"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="Spring.Util.FatalReflectionException">
        /// If cannot load the type from the assembly or the call to <code>InstantiateType(Type)</code> fails.
        /// </exception>
        public static object InstantiateType(Assembly assembly, string typeName)
        {
            AssertUtils.ArgumentNotNull(assembly, "assembly");
            AssertUtils.ArgumentNotNull(typeName, "typeName");
            Type resolvedType = assembly.GetType(typeName, false, false);
            if (resolvedType == null)
            {
                throw new FatalReflectionException(
                    string.Format(
                        CultureInfo.InvariantCulture, "Cannot load type named [{0}] from assembly [{1}].", typeName, assembly));
            }
            return InstantiateType(resolvedType);
        }
        /// <summary>
        /// Convenience method to instantiate a <see cref="System.Type"/> using
        /// its no-arg constructor.
        /// </summary>
        /// <remarks>
        /// <p>
        /// As this method doesn't try to instantiate <see cref="System.Type"/>s
        /// by name, it should avoid <see cref="System.Type"/> loading issues.
        /// </p>
        /// </remarks>
        /// <param name="type">
        /// The <see cref="System.Type"/> to instantiate*
        /// </param>
        /// <returns>A new instance of the <see cref="System.Type"/>.</returns>
        /// <exception cref="System.ArgumentNullException"> 
        /// If the <paramref name="type"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="Spring.Util.FatalReflectionException">
        /// If the <paramref name="type"/> is an abstract class, an interface, 
        /// an open generic type or does not have a public no-argument constructor.
        /// </exception>
        public static object InstantiateType(Type type)
        {
            AssertUtils.ArgumentNotNull(type, "type");

            ConstructorInfo constructor = GetZeroArgConstructorInfo(type);
            return InstantiateType(constructor, EmptyObjects);
        }

        /// <summary>
        /// Gets the zero arg ConstructorInfo object, if the type offers such functionality.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Zero argument ConstructorInfo</returns>
        /// <exception cref="FatalReflectionException">
        /// If the type is an interface, abstract, open generic type, or does not have a zero-arg constructor.
        /// </exception>
        public static ConstructorInfo GetZeroArgConstructorInfo(Type type)
        {
            IsInstantiable(type);
            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                throw new FatalReflectionException(
                    string.Format(
                        CultureInfo.InvariantCulture, "Cannot instantiate a class that does not have a public no-argument constructor [{0}].", type));
            }
            return constructor;
        }

        /// <summary>
        /// Determines whether the specified type is instantiable, i.e. not an interface, abstract class or contains
        /// open generic type parameters.
        /// </summary>
        /// <param name="type">The type.</param>
        public static void IsInstantiable(Type type)
        {
            if (type.IsInterface)
            {
                throw new FatalReflectionException(
                    string.Format(
                        CultureInfo.InvariantCulture, "Cannot instantiate an interface [{0}].", type));
            }
            if (type.IsAbstract)
            {
                throw new FatalReflectionException(
                    string.Format(
                        CultureInfo.InvariantCulture, "Cannot instantiate an abstract class [{0}].", type));
            }
            if (type.ContainsGenericParameters)
            {
                throw new FatalReflectionException(
                    string.Format(
                        CultureInfo.InvariantCulture, "Cannot instantiate an open generic type [{0}].", type));
            }
        }

        /// <summary>
        /// Convenience method to instantiate a <see cref="System.Type"/> using
        /// the given constructor.
        /// </summary>
        /// <remarks>
        /// <p>
        /// As this method doesn't try to instantiate <see cref="System.Type"/>s
        /// by name, it should avoid <see cref="System.Type"/> loading issues.
        /// </p>
        /// </remarks>
        /// <param name="constructor">
        /// The constructor to use for the instantiation.
        /// </param>
        /// <param name="arguments">
        /// The arguments to be passed to the constructor.
        /// </param>
        /// <returns>A new instance.</returns>
        /// <exception cref="System.ArgumentNullException"> 
        /// If the <paramref name="constructor"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="Spring.Util.FatalReflectionException">
        /// If the <paramref name="constructor"/>'s declaring type is an abstract class, 
        /// an interface, an open generic type or does not have a public no-argument constructor.
        /// </exception>
        public static object InstantiateType(ConstructorInfo constructor, object[] arguments)
        {
            AssertUtils.ArgumentNotNull(constructor, "constructor");

            if (log.IsTraceEnabled) log.Trace(string.Format("instantiating type [{0}] using constructor [{1}]", constructor.DeclaringType, constructor));

            if (constructor.DeclaringType.IsInterface)
            {
                throw new FatalReflectionException(
                    string.Format(
                        CultureInfo.InvariantCulture, "Cannot instantiate an interface [{0}].", constructor.DeclaringType));
            }
            if (constructor.DeclaringType.IsAbstract)
            {
                throw new FatalReflectionException(
                    string.Format(
                        CultureInfo.InvariantCulture, "Cannot instantiate an abstract class [{0}].", constructor.DeclaringType));
            }
            if (constructor.DeclaringType.ContainsGenericParameters)
            {
                throw new FatalReflectionException(
                    string.Format(
                        CultureInfo.InvariantCulture, "Cannot instantiate an open generic type [{0}].", constructor.DeclaringType));
            }
            try
            {
                // replaced with SafeConstructor() to avoid nasty "TargetInvocationException"s in NET >= 2.0
                return (new SafeConstructor(constructor)).Invoke(arguments);
            }
            catch (Exception ex)
            {
                Type ctorType = constructor.DeclaringType;
                throw new FatalReflectionException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Cannot instantiate Type [{0}] using ctor [{1}] : '{2}'",
                        constructor.DeclaringType, constructor, ex.Message),
                    ex);
            }
        }

        /// <summary>
        /// Checks whether the supplied <paramref name="instance"/> is not a transparent proxy and is
        /// assignable to the supplied <paramref name="type"/>. 
        /// </summary>
        /// <remarks>
        /// <p>
        /// Neccessary when dealing with server-activated remote objects, because the
        /// object is of the type TransparentProxy and regular <c>is</c> testing for assignable
        /// types does not work.
        /// </p>
        /// <p>
        /// Transparent proxy instances always return <see langword="true"/> when tested
        /// with the <c>'is'</c> operator (C#). This method only checks if the object
        /// is assignable to the type if it is not a transparent proxy.
        /// </p>
        /// </remarks>
        /// <param name="type">The target <see cref="System.Type"/> to be checked.</param>
        /// <param name="instance">The value that should be assigned to the type.</param>
        /// <returns>
        /// <see langword="true"/> if the supplied <paramref name="instance"/> is not a
        /// transparent proxy and is assignable to the supplied <paramref name="type"/>.
        /// </returns>
        public static bool IsAssignableAndNotTransparentProxy(Type type, object instance)
        {
            if (!RemotingServices.IsTransparentProxy(instance))
            {
                return IsAssignable(type, instance);
            }
            return false;
        }

        /// <summary>
        /// Determine if the given <see cref="System.Type"/> is assignable from the
        /// given value, assuming setting by reflection and taking care of transparent proxies.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Considers primitive wrapper classes as assignable to the
        /// corresponding primitive types.
        /// </p>
        /// <p>
        /// For example used in an object factory's constructor resolution.
        /// </p>
        /// </remarks>
        /// <param name="type">The target <see cref="System.Type"/>.</param>
        /// <param name="obj">The value that should be assigned to the type.</param>
        /// <returns>True if the type is assignable from the value.</returns>
        public static bool IsAssignable(Type type, object obj)
        {
            AssertUtils.ArgumentNotNull(type, "type");
            if (!type.IsPrimitive && obj == null)
            {
                return true;
            }

#if !NETSTANDARD
            if (RemotingServices.IsTransparentProxy(obj))
            {
                RealProxy rp = RemotingServices.GetRealProxy(obj);
                if (rp is IRemotingTypeInfo)
                {
                    return ((IRemotingTypeInfo) rp).CanCastTo(type, obj);
                }
                else if (rp != null)
                {
                    type = rp.GetProxiedType();
                }

                if (type == null)
                {
                    // cannot decide
                    return false;
                }
            }
    #endif
            if (type.IsInstanceOfType(obj))
            {
                return true;
            }

            return type.IsPrimitive &&
                   type == typeof(bool) && obj is bool ||
                   type == typeof(byte) && obj is byte ||
                   type == typeof(char) && obj is char ||
                   type == typeof(sbyte) && obj is sbyte ||
                   type == typeof(int) && obj is int ||
                   type == typeof(short) && obj is short ||
                   type == typeof(long) && obj is long ||
                   type == typeof(float) && obj is float ||
                   type == typeof(double) && obj is double;
        }

        /// <summary>
        /// Check if the given <see cref="System.Type"/> represents a
        /// "simple" property,
        /// i.e. a primitive, a <see cref="System.String"/>, a
        /// <see cref="System.Type"/>, or a corresponding array.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Used to determine properties to check for a "simple" dependency-check.
        /// </p>
        /// </remarks>
        /// <param name="type">
        /// The <see cref="System.Type"/> to check.
        /// </param>
        public static bool IsSimpleProperty(Type type)
        {
            return type.IsPrimitive
                   || type.Equals(typeof(string))
                   || type.Equals(typeof(string[]))
                   || IsPrimitiveArray(type)
                   || type.Equals(typeof(Type))
                   || type.Equals(typeof(Type[]));
        }

        /// <summary>
        /// Check if the given class represents a primitive array,
        /// i.e. boolean, byte, char, short, int, long, float, or double.
        /// </summary>
        public static bool IsPrimitiveArray(Type type)
        {
            return typeof(bool[]).Equals(type)
                   || typeof(sbyte[]).Equals(type)
                   || typeof(char[]).Equals(type)
                   || typeof(short[]).Equals(type)
                   || typeof(int[]).Equals(type)
                   || typeof(long[]).Equals(type)
                   || typeof(float[]).Equals(type)
                   || typeof(double[]).Equals(type);
        }


        /// <summary>
        /// Determines whether the specified array is null or empty.
        /// </summary>
        /// <param name="array">The array to check.</param>
        /// <returns>
        /// 	<c>true</c> if the specified array is null empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmpty(object[] array)
        {
            return (array == null || array.Length == 0);
        }
        /// <summary>
        /// Determine if the given objects are equal, returning <see langword="true"/>
        /// if both are <see langword="null"/> respectively <see langword="false"/>
        /// if only one is <see langword="null"/>.
        /// </summary>
        /// <param name="o1">The first object to compare.</param>
        /// <param name="o2">The second object to compare.</param>
        /// <returns>
        /// <see langword="true"/> if the given objects are equal.
        /// </returns>
        public static bool NullSafeEquals(object o1, object o2)
        {
            return (o1 == o2 || (o1 != null && o1.Equals(o2)));
        }


        /// <summary>
	    /// Return as hash code for the given object; typically the value of
	    /// <code>{@link Object#hashCode()}</code>. If the object is an array,
	    /// this method will delegate to any of the <code>nullSafeHashCode</code>
	    /// methods for arrays in this class. If the object is <code>null</code>,
	    /// this method returns 0.
        /// </summary>
        public static int NullSafeHashCode(object o1)
        {
            return (o1 != null ? o1.GetHashCode() : 0);
        }

        /// <summary>
        /// Returns the first element in the supplied <paramref name="enumerator"/>.
        /// </summary>
        /// <param name="enumerator">
        /// The <see cref="System.Collections.IEnumerator"/> to use to enumerate
        /// elements.
        /// </param>
        /// <returns>
        /// The first element in the supplied <paramref name="enumerator"/>.
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// If the supplied <paramref name="enumerator"/> did not have any elements.
        /// </exception>
        public static object EnumerateFirstElement(IEnumerator enumerator)
        {
            return EnumerateElementAtIndex(enumerator, 0);
        }

        /// <summary>
        /// Returns the first element in the supplied <paramref name="enumerable"/>.
        /// </summary>
        /// <param name="enumerable">
        /// The <see cref="System.Collections.IEnumerable"/> to use to enumerate
        /// elements.
        /// </param>
        /// <returns>
        /// The first element in the supplied <paramref name="enumerable"/>.
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// If the supplied <paramref name="enumerable"/> did not have any elements.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="enumerable"/> is <see langword="null"/>.
        /// </exception>
        public static object EnumerateFirstElement(IEnumerable enumerable)
        {
            AssertUtils.ArgumentNotNull(enumerable, "enumerable");
            return EnumerateElementAtIndex(enumerable.GetEnumerator(), 0);
        }

        /// <summary>
        /// Returns the element at the specified index using the supplied
        /// <paramref name="enumerator"/>.
        /// </summary>
        /// <param name="enumerator">
        /// The <see cref="System.Collections.IEnumerator"/> to use to enumerate
        /// elements until the supplied <paramref name="index"/> is reached.
        /// </param>
        /// <param name="index">
        /// The index of the element in the enumeration to return.
        /// </param>
        /// <returns>
        /// The element at the specified index using the supplied
        /// <paramref name="enumerator"/>.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If the supplied <paramref name="index"/> was less than zero, or the
        /// supplied <paramref name="enumerator"/> did not contain enough elements
        /// to be able to reach the supplied <paramref name="index"/>.
        /// </exception>
        public static object EnumerateElementAtIndex(IEnumerator enumerator, int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            object element = null;
            int i = 0;
            while (enumerator.MoveNext())
            {
                element = enumerator.Current;
                if (++i > index)
                {
                    break;
                }
            }
            if (i < index)
            {
                throw new ArgumentOutOfRangeException();
            }
            return element;
        }

        /// <summary>
        /// Returns the element at the specified index using the supplied
        /// <paramref name="enumerable"/>.
        /// </summary>
        /// <param name="enumerable">
        /// The <see cref="System.Collections.IEnumerable"/> to use to enumerate
        /// elements until the supplied <paramref name="index"/> is reached.
        /// </param>
        /// <param name="index">
        /// The index of the element in the enumeration to return.
        /// </param>
        /// <returns>
        /// The element at the specified index using the supplied
        /// <paramref name="enumerable"/>.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If the supplied <paramref name="index"/> was less than zero, or the
        /// supplied <paramref name="enumerable"/> did not contain enough elements
        /// to be able to reach the supplied <paramref name="index"/>.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="enumerable"/> is <see langword="null"/>.
        /// </exception>
        public static object EnumerateElementAtIndex(IEnumerable enumerable, int index)
        {
            AssertUtils.ArgumentNotNull(enumerable, "enumerable");
            return EnumerateElementAtIndex(enumerable.GetEnumerator(), index);
        }

        /// <summary>
        /// Gets the qualified name of the given method, consisting of 
        /// fully qualified interface/class name + "." method name.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>qualified name of the method.</returns>
        public static string GetQualifiedMethodName(MethodInfo method)
        {
            AssertUtils.ArgumentNotNull(method, "method", "MethodInfo must not be null");
            return method.DeclaringType.FullName + "." + method.Name;
        }

        /// <summary>
        /// Return a String representation of an object's overall identity.
        /// </summary>
        /// <param name="obj">The object (may be <code>null</code>).</param>
        /// <returns>The object's identity as String representation,
        /// or an empty String if the object was <code>null</code>
        /// </returns>
        public static string IdentityToString(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            return obj.GetType().FullName + "@" + GetIdentityHexString(obj);
        }

        /// <summary>
        /// Gets a hex String form of an object's identity hash code.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The object's identity code in hex notation</returns>
        public static string GetIdentityHexString(object obj)
        {
            int hashcode = (int)GetHashCodeMethodInfo.Invoke(obj, null);
            return hashcode.ToString("X6");
        }
    }
}
