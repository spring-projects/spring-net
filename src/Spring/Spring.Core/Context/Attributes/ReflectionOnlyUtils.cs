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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Policy;

namespace Spring.Context.Attributes
{
    /// <summary>
    /// Utilities to provide support for manipulating ReflectionOnly types in the <see cref="AppDomain"/>.
    /// </summary>
    public static class ReflectionOnlyUtils
    {
        /// <summary>
        /// Load the <see cref="Assembly"/> into the ReflectionsOnly context based on its partial name.
        /// </summary>
        /// <param name="partialName">The partial name.</param>
        /// <returns>The matching <see cref="Assembly"/></returns>
        public static Assembly ReflectionOnlyLoadWithPartialName(string partialName)
        {
            return ReflectionOnlyLoadWithPartialName(partialName, null);
        }

        private static Assembly ReflectionOnlyLoadWithPartialName(string partialName, Evidence securityEvidence)
        {
            if (securityEvidence != null)
                new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();

            AssemblyName fileName = new AssemblyName(partialName);

            var assembly = nLoad(fileName, null, securityEvidence, null, null, false, true);

            if (assembly != null)
                return assembly;

            var assemblyRef = EnumerateCache(fileName);

            if (assemblyRef != null)
                return InternalLoad(assemblyRef, securityEvidence, null, true);

            return assembly;
        }

        private static Assembly nLoad(params object[] args)
        {
            return (Assembly)typeof(Assembly)
                .GetMethod("nLoad", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, args);
        }

        private static AssemblyName EnumerateCache(params object[] args)
        {
            return (AssemblyName)typeof(Assembly)
                .GetMethod("EnumerateCache", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, args);
        }

        private static Assembly InternalLoad(params object[] args)
        {
            // Easiest to query because the StackCrawlMark type is internal
            IEnumerable<MethodInfo> methods =
                typeof(Assembly).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Where(
                    m => m.Name == "InternalLoad" &&
                         m.GetParameters()[0].ParameterType == typeof (AssemblyName));

            return methods.Select(methodInfo => (Assembly) methodInfo.Invoke(null, args)).FirstOrDefault();
        }
    }
}