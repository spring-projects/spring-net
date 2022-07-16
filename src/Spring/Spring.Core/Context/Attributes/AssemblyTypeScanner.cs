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
using Common.Logging;
using Spring.Context.Attributes.TypeFilters;
using Spring.Util;
using Spring.Objects.Factory.Xml;

namespace Spring.Context.Attributes
{
    /// <summary>
    /// Scans Assebmlies for Types that satisfy a given set of constraints.
    /// </summary>
    [Serializable]
    public abstract class AssemblyTypeScanner : IAssemblyTypeScanner
    {
        /// <summary>
        /// Logger Instance.
        /// </summary>
        protected static readonly ILog Logger = LogManager.GetLogger<AssemblyTypeScanner>();

        /// <summary>
        /// Names of Assemblies to exclude from being loaded for scanning.
        /// </summary>
        protected IList<Func<string, bool>> AssemblyLoadExclusionPredicates = new List<Func<string, bool>>();

        /// <summary>
        /// Names of Assemblies to include for scanning.
        /// </summary>
        protected IList<Func<string, bool>> AssemblyLoadInclusionPredicates = new List<Func<string, bool>>();

        /// <summary>
        /// Assembly Inclusion Predicates.
        /// </summary>
        protected readonly List<Func<Assembly, bool>> AssemblyInclusionPredicates = new List<Func<Assembly, bool>>();

        /// <summary>
        /// Type Exclusion Predicates.
        /// </summary>
        protected readonly List<Func<Type, bool>> TypeExclusionPredicates = new List<Func<Type, bool>>();

        /// <summary>
        /// Type Exclusion Predicates.
        /// </summary>
        protected readonly List<ITypeFilter> TypeExclusionTypeFilters = new List<ITypeFilter>();

        /// <summary>
        /// Type Inclusion Predicates.
        /// </summary>
        protected readonly List<Func<Type, bool>> TypeInclusionPredicates = new List<Func<Type, bool>>();

        /// <summary>
        /// Type Inclusion TypeFilters.
        /// </summary>
        protected readonly List<ITypeFilter> TypeInclusionTypeFilter = new List<ITypeFilter>();

        /// <summary>
        /// Assemblies to scan.
        /// </summary>
        protected readonly List<IEnumerable<Type>> TypeSources = new List<IEnumerable<Type>>();

        /// <summary>
        /// Stores the object default definitons defined in the XML configuration documnet
        /// </summary>
        private DocumentDefaultsDefinition _defaults;

        private string _scanStartFolderPath;


        /// <summary>
        /// Stores the object default definitons defined in the XML configuration documnet
        /// </summary>
        public DocumentDefaultsDefinition Defaults
        {
            get { return _defaults; }
            set { _defaults = value; }
        }

        public string ScanStartFolderPath
        {
            get
            {
                //if we have no value, set it to the current bin dir
                if (string.IsNullOrEmpty(_scanStartFolderPath))
                {
                    _scanStartFolderPath = GetCurrentBinDirectoryPath();
                }
                return _scanStartFolderPath;
            }
            set { _scanStartFolderPath = value; }
        }

        #region IAssemblyTypeScanner Members

        /// <summary>
        /// Assemblies the type of the having.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IAssemblyTypeScanner AssemblyHavingType<T>()
        {
            TypeSources.Add(new AssemblyTypeSource((typeof(T).Assembly)));
            return this;
        }

        /// <summary>
        /// Excludes the type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IAssemblyTypeScanner ExcludeType<T>()
        {
            TypeExclusionPredicates.Add(t => t.FullName == typeof(T).FullName);
            return this;
        }

        /// <summary>
        /// Includes the type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IAssemblyTypeScanner IncludeType<T>()
        {
            TypeInclusionPredicates.Add(t => t.FullName == typeof(T).FullName);
            return this;
        }

