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
using System.Security.Permissions;
using System.Text;
using System.Runtime.CompilerServices;

namespace Spring.Util
{
    /// <summary>
    /// Various reflection related methods that are missing from the standard library.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Aleksandar Seovic (.NET)</author>
    /// <author>Stan Dvoychenko (.NET)</author>
    /// <author>Bruno Baia (.NET)</author>
    public sealed class ReflectionUtils
    {
        internal static Type[] EmptyTypes = new Type[0];

        /// <summary>
        /// Convenience <see cref="System.Reflection.BindingFlags"/> value that will
        /// match all private and public, static and instance members on a class
        /// in a case inSenSItivE fashion.
        /// </summary>
        public const BindingFlags AllMembersCaseInsensitiveFlags = BindingFlags.Public |
                                                                   BindingFlags.NonPublic | BindingFlags.Instance
                                                                   | BindingFlags.Static
                                                                   | BindingFlags.IgnoreCase;

        /// <summary>
        /// Avoid BeforeFieldInit problem
        /// </summary>
        static ReflectionUtils()
        { }

        /// <summary>
        /// Checks, if the specified type is a nullable
        /// </summary>
        public static bool IsNullableType(Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        /// <summary>
        /// Returns signature for the specified <see cref="System.Type"/>, method name and argument
        /// <see cref="System.Type"/>s.
        /// </summary>
        /// <param name="type">The <see cref="System.Type"/> the method is in.</param>
        /// <param name="method">The method name.</param>
        /// <param name="argumentTypes">
        /// The argument <see cref="System.Type"/>s.
        /// </param>
        /// <returns>The method signature.</returns>
        public static string GetSignature(
            Type type, string method, Type[] argumentTypes)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(type.FullName).Append("::").Append(method).Append("(");
            string separator = "";
            for (int i = 0; i < argumentTypes.Length; i++)
            {
                sb.Append(separator).Append(argumentTypes[i].FullName);
                separator = ",";
            }
            sb.Append(")");
            return sb.ToString();
        }


        /// <summary>
        /// Returns method for the specified <see cref="System.Type"/>, method
        /// name and argument
        /// <see cref="System.Type"/>s.
        /// </summary>
        /// <remarks>
        /// <para>Searches with BindingFlags</para>
        /// <para>When dealing with interface methods, you probable want to 'normalize' method references by calling
        /// <see cref="MapInterfaceMethodToImplementationIfNecessary"/>.
        /// </para>
        /// </remarks>
        /// <param name="targetType">
        /// The target <see cref="System.Type"/> to find the method on.
        /// </param>
        /// <param name="method">The method to find.</param>
        /// <param name="argumentTypes">
        /// The argument <see cref="System.Type"/>s. May be
        /// <see langword="null"/> if the method has no arguments.
        /// </param>
        /// <returns>The target method.</returns>
        /// <seealso cref="MapInterfaceMethodToImplementationIfNecessary"/>
        public static MethodInfo GetMethod(
                    Type targetType, string method, Type[] argumentTypes)
        {
            return GetMethod(targetType, method, argumentTypes, 0);
        }

        /// <summary>
        /// Returns method for the specified <see cref="System.Type"/>, method
        /// name and argument
        /// <see cref="System.Type"/>s.
        /// </summary>
        /// <remarks>
        /// <para>Searches with BindingFlags</para>
        /// <para>When dealing with interface methods, you probable want to 'normalize' method references by calling
        /// <see cref="MapInterfaceMethodToImplementationIfNecessary"/>.
        /// </para>
        /// </remarks>
        /// <param name="targetType">
        /// The target <see cref="System.Type"/> to find the method on.
        /// </param>
        /// <param name="method">The method to find.</param>
        /// <param name="argumentTypes">
        /// The argument <see cref="System.Type"/>s. May be
        /// <see langword="null"/> if the method has no arguments.
        /// </param>
        /// <param name="genericArgumentsCount">Number of Generic Arguments in the method</param>
        /// <returns>The target method.</returns>
        /// <seealso cref="MapInterfaceMethodToImplementationIfNecessary"/>
        public static MethodInfo GetMethod(
            Type targetType, string method, Type[] argumentTypes, int genericArgumentsCount)
        {
            AssertUtils.ArgumentNotNull(targetType, "Type must not be null");

            MethodInfo retMethod = null;

            MethodInfo[] methods = targetType.GetMethods(ReflectionUtils.AllMembersCaseInsensitiveFlags);

            foreach (MethodInfo candidate in methods)
            {
                if (candidate.Name.ToLower() == method.ToLower())
                {
                    Type[] parameterTypes = Array.ConvertAll<ParameterInfo, Type>(candidate.GetParameters(), delegate(ParameterInfo i) { return i.ParameterType; });
                    bool typesMatch = false;

                    bool zeroTypeArguments = null == argumentTypes || argumentTypes.Length == 0;

                    if (!zeroTypeArguments && parameterTypes.Length == argumentTypes.Length)
                    {
                        for (int i = 0; i < parameterTypes.Length; i++)
                        {
                            typesMatch = parameterTypes[i] == argumentTypes[i];
                            if (!typesMatch)
                            {
                                break;
                            }
                        }
                    }

                    if (typesMatch || zeroTypeArguments)
                    {
                        if (candidate.GetGenericArguments().Length == genericArgumentsCount)
                        {
                            retMethod = candidate;
                            break;
                        }
                    }
                }
            }

            /*return (from method in type.GetMethods()
                    where method.Name == name
                    where parameterTypes.SequenceEqual(method.GetParameters().Select(p => p.ParameterType))
                    where method.GetGenericArguments().Count() == genericArguments
                    select method).Single();*/


            if (retMethod == null)
            {
                // try explicit interface implementation...
                int idx = method.LastIndexOf('.');
                if (idx > -1)
                {
                    method = method.Substring(idx + 1);
                    retMethod = ReflectionUtils.GetMethod(targetType, method, argumentTypes);
                }
            }
            return retMethod;
        }

        /// <summary>
        /// Resolves a given <paramref name="methodInfo"/> to the <see cref="MethodInfo"/> representing the actual implementation.
        /// </summary>
        /// <remarks>
        /// see article <a href="http://weblog.ikvm.net/CommentView.aspx?guid=7356a87f-e5d7-4723-ae49-b263ab9e40ae">How To Get an Explicit Interface Implementation Method</a>.
        /// </remarks>
        /// <param name="methodInfo">a <see cref="MethodInfo"/></param>
        /// <param name="implementingType">the type to lookup</param>
        /// <returns>the <see cref="MethodInfo"/> representing the actual implementation method of the specified <paramref name="methodInfo"/></returns>
        public static MethodInfo MapInterfaceMethodToImplementationIfNecessary(MethodInfo methodInfo, System.Type implementingType)
        {
            AssertUtils.ArgumentNotNull(methodInfo, "methodInfo");
            AssertUtils.ArgumentNotNull(implementingType, "implementingType");
            AssertUtils.IsTrue(methodInfo.DeclaringType.IsAssignableFrom(implementingType), "methodInfo and implementingType are unrelated");

            MethodInfo concreteMethodInfo = methodInfo;

            if (methodInfo.DeclaringType.IsInterface)
            {
                InterfaceMapping interfaceMapping = implementingType.GetInterfaceMap(methodInfo.DeclaringType);
                int methodIndex = Array.IndexOf(interfaceMapping.InterfaceMethods, methodInfo);
                concreteMethodInfo = interfaceMapping.TargetMethods[methodIndex];
            }

            return concreteMethodInfo;
        }

