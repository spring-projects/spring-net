#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

#region Imports

using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Spring.Collections;

#endregion

namespace Spring.Util
{
    /// <summary>
    /// Use this class for obtaining <see cref="ModuleBuilder"/> instances for dynamic code generation.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The purpose of this class is to provide a simple abstraction for creating and managing dynamic assemblies.
    /// </p>
    /// <note>
    /// Using this factory you can't define several modules within a single dynamic assembly - only a simple one2one relation between assembly/module is used.
    /// </note>
    /// </remarks>
    /// <example>
    /// <p>The following excerpt from <see cref="Spring.Proxy.DynamicProxyManager"/> demonstrates usage:</p>
    /// <code language="c#">
    /// public class DynamicProxyManager
    /// {
    ///   public const string PROXY_ASSEMBLY_NAME = "Spring.Proxy";
    ///
    ///   public static TypeBuilder CreateTypeBuilder(string name, Type baseType)
    ///   {
    ///     // Generates type name
    ///     string typeName = String.Format("{0}.{1}_{2}", PROXY_ASSEMBLY_NAME, name, Guid.NewGuid().ToString("N"));
    ///     ModuleBuilder module = DynamicCodeManager.GetModuleBuilder(PROXY_ASSEMBLY_NAME);
    ///     return module.DefineType(typeName, PROXY_TYPE_ATTRIBUTES);
    ///   }
    /// }
    /// </code>
    /// </example>
    /// <author>Erich Eichinger</author>
    /// <seealso cref="Spring.Reflection.Dynamic.DynamicReflectionManager"/>
    /// <seealso cref="Spring.Proxy.DynamicProxyManager"/>
    /// <seealso cref="Spring.Objects.Factory.Support.MethodInjectingInstantiationStrategy"/>
    public sealed class DynamicCodeManager
    {
        private static readonly Hashtable s_moduleCache = new CaseInsensitiveHashtable(); //CollectionsUtil.CreateCaseInsensitiveHashtable();

        /// <summary>
        /// prevent instantiation
        /// </summary>
        private DynamicCodeManager()
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Returns the <see cref="ModuleBuilder"/> for the dynamic module within the specified assembly.
        /// </summary>
        /// <remarks>
        /// If the assembly does not exist yet, it will be created.<br/>
        /// This factory caches any dynamic assembly it creates - calling GetModule() twice with
        /// the same name will *not* create 2 distinct modules!
        /// </remarks>
        /// <param name="assemblyName">The assembly-name of the module to be returned</param>
        /// <returns>the <see cref="ModuleBuilder"/> that can be used to define new types within the specified assembly</returns>
        public static ModuleBuilder GetModuleBuilder( string assemblyName )
        {
            lock(s_moduleCache.SyncRoot)
            {
                ModuleBuilder module = (ModuleBuilder) s_moduleCache[assemblyName];
                if (module == null)
                {
                    AssemblyName an = new AssemblyName();
                    an.Name = assemblyName;
                    module = BuildModule(an);
                    s_moduleCache[assemblyName] = module;
                }
                return module;
            }
        }

#if !NETSTANDARD
        private static ModuleBuilder BuildModule(AssemblyName an)
        {
#if DEBUG_DYNAMIC
            AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave, null, null, null, null,null, true );
            var module = assembly.DefineDynamicModule(an.Name, an.Name + ".dll", true);
#else
            an.SetPublicKey(Assembly.GetExecutingAssembly().GetName().GetPublicKey());
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run, null, null, null, null,null, true );
#if DEBUG
            var module = assembly.DefineDynamicModule(an.Name, true);
#else
			var module = assembly.DefineDynamicModule(an.Name, false);
#endif
#endif
            return module;
        }
#else
        private static ModuleBuilder BuildModule(AssemblyName an)
        {
            var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
            var module = assembly.DefineDynamicModule(an.Name);
            return module;
        }
#endif

        /// <summary>
        /// Persists the specified dynamic assembly to the file-system
        /// </summary>
        /// <param name="assemblyName">the name of the dynamic assembly to persist</param>
        /// <remarks>
        /// Can only be called in DEBUG_DYNAMIC mode, per ConditionalAttribute rules.
        /// </remarks>
        [Conditional("DEBUG_DYNAMIC")]
        public static void SaveAssembly( string assemblyName )
        {
            AssertUtils.ArgumentHasText(assemblyName, "assemblyName");

            ModuleBuilder module = null;
            lock(s_moduleCache.SyncRoot)
            {
                module = (ModuleBuilder) s_moduleCache[assemblyName];
            }

            if(module == null)
            {
                throw new ArgumentException(string.Format("'{0}' is not a valid dynamic assembly name", assemblyName), "assemblyName");
            }

            AssemblyBuilder assembly = (AssemblyBuilder) module.Assembly;

#if !NETSTANDARD
            assembly.Save(assembly.GetName().Name + ".dll");
#endif
        }

        /// <summary>
        /// Removes all registered <see cref="ModuleBuilder"/>s.
        /// </summary>
        public static void Clear()
        {
            lock (s_moduleCache.SyncRoot)
            {
                s_moduleCache.Clear();
            }
        }
    }
}
