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

using Spring.Util;

#endregion

namespace Spring.Core
{
    /// <summary>
    /// Criteria that is satisfied if the <see cref="System.Type"/> of each of the
    /// arguments matches each of the parameter <see cref="System.Type"/>s of a given
    /// <see cref="System.Reflection.MethodInfo"/>.
    /// </summary>
    /// <remarks>
    /// <p>
    /// If no <see cref="System.Type"/> array is passed to the overloaded constructor,
    /// any method that has no parameters will satisfy an instance of this
    /// class. The same effect could be achieved by passing the
    /// <see cref="Spring.Util.ObjectUtils.EmptyObjects"/> array to the overloaded constructor.
    /// </p>
    /// </remarks>
    /// <author>Rick Evans</author>
    /// <author>Bruno Baia</author>
    public class MethodArgumentsCriteria : ICriteria
    {
        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="MethodArgumentsCriteria"/> class.
        /// </summary>
        public MethodArgumentsCriteria() : this(ObjectUtils.EmptyObjects)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="MethodArgumentsCriteria"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the supplied <paramref name="arguments"/> array is null, then this
        /// constructor uses the <see cref="System.Type.EmptyTypes"/> array.
        /// </p>
        /// </remarks>
        /// <param name="arguments">
        /// The <see cref="System.Object"/> array that this criteria will use to
        /// check parameter <see cref="System.Type"/>s.
        /// </param>
        public MethodArgumentsCriteria(object[] arguments)
        {
            _parameters = ReflectionUtils.GetTypes(arguments);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Does the supplied <paramref name="datum"/> satisfy the criteria encapsulated by
        /// this instance?
        /// </summary>
        /// <remarks>
        /// <p>
        /// This implementation respects the inheritance chain of any parameter
        /// <see cref="System.Type"/>s... i.e. methods that have a base type (or
        /// interface) that is assignable to the <see cref="System.Type"/> in the
        /// same corresponding index of the parameter types will satisfy this
        /// criteria instance.
        /// </p>
        /// </remarks>
        /// <param name="datum">The datum to be checked by this criteria instance.</param>
        /// <returns>
        /// True if the supplied <paramref name="datum"/> satisfies the criteria encapsulated
        /// by this instance; false if not or the supplied <paramref name="datum"/> is null.
        /// </returns>
        public bool IsSatisfied(object datum)
        {
            bool satisfied = false;
            MethodInfo method = datum as MethodInfo;
            if (method != null)
            {
                bool isParamArray = false;
                Type paramArrayType = null;
                ParameterInfo[] parametersBeingChecked = method.GetParameters();
                if (parametersBeingChecked.Length > 0)
                {
                    ParameterInfo lastParameter = parametersBeingChecked[parametersBeingChecked.Length - 1];
                    isParamArray = lastParameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
                    if (isParamArray)
                    {
                        paramArrayType = lastParameter.ParameterType.GetElementType();
                    }
                }
                if (parametersBeingChecked != null
                    && parametersBeingChecked.Length == _parameters.Length)
                {
                    satisfied = true;
                    for (int i = 0; i < _parameters.Length; ++i)
                    {
                        Type sourceType = _parameters[i];
                        Type typeBeingChecked = parametersBeingChecked[i].ParameterType;
                        if (!typeBeingChecked.IsAssignableFrom(sourceType))
                        {
                            if (isParamArray && i == _parameters.Length - 1)
                            {
                                if (!paramArrayType.IsAssignableFrom(sourceType))
                                {
                                    satisfied = false;
                                    break;
                                }
                            }
                            else
                            {
                                satisfied = false;
                                break;
                            }
                        }
                    }
                }
                else if (isParamArray && (_parameters.Length >= parametersBeingChecked.Length - 1))
                {
                    satisfied = true;
                    for (int i = 0; i < parametersBeingChecked.Length - 1; ++i)
                    {
                        Type sourceType = _parameters[i];
                        Type typeBeingChecked = parametersBeingChecked[i].ParameterType;
                        if (!typeBeingChecked.IsAssignableFrom(sourceType))
                        {
                            satisfied = false;
                            break;
                        }
                    }
                    for (int i = parametersBeingChecked.Length - 1; i < _parameters.Length; ++i)
                    {
                        Type sourceType = _parameters[i];
                        if (!paramArrayType.IsAssignableFrom(sourceType))
                        {
                            satisfied = false;
                            break;
                        }
                    }
                }
            }
            return satisfied;
        }

        #endregion

        #region Fields

        private readonly Type[] _parameters;

        #endregion
    }
}