        /// <summary>
        /// Returns an array of parameter <see cref="System.Type"/>s for the specified method
        /// or constructor.
        /// </summary>
        /// <param name="method">The method (or constructor).</param>
        /// <returns>An array containing the parameter <see cref="System.Type"/>s.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="method"/> is <see langword="null"/>.
        /// </exception>
        public static Type[] GetParameterTypes(MethodBase method)
        {
            AssertUtils.ArgumentNotNull(method, "method");
            return GetParameterTypes(method.GetParameters());
        }

        /// <summary>
        /// Returns an array of parameter <see cref="System.Type"/>s for the
        /// specified parameter info array.
        /// </summary>
        /// <param name="args">The parameter info array.</param>
        /// <returns>An array containing parameter <see cref="System.Type"/>s.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="args"/> is <see langword="null"/> or any of the
        /// elements <paramref name="args"/> is <see langword="null"/>.
        /// </exception>
        public static Type[] GetParameterTypes(ParameterInfo[] args)
        {
            AssertUtils.ArgumentNotNull(args, "args");

            if (args.Length == 0)
            {
                return EmptyTypes;
            }

            var types = new Type[args.Length];
            for (int i = 0; i < (uint) args.Length; i++)
            {
                types[i] = args[i].ParameterType;
            }
            return types;
        }

        /// <summary>
        /// Returns an array of <see langword="string"/>s that represent
        /// the names of the generic type parameter.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>An array containing the parameter names.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="method"/> is <see langword="null"/>.
        /// </exception>
        public static string[] GetGenericParameterNames(MethodInfo method)
        {
            AssertUtils.ArgumentNotNull(method, "method");
            return GetGenericParameterNames(method.GetGenericArguments());
        }

        /// <summary>
        /// Returns an array of <see langword="string"/>s that represent
        /// the names of the generic type parameter.
        /// </summary>
        /// <param name="args">The parameter info array.</param>
        /// <returns>An array containing parameter names.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="args"/> is <see langword="null"/> or any of the
        /// elements <paramref name="args"/> is <see langword="null"/>.
        /// </exception>
        public static string[] GetGenericParameterNames(Type[] args)
        {
            AssertUtils.ArgumentNotNull(args, "args");
            string[] names = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                names[i] = args[i].Name;
            }
            return names;
        }

