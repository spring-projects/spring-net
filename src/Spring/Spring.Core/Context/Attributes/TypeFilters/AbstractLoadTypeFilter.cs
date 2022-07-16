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

using Spring.Core.TypeResolution;
using Common.Logging;

namespace Spring.Context.Attributes.TypeFilters
{
    /// <summary>
    /// Abstract Type Filter that provides methods to load a required type from assembly.
    /// </summary>
    public abstract class AbstractLoadTypeFilter : ITypeFilter
    {
        private static readonly ILog Logger = LogManager.GetLogger<AbstractLoadTypeFilter>();


        /// <summary>
        /// Required Type to compare against provided Type
        /// </summary>
        protected Type RequiredType;


        /// <summary>
        /// Determine a match based on the given type object.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>true if there is a match; false is there is no match</returns>
        public abstract bool Match(Type type);


        /// <summary>
        /// Is loading a Type from a string passed to method in the form [Type.FullName], [Assembly.Name]
        /// </summary>
        protected void GetRequiredType(string typeToLoad)
        {
            try
            {
                RequiredType = !string.IsNullOrEmpty(typeToLoad) ?
                    TypeResolutionUtils.ResolveType(typeToLoad) : 
                    null;
            }
            catch (Exception)
            {
                RequiredType = null;
                Logger.Error("Can't load type defined in expression:" + typeToLoad);
            }
        }

    }
}
