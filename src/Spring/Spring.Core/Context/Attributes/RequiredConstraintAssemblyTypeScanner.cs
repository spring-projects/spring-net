#region License

/*
 * Copyright © 2010-2011 the original author or authors.
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

namespace Spring.Context.Attributes
{
    /// <summary>
    /// AssemblyTypeScanner that provides for applying a final hard-coded Required Constraint to all types found in the the scanned assemblies
    /// in addition to respecting the constraints passed to it during its configuration.
    /// </summary>
    [Serializable]
    public abstract class RequiredConstraintAssemblyTypeScanner : AssemblyTypeScanner
    {

        /// <summary>
        /// Determines whether the compound predicate is satisfied by the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if the compound predicate is satisfied by the specified type; otherwise, <c>false</c>.
        /// </returns>
        protected override bool IsCompoundPredicateSatisfiedBy(Type type)
        {
            return IsRequiredConstraintSatisfiedBy(type) && IsIncludedType(type) && !IsExcludedType(type);
        }

        /// <summary>
        /// Determines whether the required constraint is satisfied by the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if the required constraint is satisfied by the specified type; otherwise, <c>false</c>.
        /// </returns>
        protected abstract bool IsRequiredConstraintSatisfiedBy(Type type);
    }
}
