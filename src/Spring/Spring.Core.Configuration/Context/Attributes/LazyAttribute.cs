using System;
using System.Collections.Generic;
using System.Text;

namespace Spring.Context.Attributes
{
    /// <summary>
    /// Indicates whether a object is to be lazily initialized.
    /// 
    /// <para>If this annotation is not present on a Component or object definition, eager
    /// initialization will occur. If present and set to true, the
    /// object/Component will not be initialized until referenced by another object or
    /// explicitly retrieved from the enclosing <see cref="Spring.Objects.Factory.IObjectFactory"/>.
    /// If present and set to false, the object will be instantiated on startup by object factories
    /// that perform eager initialization of singletons.
    /// </para>
    /// <para>
    /// If Lazy is present on a <see cref="Configuration"/> class, this indicates that all
    /// <see cref="Definition"/> methods within that <see cref="Configuration"/> should be lazily
    /// initialized. If Lazy is present and false on a object method within a
    /// Lazy-annotated Configuration class, this indicates overriding the 'default
    /// lazy' behavior and that the object should be eagerly initialized.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class LazyAttribute : Attribute
    {
        private bool _lazyInitialize = true;

        /// <summary>
        /// Initializes a new instance of the LazyAttribute class.
        /// </summary>
        /// <param name="lazyInitialize"></param>
        public LazyAttribute(bool lazyInitialize)
        {
            _lazyInitialize = lazyInitialize;
        }

        /// <summary>
        /// Initializes a new instance of the LazyAttribute class.
        /// </summary>
        public LazyAttribute()
        {
            
        }

        /// <summary>
        /// Whether lazy initialization should occur.
        /// </summary>
        /// <value><c>true</c> if [lazy initialize]; otherwise, <c>false</c>.</value>
        public bool LazyInitialize
        {
            get { return _lazyInitialize; }
            set
            {
                _lazyInitialize = value;
            }
        }

    }
}
