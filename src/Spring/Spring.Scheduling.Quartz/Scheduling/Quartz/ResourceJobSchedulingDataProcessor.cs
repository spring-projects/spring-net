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
using System.IO;

using Quartz.Xml;

using Spring.Context;
using Spring.Core.IO;

namespace Spring.Scheduling.Quartz
{
	/// <summary> 
	/// Subclass of Quartz' JobSchedulingDataProcessor that considers
	/// given filenames as Spring resource locations.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <seealso cref="IResourceLoader" /> 
	public class ResourceJobSchedulingDataProcessor : JobSchedulingDataProcessor, IResourceLoaderAware
	{
		private IResourceLoader resourceLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceJobSchedulingDataProcessor"/> class.
        /// </summary>
		public ResourceJobSchedulingDataProcessor()
		{
			resourceLoader = new ConfigurableResourceLoader();
		}

        /// <summary>
        /// Sets the <see cref="Spring.Core.IO.IResourceLoader"/>
        /// that this object runs in.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Invoked <b>after</b> population of normal objects properties but
        /// before an init callback such as
        /// <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
        /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet()"/>
        /// or a custom init-method. Invoked <b>before</b> setting
        /// <see cref="Spring.Context.IApplicationContextAware"/>'s
        /// <see cref="Spring.Context.IApplicationContextAware.ApplicationContext"/>
        /// property.
        /// </remarks>
		public virtual IResourceLoader ResourceLoader
		{
			set { resourceLoader = (value != null ? value : new ConfigurableResourceLoader()); }
		}

        /// <summary>
        /// Returns an <see cref="T:System.IO.Stream"/> from the fileName as a resource.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        /// an <see cref="T:System.IO.Stream"/> from the fileName as a resource.
        /// </returns>
		protected override Stream GetInputStream(string fileName)
		{
			try
			{
				return resourceLoader.GetResource(fileName).InputStream;
			}
			catch (IOException ex)
			{
				throw new SchedulingException("Could not load job scheduling data XML file", ex);
			}
		}
	}
}