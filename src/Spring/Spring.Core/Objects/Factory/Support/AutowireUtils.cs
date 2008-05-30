#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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

using System;
using System.Collections;
using System.Reflection;
using Spring.Collections;
using Spring.Core;
using Spring.Objects.Factory.Config;
using Spring.Objects.Support;
using Spring.Util;

#endregion

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// Utility class that contains various methods useful for the implementation of
	/// autowire-capable object factories.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	/// <version>$Id: AutowireUtils.cs,v 1.7 2008/04/02 18:02:24 markpollack Exp $</version>
	public sealed class AutowireUtils
	{
		#region Constructor (s) / Destructor

		// CLOVER:OFF

		/// <summary>
		/// Creates a new instance of the AutowireUtils class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is a utility class, and as such has no publicly
		/// visible constructors.
		/// </p>
		/// </remarks>
		private AutowireUtils()
		{
		}

		// CLOVER:ON

		#endregion

		/// <summary>
		/// Gets those <see cref="System.Reflection.ConstructorInfo"/>s
		/// that are applicable for autowiring the supplied <paramref name="definition"/>.
		/// </summary>
		/// <param name="definition">
		/// The <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/>
		/// (definition) that is being autowired by constructor.
		/// </param>
		/// <param name="minimumArgumentCount">
		/// The absolute minimum number of arguments that any returned constructor
		/// must have. If this parameter is equal to zero (0), then all constructors
		/// are valid (regardless of their argument count), including any default
		/// constructor.
		/// </param>
		/// <returns>
		/// Those <see cref="System.Reflection.ConstructorInfo"/>s
		/// that are applicable for autowiring the supplied <paramref name="definition"/>.
		/// </returns>
		public static ConstructorInfo[] GetConstructors(
			IObjectDefinition definition, int minimumArgumentCount)
		{
			const BindingFlags flags =
					  BindingFlags.Public | BindingFlags.NonPublic
					  | BindingFlags.Instance | BindingFlags.DeclaredOnly;
			ConstructorInfo[] constructors = null;
			if (minimumArgumentCount > 0)
			{
				MemberInfo[] ctors = definition.ObjectType.FindMembers(
					MemberTypes.Constructor,
					flags,
					new MemberFilter(new CriteriaMemberFilter().FilterMemberByCriteria),
					new MinimumArgumentCountCriteria(minimumArgumentCount));
				constructors = (ConstructorInfo[]) ArrayList.Adapter(ctors).ToArray(typeof (ConstructorInfo));
			}
			else
			{
				constructors = definition.ObjectType.GetConstructors(flags);
			}
			AutowireUtils.SortConstructors(constructors);
			return constructors;
		}

		/// <summary>
		/// Determine a weight that represents the class hierarchy difference between types and
		/// arguments.
		/// </summary>
		/// <remarks>
		/// <p>
		/// A direct match, i.e. type MyInteger -> arg of class MyInteger, does not increase
		/// the result - all direct matches means weight zero (0). A match between the argument type
		/// <see cref="System.Object"/> and a MyInteger instance argument would increase the weight by
		/// 1, due to the superclass (<see cref="System.Object"/>) being one (1) steps up in the
		/// class hierarchy being the last one that still matches the required type.
		/// </p>
		/// <p>
		/// Therefore, with an argument of type <see cref="System.Collections.Hashtable"/>, a
		/// constructor taking a <see cref="System.Collections.Hashtable"/> argument would be
		/// preferred to a constructor taking an <see cref="System.Collections.IDictionary"/> argument
		/// which would be preferred to a constructor taking an
		/// <see cref="System.Collections.ICollection"/> argument which would in turn be preferred
		/// to a constructor taking an <see cref="System.Object"/> argument.
		/// </p>
		/// <p>
		/// All argument weights get accumulated.
		/// </p>
		/// </remarks>
		/// <param name="argTypes">
		/// The argument <see cref="System.Type"/>s to match.
		/// </param>
		/// <param name="args">The arguments to match.</param>
		/// <returns>The accumulated weight for all arguments.</returns>
		public static int GetTypeDifferenceWeight(ParameterInfo[] argTypes, object[] args)
		{
			if (argTypes.Length != args.Length)
			{
				throw new ArgumentException("Cannot calculate the type difference weight for argument types and arguments with differing lengths.");
			}
			int result = 0;
			for (int i = 0; i < argTypes.Length; i++)
			{
				Type theParameterType = argTypes[i].ParameterType;
				if (!ObjectUtils.IsAssignable(theParameterType, args[i]))
				{
					return Int32.MaxValue;
				}
				if (args[i] != null
					&& !(args[i].GetType().Equals(theParameterType)))
				{
					Type superType = args[i].GetType().BaseType;
					while (superType != null)
					{
						if (theParameterType.IsAssignableFrom(superType))
						{
							++result;
							superType = superType.BaseType;
						}
						else
						{
							superType = null;
						}
					}
				}
			}
			return result;
		}

        /// <summary>
        /// Determines whether the given object property is excluded from dependency checks.
        /// </summary>
        /// <param name="pi">The PropertyInfo of the object property.</param>
        /// <returns>
        /// 	<c>true</c> if is excluded from dependency check; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean IsExcludedFromDependencyCheck(PropertyInfo pi)
        {
            return (pi.GetSetMethod() == null) ? false : true;
        }

		/// <summary>
		/// Sorts the supplied <paramref name="constructors"/>, preferring
		/// public constructors and "greedy" ones (that have lots of arguments).
		/// </summary>
		/// <remarks>
		/// <p>
		/// The result will contain public constructors first, with a decreasing number
		/// of arguments, then non-public constructors, again with a decreasing number
		/// of arguments.
		/// </p>
		/// </remarks>
		/// <param name="constructors">
		/// The <see cref="System.Reflection.ConstructorInfo"/> array to be sorted.
		/// </param>
		public static void SortConstructors(ConstructorInfo[] constructors)
		{
			if (constructors != null
				&& constructors.Length > 0)
			{
				Array.Sort(constructors, new ConstructorComparer());
			}
		}

		#region Inner Class : ConstructorComparer

		private sealed class ConstructorComparer : IComparer
		{
			public int Compare(object lhs, object rhs)
			{
				ConstructorInfo lhsCtor = (ConstructorInfo) lhs;
				ConstructorInfo rhsCtor = (ConstructorInfo) rhs;
				if (lhsCtor.IsPublic != rhsCtor.IsPublic)
				{
					return (lhsCtor.IsPublic ? -1 : 1);
				}
				int lhsParams = lhsCtor.GetParameters().Length;
				int rhsParams = rhsCtor.GetParameters().Length;

				if (lhsParams < rhsParams)
				{
					return 1;
				}
				else if (lhsParams > rhsParams)
				{
					return -1;
				}
				else
				{
					return 0;
				}
			}
		}

		#endregion

		#region Inner Class : MinimumArgumentCountCriteria

		private sealed class MinimumArgumentCountCriteria : ICriteria
		{
			public MinimumArgumentCountCriteria(int minimumArgumentCount)
			{
				_minimumArgumentCount = minimumArgumentCount;
			}

			public bool IsSatisfied(object datum)
			{
				bool satisfied = false;
				satisfied = ((MethodBase) datum).GetParameters().Length >= _minimumArgumentCount;
				return satisfied;
			}

			private int _minimumArgumentCount;
		}

		#endregion

        /// <summary>
        /// Determines whether the setter property is defined in any of the given interfaces.
        /// </summary>
        /// <param name="propertyInfo">The PropertyInfo of the object property</param>
        /// <param name="interfaces">The ISet of interfaces.</param>
        /// <returns>
        /// 	<c>true</c> if setter property is defined in interface; otherwise, <c>false</c>.
        /// </returns>
	    public static bool IsSetterDefinedInInterface(PropertyInfo propertyInfo, ISet interfaces)
	    {
	        MethodInfo setter = propertyInfo.GetSetMethod();
            if (setter != null)
            {
                Type targetType = setter.DeclaringType;
                foreach (Type interfaceType in interfaces)
                {
                    if (interfaceType.IsAssignableFrom(targetType) &&
                        ReflectionUtils.GetMethod(interfaceType, setter.Name, ReflectionUtils.GetParameterTypes(setter)) != null)                    
                    {
                        return true;
                    }
                }
            }
            return false;
	    }
	}
}