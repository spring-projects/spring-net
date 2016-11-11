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

using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// Context that gets passed along an object definition parsing process, encapsulating
    /// all relevant configuraiton as well as state.
    /// </summary>
    public class ParserContext
    {
        private readonly XmlReaderContext readerContext;

        private readonly ObjectDefinitionParserHelper parserHelper;

        private readonly IObjectDefinition containingObjectDefinition;

//        private Stack containingComponents = new Stack();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserContext"/> class.
        /// </summary>
        /// <param name="parserHelper">The parser helper.</param>
        public ParserContext(ObjectDefinitionParserHelper parserHelper)
        {
            this.readerContext = parserHelper.ReaderContext;
            this.parserHelper = parserHelper;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ParserContext"/> class.
        /// </summary>
        /// <param name="parserHelper">The parser helper.</param>
        /// <param name="containingObjectDefinition">The containing object definition.</param>
        public ParserContext(ObjectDefinitionParserHelper parserHelper, IObjectDefinition containingObjectDefinition)
        {
            this.readerContext = parserHelper.ReaderContext;
            this.parserHelper = parserHelper;
            this.containingObjectDefinition = containingObjectDefinition;
        }

        /// <summary>
        /// Gets the reader context.
        /// </summary>
        /// <value>The reader context.</value>
        public XmlReaderContext ReaderContext
        {
            get { return readerContext; }
        }

        /// <summary>
        /// Gets the registry.
        /// </summary>
        /// <value>The registry.</value>
        public IObjectDefinitionRegistry Registry
        {
            get { return readerContext.Registry; }
        }


        /// <summary>
        /// Gets the parser helper.
        /// </summary>
        /// <value>The parser helper.</value>
        public ObjectDefinitionParserHelper ParserHelper
        {
            get { return parserHelper; }
        }


        /// <summary>
        /// Gets the containing object definition.
        /// </summary>
        /// <value>The containing object definition.</value>
        public IObjectDefinition ContainingObjectDefinition
        {
            get { return containingObjectDefinition; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is nested.
        /// </summary>
        /// <value><c>true</c> if this instance is nested; otherwise, <c>false</c>.</value>
        public bool IsNested
        {
            get { return containingObjectDefinition != null; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is default lazy init.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is default lazy init; otherwise, <c>false</c>.
        /// </value>
        public bool IsDefaultLazyInit
        {
            get { return ObjectDefinitionConstants.TrueValue.Equals(ParserHelper.Defaults.LazyInit); }
        }


    }
}
