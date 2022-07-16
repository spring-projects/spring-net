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

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Implementation of <see cref="IVariableSource"/> that
    /// resolves variable name against special folders (as defined by
    /// <see cref="Environment.SpecialFolder"/> enumeration).
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class SpecialFolderVariableSource : IVariableSource
    {
        /// <summary>
        /// Before requesting a variable resolution, a client should
        /// ask, whether the source can resolve a particular variable name.
        /// </summary>
        /// <param name="name">the name of the variable to resolve</param>
        /// <returns><c>true</c> if the variable can be resolved, <c>false</c> otherwise</returns>
        public bool CanResolveVariable(string name)
        {
            return ResolveVariable(name) != null;
        }

        /// <summary>
        /// Resolves specified special folder to its full path.
        /// </summary>
        /// <param name="name">
        /// The name of the special folder to resolve. Should be one of the values
        /// defined by the <see cref="Environment.SpecialFolder"/> enumeration.
        /// </param>
        /// <returns>
        /// The folder path if able to resolve, <c>null</c> otherwise.
        /// </returns>
        public string ResolveVariable(string name)
        {
            try
            {
                Environment.SpecialFolder folder =
                    (Environment.SpecialFolder) Enum.Parse(typeof (Environment.SpecialFolder), name, true);

                return Environment.GetFolderPath(folder);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
