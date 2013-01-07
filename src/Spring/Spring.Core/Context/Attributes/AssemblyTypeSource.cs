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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Spring.Util;

namespace Spring.Context.Attributes
{
    /// <summary>
    /// Represents a collection of Types.
    /// </summary>
    [Serializable]
    public class AssemblyTypeSource : IEnumerable<Type>
    {
        private readonly _Assembly _assembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyTypeSource"/> class.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public AssemblyTypeSource(Assembly assembly)
        {
            AssertUtils.ArgumentNotNull(assembly, "assembly");
            this._assembly = assembly;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Type> GetEnumerator()
        {
            foreach (var type in _assembly.GetTypes())
                yield return type;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}