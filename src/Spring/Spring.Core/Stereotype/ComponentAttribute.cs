

namespace Spring.Stereotype {

    /// <summary>
    /// Indicates that an annotated class is a "component".
    /// Such classes are considered as candidates for future features such
    /// as auto-detection  when using attribute-based configuration and assembly scanning.
    /// </summary>
    /// <remarks>Other class-level annotations may be considered as identifying
    /// a component as well, typically a special kind of component:
    /// e.g. the Repository attribute.
    /// </remarks>
    /// <author>Mark Fisher</author>
    /// <author>Mark Pollack (.NET)</author>
    /// <seealso cref="RepositoryAttribute"/>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true)]
    [Serializable]
    public class ComponentAttribute : Attribute
    {
        private string name = "";


        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentAttribute"/> class.
        /// </summary>
        public ComponentAttribute()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        public ComponentAttribute(string name)
        {
            this.name = name;
        }


        /// <summary>
        /// Gets or sets the name of the component
        /// </summary>
        /// <value>The name of the component.</value>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
