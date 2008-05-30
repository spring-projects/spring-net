#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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

using System;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Implementation of <see cref="IVariableSource"/> that
    /// resolves variable name against environment variables.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <version>$Id: EnvironmentVariableSource.cs,v 1.3 2007/08/02 22:18:32 markpollack Exp $</version>
    [Serializable]
    public class EnvironmentVariableSource : IVariableSource
    {
        /// <summary>
        /// Resolves variable value for the specified variable name.
        /// </summary>
        /// <param name="name">
        /// The name of the variable to resolve.
        /// </param>
        /// <returns>
        /// The variable value if able to resolve, <c>null</c> otherwise.
        /// </returns>
        public string ResolveVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }
    }
}