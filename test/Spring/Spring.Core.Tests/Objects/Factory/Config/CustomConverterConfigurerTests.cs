#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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

#region Imports

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using DotNetMock.Dynamic;
using NUnit.Framework;

#endregion

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Unit tests for the CustomConverterConfigurer class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class CustomConverterConfigurerTests
    {
		[Test]
		[ExpectedException(typeof(ObjectInitializationException))]
        public void UseInvalidKeyForConverterMapKey()
		{
        	IDictionary converters = new Hashtable();
			converters.Add(12, typeof(DateTimeConverter));

        	DynamicMock mock = new DynamicMock(typeof(IConfigurableListableObjectFactory));
			IConfigurableListableObjectFactory factory
				= (IConfigurableListableObjectFactory) mock.Object;

			CustomConverterConfigurer config = new CustomConverterConfigurer();
			config.CustomConverters = converters;
			config.PostProcessObjectFactory(factory);
		}

		[Test]
		[ExpectedException(typeof(ObjectInitializationException))]
		public void UseNonTypeConverterForConverterMapValue()
		{
			IDictionary converters = new Hashtable();
			converters.Add( typeof(DateTime), null);

			DynamicMock mock = new DynamicMock(typeof(IConfigurableListableObjectFactory));
			IConfigurableListableObjectFactory factory
				= (IConfigurableListableObjectFactory) mock.Object;

			CustomConverterConfigurer config = new CustomConverterConfigurer();
			config.CustomConverters = converters;
			config.PostProcessObjectFactory(factory);
		}

		[Test]
		[ExpectedException(typeof(ObjectInitializationException))]
		public void UseNonResolvableTypeForConverterMapKey()
		{
			IDictionary converters = new Hashtable();
			// purposely misspelled... :D
			converters.Add("Systemm.Date", typeof(DateTimeConverter));

			DynamicMock mock = new DynamicMock(typeof(IConfigurableListableObjectFactory));
			IConfigurableListableObjectFactory factory
				= (IConfigurableListableObjectFactory) mock.Object;

			CustomConverterConfigurer config = new CustomConverterConfigurer();
			config.CustomConverters = converters;
			config.PostProcessObjectFactory(factory);
		}

		/// <summary>
		/// Just tests that the configurer doesn't blow up and
		/// doesn't register anything ('cos we ain't supplied anything).
		/// </summary>
		[Test]
		public void DontSupplyAnyCustomConverters()
		{
			DynamicMock mock = new DynamicMock(typeof(IConfigurableListableObjectFactory));
			IConfigurableListableObjectFactory factory
				= (IConfigurableListableObjectFactory) mock.Object;
			CustomConverterConfigurer config = new CustomConverterConfigurer();
			config.CustomConverters = null;
			config.PostProcessObjectFactory(factory);
			mock.Verify();
		}

		[Test]
		public void SunnyDayScenario()
		{
			IDictionary converters = new Hashtable();
			converters.Add( typeof(DateTime), new DateTimeConverter());
			converters.Add( typeof(Color), new ColorConverter());

			DynamicMock mock = new DynamicMock(typeof(IConfigurableListableObjectFactory));
			mock.Expect("RegisterCustomConverter");
			mock.Expect("RegisterCustomConverter");
			IConfigurableListableObjectFactory mockFactory
				= (IConfigurableListableObjectFactory) mock.Object;

			CustomConverterConfigurer config = new CustomConverterConfigurer();
			config.CustomConverters = converters;
			config.PostProcessObjectFactory(mockFactory);
			mock.Verify();
		}
    }
}
