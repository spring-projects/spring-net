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
using System.Reflection;
using Spring.Context.Attributes;
using Spring.Objects.Factory.Support;
using Spring.Util;

namespace Spring.Context.Support
{
    /// <summary>
    /// Extensions to enable scanning on any AbstractApplicationContext-derived type.
    /// </summary>
    public static class GenericApplicationContextExtensions
    {
        /// <summary>
        /// Scans for types using the provided scanner.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="scanner">The scanner.</param>
        public static void Scan(this GenericApplicationContext context, AssemblyObjectDefinitionScanner scanner)
        {
            var registry = context.ObjectFactory as IObjectDefinitionRegistry;
            scanner.ScanAndRegisterTypes(registry);

            AttributeConfigUtils.RegisterAttributeConfigProcessors(registry);
        }

        /// <summary>
        /// Scans for types that satisfy specified predicates located in the specified scan path.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="assemblyScanPath">The assembly scan path.</param>
        /// <param name="assemblyPredicate">The assembly predicate.</param>
        /// <param name="typePredicate">The type predicate.</param>
        public static void Scan(this GenericApplicationContext context, string assemblyScanPath, Func<Assembly, bool> assemblyPredicate,
                                Func<Type, bool> typePredicate)
        {
            Scan(context, assemblyScanPath, assemblyPredicate, typePredicate, new string[0]);
        }


        /// <summary>
        /// Scans the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="assemblyScanPath">The assembly scan path.</param>
        /// <param name="assemblyPredicate">The assembly predicate.</param>
        /// <param name="typePredicate">The type predicate.</param>
        /// <param name="assembliesToScan">The assemblies to scan.</param>
        public static void Scan(this GenericApplicationContext context, string assemblyScanPath, Func<Assembly, bool> assemblyPredicate, Func<Type, bool> typePredicate, params string[] assembliesToScan)
        {
            AssemblyObjectDefinitionScanner scanner = ArrayUtils.HasElements(assembliesToScan) 
                ? new AssemblyObjectDefinitionScanner(assembliesToScan) 
                : new AssemblyObjectDefinitionScanner();
            
            scanner.ScanStartFolderPath = assemblyScanPath;
            
            //configure the scanner per the provided constraints
            scanner.WithAssemblyFilter(assemblyPredicate).WithIncludeFilter(typePredicate);

            //pass the scanner to primary Scan method to actually do the work
            Scan(context, scanner);
        }

        /// <summary>
        /// Scans for types that satisfy specified predicates.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="assemblyPredicate">The assembly predicate.</param>
        /// <param name="typePredicate">The type predicate.</param>
        public static void Scan(this GenericApplicationContext context, Func<Assembly, bool> assemblyPredicate, Func<Type, bool> typePredicate)
        {
            Scan(context, null, assemblyPredicate, typePredicate);
        }

        /// <summary>
        /// Scans for types using the default scanner.
        /// </summary>
        /// <param name="context">The context.</param>
        public static void ScanAllAssemblies(this GenericApplicationContext context)
        {
            Scan(context, new AssemblyObjectDefinitionScanner());
        }


        /// <summary>
        /// Scans the with assembly filter.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="assemblyPredicate">The assembly predicate.</param>
        public static void ScanWithAssemblyFilter(this GenericApplicationContext context, Func<Assembly, bool> assemblyPredicate)
        {
            Scan(context, null, assemblyPredicate, obj => true);
        }

        /// <summary>
        /// Scans the with type filter.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="typePredicate">The type predicate.</param>
        public static void ScanWithTypeFilter(this GenericApplicationContext context, Func<Type, bool> typePredicate)
        {
            Scan(context, null, obj => true, typePredicate);
        }


    }
}