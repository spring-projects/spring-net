#region License

/*
 * Copyright © 2010-2011 the original author or authors.
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

using Spring.Objects.Factory.Support;

namespace Spring.Context.Attributes
{
    /// <summary>
    /// When used as a type-level attribute, indicates the name of a scope to use
    /// for instances of the attributed type.
    /// 
    /// <para>When used as a method-level attribute in conjunction with the
    /// <see cref="ObjectDefAttribute"/> attribute, indicates the name of a scope to use for
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
