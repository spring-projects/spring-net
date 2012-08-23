using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spring.Objects.Factory.Attributes
{
    /// <summary>
    /// This annotation may be used on a field or parameter as a qualifier for
    /// candidate beans when autowiring. It may also be used to annotate other
    /// custom annotations that can then in turn be used as qualifiers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class QualifierAttribute : Attribute
    {
        private readonly string _name;

        /// <summary>
        /// Instantiate a new qualifier type
        /// </summary>
        /// <param name="name">name to use as qualifier</param>
        public QualifierAttribute(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Gets the name associated with this qualifier
        /// </summary>
        public string Name { get { return _name; } }    
    }
}
