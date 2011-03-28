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

#region Imports

using Spring.Util;

#endregion

namespace Spring.Core.TypeResolution
{
    /// <summary>
    /// Holds data about a <see cref="System.Type"/> and it's
    /// attendant <see cref="System.Reflection.Assembly"/>.
    /// </summary>
    public class TypeAssemblyHolder
    {
        #region Constants

        /// <summary>
        /// The string that separates a <see cref="System.Type"/> name
        /// from the name of it's attendant <see cref="System.Reflection.Assembly"/>
        /// in an assembly qualified type name.
        /// </summary>
        public const string TypeAssemblySeparator = ",";

        #endregion

        #region Fields

        private string unresolvedAssemblyName;
        private string unresolvedTypeName;

        #endregion

        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the TypeAssemblyHolder class.
        /// </summary>
        /// <param name="unresolvedTypeName">
        /// The unresolved name of a <see cref="System.Type"/>.
        /// </param>
        public TypeAssemblyHolder(string unresolvedTypeName)
        {
            SplitTypeAndAssemblyNames(unresolvedTypeName);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The (unresolved) type name portion of the original type name.
        /// </summary>
        public string TypeName
        {
            get { return unresolvedTypeName; }
        }

        /// <summary>
        /// The (unresolved, possibly partial) name of the attandant assembly.
        /// </summary>
        public string AssemblyName
        {
            get { return unresolvedAssemblyName; }
        }

        /// <summary>
        /// Is the type name being resolved assembly qualified?
        /// </summary>
        public bool IsAssemblyQualified
        {
            get { return StringUtils.HasText(AssemblyName); }
        }

        #endregion

        #region Methods

        private void SplitTypeAndAssemblyNames(string originalTypeName)
        {
            // generic types may look like:
            // Spring.Objects.TestGenericObject`2[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]][] , Spring.Core.Tests, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
            //
            // start searching for assembly separator after the last bracket if any
            int typeAssemblyIndex = originalTypeName.LastIndexOf(']');
            typeAssemblyIndex = originalTypeName.IndexOf(TypeAssemblySeparator, typeAssemblyIndex+1);
            if (typeAssemblyIndex < 0)
            {
                unresolvedTypeName = originalTypeName;
            }
            else
            {
                unresolvedTypeName = originalTypeName.Substring(
                    0, typeAssemblyIndex).Trim();
                unresolvedAssemblyName = originalTypeName.Substring(
                    typeAssemblyIndex + 1).Trim();
            }
        }

        #endregion
    }
}
