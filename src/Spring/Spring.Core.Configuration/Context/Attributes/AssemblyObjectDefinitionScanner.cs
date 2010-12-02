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

    public interface IAssemblyTypeScanner
    {
        IAssemblyTypeScanner AssemblyHavingType<T>();
        IAssemblyTypeScanner WithAssemblyFilter(Predicate<Assembly> assemblyPredicate);

        IAssemblyTypeScanner WithIncludeFilter(Predicate<Type> predicate);
        IAssemblyTypeScanner WithExcludeFilter(Predicate<Type> predicate);

        IAssemblyTypeScanner IncludeTypes(IEnumerable<Type> typeSource);
        IAssemblyTypeScanner IncludeType<T>();
        IAssemblyTypeScanner ExcludeType<T>();

        IEnumerable<Type> Scan();
    }

    public abstract class AssemblyTypeScanner : IAssemblyTypeScanner
    {
        protected readonly List<Predicate<Assembly>> _assemblyPredicates = new List<Predicate<Assembly>>();

        protected readonly List<Predicate<Type>> _excludePredicates = new List<Predicate<Type>>();

        protected string _folderScanPath;

        protected readonly List<Predicate<Type>> _includePredicates = new List<Predicate<Type>>();

        protected static ILog _logger = LogManager.GetLogger(typeof(AssemblyTypeScanner));

        protected readonly List<IEnumerable<Type>> _typeSources = new List<IEnumerable<Type>>();

        /// <summary>
        /// Initializes a new instance of the AssemblyTypeScanner class.
        /// </summary>
        /// <param name="folderScanPath"></param>
        public AssemblyTypeScanner(string folderScanPath)
        {
            if (!string.IsNullOrEmpty(folderScanPath))
            {
                _folderScanPath = folderScanPath;
            }
            else
            {
                _folderScanPath = GetCurrentBinDirectoryPath();
            }
        }

        /// <summary>
        /// Initializes a new instance of the AssemblyTypeScanner class.
        /// </summary>
        public AssemblyTypeScanner()
        {

        }

        public IAssemblyTypeScanner AssemblyHavingType<T>()
        {
            _typeSources.Add(new AssemblyTypeSource((typeof(T).Assembly)));
            return this;
        }

        public IAssemblyTypeScanner ExcludeType<T>()
        {
            _excludePredicates.Add(t => t == typeof(T));
            return this;
        }

        public IAssemblyTypeScanner IncludeType<T>()
        {
            _includePredicates.Add(t => t == typeof(T));
            return this;
        }

        public IAssemblyTypeScanner IncludeTypes(IEnumerable<Type> typeSource)
        {
            AssertUtils.ArgumentNotNull(typeSource, "typeSource");
            _typeSources.Add(typeSource);
            _includePredicates.Add(t => typeSource.Any(t1 => t1 == t));
            return this;
        }

        public virtual IEnumerable<Type> Scan()
        {
            SetDefaultFilters();

            IList<Type> types = new List<Type>();

            foreach (Assembly assembly in GetAllMatchingAssemblies())
            {
                _typeSources.Add(new AssemblyTypeSource(assembly));
            }

            foreach (var typeSource in _typeSources)
            {
                foreach (Type type in typeSource)
                {
                    if (IsIncludedType(type) && !IsExcludedType(type) && FinalVetoConstraintIsSatisfiedBy(type))
                    {
                        types.Add(type);
                    }
                }
            }

            return types;
        }

        public IAssemblyTypeScanner WithAssemblyFilter(Predicate<Assembly> assemblyPredicate)
        {
            _assemblyPredicates.Add(assemblyPredicate);
            return this;
        }

        public IAssemblyTypeScanner WithExcludeFilter(Predicate<Type> predicate)
        {
            _excludePredicates.Add(predicate);
            return this;
        }

        public IAssemblyTypeScanner WithIncludeFilter(Predicate<Type> predicate)
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

        protected virtual bool IsIncludedAssembly(Assembly assembly)
        {
            foreach (var include in _assemblyPredicates)
            {
                if (include(assembly))
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

        protected virtual void SetDefaultFilters()
        {
            if (_includePredicates.Count == 0)
                _includePredicates.Add(t => true);

            if (_excludePredicates.Count == 0)
                _excludePredicates.Add(t => false);

            if (_assemblyPredicates.Count == 0)
                _assemblyPredicates.Add(a => true);
        }

        protected virtual bool FinalVetoConstraintIsSatisfiedBy(Type type)
        {
            return true;
        }

        private IEnumerable<Assembly> ApplyAssemblyFiltersTo(IEnumerable<Assembly> assemblyCandidates)
        {
            IList<Assembly> matchingAssemblies = new List<Assembly>();
            foreach (Assembly assemblyCandidate in assemblyCandidates)
                if (IsIncludedAssembly(assemblyCandidate))
                    matchingAssemblies.Add(assemblyCandidate);
            return matchingAssemblies;
        }

        private IEnumerable<Assembly> GetAllAssembliesInPath(string folderPath)
        {
            IList<Assembly> assemblies = new List<Assembly>();
            IEnumerable<string> files = Directory.GetFiles(folderPath, "*.dll");
            foreach (string file in files)
                try
                {
                    assemblies.Add(Assembly.LoadFrom(file));
                }
                catch (Exception ex)
                {
                    //log and swallow everything that might go wrong here...
                    if (_logger.IsDebugEnabled)
                        _logger.Debug("Failed to load type while scanning Assemblies for Defintions!", ex);
                }
            return assemblies;
        }

        private IEnumerable<Assembly> GetAllMatchingAssemblies()
        {
            IEnumerable<Assembly> assemblyCandidates = GetAllAssembliesInPath(_folderScanPath);
            return ApplyAssemblyFiltersTo(assemblyCandidates);
        }

        private string GetCurrentBinDirectoryPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

    }

    public class AssemblyObjectDefinitionScanner : AssemblyTypeScanner
    {
        /// <summary>
        /// Initializes a new instance of the AssemblyObjectDefinitionScanner class.
        /// </summary>
        /// <param name="folderScanPath">The folder scan path.</param>
        public AssemblyObjectDefinitionScanner(string folderScanPath)
            : base(folderScanPath)
        { }

        /// <summary>
        /// Initializes a new instance of the AssemblyObjectDefinitionScanner class.
        /// </summary>
        public AssemblyObjectDefinitionScanner()
            : base(null)
        { }

        //protected override bool RequiredConstraintIsSatisfiedBy(Type type)
        //{
        //    return Attribute.GetCustomAttribute(type, typeof(ConfigurationAttribute), true) != null;
        //}

        protected override void SetDefaultFilters()
        {
            _includePredicates.Add(t => Attribute.GetCustomAttribute(t, typeof(ConfigurationAttribute), true) != null);

            base.SetDefaultFilters();
        }

    }
}
