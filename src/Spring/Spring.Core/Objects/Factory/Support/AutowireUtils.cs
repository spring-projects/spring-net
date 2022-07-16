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

using System.Collections;
using System.Reflection;

using Spring.Collections;
using Spring.Core;
using Spring.Objects.Factory.Attributes;
using Spring.Objects.Factory.Config;
using Spring.Util;

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// Utility class that contains various methods useful for the implementation of
	/// autowire-capable object factories.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	public sealed class AutowireUtils
	{
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
		    var rootObjectDefinition = definition as RootObjectDefinition;
		    if (minimumArgumentCount == 0 && rootObjectDefinition?.defaultConstructor != null)
		    {
			    return rootObjectDefinition.defaultConstructor;
		    }

			const BindingFlags flags =
					  BindingFlags.Public
					  | BindingFlags.NonPublic
					  | BindingFlags.Instance
					  | BindingFlags.DeclaredOnly;

			ConstructorInfo[] constructors;
			if (minimumArgumentCount > 0)
			{
				MemberInfo[] ctors = definition.ObjectType.FindMembers(
					MemberTypes.Constructor,
					flags,
					new CriteriaMemberFilter().FilterMemberByCriteria,
					new MinimumArgumentCountCriteria(minimumArgumentCount));
				constructors = (ConstructorInfo[]) ArrayList.Adapter(ctors).ToArray(typeof (ConstructorInfo));
			}
			else
			{
				constructors = definition.ObjectType.GetConstructors(flags);
				if (rootObjectDefinition != null)
				{
					rootObjectDefinition.defaultConstructor = constructors;
				}
			}
			SortConstructors(constructors);
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
		public static int GetTypeDifferenceWeightOld(ParameterInfo[] argTypes, object[] args)
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
					return int.MaxValue;
				}
				if (args[i] != null && args[i].GetType() != theParameterType)
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
        /// Algorithm that judges the match between the declared parameter types of a candidate method
        /// and a specific list of arguments that this method is supposed to be invoked with.
        /// </summary>
        /// <remarks>
        /// Determines a weight that represents the class hierarchy difference between types and
        /// arguments.  The following a an example based on the Java class hierarchy for Integer.
        /// A direct match, i.e. type Integer -> arg of class Integer, does not increase
        /// the result - all direct matches means weight 0. A match between type Object and arg of
        /// class Integer would increase the weight by 2, due to the superclass 2 steps up in the
        /// hierarchy (i.e. Object) being the last one that still matches the required type Object.
        /// Type Number and class Integer would increase the weight by 1 accordingly, due to the
        /// superclass 1 step up the hierarchy (i.e. Number) still matching the required type Number.
        /// Therefore, with an arg of type Integer, a constructor (Integer) would be preferred to a
        /// constructor (Number) which would in turn be preferred to a constructor (Object).
        /// All argument weights get accumulated.
        /// </remarks>
        /// <param name="paramTypes">The param types.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public static int GetTypeDifferenceWeight(Type[] paramTypes, object[] args)
        {
            int result = 0;
            for (int i = 0; i < (uint) paramTypes.Length; i++)
            {
                if (!ObjectUtils.IsAssignable(paramTypes[i], args[i]))
                {
                    return int.MaxValue;
                }
                if (args[i] != null)
                {
                    Type paramType = paramTypes[i];
                    Type superType = args[i].GetType().BaseType;
                    while (superType != null)
                    {
                        if (paramType == superType)
                        {
                            result = result + 2;
                            superType = null;
                        }
                        if (paramType.IsAssignableFrom(superType))
                        {
                            result = result + 2;
                            superType = superType.BaseType;
                        }
                        else
                        {
                            superType = null;
                        }
                    }
                    if (paramType.IsInterface)
                    {
                        result = result + 1;
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
            return pi.GetSetMethod() != null;
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
			if (constructors != null && constructors.Length > 1)
			{
				Array.Sort(constructors, ConstructorComparer.Instance);
			}
		}

	    private sealed class ConstructorComparer : IComparer
	    {
		    internal static readonly ConstructorComparer Instance = new ConstructorComparer();

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

	    private sealed class MinimumArgumentCountCriteria : ICriteria
		{
			private readonly int _minimumArgumentCount;

			public MinimumArgumentCountCriteria(int minimumArgumentCount)
			{
				_minimumArgumentCount = minimumArgumentCount;
			}

			public bool IsSatisfied(object datum)
			{
				return ((MethodBase) datum).GetParameters().Length >= _minimumArgumentCount;
			}
		}

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

        /// <summary>
        /// Creates the autowire candidate resolver.
        /// </summary>
        /// <returns>A SimpleAutowireCandidateResolver</returns>
	    public static IAutowireCandidateResolver CreateAutowireCandidateResolver()
	    {
            return new QualifierAnnotationAutowireCandidateResolver();
	    }

        /// <summary>
        /// Returns the list of <paramref name="propertyInfos"/> that are not satisfied by <paramref name="properties"/>.
        /// </summary>
        /// <returns>the filtered list. Is never <c>null</c></returns>
        public static IList<PropertyInfo> GetUnsatisfiedDependencies(IList<PropertyInfo> propertyInfos, IPropertyValues properties, DependencyCheckingMode dependencyCheck)
        {
            List<PropertyInfo> unsatisfiedDependenciesList = new List<PropertyInfo>();
            foreach (PropertyInfo property in propertyInfos)
            {
                if (property.CanWrite && properties.GetPropertyValue(property.Name) == null)
                {
                    bool isSimple = ObjectUtils.IsSimpleProperty(property.PropertyType);
                    bool unsatisfied = (dependencyCheck == DependencyCheckingMode.All) || (isSimple && dependencyCheck == DependencyCheckingMode.Simple)
                                       || (!isSimple && dependencyCheck == DependencyCheckingMode.Objects);
                    if (unsatisfied)
                    {
                        unsatisfiedDependenciesList.Add(property);
                    }
                }
            }
            return unsatisfiedDependenciesList;
        }
	}
}
