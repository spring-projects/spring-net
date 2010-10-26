using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spring.Context.Support;
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
                log.Debug("created instance " + this.ToString());
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

        protected override string[] ConfigurationLocations
        {
            get { return _configurationLocations; }
        }

        protected override IResource[] ConfigurationResources
        {
            get { return _configurationResources; }
        }

    }
}
