/*
/// Copyright 2002-2010 the original author or authors.
 *
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
 *
///      http://www.apache.org/licenses/LICENSE-2.0
 *
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
 */

using System.Diagnostics;

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Qualifier for resolving autowire candidates. A bean definition that
    /// includes one or more such qualifiers enables fine-grained matching
    /// against annotations on a field or parameter to be autowired.
    /// </summary>
    public class AutowireCandidateQualifier : ObjectMetadataAttributeAccessor
    {
	    public static string VALUE_KEY = "Value";

	    private readonly string _typeName;


	    /// <summary>
	    /// Construct a qualifier to match against an annotation of the
	    /// given type.
        /// </summary>
	    /// <param name="type">type the annotation type</param>
	    public AutowireCandidateQualifier(Type type) : this(type.Name)
        {
	    }

	    /// <summary>
	    /// Construct a qualifier to match against an annotation of the
	    /// given type name.
	    /// <p>The type name may match the fully-qualified class name of
	    /// the annotation or the short class name (without the package).</p>
        /// </summary>
        /// <param name="typeName">the name of the annotation type</param>
	    public AutowireCandidateQualifier(string typeName)
        {
		    Trace.Assert(typeName != null, "Type name must not be null");
		    _typeName = typeName;
	    }

	    /// <summary>
	    /// Construct a qualifier to match against an annotation of the
	    /// given type whose <code>value</code> attribute also matches
	    /// the specified value.
        /// </summary>
        /// <param name="type">the annotation type</param>
        /// <param name="value">the annotation value to match</param>
	    public AutowireCandidateQualifier(Type type, object value) : this(type.Name, value)
        {
	    }

	    /// <summary>
	    /// Construct a qualifier to match against an annotation of the
	    /// given type name whose <code>value</code> attribute also matches
	    /// the specified value.
	    /// <p>The type name may match the fully-qualified class name of
	    /// the annotation or the short class name (without the package).</p>
	    /// </summary>
        /// <param name="typeName">the name of the annotation type</param>
        /// <param name="value">the annotation value to match</param>
	    public AutowireCandidateQualifier(string typeName, object value)
        {
		    Trace.Assert(typeName != null, "Type name must not be null");
		    _typeName = typeName;
		    SetAttribute(VALUE_KEY, value);
	    }


	    /// <summary>
	    /// Retrieve the type name. This value will be the same as the
	    /// type name provided to the constructor or the fully-qualified
	    /// class name if a Class instance was provided to the constructor.
	    /// </summary>
	    public String TypeName
        {
            get { return _typeName; }
        }

    }
}
