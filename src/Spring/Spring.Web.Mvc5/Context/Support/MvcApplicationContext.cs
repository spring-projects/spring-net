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

using Spring.Core.IO;

namespace Spring.Context.Support
{
    /// <summary>
    /// Application Context for ASP.NET MVC Applications
    /// </summary>
    public class MvcApplicationContext : AbstractXmlApplicationContext
    {
        private readonly string[] _configurationLocations;

        private readonly IResource[] _configurationResources;

        /// <summary>
        /// Create a new MvcApplicationContext, loading the definitions
        /// from the given XML resource.
        /// </summary>
        /// <param name="name">The application context name.</param>
        /// <param name="caseSensitive">Flag specifying whether to make this context case sensitive or not.</param>
        /// <param name="configurationLocations">Names of configuration resources.</param>
        public MvcApplicationContext(string name, bool caseSensitive, params string[] configurationLocations)
            : this(new MvcApplicationContextArgs(name, null, configurationLocations, null, caseSensitive))
        {
        }

        /// <summary>
        /// Create a new MvcApplicationContext with the given parent,
        /// loading the definitions from the given XML resources.
        /// </summary>
        /// <param name="name">The application context name.</param>
        /// <param name="caseSensitive">Flag specifying whether to make this context case sensitive or not.</param>
        /// <param name="parentContext">The parent context.</param>
        /// <param name="configurationLocations">Names of configuration resources.</param>
        public MvcApplicationContext(string name, bool caseSensitive, IApplicationContext parentContext,
                                     params string[] configurationLocations)
            : this(new MvcApplicationContextArgs(name, parentContext, configurationLocations, null, caseSensitive))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MvcApplicationContext"/> class.
        /// </summary>
        /// <param name="args">The args.</param>
        public MvcApplicationContext(MvcApplicationContextArgs args)
            : base(args.Name, args.CaseSensitive, args.ParentContext)
        {
            _configurationLocations = args.ConfigurationLocations;
            _configurationResources = args.ConfigurationResources;

            Refresh();

            if (log.IsDebugEnabled)
            {
                log.Debug("created instance " + this);
            }
        }

        /// <summary>
        /// Create a new MvcApplicationContext, loading the definitions
        /// from the given XML resource.
        /// </summary>
        /// <param name="name">The application context name.</param>
        /// <param name="caseSensitive">Flag specifying whether to make this context case sensitive or not.</param>
        /// <param name="configurationLocations">Names of configuration resources.</param>
        /// <param name="configurationResources">Configuration resources.</param>
        public MvcApplicationContext(string name, bool caseSensitive, string[] configurationLocations, IResource[] configurationResources)
            : this(new MvcApplicationContextArgs(name, null, configurationLocations, configurationResources, caseSensitive))
        {
        }

        /// <summary>
        /// Create a new MvcApplicationContext, loading the definitions
        /// from the given XML resource.
        /// </summary>
        /// <param name="configurationLocations">Names of configuration resources.</param>
        public MvcApplicationContext(params string[] configurationLocations)
            : this(new MvcApplicationContextArgs(string.Empty, null, configurationLocations, null, false))
        {
        }

        /// <summary>
        /// An array of resource locations, referring to the XML object
        /// definition files that this context is to be built with.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// 	<p>
        /// Examples of the format of the various strings that would be
        /// returned by accessing this property can be found in the overview
        /// documentation of with the <see cref="XmlApplicationContext"/>
        /// class.
        /// </p>
        /// </remarks>
        /// <returns>
        /// An array of resource locations, or <see langword="null"/> if none.
        /// </returns>
        protected override string[] ConfigurationLocations
        {
            get { return _configurationLocations; }
        }

        /// <summary>
        /// An array of resources that this context is to be built with.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// 	<p>
        /// Examples of the format of the various strings that would be
        /// returned by accessing this property can be found in the overview
        /// documentation of with the <see cref="XmlApplicationContext"/>
        /// class.
        /// </p>
        /// </remarks>
        /// <returns>
        /// An array of <see cref="Spring.Core.IO.IResource"/>s, or <see langword="null"/> if none.
        /// </returns>
        protected override IResource[] ConfigurationResources
        {
            get { return _configurationResources; }
        }

    }
}
