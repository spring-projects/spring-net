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

using Spring.Objects.Factory.Xml;

namespace Spring.Context.Config
{
    /// <summary>
    /// <code>NamespaceParser</code> allowing for the configuration of
    /// declarative transaction management using either XML or using attributes.
    /// This namespace handler is the central piece of functionality in the
    /// Spring transaction management facilities and offers two appraoches
    /// to declaratively manage transactions.
    /// One approach uses transaction semantics defined in XML using the
    /// <code>&lt;tx:advice&gt;</code> elements, the other uses attributes
    /// in combination with the <code>&lt;tx:annotation-driven&gt;</code> element.
    /// Both approached are detailed in the Spring reference manual.
    /// </summary>
    [
        NamespaceParser(
            Namespace = "http://www.springframework.net/context",
            SchemaLocationAssemblyHint = typeof(ContextNamespaceParser),
            SchemaLocation = "/Spring.Context.Config/spring-context-2.0.xsd"
        )
    ]
    public class ContextNamespaceParser : NamespaceParserSupport
    {
        /// <summary>
        /// Register the <see cref="IObjectDefinitionParser"/> for the '<code>advice</code>' and
        /// '<code>attribute-driven'</code> tags.
        /// </summary>
        public override void Init()
        {
            RegisterObjectDefinitionParser("attribute-config", new AttributeConfigObjectDefinitionParser());
            RegisterObjectDefinitionParser("component-scan", new ComponentScanObjectDefinitionParser());
        }
    }
}
