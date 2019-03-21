#region License

/*
* Copyright 2002-2010 the original author or authors.
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* 
*      https://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

#endregion

#region Imports

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

#endregion

namespace Spring.Objects
{
	/// <summary>
	/// Simple nested test object used for testing object factories, AOP framework etc.
	/// </summary>
	/// <author>Trevor D. Cook</author>
	/// <author>Mark Pollack (.NET)</author>
	public class NestedTestObject : INestedTestObject
	{
		internal string company = string.Empty;
		private IDictionary _partners = new Hashtable();

		public NestedTestObject()
		{
		}

		public NestedTestObject(string comp)
		{
			Company = comp;
		}

		public NestedTestObject(string comp, IDictionary partners)
		{
			Company = comp;
			Partners = partners;
		}

		public ITestObject this[string partnersSurname]
		{
			get { return Partners[partnersSurname] as ITestObject; }
			set { Partners[partnersSurname] = value; }
		}

		public string Company
		{
			get { return this.company; }
			set { this.company = value; }
		}

		/// <summary>
		/// Maps names (string) to ITestObjects...
		/// </summary>
		public IDictionary Partners
		{
			get { return _partners; }
			set { _partners = value; }
		}
	}

	#region Inner Class : NestedTestObjectConverter

	public class NestedTestObjectConverter : TypeConverter
	{
		public override bool CanConvertTo(
			ITypeDescriptorContext context, Type destinationType)
		{
			if ((destinationType == typeof (NestedTestObjectConverter))
				|| (destinationType == typeof (InstanceDescriptor)))
			{
				return true;
			}
			return base.CanConvertTo(context, destinationType);
		}

		public override bool CanConvertFrom
			(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string))
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom
			(ITypeDescriptorContext context, CultureInfo culture, object text_obj)
		{
			if (text_obj is string)
			{
				string text = (string) text_obj;
				NestedTestObject tb = new NestedTestObject(text);
				return tb;
			}
			return base.ConvertFrom(context, culture, text_obj);
		}

		public override object ConvertTo(
			ITypeDescriptorContext context, CultureInfo culture, object param, Type destinationType)
		{
			return base.ConvertTo(context, culture, param, destinationType);
		}
	}

	#endregion
}