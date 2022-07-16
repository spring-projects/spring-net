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

using System.Collections;
using System.Reflection;
using Common.Logging;
using Spring.Util;

namespace Spring.Context.Attributes
{
    /// <summary>
    /// Represents a collection of Types.
    /// </summary>
    [Serializable]
    public class AssemblyTypeSource : IEnumerable<Type>
    {
        /// <summary>
        /// Logger Instance.
        /// </summary>
        protected static readonly ILog Logger = LogManager.GetLogger<AssemblyTypeSource>();

        private readonly Assembly _assembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyTypeSource"/> class.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public AssemblyTypeSource(Assembly assembly)
        {
            AssertUtils.ArgumentNotNull(assembly, "assembly");
            _assembly = assembly;
        }

        #region IEnumerable<Type> Members

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Type> GetEnumerator()
        {
            Type[] types = new Type[]{};
            try
            {
                types = _assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                //log and swallow everything that might go wrong here...
                Logger.Debug(m => m("Failed to get types " +  ex.LoaderExceptions), ex);
            }
            catch (Exception ex)
            {
                //log and swallow everything that might go wrong here...
                Logger.Debug(m => m("Failed to get types "), ex);
            }


            foreach (Type type in types)
                yield return type;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
