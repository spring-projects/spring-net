#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using Spring.Objects.Factory.Config;

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// GenericObjectDefinition is a one-stop shop for standard object definition purposes.
    /// Like any object definition, it allows for specifying a class plus optionally
    /// constructor argument values and property values. Additionally, deriving from a
    /// parent bean definition can be flexibly configured through the &quot;parentName&quot; property.
    /// </summary>
    /// <remarks>In general, use this <see cref="GenericObjectDefinition"/> class for the purpose of
    /// registering user-visible object definitions (which a post-processor might operate on,
    /// potentially even reconfiguring the parent name).
    /// Use <see cref="RootObjectDefinition"/>/<see cref="ChildObjectDefinition"/>
    /// where parent/child relationships happen to be pre-determined.
    /// </remarks>
    /// <seealso cref="RootObjectDefinition"/>
    /// <seealso cref="ChildObjectDefinition"/>
    /// <author>Juergen Hoeller</author>
    /// <author>Erich Eichinger</author>
    [Serializable]
    public class GenericObjectDefinition : AbstractObjectDefinition
    {
        private string parentName;

        /// <summary>
        /// The name of the parent object definition.
        /// </summary>
        /// <remarks>
        /// This value is <b>required</b>.
        /// </remarks>
        /// <value>
        /// The name of the parent object definition.
        /// </value>
        public override string ParentName
        {
            get { return parentName; }
            set { parentName = value; }
        }

        /// <summary>
        /// Creates a new <see cref="GenericObjectDefinition"/> to be configured through its
        /// object properties and configuration methods.
        /// </summary>
        public GenericObjectDefinition()
        { }

        /// <summary>
        /// Creates a new <see cref="GenericObjectDefinition"/> as deep copy of the given
        /// object definition.
        /// </summary>
        /// <param name="original">the original object definition to copy from</param>
        public GenericObjectDefinition(IObjectDefinition original)
            : base(original)
        {
            GenericObjectDefinition god = original as GenericObjectDefinition;
            if (god != null)
            {
                this.parentName = god.parentName;
            }
        }

        /* TODO (EE): this is not supported atm, need to implement AbstractObjectDefinition.Equals() first */
//        /// <summary>
//        /// Checks, if <paramref name="other"/> equals this object definition.
//        /// </summary>
//        /// <param name="other"></param>
//        /// <returns></returns>
//        public override bool Equals(object other)
//        {
//            return object.ReferenceEquals(this, other)
//                || (other is GenericObjectDefinition && base.Equals(obj));
//        }
//
//        public override int GetHashCode()
//        {
//            return base.GetHashCode();
//        }

        /// <summary>
        /// Returns a <see cref="System.String"/> representation of this
        /// <see cref="GenericObjectDefinition"/> for debugging purposes.
        /// </summary>
        public override string ToString()
        {
            return "Generic Object:" + base.ToString();
        }
    }
}
