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

using System.Diagnostics;
using Spring.Util;

namespace Spring.Objects
{
    /// <summary>
    /// Holder for a key-value style attribute that is part of a bean definition.
    /// Keeps track of the definition source in addition to the key-value pair.
    /// </summary>
    public class ObjectMetadataAttribute : IObjectMetadataElement
    {
	    private readonly string _name;

	    private readonly object _value;

	    private object _source;


	    /// <summary>
	    /// Create a new AttributeValue instance.
        /// </summary>
	    /// <param name="name">the name of the attribute (never <code>null</code>)</param>
	    /// <param name="value">the value of the attribute (possibly before type conversion)</param>
	    public ObjectMetadataAttribute(string name, object value)
        {
		    Trace.Assert(name != null, "Name must not be null");
		    _name = name;
		    _value = value;
	    }


	    /// <summary>
	    /// Return the name of the attribute.
	    /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Return the value of the attribute.
        /// </summary>
        public object Value { get { return _value; } }

	    /// <summary>
	    /// Set the configuration source <code>Object</code> for this metadata element.
	    /// <p>The exact type of the object will depend on the configuration mechanism used.</p>
	    /// </summary>
        public object Source { get { return _source; } set { _source = value; } }


	    public override bool Equals(Object other)
        {
		    if (this == other) {
			    return true;
		    }
		    if (!(other is ObjectMetadataAttribute)) {
			    return false;
		    }
		    var otherMa = (ObjectMetadataAttribute) other;
		    return (_name.Equals(otherMa._name) &&
				    ObjectUtils.NullSafeEquals(_value, otherMa._value) &&
				    ObjectUtils.NullSafeEquals(_source, otherMa._source));
	    }


	    public override int GetHashCode()
        {
		    return _name.GetHashCode() * 29 + ObjectUtils.NullSafeHashCode(_value);
	    }

	    public override string ToString()
        {
		    return "metadata attribute '" + _name + "'";
	    }
    }
}
