using System;
using System.Collections.Generic;
using System.Text;
using Spring.Core.IO;
using Spring.Util;

namespace Spring.Objects.Factory.Parsing
{
    public class Location
    {
        private IResource resource;
        private object source;

        /// <summary>
        /// Initializes a new instance of the Location class.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="source"></param>
        public Location(IResource resource, object source)
        {
            //TODO: look into re-enabling this since resource *is* NULL when parsing config classes vs. acquiring IResources
            //AssertUtils.ArgumentNotNull(resource, "resource");
            this.resource = resource;
            this.source = source;
        }

        /// <summary>
        /// Initializes a new instance of the Location class.
        /// </summary>
        /// <param name="resource"></param>
        public Location(IResource resource)
            : this(resource, null)
        {
            
        }
        public IResource Resource
        {
            get
            {
                return resource;
            }
        }
        public object Source
        {
            get
            {
                return source;
            }
        }


    }
}
