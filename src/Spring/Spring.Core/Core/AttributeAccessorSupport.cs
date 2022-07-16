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

namespace Spring.Core
{
    /// <summary>
    /// Support class for <see cref="IAttributeAccessor"/>, providing
    /// a base implementation of all methods. To be extended by subclasses.
    /// </summary>
    [Serializable]
    public abstract class AttributeAccessorSupport : IAttributeAccessor
    {
	    /** Map with String keys and Object values */
	    private readonly IDictionary<string, object> _attributes = new Dictionary<string, object>();


	    public virtual void SetAttribute(string name, object value)
        {
		    Trace.Assert(name != null, "Name must not be null");
		    if (value != null) {
			    _attributes.Add(name, value);
		    }
		    else {
			    RemoveAttribute(name);
		    }
	    }

	    public virtual object GetAttribute(string name)
        {
		    Trace.Assert(name != null, "Name must not be null");
            if (_attributes.ContainsKey(name))
                return _attributes[name];
	        return null;
        }

	    public virtual object RemoveAttribute(string name) {
            Trace.Assert(name != null, "Name must not be null");
            if (_attributes.ContainsKey(name))
                return _attributes.Remove(name);
	        return false;
	    }

	    public bool HasAttribute(string name)
        {
            Trace.Assert(name != null, "Name must not be null");
		    return _attributes.ContainsKey(name);
	    }

	    public String[] AttributeNames
        {
            get
            {
                return _attributes.Keys.ToArray();
            }
        }


	    /**
	     * Copy the attributes from the supplied AttributeAccessor to this accessor.
	     * @param source the AttributeAccessor to copy from
	     */
	    protected void CopyAttributesFrom(IAttributeAccessor source) {
		    Trace.Assert(source != null, "Source must not be null");
		    string[] attributeNames = source.AttributeNames;
		    foreach(string attributeName in attributeNames)
            {
			    SetAttribute(attributeName, source.GetAttribute(attributeName));
		    }
	    }


	    public override bool Equals(object other) {
		    if (this == other) {
			    return true;
		    }
		    if (!(other is AttributeAccessorSupport)) {
			    return false;
		    }
		    var that = (AttributeAccessorSupport) other;
		    return _attributes.Equals(that._attributes);
	    }

	    public override int  GetHashCode()
        {
		    return _attributes.GetHashCode();
	    }
    }
}