        /// <summary>
        /// Includes the types.
        /// </summary>
        /// <param name="typeSource">The type source.</param>
        /// <returns></returns>
        public IAssemblyTypeScanner IncludeTypes(IEnumerable<Type> typeSource)
        {
            AssertUtils.ArgumentNotNull(typeSource, "typeSource");
            TypeSources.Add(typeSource);
            TypeInclusionPredicates.Add(
                t => typeSource.Any(t1 => t1.FullName == t.FullName));
            return this;
        }

        /// <summary>
        /// Performs the Scan, respecting all filter settings.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<Type> Scan()
        {
            SetDefaultFilters();

            IList<Type> types = new List<Type>();

            foreach (Assembly assembly in GetAllMatchingAssemblies(ScanStartFolderPath))
            {
                TypeSources.Add(new AssemblyTypeSource(assembly));
            }

            foreach (var typeSource in TypeSources)
            {
                foreach (Type type in typeSource)
                {
                    if (IsCompoundPredicateSatisfiedBy(type))
                    {
                        Logger.Debug(m => m("Satisfied Type: {0}", type.FullName));
                        types.Add(type);
                    }
                }
            }

            return types;
        }

        /// <summary>
        /// Adds the assembly filter.
        /// </summary>
        /// <param name="assemblyPredicate">The assembly predicate.</param>
        /// <returns></returns>
        public IAssemblyTypeScanner WithAssemblyFilter(Func<Assembly, bool> assemblyPredicate)
        {
            AssemblyInclusionPredicates.Add(assemblyPredicate);
            return this;
        }

        /// <summary>
        /// Adds the exclude filter.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public IAssemblyTypeScanner WithExcludeFilter(Func<Type, bool> predicate)
        {
            TypeExclusionPredicates.Add(predicate);
            return this;
        }

        /// <summary>
        /// Adds the exclude filter.
        /// </summary>
        /// <param name="filter">The type filter.</param>
        /// <returns></returns>
        public IAssemblyTypeScanner WithExcludeFilter(ITypeFilter filter)
        {
            if (filter != null)
                TypeExclusionTypeFilters.Add(filter);

            return this;
        }

        /// <summary>
        /// Adds the include filter.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public IAssemblyTypeScanner WithIncludeFilter(Func<Type, bool> predicate)
        {
            TypeInclusionPredicates.Add(predicate);
            return this;
        }

        /// <summary>
        /// Adds the include filter.
        /// </summary>
        /// <param name="filter">The filter type.</param>
        /// <returns></returns>
        public IAssemblyTypeScanner WithIncludeFilter(ITypeFilter filter)
        {
            if (filter != null)
                TypeInclusionTypeFilter.Add(filter);

            return this;
        }

        #endregion

        private List<string> GetAllAssembliesInPath(string folderPath)
        {

            var assemblies = new List<string>();
            assemblies.AddRange(DiscoverAssemblies(folderPath, "*.dll"));
            assemblies.AddRange(DiscoverAssemblies(folderPath, "*.exe"));

            Logger.Debug(m => m("Assemblies to be scanned: {0}", StringUtils.ArrayToCommaDelimitedString(assemblies.ToArray())));

            return assemblies;
        }

        private IEnumerable<Assembly> GetAllMatchingAssemblies(string folderPath)
        {
            IEnumerable<string> assemblyCandidates = GetAllAssembliesInPath(folderPath);

            IList<Assembly> assemblies = new List<Assembly>();

            foreach (string assembly in assemblyCandidates)
            {
                if (!string.IsNullOrEmpty(assembly))
                {
                    Assembly loadedAssembly = TryLoadAssemblyFromPath(assembly);

                    if (null != loadedAssembly)
                    {
                        string fullname = loadedAssembly.FullName;
                        Logger.Debug(m => m("Add Assembly: {0}", fullname));

                        assemblies.Add(loadedAssembly);
                    }
                }
            }

            return ApplyAssemblyFiltersTo(assemblies);
        }

