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
#region Imports

using System.IO;
using Spring.Core.IO;
#endregion
namespace Spring.Objects
{
    /// <summary>
    /// Object for testing IResource instances.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class ResourceTestObject
    {
        private IResource resource;
        private Stream inputStream;
        public ResourceTestObject()
        {
        }
        public ResourceTestObject(IResource resource)
        {
            this.resource = resource;
        }
        public ResourceTestObject(IResource resource, Stream inputStream)
        {
            this.resource = resource;
            this.inputStream = inputStream;
        }
        public IResource Resource
        {
            get { return this.resource; }
            set { this.resource = value; }
        }
        public Stream InputStream
        {
            get { return this.inputStream; }
            set { this.inputStream = value; }
        }
    }
}