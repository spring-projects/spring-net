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

namespace Spring.Messaging.Nms.Config
{
    /// <summary>
    /// Namespace parser for the nms namespace.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    [
        NamespaceParser(
            Namespace = "http://www.springframework.net/nms",
            SchemaLocationAssemblyHint = typeof (NmsNamespaceParser),
            SchemaLocation = "/Spring.Messaging.Nms.Config/spring-nms-1.3.xsd"
            )
    ]
    public class NmsNamespaceParser : NamespaceParserSupport
    {
        /// <summary>
        /// Register a MessageListenerContainer for the '<code>listener-container</code>' tag.
        /// </summary>
        public override void Init()
        {
            RegisterObjectDefinitionParser("listener-container", new MessageListenerContainerObjectDefinitionParser());
        }
    }
}