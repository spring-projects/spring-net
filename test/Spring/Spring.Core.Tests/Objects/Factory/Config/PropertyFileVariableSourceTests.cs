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

#region

using NUnit.Framework;
using Spring.Core.IO;

#endregion

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Unit tests for the PropertyFileVariableSource class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [TestFixture]
    public sealed class PropertyFileVariableSourceTests
    {
        [Test]
        public void TestVariablesResolutionWithSingleLocation()
        {
            PropertyFileVariableSource vs = new PropertyFileVariableSource();
            vs.Location =
                new AssemblyResource(
                    "assembly://Spring.Core.Tests/Spring.Data.Spring.Objects.Factory.Config/one.properties");

            // existing vars
            Assert.AreEqual("Aleks Seovic", vs.ResolveVariable("name"));
            Assert.AreEqual("32", vs.ResolveVariable("age"));

            // non-existant variable
            Assert.IsNull(vs.ResolveVariable("dummy"));
        }

        [Test]
        public void TestMissingResourceLocation()
        {
            PropertyFileVariableSource vs = new PropertyFileVariableSource();
            vs.IgnoreMissingResources = true;
            vs.Locations = new IResource[]
                               {
                                   new AssemblyResource(
                                       "assembly://Spring.Core.Tests/Spring.Data.Spring.Objects.Factory.Config/non-existent.properties")
                                   ,
                                   new AssemblyResource(
                                       "assembly://Spring.Core.Tests/Spring.Data.Spring.Objects.Factory.Config/one.properties")
                                   ,
                               };

            // existing vars
            Assert.AreEqual("Aleks Seovic", vs.ResolveVariable("name"));
            Assert.AreEqual("32", vs.ResolveVariable("age"));
        }


        [Test]
        public void TestVariablesResolutionWithTwoLocations()
        {
            PropertyFileVariableSource vs = new PropertyFileVariableSource();
            vs.Locations = new IResource[]
                               {
                                   new AssemblyResource(
                                       "assembly://Spring.Core.Tests/Spring.Data.Spring.Objects.Factory.Config/one.properties")
                                   ,
                                   new AssemblyResource(
                                       "assembly://Spring.Core.Tests/Spring.Data.Spring.Objects.Factory.Config/two.properties")
                               };

            // existing vars
            Assert.AreEqual("Aleksandar Seovic", vs.ResolveVariable("name")); // should be overriden by the second file
            Assert.AreEqual("32", vs.ResolveVariable("age"));
            Assert.AreEqual("Marija,Ana,Nadja", vs.ResolveVariable("family"));

            // non-existant variable
            Assert.IsNull(vs.ResolveVariable("dummy"));
        }
    }
}