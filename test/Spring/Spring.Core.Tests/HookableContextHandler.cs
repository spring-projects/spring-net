#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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

using System;
using System.Configuration;
using System.Xml;
using Spring.Context.Support;

#endregion

namespace Spring
{
    /// <summary>
    /// Replace the original context handler with this hookable version for testing ContextRegistry
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class HookableContextHandler : ContextHandler, IConfigurationSectionHandler
    {
        public delegate object CreateContextFromSectionHandler(object parent, object configContext, XmlNode section);

        private static CreateContextFromSectionHandler s_callback;

        public static CreateContextFromSectionHandler callback
        {
            get { return s_callback; }
        }

        public static CreateContextFromSectionHandler SetSectionHandler(CreateContextFromSectionHandler sectionHandler)
        {
            CreateContextFromSectionHandler prevInstance = s_callback;
            s_callback = sectionHandler;
            return prevInstance;
        }

        ///<summary>
        ///Creates a configuration section handler.
        ///</summary>
        ///
        ///<returns>
        ///The created section handler object.
        ///</returns>
        ///
        ///<param name="parent">Parent object.</param>
        ///<param name="section">Section XML node.</param>
        ///<param name="configContext">Configuration context object.</param><filterpriority>2</filterpriority>
        new virtual public object Create(object parent, object configContext, XmlNode section)
        {
            if (s_callback != null)
            {
                return s_callback(parent, configContext, section);
            }

            return base.Create(parent, configContext, section);
        }
    }
}