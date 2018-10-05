#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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
using System.Linq;
using System.Web.Mvc;
using System.Reflection;

namespace Spring.Web.Mvc.Tests
{
    public static class ControllerFactoryTestExtension
    {
        private static readonly PropertyInfo _typeCacheProperty;
        private static readonly FieldInfo _cacheField;

        static ControllerFactoryTestExtension()
        {
            _typeCacheProperty = typeof(DefaultControllerFactory).GetProperty("ControllerTypeCache", BindingFlags.Instance | BindingFlags.NonPublic);
            _cacheField = _typeCacheProperty.PropertyType.GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Replaces the cache field of a the DefaultControllerFactory's ControllerTypeCache.
        /// This ensures that only the specified controller types will be searched when instantiating a controller.
        /// As the ControllerTypeCache is internal, this uses some reflection hackery.
        /// </summary>
        public static void InitializeWithControllerTypes(this IControllerFactory factory, params Type[] controllerTypes)
        {
            var cache = controllerTypes
                .GroupBy(t => t.Name.Substring(0, t.Name.Length - "Controller".Length), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToLookup(t => t.Namespace ?? string.Empty, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);

            var buildManager = _typeCacheProperty.GetValue(factory, null);
            _cacheField.SetValue(buildManager, cache);
        }
    }
}
