using System;
using System.Collections.Generic;
using System.Text;
using Spring.Objects.Factory.Support;

namespace Spring.Context.Annotation
{
    /// <summary>
    /// When used as a type-level attribute, indicates the name of a scope to use
    /// for instances of the attributed type.
    /// 
    /// <para>When used as a method-level attribute in conjunction with the
    /// <see cref="Definition"/> attribute, indicates the name of a scope to use for
    /// the instance returned from the method.
    /// </para>
    /// <para>In this context, scope means the lifecycle of an instance, such as
    /// <code>singleton</code>, <code>prototype</code>, and so forth.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ScopeAttribute : Attribute
    {
        private ObjectScope _scope = ObjectScope.Singleton;

        /// <summary>
        /// Initializes a new instance of the Scope class.
        /// </summary>
        /// <param name="scope"></param>
        public ScopeAttribute(ObjectScope scope)
        {
            _scope = scope;
        }

        /// <summary>
        /// Specifies the scope to use for the annotated object.
        /// </summary>
        /// <value>The scope.</value>
        public ObjectScope ObjectScope
        {
            get { return _scope; }
            set
            {
                _scope = value;
            }
        }

    }
}
