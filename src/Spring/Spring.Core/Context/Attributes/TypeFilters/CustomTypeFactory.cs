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

using Common.Logging;
using Spring.Core.TypeResolution;
using Spring.Util;
using Spring.Objects.Factory.Support;

namespace Spring.Context.Attributes.TypeFilters
{
    /// <summary>
    /// Creates a new instance of a given type string
    /// </summary>
    public static class CustomTypeFactory
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CustomTypeFactory).FullName);

        /// <summary>
        /// Creates a new instance of given type filter type string
        /// </summary>
        /// <param name="expression">Custom type filter to create</param>
        /// <returns>An instance of ITypeFilter or NULL if no instance can be created</returns>
        public static ITypeFilter GetTypeFilter(string expression)
        {
            return GetCustomType(expression) as ITypeFilter;
        }


        /// <summary>
        /// Creates a new instance of given name generator type string
        /// </summary>
        /// <param name="expression">Custom type name generator string to create</param>
        /// <returns>An instance of IObjectNameGenerator or NULL if no instance can be created</returns>
        public static IObjectNameGenerator GetNameGenerator(string expression)
        {
            return GetCustomType(expression) as IObjectNameGenerator;
        }

        private static object GetCustomType(string expression)
        {
            var customTypeFilterType = LoadType(expression);
            if (customTypeFilterType == null)
                return null;

            try
            {
                var instance = ObjectUtils.InstantiateType(customTypeFilterType);
                return instance;
            }
            catch
            {
                Logger.Error(string.Format("Can't instatiate {0}. Type needs to have a non arg constructor.", expression));
            }

            return null;            
        }


        private static Type LoadType(string typeToLoad)
        {
            try
            {
                return TypeResolutionUtils.ResolveType(typeToLoad);
            }
            catch (Exception)
            {
                Logger.Error("Can't load type defined in exoression:" + typeToLoad);
            }

            return null;
        }
    }
}
