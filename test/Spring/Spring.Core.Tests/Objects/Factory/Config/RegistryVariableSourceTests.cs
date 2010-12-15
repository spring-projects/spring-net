#region License

/*
 * Copyright © 2002-2010 the original author or authors.
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
using Microsoft.Win32;
using NUnit.Framework;

#endregion

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Unit tests for the RegistryVariableSource class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [TestFixture]
    public sealed class RegistryVariableSourceTests
    {
	    private RegistryKey key;

        [SetUp]
        public void SetUp()
        {
            key = Registry.CurrentUser.CreateSubKey("RegistryVariableSourceTests");
            key.SetValue("name", "Aleks Seovic");
#if NET_2_0 
            key.SetValue("computer_name", "%COMPUTERNAME% is the name of my computer", RegistryValueKind.ExpandString);
            key.SetValue("age", 32, RegistryValueKind.DWord);
#endif            
			key.SetValue("family", new string[] {"Marija", "Ana", "Nadja"});
            key.SetValue("bday", new byte[] {24, 8, 74});
			key.Flush();
        }

        [TearDown]
        public void TearDown()
        {
            Registry.CurrentUser.DeleteSubKey("RegistryVariableSourceTests");
        }

        [Test]
        public void TestVariablesResolution()
        {
            RegistryVariableSource rvs = new RegistryVariableSource();
            rvs.Key = key;

            // existing vars
            Assert.AreEqual("Aleks Seovic", rvs.ResolveVariable("name"));
#if NET_2_0 
            Assert.AreEqual(Environment.GetEnvironmentVariable("COMPUTERNAME") + " is the name of my computer",
                            rvs.ResolveVariable("computer_name"));
            Assert.AreEqual("32", rvs.ResolveVariable("age"));
#endif            
			// multi_sz
			Assert.AreEqual( "Marija,Ana,Nadja", rvs.ResolveVariable("family"));
			// binary
            Assert.AreEqual( null, rvs.ResolveVariable("bday"));

            // non-existant variable
            Assert.IsNull(rvs.ResolveVariable("xyz"));
        }
    }
}
