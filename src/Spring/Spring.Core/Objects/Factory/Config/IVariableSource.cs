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

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Defines contract that different variable sources have to implement.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The "variable sources" are objects containing name-value pairs
    /// that allow a variable value to be retrieved for the given name.</p>
    /// <p>
    /// Out of the box, Spring.NET supports a number of variable sources, 
    /// that allow users to obtain variable values from .NET config files,
    /// Java-style property files, environment, registry, etc.</p>
    /// <p>
    /// Users can always write their own variable sources implementations,
    /// that will allow them to load variable values from the database or 
    /// other proprietary data source.</p>
    /// </remarks>
    /// <seealso cref="ConfigSectionVariableSource"/>
    /// <seealso cref="PropertyFileVariableSource"/>
    /// <seealso cref="EnvironmentVariableSource"/>
    /// <seealso cref="CommandLineArgsVariableSource"/>
    /// <seealso cref="RegistryVariableSource"/>
    /// <seealso cref="SpecialFolderVariableSource"/>
	/// <author>Aleksandar Seovic</author>
    public interface IVariableSource
    {
        /// <summary>
        /// Before requesting a variable resolution, a client should
        /// ask, whether the source can resolve a particular variable name.
        /// </summary>
        /// <param name="name">the name of the variable to resolve</param>
        /// <returns><c>true</c> if the variable can be resolved, <c>false</c> otherwise</returns>
        bool CanResolveVariable(string name);

        /// <summary>
        /// Resolves variable value for the specified variable name.
        /// </summary>
        /// <param name="name">
        /// The name of the variable to resolve.
        /// </param>
        /// <returns>
        /// The variable value if able to resolve, <c>null</c> otherwise.
        /// </returns>
        string ResolveVariable(string name);
    }
}