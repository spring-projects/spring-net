using System;
using System.Collections.Generic;
using System.Text;

namespace Spring.Context.Annotation
{
    /// <summary>
    /// Indicates that a class declares one or more <see cref="Definition"/> methods and may be processed
    /// by the Spring container to generate object definitions and service requests for those objects
    /// at runtime.
    ///
    /// <para>Configuration is meta-annotated as a {@link Component}, therefore Configuration
    /// classes are candidates for component-scanning and may also take advantage of
    /// <see cref="AutoWired"/> at the field and method but not at the constructor level.
    /// </para>
    /// <para>May be used in conjunction with the <see cref="Lazy"/> attribute to indicate that all object
    /// methods declared within this class are by default lazily initialized.
    ///</para>
    /// <h3>Constraints</h3>
    /// <ul>
    ///    <li>Configuration classes must be non-final</li>
    ///    <li>Configuration classes must be non-local (may not be declared within a method)</li>
    ///    <li>Configuration classes must have a default/no-arg constructor and may not use
    ///        {@link Autowired} constructor parameters</li>
    /// </ul>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigurationAttribute : Attribute
    {
        private string _name;

        /// <summary>
        /// Initializes a new instance of the Configuration class.
        /// </summary>
        /// <param name="name"></param>
        public ConfigurationAttribute(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Explicitly specify the name of the Spring object definition associated
        /// with this Configuration class.  If left unspecified (the common case),
        /// a object name will be automatically generated.
        ///
        /// <para>The custom name applies only if the Configuration class is picked up via
        /// component scanning or supplied directly to a <see cref="AnnotationConfigApplicationContext"/>.
        /// If the Configuration class is registered as a traditional XML object definition,
        /// the name/id of the object element will take precedence.
        /// </para>
        /// <see cref="Spring.Objects.Factory.Support.DefaultObjectNameGenerator"/>
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

    }
}
