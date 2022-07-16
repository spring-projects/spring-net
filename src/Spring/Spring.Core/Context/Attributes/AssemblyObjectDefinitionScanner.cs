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
using Spring.Objects.Factory.Support;
using Spring.Stereotype;
using Spring.Util;

namespace Spring.Context.Attributes
{
    /// <summary>
    /// AssemblyTypeScanner that only accepts types that also meet the requirements of being ObjectDefintions.
    /// </summary>
    [Serializable]
    public class AssemblyObjectDefinitionScanner : RequiredConstraintAssemblyTypeScanner
    {
        private readonly List<Func<Assembly, bool>> _assemblyExclusionPredicates = new List<Func<Assembly, bool>>();

        private readonly List<string> _springAssemblyExcludePrefixes = new List<string>
        {
            "Antlr3.Runtime",
            "Spring.",
            "NHibernate.",
            "Common.Logging",
            "log4net",
            "Mono.Cecil",
            "NUnit",
            "Quartz",
            "NVelocity",
            "FakeItEasy",
            "Apache.NMS"
        };

        private IObjectNameGenerator _objectNameGenerator = new AttributeObjectNameGenerator();

        /// <summary>
        /// Provides the name generator for all scanned objects.
        /// Default is <see cref="AttributeObjectNameGenerator"/>
        /// </summary>
        public IObjectNameGenerator ObjectNameGenerator
        {
            get { return _objectNameGenerator; }
            set { _objectNameGenerator = value; }
        }

        /// <summary>
        /// Registers the defintions for types.
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <param name="typesToRegister">The types to register.</param>
        private void RegisterDefinitionsForTypes(IObjectDefinitionRegistry registry, IEnumerable<Type> typesToRegister)
        {
            foreach (Type type in typesToRegister)
            {
                var definition = new ScannedGenericObjectDefinition(type, Defaults);
                string objectName = ObjectNameGenerator.GenerateObjectName(definition, registry);
                string fullname = type.FullName;

                Logger.Debug(m => m("Register Type: {0} with object name '{1}'", fullname, objectName));
                registry.RegisterObjectDefinition(objectName, definition);
            }
        }


        /// <summary>
        /// Applies the assembly filters to the assembly candidates.
        /// </summary>
        /// <param name="assemblyCandidates">The assembly candidates.</param>
        /// <returns></returns>
        protected override IEnumerable<Assembly> ApplyAssemblyFiltersTo(IEnumerable<Assembly> assemblyCandidates)
        {
            foreach (Assembly candidate in assemblyCandidates)
            {
                if (IsIncludedAssembly(candidate) && !IsExcludedAssembly(candidate))
                {
                    yield return candidate;
                }
            }
        }

        /// <summary>
        /// Determines whether the specified candidate is and excluded assembly.
        /// </summary>
        /// <param name="candidate">The candidate.</param>
        /// <returns>
        /// 	<c>true</c> if the specified candidate is an excluded assembly ; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsExcludedAssembly(Assembly candidate)
        {
            return _assemblyExclusionPredicates.Any(exclude => exclude(candidate));
        }

        /// <summary>
        /// Determines whether the required constraint is satisfied by the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if the required constraint is satisfied by the specified type; otherwise, <c>false</c>.
        /// </returns>
        protected override bool IsRequiredConstraintSatisfiedBy(Type type)
        {
            if (!type.Assembly.ReflectionOnly)
            {
                try
                {
                    return Attribute.GetCustomAttribute(type, typeof(ComponentAttribute), true) != null &&
                           !type.IsAbstract;
                }
                catch (AmbiguousMatchException)
                {
                    Logger.Error(m => m("Type {0} has more than one ComponentAttributes assigned to it.", type.FullName));
                    return false;
                }
            }

            bool satisfied = false;

            foreach (CustomAttributeData customAttributeData in CustomAttributeData.GetCustomAttributes(type))
            {
                if (customAttributeData.Constructor.DeclaringType != null &&
                    (customAttributeData.Constructor.DeclaringType.FullName == typeof(ComponentAttribute).FullName &&
                    !type.IsAbstract))
                {
                    satisfied = true;
                    break;
                }
            }

            return satisfied;
        }

        /// <summary>
        /// Sets the default filters.
        /// </summary>
        protected override void SetDefaultFilters()
        {
            //set the built-in defaults
            base.SetDefaultFilters();

            //add the desired assembly exclusions to the list
            _assemblyExclusionPredicates.Add(assembly =>
            {
                var assemblyName = assembly.GetName().Name;
                if ("Spring.Core.Tests".Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                for (var i = 0; i < _springAssemblyExcludePrefixes.Count; i++)
                {
                    var name = _springAssemblyExcludePrefixes[i];
                    if (assemblyName.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            });
            _assemblyExclusionPredicates.Add(assembly => assembly.GetName().Name.StartsWith("System."));
            _assemblyExclusionPredicates.Add(assembly => assembly.GetName().Name.StartsWith("Microsoft."));
            _assemblyExclusionPredicates.Add(assembly => assembly.GetName().Name == "mscorlib");
            _assemblyExclusionPredicates.Add(assembly => assembly.GetName().Name == "System");
        }

        /// <summary>
        /// Scans the and register types.
        /// </summary>
        /// <param name="registry">The registry within which to register the types.</param>
        public virtual void ScanAndRegisterTypes(IObjectDefinitionRegistry registry)
        {
            IEnumerable<Type> configTypes = Scan();
            RegisterDefinitionsForTypes(registry, configTypes);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyObjectDefinitionScanner"/> class.
        /// </summary>
        public AssemblyObjectDefinitionScanner()
        {
            AssemblyLoadExclusionPredicates.Add(candidate =>
            {
                if ("Spring.Core.Tests".Equals(candidate, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                foreach (var prefix in _springAssemblyExcludePrefixes)
                {
                    if (candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            });
            AssemblyLoadExclusionPredicates.Add(name => name.StartsWith("System."));
            AssemblyLoadExclusionPredicates.Add(name => name.StartsWith("Microsoft."));
            AssemblyLoadExclusionPredicates.Add(name => name == "mscorlib");
            AssemblyLoadExclusionPredicates.Add(name => name == "System");
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyObjectDefinitionScanner"/> class.
        /// </summary>
        /// <param name="assembliesToIncludePredicates">The assemblies to include predicates.</param>
        public AssemblyObjectDefinitionScanner(params Func<string, bool>[] assembliesToIncludePredicates)
        {
            //force exclude for ALL assemblies
            AssemblyLoadExclusionPredicates.Add(name => true);

            //since all assemblies are EXCLUDED above, these will be the ONLY assemblies to be loaded
            foreach (var predicate in assembliesToIncludePredicates)
            {
                AssemblyLoadInclusionPredicates.Add(predicate);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyObjectDefinitionScanner"/> class.
        /// </summary>
        /// <param name="assembliesToInclude">The names of assemblies to include.</param>
        public AssemblyObjectDefinitionScanner(params string[] assembliesToInclude)
            : this(name => assembliesToInclude.Any(candidate => candidate == name))
        {
            AssertUtils.ArgumentNotNull(assembliesToInclude, "assembliesToInclude");
        }
    }
}
