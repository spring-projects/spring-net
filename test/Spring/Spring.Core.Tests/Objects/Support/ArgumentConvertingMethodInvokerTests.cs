#region License

/*
 * Copyright 2004 the original author or authors.
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

using NUnit.Framework;

#endregion

namespace Spring.Objects.Support
{
	/// <summary>
	/// Unit tests for the ArgumentConvertingMethodInvoker class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class ArgumentConvertingMethodInvokerTests
    {
        [Test]
        public void Invoke () {
            ArgumentConvertingMethodInvoker vkr = new ArgumentConvertingMethodInvoker ();
            vkr.TargetObject = new Voker ();
            vkr.TargetMethod = "Hi";
            vkr.Arguments = new object [] {"Rick, Mark, Griffin, Si, Choy, Aleks"};
            vkr.Prepare ();
            string actual = vkr.Invoke () as string;
            Assert.IsNotNull (actual);
            Assert.AreEqual ("Hi Rick, Mark, Griffin, Si, Choy, Aleks", actual);
        }

        [Test]
        public void InvokeWithConversion () 
        {
            ArgumentConvertingMethodInvoker vkr = new ArgumentConvertingMethodInvoker ();
            Voker instance = new Voker ();
            vkr.TargetObject = instance;
            vkr.TargetMethod = "HiEverybody";
            // CSV string should be converted to string []
            vkr.Arguments = new object [] {"Rick, Mark, Griffin, Federico, Choy, Aleks"};
            vkr.Prepare ();
            string actual = vkr.Invoke () as string;
            Assert.IsNotNull (actual);
            Assert.AreEqual ("Hi ya'll", actual);
            Assert.IsNotNull (instance.developers);
		}

		[Test]
		public void InvokeAllNamedArgumentsWithConversion () 
		{
			ArgumentConvertingMethodInvoker vkr = new ArgumentConvertingMethodInvoker ();
			Voker instance = new Voker ();
			vkr.TargetObject = instance;
			vkr.TargetMethod = "HiEverybody";
			// CSV string should be converted to string []
			vkr.AddNamedArgument("nameS", new object [] {"Rick, Mark, Griffin, Federico, Choy, Aleks"});
			vkr.Prepare ();
			string actual = vkr.Invoke () as string;
			Assert.IsNotNull (actual);
			Assert.AreEqual ("Hi ya'll", actual);
			Assert.IsNotNull (instance.developers);
		}

        [Test]
        public void InvokeWithRegisteredConversion () 
        {
            ArgumentConvertingMethodInvoker vkr = new ArgumentConvertingMethodInvoker ();
            // see if custom registration filters thru...
            vkr.RegisterCustomConverter (typeof (Voker), new VokerConverter ());
            vkr.TargetType = typeof (Voker);
            vkr.TargetMethod = "HiVoker";
            // arg should be converted to Voker
            vkr.Arguments = new object [] {"Lebowski"};
            vkr.Prepare ();
            string actual = vkr.Invoke () as string;
            Assert.IsNotNull (actual);
            Assert.AreEqual ("Hi Lebowski", actual);
        }
    }

    public sealed class Voker 
    {
        public Voker () : this ("HiroProtagonist") {}

        public Voker (string name) 
        {
            this.name = name;
        }

        public string Hi (string name)
        {
            return "Hi " + name;
        }

        public string HiEverybody (string [] names)
        {
            developers = names;
            return "Hi ya'll";
        }

        public static string HiVoker (Voker buddy) 
        {
            return "Hi " + buddy.name;
        }

        public string [] developers;
        public string name;
    }

    public sealed class VokerConverter : System.ComponentModel.TypeConverter 
    {
        public override bool CanConvertFrom (
            System.ComponentModel.ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof (string)) 
            {
                return true;
            }
            return base.CanConvertFrom (context, sourceType);
        }

        public override object ConvertFrom (
            System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string) 
            {
                return new Voker (value as string);
            }
            return base.ConvertFrom (context, culture, value);
        }
    }
}
