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
    /// Criteria that is satisfied if the return <see cref="System.Type"/> of a given
    /// <see cref="System.Reflection.MethodInfo"/> matches a given <see cref="System.Type"/>.
    /// </summary>
    /// <author>Rick Evans</author>
    public class MethodReturnTypeCriteria : ICriteria
    {
        #region Constants

        /// <summary>
        /// The return <see cref="System.Type"/> to match against if no
        /// <see cref="System.Type"/> is provided explictly.
        /// </summary>
        private static readonly Type DefaultType = typeof (void);

        #endregion

        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="MethodReturnTypeCriteria"/> class.
        /// </summary>
        public MethodReturnTypeCriteria()
            : this(DefaultType)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="MethodReturnTypeCriteria"/> class.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> that the return type of a given
        /// <see cref="System.Reflection.MethodInfo"/> must match in order to satisfy
        /// this criteria.
        /// </param>
        public MethodReturnTypeCriteria(Type type)
        {
            ReturnType = type;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The <see cref="System.Type"/> that the return type of a given
        /// <see cref="System.Reflection.MethodInfo"/> must match in order to satisfy
        /// this criteria.
        /// </summary>
        public Type ReturnType
        {
            get { return _type; }
            set { _type = value == null ? DefaultType : value; }
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
                satisfied = method.ReturnType.Equals(ReturnType);
            }
            return satisfied;
        }

        #endregion

        #region Fields

        private Type _type;

        #endregion
    }
}