        private Assembly TryLoadAssemblyFromPath(string filename)
        {
            Assembly assembly = null;

            try
            {
                assembly = Assembly.LoadFrom(filename);
            }
            catch (Exception ex)
            {
                //log and swallow everything that might go wrong here...
                Logger.Debug(m => m("Failed to load assembly {0} to inspect for [Configuration] types!", filename), ex);
            }

            return assembly;
        }

        private string GetCurrentBinDirectoryPath()
        {
            return string.IsNullOrEmpty(AppDomain.CurrentDomain.DynamicDirectory)
                       ? AppDomain.CurrentDomain.BaseDirectory
                       : AppDomain.CurrentDomain.DynamicDirectory;
        }

        /// <summary>
        /// Applies the assembly filters to the assembly candidates.
        /// </summary>
        /// <param name="assemblyCandidates">The assembly candidates.</param>
        /// <returns></returns>
        protected virtual IEnumerable<Assembly> ApplyAssemblyFiltersTo(IEnumerable<Assembly> assemblyCandidates)
        {
            var filteredAssemblies = assemblyCandidates.Where(IsIncludedAssembly).
                                                        AsEnumerable();

            Logger.Debug(m => m("Filtered Assemblies: {0}", StringUtils.ArrayToCommaDelimitedString(filteredAssemblies.ToArray())));

            return filteredAssemblies;
        }

        /// <summary>
        /// Determines whether the compound predicate is satisfied by the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if the compound predicate is satisfied by the specified type; otherwise, <c>false</c>.
        /// </returns>
        protected abstract bool IsCompoundPredicateSatisfiedBy(Type type);

        /// <summary>
        /// Determines whether [is excluded type] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if [is excluded type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsExcludedType(Type type)
        {
            if (TypeExclusionPredicates.Count > 0 && TypeExclusionPredicates.Any(exclude => exclude(type)))
                return true;

            return Enumerable.Any(TypeExclusionTypeFilters, filter => filter.Match(type));
        }

        /// <summary>
        /// Determines whether [is included assembly] [the specified assembly].
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>
        /// 	<c>true</c> if [is included assembly] [the specified assembly]; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsIncludedAssembly(Assembly assembly)
        {
            foreach (var include in AssemblyInclusionPredicates)
            {
                if (include(assembly))
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug(m => m("Include Assembly: {0}", assembly.FullName));
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether [is included type] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if [is included type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsIncludedType(Type type)
        {
            for (var i = 0; i < TypeInclusionPredicates.Count; i++)
            {
                var include = TypeInclusionPredicates[i];
                if (include(type))
                {
                    return true;
                }
            }

            for (var i = 0; i < TypeInclusionTypeFilter.Count; i++)
            {
                var filter = TypeInclusionTypeFilter[i];
                if (filter.Match(type))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Sets the default filters.
        /// </summary>
        protected virtual void SetDefaultFilters()
        {
            if (TypeInclusionPredicates.Count == 0 && TypeInclusionTypeFilter.Count == 0)
                TypeInclusionPredicates.Add(obj => true);

            if (TypeExclusionPredicates.Count == 0 && TypeExclusionTypeFilters.Count == 0)
                TypeExclusionPredicates.Add(obj => false);

            if (AssemblyInclusionPredicates.Count == 0)
                AssemblyInclusionPredicates.Add(obj => true);
        }

        /// <summary>
        /// Loads the assemblies found.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="extension">The extension.</param>
        private IList<string> DiscoverAssemblies(string folderPath, string extension)
        {
            IList<string> assemblies = new List<string>();

            IEnumerable<string> files = Directory.GetFiles(folderPath, extension, SearchOption.AllDirectories);

            foreach (string file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);

                bool isNotExcluded = !AssemblyLoadExclusionPredicates.Any(exclude => exclude(name));
                bool isIncluded = AssemblyLoadInclusionPredicates.Any(include => include(name));

                if (isNotExcluded || isIncluded)
                {
                    assemblies.Add(file);
                }

            }

            return assemblies;
        }
    }
}
