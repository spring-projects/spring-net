#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

using Spring.Core;

namespace Spring.Objects
{
    /// <summary>
    /// Extension of <see cref="AttributeAccessorSupport"/>,
    /// holding attributes as <see cref="IObjectMetadataElement"/> objects in order
    /// to keep track of the definition source.
    /// </summary>
    [Serializable]
    public class ObjectMetadataAttributeAccessor : AttributeAccessorSupport, IObjectMetadataElement
    {
	    private object _source;

	    /// <summary>
	    /// Set the configuration source <code>object</code> for this metadata element.
	    /// <p>The exact type of the object will depend on the configuration mechanism used.</p>
	    /// </summary>
	    public object Source
        {
            get { return _source; }
            set { _source = value; }
        }

	    /// <summary>
	    /// Add the given BeanMetadataAttribute to this accessor's set of attributes.
	    /// </summary>
	    /// <param name="attribute">The BeanMetadataAttribute object to register</param>
	    public void AddMetadataAttribute(ObjectMetadataAttribute attribute)
        {
		    base.SetAttribute(attribute.Name, attribute);
	    }

	    /// <summary>
	    /// Look up the given BeanMetadataAttribute in this accessor's set of attributes.
        /// </summary>
        /// <param name="name">the name of the attribute</param>
	    /// <returns>the corresponding BeanMetadataAttribute object,
	    /// or <code>null</code> if no such attribute defined
        /// </returns>
	    public ObjectMetadataAttribute GetMetadataAttribute(string name)
	    {
	        return (ObjectMetadataAttribute) base.GetAttribute(name);
	    }

        public override void SetAttribute(string name, object value)
        {
            base.SetAttribute(name, new ObjectMetadataAttribute(name, value));
        }

	    public override object GetAttribute(string name)
	    {
	        var attribute = (ObjectMetadataAttribute) base.GetAttribute(name);
		    return (attribute != null ? attribute.Value : null);
	    }

	    public override object RemoveAttribute(string name)
	    {
	        var attribute = (ObjectMetadataAttribute) base.RemoveAttribute(name);
		    return (attribute != null ? attribute.Value : null);
	    }

    }
}
