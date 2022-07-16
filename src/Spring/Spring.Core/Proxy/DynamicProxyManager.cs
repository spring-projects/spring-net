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

using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Spring.Util;

#endregion

namespace Spring.Proxy
{
    /// <summary>
    /// Allows easy access to existing and creation of new dynamic proxies.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <author>Bruno Baia</author>
    public sealed class DynamicProxyManager
    {
        #region Fields

        /// <summary>
        /// The name of the assembly that defines proxy types created.
        /// </summary>
        public const string ASSEMBLY_NAME = "Spring.Proxy";

        /// <summary>
        /// The attributes of the proxy type to generate.
        /// </summary>
        private const TypeAttributes TYPE_ATTRIBUTES = TypeAttributes.BeforeFieldInit | TypeAttributes.Public;

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates an appropriate type builder.
        /// </summary>
        /// <param name="typeName">The proxy type name.</param>
        /// <param name="baseType">The type to extends if provided.</param>
        /// <returns>The type builder to use.</returns>
        public static TypeBuilder CreateTypeBuilder(string typeName, Type baseType)
        {
            ModuleBuilder module = DynamicCodeManager.GetModuleBuilder(ASSEMBLY_NAME);

            try
            {
                if (baseType == null)
                {
                    return module.DefineType(typeName, TYPE_ATTRIBUTES);
                }
                else
                {
                    return module.DefineType(typeName, TYPE_ATTRIBUTES, baseType);
                }
            }
            catch (ArgumentException ex)
            {
                Type alreadyRegisteredType = module.GetType(typeName, true);

                string msg;

                if (alreadyRegisteredType != null)
                    msg = "Proxy already registered for \"{0}\" as Type \"{1}\".";
                else
                    msg = "Proxy already registered for \"{0}\".";

                throw new ArgumentException(string.Format(msg, typeName, alreadyRegisteredType.FullName), ex);
            }
        }

        /// <summary>
        /// Saves dynamically generated assembly to disk.
        /// Can only be called in DEBUG_DYNAMIC mode, per ConditionalAttribute rules.
        /// </summary>
        [Conditional("DEBUG_DYNAMIC")]
        public static void SaveAssembly()
        {
            DynamicCodeManager.SaveAssembly(ASSEMBLY_NAME);
        }

        #endregion
    }
}
