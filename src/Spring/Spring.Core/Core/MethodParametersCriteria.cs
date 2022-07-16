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

#endregion

namespace Spring.Core
{
    /// <summary>
    /// Criteria that is satisfied if the <see cref="System.Type"/> of each of the
    /// parameters of a given <see cref="System.Reflection.MethodInfo"/> matches each
    /// of the parameter <see cref="System.Type"/>s of a given
    /// <see cref="System.Reflection.MethodInfo"/>.
    /// </summary>
    /// <remarks>
    /// <p>
    /// If no <see cref="System.Type"/> array is passed to the overloaded constructor,
    /// any method that has no parameters will satisfy an instance of this
    /// class. The same effect could be achieved by passing the
    /// <see cref="System.Type.EmptyTypes"/> array to the overloaded constructor.
    /// </p>
    /// </remarks>
    /// <author>Rick Evans</author>
    /// <author>Bruno Baia</author>
    public class MethodParametersCriteria : ICriteria
    {
        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="MethodParametersCriteria"/> class.
        /// </summary>
        public MethodParametersCriteria() : this(Type.EmptyTypes)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="MethodParametersCriteria"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the supplied <paramref name="parameters"/> array is null, then this
        /// constructor uses the <see cref="System.Type.EmptyTypes"/> array.
        /// </p>
        /// </remarks>
        /// <param name="parameters">
        /// The <see cref="System.Type"/> array that this criteria will use to
        /// check parameter <see cref="System.Type"/>s.
        /// </param>
        public MethodParametersCriteria(Type[] parameters)
        {
            _parameters = parameters;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Does the supplied <paramref name="datum"/> satisfy the criteria encapsulated by
        /// this instance?
        /// </summary>
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
                ParameterInfo[] parameterInfosBeingChecked = method.GetParameters();
                if (parameterInfosBeingChecked != null
                    && parameterInfosBeingChecked.Length == _parameters.Length)
                {
                    satisfied = true;
                    for (int i = 0; i < _parameters.Length; ++i)
                    {
                        if (parameterInfosBeingChecked[i].ParameterType == _parameters[i])
                        {
                            satisfied = true;
                        }
                        else
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
