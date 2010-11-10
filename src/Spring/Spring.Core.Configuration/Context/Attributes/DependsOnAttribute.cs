using System;
using System.Collections.Generic;
using System.Text;

namespace Spring.Context.Attributes
{
    /// <summary>
    /// objects on which the current object depends. Any objects specified are guaranteed to be
    /// created by the container before this object. Used infrequently in cases where a object
    /// does not explicitly depend on another through properties or constructor arguments,
    /// but rather depends on the side effects of another object's initialization.
    /// <para>Note: This attribute will not be inherited by child object definitions,
    /// hence it needs to be specified per concrete object definition.
    /// </para>
    /// <para>Using <see cref="DependsOn"/> at the class level has no effect unless component-scanning
    /// is being used. If a <see cref="DependsOn"/>-attributed class is declared via XML,
    /// <see cref="DependsOn"/> attribute metadata is ignored, and
    /// &lt;object depends-on="..."/&gt; is respected instead.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class DependsOnAttribute : Attribute
    {
        private string[] _name;

        /// <summary>
        /// Initializes a new instance of the DependsOn class.
        /// </summary>
        /// <param name="name"></param>
        public DependsOnAttribute(string[] name)
        {
            _name = name;
        }

        public string[] Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

    }
}
