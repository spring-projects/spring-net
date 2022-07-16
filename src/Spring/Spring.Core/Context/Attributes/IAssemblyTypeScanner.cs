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

using System.Reflection;

namespace Spring.Context.Attributes
{

    /// <summary>
    /// Scanner that can filter types from assemblies based on constraints.
    /// </summary>
    public interface IAssemblyTypeScanner
    {
        /// <summary>
        /// Add the Assembly containing the specified <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IAssemblyTypeScanner AssemblyHavingType<T>();
        
        /// <summary>
        /// Adds the predicate to the assembly filter constraints.
        /// </summary>
        /// <param name="assemblyPredicate">The assembly predicate.</param>
        /// <returns></returns>
        IAssemblyTypeScanner WithAssemblyFilter(Func<Assembly, bool> assemblyPredicate);

        /// <summary>
        /// Adds the predicte to the include filter for <see cref="Type"/>.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        IAssemblyTypeScanner WithIncludeFilter(Func<Type, bool> predicate);

        /// <summary>
        /// Adds the predicte to the exclude filter for <see cref="Type"/>.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        IAssemblyTypeScanner WithExcludeFilter(Func<Type, bool> predicate);

        /// <summary>
        /// Includes the specific types.
        /// </summary>
        /// <param name="typeSource">The types.</param>
        /// <returns></returns>
        IAssemblyTypeScanner IncludeTypes(IEnumerable<Type> typeSource);


        /// <summary>
        /// Includes the type.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> to include.</typeparam>
        /// <returns></returns>
        IAssemblyTypeScanner IncludeType<T>();

        /// <summary>
        /// Excludes the type.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> to exclude.</typeparam>
        /// <returns></returns>
        IAssemblyTypeScanner ExcludeType<T>();

        /// <summary>
        /// Perform the Scan, applying all provided 
        /// </summary>
        /// <returns></returns>
        IEnumerable<Type> Scan();
    }
}
