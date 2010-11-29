using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using Common.Logging;
using Spring.Util;

namespace Spring.Context.Attributes
{

    public interface IAssemblyObjectDefinitionScanner
    {
        IAssemblyObjectDefinitionScanner AssemblyHavingType<T>();
        IAssemblyObjectDefinitionScanner WithAssemblyFilter(Predicate<Assembly> assemblyPredicate);

        IAssemblyObjectDefinitionScanner WithIncludeFilter(Predicate<Type> predicate);
        IAssemblyObjectDefinitionScanner WithExcludeFilter(Predicate<Type> predicate);

        IAssemblyObjectDefinitionScanner IncludeTypes(IEnumerable<Type> typeSource);
        IAssemblyObjectDefinitionScanner IncludeType<T>();

        IEnumerable<Type> Scan();
    }

    public class AssemblyObjectDefinitionScanner : IAssemblyObjectDefinitionScanner
    {
        private readonly List<Predicate<Assembly>> _assemblyPredicates = new List<Predicate<Assembly>>();

        private readonly List<Predicate<Type>> _excludePredicates = new List<Predicate<Type>>();

        private string _folderScanPath;

        private readonly List<Predicate<Type>> _includePredicates = new List<Predicate<Type>>();

        private static ILog _logger = LogManager.GetLogger(typeof(AssemblyObjectDefinitionScanner));

        private readonly List<IEnumerable<Type>> _typeSources = new List<IEnumerable<Type>>();

        /// <summary>
        /// Initializes a new instance of the AssemblyObjectDefinitionScanner class.
        /// </summary>
        public AssemblyObjectDefinitionScanner()
        {
            _folderScanPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// Initializes a new instance of the AssemblyObjectDefinitionScanner class.
        /// </summary>
        /// <param name="folderScanPath">The folder scan path.</param>
        public AssemblyObjectDefinitionScanner(string folderScanPath)
        {
            _folderScanPath = folderScanPath;
        }

        public IAssemblyObjectDefinitionScanner AssemblyHavingType<T>()
        {
            _typeSources.Add(new AssemblyTypeSource((typeof(T).Assembly)));
            return this;
        }

        public IAssemblyObjectDefinitionScanner IncludeType<T>()
        {
            _includePredicates.Add(t => t == typeof(T));
            return this;
        }

        public IAssemblyObjectDefinitionScanner IncludeTypes(IEnumerable<Type> typeSource)
        {
            AssertUtils.ArgumentNotNull(typeSource, "typeSource");
            _typeSources.Add(typeSource);
            _includePredicates.Add(t => typeSource.Any(t1 => t1 == t));
            return this;
        }

        public IEnumerable<Type> Scan()
        {
            SetDefaultFiltersIfNeeded();

            IList<Type> types = new List<Type>();

            foreach (Assembly assembly in GetAllMatchingAssemblies())
            {
                _typeSources.Add(new AssemblyTypeSource(assembly));
            }

            foreach (var typeSource in _typeSources)
            {
                foreach (Type type in typeSource)
                {
                    if (IsIncludedType(type) && !IsExcludedType(type) && HasComponentAttribute(type))
                    {
                        types.Add(type);
                    }
                }
            }

            return types;
        }

        public IAssemblyObjectDefinitionScanner WithAssemblyFilter(Predicate<Assembly> assemblyPredicate)
        {
            _assemblyPredicates.Add(assemblyPredicate);
            return this;
        }

        public IAssemblyObjectDefinitionScanner WithExcludeFilter(Predicate<Type> predicate)
        {
            _excludePredicates.Add(predicate);
            return this;
        }

        public IAssemblyObjectDefinitionScanner WithIncludeFilter(Predicate<Type> predicate)
        {
            _includePredicates.Add(predicate);
            return this;
        }

        protected virtual bool IsExcludedType(Type type)
        {
            foreach (var exclude in _excludePredicates)
            {
                if (exclude(type))
                {
                    return true;
                }
            }
            return false;
        }

        protected virtual bool IsIncludedType(Type type)
        {
            foreach (var include in _includePredicates)
            {
                if (include(type))
                {
                    return true;
                }
            }
            return false;
        }


        private bool HasComponentAttribute(Type type)
        {
            return Attribute.GetCustomAttribute(type, typeof(ConfigurationAttribute), true) != null;
        }


        private IEnumerable<Assembly> GetAllMatchingAssemblies()
        {
            IList<Assembly> assemblyCandidates = new List<Assembly>();

            IEnumerable<string> files = Directory.GetFiles(_folderScanPath, "*.dll");

            foreach (string file in files)
            {
                try
                {
                    assemblyCandidates.Add(Assembly.LoadFrom(file));
                }
                catch (Exception ex)
                {
                    //log and swallow everything that might go wrong here...
                    if (_logger.IsDebugEnabled)
                        _logger.Debug("Failed to load type while scanning Assemblies for Defintions!", ex);

                }
            }

            IList<Assembly> assemblies = new List<Assembly>();

            foreach (Assembly assemblyCandidate in assemblyCandidates)
            {
                foreach (var include in _assemblyPredicates)
                {
                    if (include(assemblyCandidate))
                    {
                        assemblies.Add(assemblyCandidate);
                        break;
                    }
                }
            }

            return assemblies;
        }

        private void SetDefaultFiltersIfNeeded()
        {
            if (_includePredicates.Count == 0)
            {
                _includePredicates.Add(t => true);
            }

            if (_excludePredicates.Count == 0)
            {
                _excludePredicates.Add(t => false);
            }

            if (_assemblyPredicates.Count == 0)
            {
                _assemblyPredicates.Add(a => true);
            }
        }

    }
}
