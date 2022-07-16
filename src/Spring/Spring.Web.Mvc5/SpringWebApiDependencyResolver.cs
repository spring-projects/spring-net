using System.Web.Http.Dependencies;

using Spring.Context;
using Spring.Context.Support;
using Spring.Core.IO;


namespace Spring.Web.Mvc
{
    /// <summary>
    /// Spring-based implementation of the <see cref="System.Web.Http.Dependencies.IDependencyResolver"/> interface for ASP.NET MVC Web API.
    /// </summary>
    public class SpringWebApiDependencyResolver : SpringMvcDependencyResolver, System.Web.Http.Dependencies.IDependencyResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpringMvcDependencyResolver"/> class.
        /// </summary>
        /// <param name="context">The <see cref="IApplicationContext"/> to be used by the resolver</param>
        public SpringWebApiDependencyResolver(IApplicationContext context)
            : base(context)
        {
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            //no unmanaged resources to dispose
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>The initialized <see cref="IDependencyScope"/> instance.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual IDependencyScope BeginScope()
        {
            if (HasApplicationContext && (HasChildApplicationContextConfigurationLocations || HasChildApplicationContextConfigurationResources))
            {
                string[] configurationLocations = null;
                if (HasChildApplicationContextConfigurationLocations)
                {
                    configurationLocations = ChildApplicationContextConfigurationLocations.ToArray();
                }

                IResource[] configurationResources = null;
                if (HasChildApplicationContextConfigurationResources)
                {
                    configurationResources = ChildApplicationContextConfigurationResources.ToArray();
                }

                var childContextName = string.Format("child_of_{0}", ApplicationContext.Name);
                var args = new MvcApplicationContextArgs(childContextName, ApplicationContext, configurationLocations, configurationResources, false);

                var childContext = new MvcApplicationContext(args);
                var newResolver = new SpringWebApiDependencyResolver(childContext) { ApplicationContextName = childContextName };

                RegisterContextIfNeeded(childContext);

                return newResolver;
            }
            else
            {
                return this;
            }
        }

        private void RegisterContextIfNeeded(IApplicationContext childContext)
        {
            if (!ContextRegistry.IsContextRegistered(childContext.Name))
            {
                ContextRegistry.RegisterContext(childContext);
            }
        }

        private bool HasChildApplicationContextConfigurationResources
        {
            get
            {
                return ChildApplicationContextConfigurationResources != null &&
                       ChildApplicationContextConfigurationResources.Count > 0;
            }
        }

        private bool HasChildApplicationContextConfigurationLocations
        {
            get
            {
                return ChildApplicationContextConfigurationLocations != null &&
                       ChildApplicationContextConfigurationLocations.Count > 0;
            }
        }

        private bool HasApplicationContext
        {
            get { return ApplicationContext != null; }
        }

        /// <summary>
        /// Gets or sets the child configuration locations.
        /// </summary>
        /// <value>The child configuration locations.</value>
        public virtual IList<string> ChildApplicationContextConfigurationLocations { protected get; set; }

        /// <summary>
        /// Gets or sets the child configuration resources.
        /// </summary>
        /// <value>The child configuration resources.</value>
        public virtual IList<IResource> ChildApplicationContextConfigurationResources { protected get; set; }

        /// <summary>
        /// Adds the child configuration resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        public virtual void AddChildApplicationContextConfigurationResource(IResource resource)
        {
            if (null == ChildApplicationContextConfigurationResources)
            {
                ChildApplicationContextConfigurationResources = new List<IResource>();
            }

            ChildApplicationContextConfigurationResources.Add(resource);
        }

        /// <summary>
        /// Adds the child configuration location.
        /// </summary>
        /// <param name="location">The location.</param>
        public virtual void AddChildApplicationContextConfigurationLocation(string location)
        {
            if (null == ChildApplicationContextConfigurationLocations)
            {
                ChildApplicationContextConfigurationLocations = new List<string>();
            }

            ChildApplicationContextConfigurationLocations.Add(location);
        }
    }
}