        public static MethodInfo GetGenericMethod(Type type, string methodName, Type[] typeArguments, Type[] parameterTypes)
        {
            MethodInfo methodInfo = null;

            if (typeArguments == null)
            {
                // Non-Generic Method
                methodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameterTypes, null);
            }
            else
            {
                // Generic Method
                MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                // Loop thru all Methods
                foreach (MethodInfo method in methods)
                {
                    if (method.Name != methodName)
                    {
                        // Name does not match
                        continue;
                    }

                    if (!method.IsGenericMethod)
                    {
                        // Non-Generic
                        continue;
                    }

                    // Compare the Method Parameters
                    bool paramsOk = false;
                    if (method.GetParameters().Length == parameterTypes.Length)
                    {
                        // Count Matches
                        paramsOk = true;
                        // Check each Type
                        for (int i = 0; i < method.GetParameters().Length; i++)
                        {
                            if (method.GetParameters()[i].ParameterType != parameterTypes[i])
                            {
                                // Parameter Type doesn't Match
                                paramsOk = false;
                                break;
                            }
                        }
                    }
                    if (!paramsOk)
                    {
                        // Parameters didn't match
                        continue;
                    }

                    // Check the Generic Arguments
                    bool argsOk = false;
                    if (method.GetGenericArguments().Length == typeArguments.Length)
                    {
                        // Count Matches
                        argsOk = true;
                        // TODO: Check for "where" Limitation in the Generic Definition
                    }
                    if (!argsOk)
                    {
                        // Generic Arguments didn't match
                        continue;
                    }

                    // If we're here, we got the right Method
                    methodInfo = method.MakeGenericMethod(typeArguments);
                    break;
                }
            }
            return methodInfo;
        }

        /// <summary>
        /// From a given list of methods, selects the method having an exact match on the given <paramref name="argValues"/>' types.
        /// </summary>
        /// <param name="methods">the list of methods to choose from</param>
        /// <param name="argValues">the arguments to the method</param>
        /// <returns>the method matching exactly the passed <paramref name="argValues"/>' types</returns>
        /// <exception cref="AmbiguousMatchException">
        /// If more than 1 matching methods are found in the <paramref name="methods"/> list.
        /// </exception>
        public static MethodInfo GetMethodByArgumentValues<T>(IEnumerable<T> methods, object[] argValues) where T : MethodBase
        {
            return (MethodInfo)GetMethodBaseByArgumentValues("method", methods, argValues);
        }

        /// <summary>
        /// From a given list of methods, selects the method having an exact match on the given <paramref name="argValues"/>' types.
        /// </summary>
        /// <param name="methodTypeName">the type of method (used for exception reporting only)</param>
        /// <param name="methods">the list of methods to choose from</param>
        /// <param name="argValues">the arguments to the method</param>
        /// <returns>the method matching exactly the passed <paramref name="argValues"/>' types</returns>
        /// <exception cref="AmbiguousMatchException">
        /// If more than 1 matching methods are found in the <paramref name="methods"/> list.
        /// </exception>
        private static MethodBase GetMethodBaseByArgumentValues<T>(string methodTypeName, IEnumerable<T> methods, object[] argValues) where T : MethodBase
        {
            MethodBase match = null;
            int matchCount = 0;

            foreach (MethodBase m in methods)
            {
                ParameterInfo[] parameters = m.GetParameters();
                bool isMatch = true;
                bool isExactMatch = true;
                object[] paramValues = (argValues == null) ? new object[0] : argValues;

                try
                {
                    if (parameters.Length > 0)
                    {
                        ParameterInfo lastParameter = parameters[parameters.Length - 1];
                        if (lastParameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0 && argValues.Length >= parameters.Length)
                        {
                            paramValues =
                                PackageParamArray(argValues, parameters.Length,
                                                  lastParameter.ParameterType.GetElementType());
                        }
                    }

                    if (parameters.Length != paramValues.Length)
                    {
                        isMatch = false;
                    }
                    else
                    {
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            Type paramType = parameters[i].ParameterType;
                            object paramValue = paramValues[i];

                            if ((paramValue == null && paramType.IsValueType && !IsNullableType(paramType))
                                || (paramValue != null && !paramType.IsAssignableFrom(paramValue.GetType())))
                            {
                                isMatch = false;
                                break;
                            }

                            if (paramValue == null || paramType != paramValue.GetType())
                            {
                                isExactMatch = false;
                            }
                        }
                    }
                }
                catch (InvalidCastException)
                {
                    isMatch = false;
                }

                if (isMatch)
                {
                    if (isExactMatch)
                    {
                        return m;
                    }

                    matchCount++;
                    if (matchCount == 1)
                    {
                        match = m;
                    }
                    else
                    {
                        throw new AmbiguousMatchException(
                            string.Format("Ambiguous match for {0} '{1}' for the specified number and types of arguments.", methodTypeName,
                                          m.Name));
                    }
                }
            }

            return match;
        }

        /// <summary>
        /// From a given list of constructors, selects the constructor having an exact match on the given <paramref name="argValues"/>' types.
        /// </summary>
        /// <param name="methods">the list of constructors to choose from</param>
        /// <param name="argValues">the arguments to the method</param>
        /// <returns>the constructor matching exactly the passed <paramref name="argValues"/>' types</returns>
        /// <exception cref="AmbiguousMatchException">
        /// If more than 1 matching methods are found in the <paramref name="methods"/> list.
        /// </exception>
        public static ConstructorInfo GetConstructorByArgumentValues<T>(IList<T> methods, object[] argValues) where T : MethodBase
        {
            return (ConstructorInfo)GetMethodBaseByArgumentValues("constructor", methods, argValues);
        }


        /// <summary>
        /// Packages arguments into argument list containing parameter array as a last argument.
        /// </summary>
        /// <param name="argValues">Argument vaklues to package.</param>
        /// <param name="argCount">Total number of oarameters.</param>
        /// <param name="elementType">Type of the param array element.</param>
        /// <returns>Packaged arguments.</returns>
        public static object[] PackageParamArray(object[] argValues, int argCount, Type elementType)
        {
            object[] values = new object[argCount];
            int i = 0;

            // copy regular arguments
            while (i < argCount - 1)
            {
                values[i] = argValues[i];
                i++;
            }

            // package param array into last argument
            Array paramArray = Array.CreateInstance(elementType, argValues.Length - i);
            int j = 0;
            while (i < argValues.Length)
            {
                paramArray.SetValue(argValues[i++], j++);
            }
            values[values.Length - 1] = paramArray;

            return values;
        }

        /// <summary>
        /// Convenience method to convert an interface <see cref="System.Type"/>
        /// to a <see cref="System.Type"/> array that contains
        /// all the interfaces inherited and the specified interface.
        /// </summary>
        /// <param name="intf">The interface to convert.</param>
        /// <returns>An array of interface <see cref="System.Type"/>s.</returns>
        /// <exception cref="System.ArgumentException">
        /// If the <see cref="System.Type"/> specified is not an interface.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="intf"/> is <see langword="null"/>.
        /// </exception>
        public static IList<Type> ToInterfaceArray(Type intf)
        {
            AssertUtils.ArgumentNotNull(intf, "intf");

            if (!intf.IsInterface)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture,
                                  "[{0}] is a class.",
                                  intf.FullName));
            }

            List<Type> interfaces = new List<Type>(intf.GetInterfaces());
            interfaces.Add(intf);

            return interfaces;
        }

        /// <summary>
        /// Is the supplied <paramref name="propertyName"/> the default indexer for the
        /// supplied <paramref name="type"/>?
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property on the supplied <paramref name="type"/> to be checked.
        /// </param>
        /// <param name="type">
        /// The <see cref="System.Type"/>  to be checked.
        /// </param>
        /// <returns>
        /// <see lang="true"/> if the supplied <paramref name="propertyName"/> is the
        /// default indexer for the supplied <paramref name="type"/>.
        /// </returns>
        /// <exception cref="System.NullReferenceException">
        /// If the supplied <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        public static bool PropertyIsIndexer(string propertyName, Type type)
        {
            DefaultMemberAttribute[] attribs =
                (DefaultMemberAttribute[])type.GetCustomAttributes(typeof(DefaultMemberAttribute), true);
            if (attribs.Length != 0)
            {
                foreach (DefaultMemberAttribute attrib in attribs)
                {
                    if (attrib.MemberName.Equals(propertyName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Is the supplied <paramref name="method"/> declared on one of these interfaces?
        /// </summary>
        /// <param name="method">The method to check.</param>
        /// <param name="interfaces">The array of interfaces we want to check.</param>
        /// <returns>
        /// <see lang="true"/> if the method is declared on one of these interfaces.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// If any of the <see cref="System.Type"/>s specified is not an interface.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="method"/> or any of the specified interfaces is
        /// <see langword="null"/>.
        /// </exception>
        public static bool MethodIsOnOneOfTheseInterfaces(MethodBase method, Type[] interfaces)
        {
            AssertUtils.ArgumentNotNull(method, "method");
            if (interfaces == null)
            {
                return false;
            }
            Type[] paramTypes = GetParameterTypes(method.GetParameters());
            for (int i = 0; i < interfaces.Length; i++)
            {
                Type interfaceType = interfaces[i];
                AssertUtils.ArgumentNotNull(interfaceType, StringUtils.Surround("interfaces[", i, "]"));
                if (!interfaceType.IsInterface)
                {
                    throw new ArgumentException(interfaces[i].FullName + " is not an interface");
                }
                try
                {
                    MethodInfo mi = interfaceType.GetMethod(
                        method.Name,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly,
                        null, paramTypes, null);
                    if (mi != null)
                    {
                        // found it...
                        return true;
                    }
                }
                catch
                {
                    // didn't find it, so keep going...
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the default value for the specified <see cref="System.Type"/>
        /// </summary>
        /// <remarks>
        /// <p>
        /// Follows the standard .NET conventions for default values where
        /// relevant; for example, all numeric types default to the value
        /// <c>0</c>.
        /// </p>
        /// </remarks>
        /// <param name="type">
        /// The <see cref="System.Type"/> to return default value for.
        /// </param>
        /// <returns>
        /// The default value for the specified <see cref="System.Type"/>.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// If the supplied <paramref name="type"/> is an enumerated type that
        /// has no values.
        /// </exception>
        public static object GetDefaultValue(Type type)
        {
            if (!type.IsValueType)
            {
                return null;
            }
            if (type == typeof(Boolean))
            {
                return false;
            }
            if (type == typeof(DateTime))
            {
                return DateTime.MinValue;
            }
            if (type == typeof(Char))
            {
                return Char.MinValue;
            }
            if (type.IsEnum)
            {
                Array values = Enum.GetValues(type);
                if (values == null || values.Length == 0)
                {
                    throw new ArgumentException("Bad 'enum' Type : cannot get default value because 'enum' has no values.");
                }
                return values.GetValue(0);
            }
            return 0;
        }

        /// <summary>
        /// Returns an array consisting of the default values for the supplied
        /// <paramref name="types"/>.
        /// </summary>
        /// <param name="types">
        /// The array of <see cref="System.Type"/>s to return default values for.
        /// </param>
        /// <returns>
        /// An array consisting of the default values for the supplied
        /// <paramref name="types"/>.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// If any of the elements in the supplied <paramref name="types"/>
        /// array is an enumerated type that has no values.
        /// </exception>
        /// <seealso cref="Spring.Util.ReflectionUtils.GetDefaultValue(Type)"/>
        public static object[] GetDefaultValues(Type[] types)
        {
            object[] defaults = new object[types.Length];
            for (int i = 0; i < types.Length; ++i)
            {
                defaults[i] = GetDefaultValue(types[i]);
            }
            return defaults;
        }

        /// <summary>
        /// Checks that the parameter <see cref="System.Type"/>s of the
        /// supplied <paramref name="candidate"/> match the parameter
        /// <see cref="System.Type"/>s of the supplied
        /// <paramref name="parameterTypes"/>.
        /// </summary>
        /// <param name="candidate">The method to be checked.</param>
        /// <param name="parameterTypes">
        /// The array of parameter <see cref="System.Type"/>s to check against.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the parameter <see cref="System.Type"/>s
        /// match.
        /// </returns>
        public static bool ParameterTypesMatch(
            MethodInfo candidate, Type[] parameterTypes)
        {
            #region Sanity Checks

            AssertUtils.ArgumentNotNull(candidate, "candidate");
            AssertUtils.ArgumentNotNull(parameterTypes, "parameterTypes");

            #endregion

            Type[] candidatesParameterTypes
                = ReflectionUtils.GetParameterTypes(candidate);
            if (candidatesParameterTypes.Length != parameterTypes.Length)
            {
                return false;
            }
            for (int i = 0; i < candidatesParameterTypes.Length; ++i)
            {
                if (!candidatesParameterTypes[i].Equals(parameterTypes[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns an array containing the <see cref="System.Type"/>s of the
        /// objects in the supplied array.
        /// </summary>
        /// <param name="args">
        /// The objects array for which the corresponding <see cref="System.Type"/>s
        /// are needed.
        /// </param>
        /// <returns>
        /// An array containing the <see cref="System.Type"/>s of the objects
        /// in the supplied array; this array will be empty (but not
        /// <see langword="null"/> if the supplied <paramref name="args"/>
        /// is null or has no elements.
        /// </returns>
        /// <example>
        /// <p>
        /// [C#]<br/>
        /// Given an array containing the following objects,
        /// <code>[83, "Foo", new object ()]</code>, the <see cref="System.Type"/>
        /// array returned from this method call would consist of the following
        /// <see cref="System.Type"/> elements...
        /// <code>[Int32, String, Object]</code>.
        /// </p>
        /// </example>
        public static Type[] GetTypes(object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return Type.EmptyTypes;
            }
            Type[] paramsType = new Type[args.Length];
            for (int i = 0; i < args.Length; ++i)
            {
                object arg = args[i];
                paramsType[i] = (arg != null) ? args[i].GetType() : typeof(object);
            }
            return paramsType;
        }


        /// <summary>
        /// Given the <see cref="System.Type"/> return its representation as
        /// it would appear in the source code files.
        /// </summary>
        /// <remarks>
        /// Largely intended to handle generic types where .ToString() will typically return:
        /// "System.Collections.Generic.List`1[System.Collections.Generic.Dictionary`2[System.String,System.Int32]]"
        /// and this method will instead return:
        /// "System.Collections.Generic.List&lt;System.Collections.Generic.Dictionary&lt;string,int&gt;&gt;"
        /// </remarks>
        /// <param name="type">The type.</param>
        /// <returns>Friendly string representing the Type</returns>
        public static string GetTypeFriendlyName(Type type)
        {
#if MONO
            //no csharp services in mono (verify this!) so have to fall back to returning the ToString() for now
            //TODO: investigate whether there is another equivalent manner of providing this functionality under MONO
            return type.ToString();
#endif
            return (new Microsoft.CSharp.CSharpCodeProvider()).GetTypeOutput(new System.CodeDom.CodeTypeReference(type));
        }


        /// <summary>
        /// Does the given <see cref="System.Type"/> and/or it's superclasses
        /// have at least one or more methods with the given name (with any
        /// argument types)?
        /// </summary>
        /// <remarks>
        /// <p>
        /// Includes non-public methods in the methods searched.
        /// </p>
        /// </remarks>
        /// <param name="type">
        /// The <see cref="System.Type"/> to be checked.
        /// </param>
        /// <param name="name">
        /// The name of the method to be searched for. Case inSenSItivE.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the given <see cref="System.Type"/> or / and it's
        /// superclasses have at least one or more methods (with any argument types);
        /// <see langword="false"/> if not, or either of the parameters is <see langword="null"/>.
        /// </returns>
        public static bool HasAtLeastOneMethodWithName(Type type, string name)
        {
            if (type == null || StringUtils.IsNullOrEmpty(name))
            {
                return false;
            }
            return MethodCountForName(type, name) > 0;
        }

        /// <summary>
        ///  Within <paramref name="type"/>, counts the number of overloads for the method with the given (case-insensitive!) <paramref name="name"/>
        /// </summary>
        /// <param name="type">The type to be searched</param>
        /// <param name="name">the name of the method for which overloads shall be counted</param>
        /// <returns>The number of overloads for method <paramref name="name"/> within type <paramref name="type"/></returns>
        public static int MethodCountForName(Type type, string name)
        {
            AssertUtils.ArgumentNotNull(type, "type", "Type must not be null");
            AssertUtils.ArgumentNotNull(name, "name", "Method name must not be null");
            MemberInfo[] methods = type.FindMembers(
                MemberTypes.Method,
                ReflectionUtils.AllMembersCaseInsensitiveFlags,
                new MemberFilter(ReflectionUtils.MethodNameFilter),
                name);
            return methods.Length;
        }

        private static bool MethodNameFilter(MemberInfo member, object criteria)
        {
            MethodInfo method = member as MethodInfo;
            string name = criteria as string;
            return String.Compare(method.Name, name, true, CultureInfo.InvariantCulture) == 0;
        }

        /// <summary>
        /// Creates a <see cref="System.Reflection.Emit.CustomAttributeBuilder"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Note that if a non-<see langword="null"/> <paramref name="sourceAttribute"/>
        /// is supplied, any read write properties exposed by the <paramref name="sourceAttribute"/>
        /// will be used to overwrite values that may have been passed in via the
        /// <paramref name="ctorArgs"/>. That is, the <paramref name="ctorArgs"/> will be used
        /// to initialize the custom attribute, and then any read-write properties on the
        /// <paramref name="sourceAttribute"/> will be plugged in.
        /// </p>
        /// </remarks>
        /// <param name="type">
        /// The desired <see cref="System.Attribute"/> <see cref="System.Type"/>.
        /// </param>
        /// <param name="ctorArgs">
        /// Any constructor arguments for the attribute (may be <see langword="null"/>
        /// in the case of no arguments).
        /// </param>
        /// <param name="sourceAttribute">
        /// Source attribute to copy properties from (may be <see langword="null"/>).
        /// </param>
        /// <returns>A custom attribute builder.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="type"/> parameter is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="type"/> parameter is not a <see cref="System.Type"/>
        /// that derives from the <see cref="System.Attribute"/> class.
        /// </exception>
        /// <seealso cref="System.Reflection.Emit.CustomAttributeBuilder"/>
        public static CustomAttributeBuilder CreateCustomAttribute(
            Type type, object[] ctorArgs, Attribute sourceAttribute)
        {
            #region Sanity Checks

            AssertUtils.ArgumentNotNull(type, "type");
            if (!typeof(Attribute).IsAssignableFrom(type))
            {
                throw new ArgumentException(
                    string.Format("[{0}] does not derive from the [System.Attribute] class.",
                                  type.FullName));
            }

            #endregion

            ConstructorInfo ci = type.GetConstructor(ReflectionUtils.GetTypes(ctorArgs));
            if (ci == null && ctorArgs.Length == 0)
            {
                ci = type.GetConstructors()[0];
                ctorArgs = GetDefaultValues(GetParameterTypes(ci.GetParameters()));
            }

            if (sourceAttribute != null)
            {
                object defaultAttribute = null;
                try
                {
                    defaultAttribute = ci.Invoke(ctorArgs);
                }
                catch
                {
                }

                IList<PropertyInfo> getSetProps = new List<PropertyInfo>();
                IList getSetValues = new ArrayList();
                IList<PropertyInfo> readOnlyProps = new List<PropertyInfo>();
                IList readOnlyValues = new ArrayList();
                foreach (PropertyInfo pi in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (pi.DeclaringType == typeof(Attribute))
                        continue;

                    if (pi.CanRead)
                    {
                        if (pi.CanWrite)
                        {
                            object propValue = pi.GetValue(sourceAttribute, null);
                            if (defaultAttribute != null)
                            {
                                object defaultValue = pi.GetValue(defaultAttribute, null);
                                if ((propValue == null && defaultValue == null) ||
                                    (propValue != null && propValue.Equals(defaultValue)))
                                    continue;
                            }
                            getSetProps.Add(pi);
                            getSetValues.Add(propValue);
                        }
                        else
                        {
                            readOnlyProps.Add(pi);
                            readOnlyValues.Add(pi.GetValue(sourceAttribute, null));
                        }
                    }
                }

                if (readOnlyProps.Count == 1)
                {
                    PropertyInfo pi = readOnlyProps[0];
                    ConstructorInfo ciTemp = type.GetConstructor(new Type[1] { pi.PropertyType });
                    if (ciTemp != null)
                    {
                        ci = ciTemp;
                        ctorArgs = new object[1] { readOnlyValues[0] };
                    }
                    else
                    {
                        ciTemp = type.GetConstructor(new Type[1] { readOnlyValues[0].GetType() });
                        if (ciTemp != null)
                        {
                            ci = ciTemp;
                            ctorArgs = new object[1] { readOnlyValues[0] };
                        }
                    }
                }

                PropertyInfo[] propertyInfos = new PropertyInfo[getSetProps.Count];
                getSetProps.CopyTo(propertyInfos, 0);

                object[] propertyValues = new object[getSetValues.Count];
                getSetValues.CopyTo(propertyValues, 0);

                return new CustomAttributeBuilder(ci, ctorArgs, propertyInfos, propertyValues);
            }
            else
            {
                return new CustomAttributeBuilder(ci, ctorArgs);
            }
        }

        /// <summary>
        /// Creates a <see cref="System.Reflection.Emit.CustomAttributeBuilder"/>.
        /// </summary>
        /// <param name="type">
        /// The desired <see cref="System.Attribute"/> <see cref="System.Type"/>.
        /// </param>
        /// <param name="sourceAttribute">
        /// Source attribute to copy properties from (may be <see langword="null"/>).
        /// </param>
        /// <returns>A custom attribute builder.</returns>
        public static CustomAttributeBuilder CreateCustomAttribute(
            Type type, Attribute sourceAttribute)
        {
            return CreateCustomAttribute(type, new object[] { }, sourceAttribute);
        }

        /// <summary>
        /// Creates a <see cref="System.Reflection.Emit.CustomAttributeBuilder"/>.
        /// </summary>
        /// <param name="sourceAttribute">
        /// The source attribute to copy properties from.
        /// </param>
        /// <returns>A custom attribute builder.</returns>
        /// <exception cref="System.NullReferenceException">
        /// If the supplied <paramref name="sourceAttribute"/> is
        /// <see langword="null"/>.
        /// </exception>
        public static CustomAttributeBuilder CreateCustomAttribute(Attribute sourceAttribute)
        {
            return CreateCustomAttribute(sourceAttribute.GetType(), sourceAttribute);
        }

        /// <summary>
        /// Creates a <see cref="System.Reflection.Emit.CustomAttributeBuilder"/>.
        /// </summary>
        /// <param name="type">
        /// The desired <see cref="System.Attribute"/> <see cref="System.Type"/>.
        /// </param>
        /// <returns>A custom attribute builder.</returns>
        public static CustomAttributeBuilder CreateCustomAttribute(Type type)
        {
            return CreateCustomAttribute(type, new object[] { }, null);
        }

        /// <summary>
        /// Creates a <see cref="System.Reflection.Emit.CustomAttributeBuilder"/>.
        /// </summary>
        /// <param name="type">
        /// The desired <see cref="System.Attribute"/> <see cref="System.Type"/>.
        /// </param>
        /// <param name="ctorArgs">
        /// Any constructor arguments for the attribute (may be <see langword="null"/>
        /// in the case of no arguments).
        /// </param>
        /// <returns>A custom attribute builder.</returns>
        public static CustomAttributeBuilder CreateCustomAttribute(
            Type type, params object[] ctorArgs)
        {
            return CreateCustomAttribute(type, ctorArgs, null);
        }

        /// <summary>
        /// Creates a <see cref="System.Reflection.Emit.CustomAttributeBuilder"/>.
        /// </summary>
        /// <param name="attributeData">
        /// The <see cref="System.Reflection.CustomAttributeData"/> to create
        /// the custom attribute builder from.
        /// </param>
        /// <returns>A custom attribute builder.</returns>
        public static CustomAttributeBuilder CreateCustomAttribute(CustomAttributeData attributeData)
        {
            object[] parameterValues = new object[attributeData.ConstructorArguments.Count];
            Type[] parameterTypes = new Type[attributeData.ConstructorArguments.Count];

            IList namedParameterValues = new ArrayList();
            IList namedFieldValues = new ArrayList();

            // Fill arrays of the constructor parameters
            for (int i = 0; i < attributeData.ConstructorArguments.Count; i++)
            {
                parameterTypes[i] = attributeData.ConstructorArguments[i].ArgumentType;
                parameterValues[i] = ConvertConstructorArgsToObjectArrayIfNecessary(attributeData.ConstructorArguments[i].Value);
            }

            Type attributeType = attributeData.Constructor.DeclaringType;
            PropertyInfo[] attributeProperties = attributeType.GetProperties(
                BindingFlags.Instance | BindingFlags.Public);
            FieldInfo[] attributeFields = attributeType.GetFields(
                BindingFlags.Instance | BindingFlags.Public);

            // Not using generics bellow as probably Spring.NET tries to keep
            // it on .NET1 compatibility level right now I believe (SD)
            // In case of using List<CustomAttributesData> the above note makes
            // no sense (SD:)
            IList propertiesToSet = new ArrayList();

            IList fieldsToSet = new ArrayList();

            // Fills arrays of the constructor named parameters
            foreach (CustomAttributeNamedArgument namedArgument in attributeData.NamedArguments)
            {
                bool noMatchingProperty = false;

                // Now iterate through all of the PropertyInfo, find the
                // one with the corresponding to the NamedProperty name
                // and add it to the array of properties to set.
                for (int j = 0; j < attributeProperties.Length; j++)
                {
                    if (attributeProperties[j].Name == namedArgument.MemberInfo.Name)
                    {
                        propertiesToSet.Add(attributeProperties[j]);
                        namedParameterValues.Add(ConvertConstructorArgsToObjectArrayIfNecessary(namedArgument.TypedValue.Value));
                        break;
                    }
                    else
                    {
                        if (j == attributeProperties.Length - 1)
                        {
                            // In case of no match, throw
                            noMatchingProperty = true;
                            /*
                            throw new InvalidOperationException(
                                String.Format(CultureInfo.InvariantCulture,
                                "The property with name {0} can't be found in the " +
                                "type {1}, but is present as a named property " +
                                "on the attributeData {2}", namedArgument.MemberInfo.Name,
                                attributeType.FullName, attributeData));
                            */
                        }
                    }
                }
                if (noMatchingProperty)
                {
                    for (int j = 0; j < attributeFields.Length; j++)
                    {
                        if (attributeFields[j].Name == namedArgument.MemberInfo.Name)
                        {
                            fieldsToSet.Add(attributeFields[j]);
                            namedFieldValues.Add(ConvertConstructorArgsToObjectArrayIfNecessary(namedArgument.TypedValue.Value));
                            break;
                        }
                        else
                        {
                            if (j == attributeFields.Length - 1)
                            {
                                throw new InvalidOperationException(
                                        String.Format(CultureInfo.InvariantCulture,
                                        "A property or public field with name {0} can't be found in the " +
                                        "type {1}, but is present as a named property " +
                                        "on the attributeData {2}", namedArgument.MemberInfo.Name,
                                        attributeType.FullName, attributeData));
                            }
                        }
                    }
                }
            }
            // Get constructor corresponding to the parameters and their types
            ConstructorInfo constructor = attributeType.GetConstructor(parameterTypes);

            PropertyInfo[] namedProperties = new PropertyInfo[propertiesToSet.Count];
            propertiesToSet.CopyTo(namedProperties, 0);

            object[] propertyValues = new object[namedParameterValues.Count];
            namedParameterValues.CopyTo(propertyValues, 0);

            if (fieldsToSet.Count == 0)
            {
                return new CustomAttributeBuilder(
                        constructor, parameterValues, namedProperties, propertyValues);
            }
            else
            {
                FieldInfo[] namedFields = new FieldInfo[fieldsToSet.Count];
                fieldsToSet.CopyTo(namedFields, 0);

                object[] fieldValues = new object[namedFieldValues.Count];
                namedFieldValues.CopyTo(fieldValues, 0);

                return new CustomAttributeBuilder(
                        constructor, parameterValues, namedProperties, propertyValues, namedFields, fieldValues);
            }



        }

        private static object ConvertConstructorArgsToObjectArrayIfNecessary(object value)
        {
            if (value == null)
                return value;

            IList<CustomAttributeTypedArgument> constructorArguments = value as IList<CustomAttributeTypedArgument>;

            if (constructorArguments == null)
                return value;

            object[] arguments = new object[constructorArguments.Count];

            for (int i = 0; i < constructorArguments.Count; i++)
            {
                arguments[i] = constructorArguments[i].Value;
            }

            return arguments;
        }

        /// <summary>
        /// Calculates and returns the list of attributes that apply to the
        /// specified type or method.
        /// </summary>
        /// <param name="member">The type or method to find attributes for.</param>
        /// <returns>
        /// A list of custom attributes (CustomAttributeData or Attribute instances)
        /// that should be applied to type or method.
        /// </returns>
        public static IList GetCustomAttributes(MemberInfo member)
        {
            ArrayList attributes = new ArrayList();

            // add attributes that apply to the target type or method
            object[] attrs = member.GetCustomAttributes(false);
            try
            {
                IList<CustomAttributeData> attrsData = CustomAttributeData.GetCustomAttributes(member);

                if (attrs.Length != attrsData.Count)
                {
                    // http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=94803
                    attributes.AddRange(attrs);
                }
                else if (attrsData.Count > 0)
                {
                    bool hasSecurityAttribute = false;
                    foreach (CustomAttributeData cad in attrsData)
                    {
                        if (typeof(SecurityAttribute).IsAssignableFrom(cad.Constructor.DeclaringType))
                        {
                            hasSecurityAttribute = true;
                            break;
                        }
                    }

                    if (hasSecurityAttribute)
                    {
                        attributes.AddRange(attrs);
                    }
                    else
                    {
                        foreach (CustomAttributeData cad in attrsData)
                        {
                            attributes.Add(cad);
                        }
                    }
                }
            }
            catch (ArgumentException)
            {
                // http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=296032
                attributes.AddRange(attrs);
            }
            catch (CustomAttributeFormatException)
            {
                // http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=161522
                attributes.AddRange(attrs);
            }
            return attributes;
        }

        /// <summary>
        /// Tries to find matching methods in the specified <see cref="System.Type"/>
        /// for each method in the supplied <paramref name="methods"/> list.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> to look for matching methods in.
        /// </param>
        /// <param name="methods">The methods to match.</param>
        /// <param name="strict">
        /// A flag that specifies whether to throw an exception if a matching
        /// method is not found.
        /// </param>
        /// <returns>A list of the matched methods.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// If either of the <paramref name="type"/> or
        /// <paramref name="methods"/> parameters are <see langword="null"/>.
        /// </exception>
        public static MethodInfo[] GetMatchingMethods(Type type, MethodInfo[] methods, bool strict)
        {
            AssertUtils.ArgumentNotNull(type, "type");
            AssertUtils.ArgumentNotNull(methods, "methods");

            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

            MethodInfo[] matched = new MethodInfo[methods.Length];
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                MethodInfo match = type.GetMethod(method.Name, flags, null, ReflectionUtils.GetParameterTypes(method), null);
                if ((match == null || match.ReturnType != method.ReturnType) && strict)
                {
                    throw new Exception(
                        string.Format("Method '{0}' could not be matched in the target class [{1}].",
                                      method.Name, type.FullName));
                }
                matched[i] = match;
            }
            return matched;
        }

        /// <summary>
        /// Returns the <see cref="System.Type"/> of the supplied
        /// <paramref name="source"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the <paramref name="source"/> is a <see cref="System.Type"/>
        /// instance, the return value of this method call with be the
        /// <paramref name="source"/> parameter cast to a
        /// <see cref="System.Type"/>. If the <paramref name="source"/> is
        /// anything other than a <see cref="System.Type"/>, the return value
        /// will be the result of invoking the <paramref name="source"/>'s
        /// <see cref="System.Object.GetType()"/> method.
        /// </p>
        /// </remarks>
        /// <param name="source">
        /// A <see cref="Type"/> or <see cref="object"/> instance.
        /// </param>
        /// <returns>
        /// The <paramref name="source"/>argument if it is a
        /// <see cref="Type"/> or the result of invoking
        /// <see cref="Object.GetType"/> on the argument if it
        /// is an <see cref="Object"/>.
        /// </returns>
        /// <exception cref="System.NullReferenceException">
        /// If the <paramref name="source"/> is <see langword="null"/>.
        /// </exception>
        public static Type TypeOfOrType(object source)
        {
            return source is Type ? source as Type : source.GetType();
        }


        private static readonly MethodInfo Exception_InternalPreserveStackTrace =
            typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Unwraps the supplied <see cref="System.Reflection.TargetInvocationException"/>
        /// and returns the inner exception preserving the stack trace.
        /// </summary>
        /// <param name="ex">
        /// The <see cref="System.Reflection.TargetInvocationException"/> to unwrap.
        /// </param>
        /// <returns>The unwrapped exception.</returns>
        public static Exception UnwrapTargetInvocationException(TargetInvocationException ex)
        {
            if (SystemUtils.MonoRuntime)
            {
                return ex.InnerException;
            }
            Exception_InternalPreserveStackTrace.Invoke(ex.InnerException, new Object[] { });
            return ex.InnerException;
        }

        /// <summary>
        /// Is the supplied <paramref name="type"/> can be accessed outside the assembly ?
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        /// <see langword="true"/> if the type can be accessed outside the assembly;
        /// Otherwise <see langword="false"/>.
        /// </returns>
        public static bool IsTypeVisible(Type type)
        {
            return IsTypeVisible(type, null);
        }

        /// <summary>
        /// Determines whether the specified type is nullable.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if the specified type is ullable]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTypeNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Is the supplied <paramref name="type"/> can be accessed
        /// from the supplied friendly assembly ?
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="friendlyAssemblyName">The friendly assembly name.</param>
        /// <returns>
        /// <see langword="true"/> if the type can be accessed
        /// from the supplied friendly assembly; Otherwise <see langword="false"/>.
        /// </returns>
        public static bool IsTypeVisible(Type type, string friendlyAssemblyName)
        {
            if (type.IsVisible)
            {
                return true;
            }
            else
            {
                if (friendlyAssemblyName != null
                    && friendlyAssemblyName.Length > 0
                    && (!type.IsNested || type.IsNestedPublic ||
                     (!type.IsNestedPrivate && (type.IsNestedAssembly || type.IsNestedFamORAssem))))
                {
                    object[] attrs = type.Assembly.GetCustomAttributes(typeof(InternalsVisibleToAttribute), false);
                    foreach (InternalsVisibleToAttribute ivta in attrs)
                    {
                        if (ivta.AssemblyName.Split(',')[0] == friendlyAssemblyName)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets all of the interfaces implemented by
        /// the specified <see cref="System.Type"/>.
        /// </summary>
        /// <param name="type">
        /// The object to get the interfaces of.
        /// </param>
        /// <returns>
        /// All of the interfaces implemented by the
        /// <see cref="System.Type"/>.
        /// </returns>
        public static Type[] GetInterfaces(Type type)
        {
            AssertUtils.ArgumentNotNull(type, "type");

            if (type.IsInterface)
            {
                List<Type> interfaces = new List<Type>();
                interfaces.Add(type);
                interfaces.AddRange(type.GetInterfaces());
                return interfaces.ToArray();
            }
            else
            {
                return type.GetInterfaces();
            }
        }

        /// <summary>
        /// Returns the explicit <see cref="System.Exception"/> that is the root cause of an exception.
        /// </summary>
        /// <remarks>
        /// If the InnerException property of the current exception is a null reference
        /// or a <see cref="System.NullReferenceException"/>, returns the current exception.
        /// </remarks>
        /// <param name="ex">The last exception thrown.</param>
        /// <returns>
        /// The first explicit exception thrown in a chain of exceptions.
        /// </returns>
        public static Exception GetExplicitBaseException(Exception ex)
        {
            Exception innerEx = ex.InnerException;
            while (innerEx != null &&
                !(innerEx is NullReferenceException))
            {
                ex = innerEx;
                innerEx = innerEx.InnerException;
            }
            return ex;
        }

        /// <summary>
        /// Copies all fields from one object to another.
        /// </summary>
        /// <remarks>
        /// The types of both objects must be related. This means, that either of the following is true:
        /// <list type="bullet">
        ///		<item><description>fromObject.GetType() == toObject.GetType()</description></item>
        ///		<item><description>fromObject.GetType() is derived from toObject.GetType()</description></item>
        ///		<item><description>toObject.GetType() is derived from fromObject.GetType()</description></item>
        /// </list>
        /// </remarks>
        /// <param name="fromObject">The source object</param>
        /// <param name="toObject">The object, who's fields will be populated with values from the source object</param>
        /// <exception cref="ArgumentException">If the object's types are not related</exception>
        public static void MemberwiseCopy(object fromObject, object toObject)
        {
            Type fromType = fromObject.GetType();
            Type toType = toObject.GetType();

            Type smallerType;

            if (fromType.IsAssignableFrom(toType))
            {
                smallerType = fromType;
            }
            else if (toType.IsAssignableFrom(fromType))
            {
                smallerType = toType;
            }
            else
            {
                throw new ArgumentException("object types are not related");
            }

            MemberwiseCopyInternal(fromObject, toObject, smallerType);
        }


        /// <summary>
        /// Convenience method that uses reflection to return the value of a non-public field of a given object.
        /// </summary>
        /// <remarks>Useful in certain instances during testing to avoid the need to add protected properties, etc. to a class just to facilitate testing.</remarks>
        /// <param name="obj">The instance of the object from which to retrieve the field value.</param>
        /// <param name="fieldName">Name of the field on the object from which to retrieve the value.</param>
        /// <returns></returns>
        public static object GetInstanceFieldValue(object obj, string fieldName)
        {
            if (obj == null)
                throw new ArgumentNullException("obj", "obj is null.");
            if (StringUtils.IsNullOrEmpty(fieldName))
                throw new ArgumentException("fieldName is null or empty.", "fieldName");

            FieldInfo f = obj.GetType().GetField(fieldName, BindingFlags.SetField | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (f != null)
                return f.GetValue(obj);
            else
            {
                throw new ArgumentException(string.Format("Non-public instance field '{0}' could not be found in class of type '{1}'", fieldName, obj.GetType().ToString()));
            }
        }

        /// <summary>
        /// Convenience method that uses reflection to set the value of a non-public field of a given object.
        /// </summary>
        /// <remarks>Useful in certain instances during testing to avoid the need to add protected properties, etc. to a class just to facilitate testing.</remarks>
        /// <param name="obj">The instance of the object from which to set the field value.</param>
        /// <param name="fieldName">Name of the field on the object to which to set the value.</param>
        /// <param name="fieldValue">The field value to set.</param>
        public static void SetInstanceFieldValue(object obj, string fieldName, object fieldValue)
        {

            if (obj == null)
                throw new ArgumentNullException("obj", "obj is null.");
            if (StringUtils.IsNullOrEmpty(fieldName))
                throw new ArgumentException("fieldName is null or empty.", "fieldName");
            if (fieldValue == null)
                throw new ArgumentNullException("fieldValue", "fieldValue is null.");

            FieldInfo f = obj.GetType().GetField(fieldName, BindingFlags.SetField | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (f != null)
            {
                if (f.FieldType != fieldValue.GetType())
                    throw new ArgumentException(string.Format("fieldValue for fieldName '{0}' of object type '{1}' must be of type '{2}' but was of type '{3}'", fieldName, obj.GetType().ToString(), f.FieldType.ToString(), fieldValue.GetType().ToString()), "fieldValue");

                f.SetValue(obj, fieldValue);
            }
            else
            {
                throw new ArgumentException(string.Format("Non-public instance field '{0}' could not be found in class of type '{1}'", fieldName, obj.GetType().ToString()));
            }
        }

        private static void MemberwiseCopyInternal(object fromObject, object toObject, Type smallerType)
        {
            MemberwiseCopyHandler impl = GetImpl(smallerType);
            impl(fromObject, toObject);
        }

        private delegate void MemberwiseCopyHandler(object a, object b);

        private static readonly Dictionary<Type, MemberwiseCopyHandler> s_handlerCache = new Dictionary<Type, MemberwiseCopyHandler>();

        private static MemberwiseCopyHandler GetImpl(Type type)
        {
            MemberwiseCopyHandler handler;
            if (s_handlerCache.TryGetValue(type, out handler))
            {
                return handler;
            }

            lock (s_handlerCache)
            {
                if (s_handlerCache.TryGetValue(type, out handler))
                {
                    return handler;
                }

                FieldInfo[] fields = GetFields(type);
                Action callback = () =>
                {
                    DynamicMethod dm = new DynamicMethod(type.FullName + ".ShallowCopy", null, new Type[] {typeof(object), typeof(object)}, type.Module, true);
                    ILGenerator ilGen = dm.GetILGenerator();
                    ilGen.DeclareLocal(type);
                    ilGen.DeclareLocal(type);
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Castclass, type);
                    ilGen.Emit(OpCodes.Stloc_0);
                    ilGen.Emit(OpCodes.Ldarg_1);
                    ilGen.Emit(OpCodes.Castclass, type);
                    ilGen.Emit(OpCodes.Stloc_1);

                    foreach (FieldInfo field in fields)
                    {
                        ilGen.Emit(OpCodes.Ldloc_1);
                        ilGen.Emit(OpCodes.Ldloc_0);
                        ilGen.Emit(OpCodes.Ldfld, field);
                        ilGen.Emit(OpCodes.Stfld, field);
                    }

                    ilGen.Emit(OpCodes.Ret);

                    handler = (MemberwiseCopyHandler) dm.CreateDelegate(typeof(MemberwiseCopyHandler));
                };

#if NETSTANDARD
                callback();
#else
                SecurityCritical.ExecutePrivileged(new System.Security.PermissionSet(PermissionState.Unrestricted), callback);
#endif

                s_handlerCache[type] = handler;
            }
            return handler;
        }


        #region Field Cache Management for "MemberwiseCopy"

        private const BindingFlags FIELDBINDINGS =
            BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Dictionary<Type, FieldInfo[]> s_fieldCache = new Dictionary<Type, FieldInfo[]>();

        private static FieldInfo[] GetFields(Type type)
        {
            lock (s_fieldCache)
            {
                FieldInfo[] fields;
                if (!s_fieldCache.TryGetValue(type, out fields))
                {
                    List<FieldInfo> fieldList = new List<FieldInfo>();
                    CollectFieldsRecursive(type, fieldList);
                    fields = fieldList.ToArray();
                    s_fieldCache[type] = fields;
                }
                return fields;
            }
        }

        private static void CollectFieldsRecursive(Type type, List<FieldInfo> fieldList)
        {
            if (type == typeof(object))
                return;

            FieldInfo[] fields = type.GetFields(FIELDBINDINGS);
            fieldList.AddRange(fields);
            CollectFieldsRecursive(type.BaseType, fieldList);
        }

        #endregion Field Cache Management for "MemberwiseCopy"


        #region CustomAttributeBuilderBuilder inner class definition

        /// <summary>
        /// Creates a <see cref=" CustomAttributeBuilder"/>.
        /// </summary>
        /// <author>Bruno Baia</author>
        public class CustomAttributeBuilderBuilder
        {
            #region Fields

            private Type type;
            private ArrayList constructorArgs;
            private List<PropertyInfo> namedProperties;
            private List<object> propertyValues;

            #endregion

            #region Constructor(s) / Destructor

            /// <summary>
            /// Creates a new instance of the
            /// <see cref="CustomAttributeBuilderBuilder"/> class.
            /// </summary>
            /// <param name="attributeType">The custom attribute type.</param>
            public CustomAttributeBuilderBuilder(Type attributeType)
                :
                this(attributeType, ObjectUtils.EmptyObjects)
            {
            }

            /// <summary>
            /// Creates a new instance of the
            /// <see cref="CustomAttributeBuilderBuilder"/> class.
            /// </summary>
            /// <param name="attributeType">The custom attribute type.</param>
            /// <param name="constructorArgs">The custom attribute constructor arguments.</param>
            public CustomAttributeBuilderBuilder(Type attributeType, params object[] constructorArgs)
            {
                AssertUtils.ArgumentNotNull(attributeType, "attributeType");
                if (!typeof(Attribute).IsAssignableFrom(attributeType))
                {
                    throw new ArgumentException(
                        string.Format("[{0}] does not derive from the [System.Attribute] class.",
                                      attributeType.FullName));
                }
                this.type = attributeType;
                this.constructorArgs = new ArrayList(constructorArgs);
                this.namedProperties = new List<PropertyInfo>();
                this.propertyValues = new List<object>();
            }

            #endregion

            #region Public Methods

            /// <summary>
            /// Adds the specified values to the constructor argument list
            /// used to create the custom attribute.
            /// </summary>
            /// <param name="values">An array of argument values.</param>
            public void AddContructorArgument(params object[] values)
            {
                this.constructorArgs.AddRange(values);
            }

            /// <summary>
            /// Adds a property value to the custom attribute.
            /// </summary>
            /// <param name="name">The property name.</param>
            /// <param name="value">The property value.</param>
            public void AddPropertyValue(string name, object value)
            {
                PropertyInfo propertyInfo = this.type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
                if (propertyInfo == null)
                {
                    throw new ArgumentException(
                        String.Format("The property '{0}' does no exist in the attribute '{1}'.", name, this.type));
                }

                this.namedProperties.Add(propertyInfo);
                this.propertyValues.Add(value);
            }

            /// <summary>
            /// Creates the <see cref="CustomAttributeBuilderBuilder"/>.
            /// </summary>
            /// <returns>The created <see cref="CustomAttributeBuilderBuilder"/>.</returns>
            public CustomAttributeBuilder Build()
            {
                object[] caArray = (object[])this.constructorArgs.ToArray(typeof(object));
                ConstructorInfo ci = this.type.GetConstructor(ReflectionUtils.GetTypes(caArray));
                if (ci == null && caArray.Length == 0)
                {
                    ci = this.type.GetConstructors()[0];
                    caArray = ReflectionUtils.GetDefaultValues(ReflectionUtils.GetParameterTypes(ci.GetParameters()));
                }

                if (namedProperties.Count > 0)
                {
                    PropertyInfo[] npArray = this.namedProperties.ToArray();
                    object[] pvArray = this.propertyValues.ToArray();
                    return new CustomAttributeBuilder(ci, caArray, npArray, pvArray);
                }
                else
                {
                    return new CustomAttributeBuilder(ci, caArray);
                }

            }

            #endregion
        }

        #endregion
    }
}
