#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using Spring.Objects.Factory.Xml;

namespace Spring.Aop.Config
{
    /// <summary>
    /// Namespace parser for the aop namespace.
    /// </summary>
    /// <remarks>
    /// Using the <code>advisor</code> tag you can configure an <see cref="IAdvisor"/> and have it
    /// applied to all the relevant objects in your application context automatically.  The
    /// <code>advisor</code> tag supports only referenced <see cref="IPointcut"/>s.
    /// </remarks>
    /// <author>Rob harrop</author>
    /// <author>Adrian Colyer</author>
    /// <author>Rod Johnson</author>
    /// <author>Mark Pollack (.NET)</author>
    [
        NamespaceParser(
            Namespace = "http://www.springframework.net/aop",
            SchemaLocationAssemblyHint = typeof (AopNamespaceParser),
            SchemaLocation = "/Spring.Aop.Config/spring-aop-1.1.xsd"
            )
    ]
    public class AopNamespaceParser : NamespaceParserSupport
    {
        /// <summary>
        /// Register the <see cref="IObjectDefinitionParser"/> for the '<code>config</code>' tag.
        /// </summary>
        public override void Init()
        {
            RegisterObjectDefinitionParser("config", new ConfigObjectDefinitionParser());
        }
    }
}