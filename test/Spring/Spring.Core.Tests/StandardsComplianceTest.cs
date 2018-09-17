#region License

/*
 * Copyright 2004 the original author or authors.
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
using System.Reflection;

#endregion

namespace Spring
{
	/// <summary>
	/// Base class for tests that check standards compliance.
	/// </summary>
    public abstract class StandardsComplianceTest
    {
        #region Constructor (s) / Destructor
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.StandardsComplianceTest"/> class.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is an <see langword="abstract"/> class, and as such has no publicly visible
        /// constructors.
        /// </para>
        /// </remarks>
        protected StandardsComplianceTest () {}
        #endregion

        #region Properties
        protected Type CheckedType {
            get {
                return checkedType;
            }
            set {
                checkedType = value;
            }
        }
        #endregion

        private bool IsCheckedType (Type type) {
            if (CheckedType.IsInterface) {
                Type iface = type.GetInterface (CheckedType.Name, false);
                return iface != null;
            } else {
                Type baseType = null;
                while ((baseType = type.BaseType) != null) {
                    if (baseType == CheckedType) {
                        return true;
                    }
                    type = baseType;
                }
            }
            return false;
        }

        protected void ProcessAssembly (Assembly a) {
            foreach (Type t in a.GetTypes ()) {
                
                // TODO: make antlr compliant
                if (t.FullName.IndexOf(".antlr.")>-1) continue;

                if ( (t.IsPublic||t.IsNestedPublic)
                    && IsCheckedType (t)) {
                    CheckStandardsCompliance (a, t);
                }
            }
        }

        protected abstract void CheckStandardsCompliance (Assembly assembly, Type t);

        #region Fields
        private Type checkedType;
        #endregion
	}
}
