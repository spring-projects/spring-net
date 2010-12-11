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

using Spring.Core.IO;

namespace Spring.Objects.Factory.Parsing
{
    /// <summary>
    /// Context that gets passed along an object definition reading process,
    /// encapsulating all relevant configuraiton as well as state.
    /// </summary>
    /// <author>Rob Harrop</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class ReaderContext
    {
        private IResource resource;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderContext"/> class.
        /// </summary>
        /// <param name="resource">The resource.</param>
        public ReaderContext(IResource resource)
        {
            this.resource = resource;
        }


        /// <summary>
        /// Gets the resource.
        /// </summary>
        /// <value>The resource.</value>
        public IResource Resource
        {
            get { return resource; }
        }
    }
}
