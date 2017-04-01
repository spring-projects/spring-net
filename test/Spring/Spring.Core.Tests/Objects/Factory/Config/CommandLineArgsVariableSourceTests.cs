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

#region Imports

using NUnit.Framework;

#endregion

namespace Spring.Objects.Factory.Config
{
	/// <summary>
    /// Unit tests for the CommandLineArgsVariableSource class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [TestFixture]
    public sealed class CommandLineArgsVariableSourceTests
    {
        [Test]
        public void TestVariablesResolution()
        {
            CommandLineArgsVariableSource vs = new CommandLineArgsVariableSource(
                new string[] {"program.exe", "file.txt", "/name:Aleks Seovic", "/framework:Spring.NET"});

            // existing vars
            Assert.AreEqual("Spring.NET", vs.ResolveVariable("FRAMEWORK"));
            Assert.AreEqual("Spring.NET", vs.ResolveVariable("framework"));
            Assert.AreEqual("Aleks Seovic", vs.ResolveVariable("name"));
            Assert.AreEqual("Aleks Seovic", vs.ResolveVariable("NAME"));

            // non-existant variable
            Assert.IsNull(vs.ResolveVariable("dummy"));
        }

        [Test]
        public void TestVariablesResolutionWithCustomPrefixAndSeparator()
        {
            CommandLineArgsVariableSource vs = new CommandLineArgsVariableSource(
                new string[] { "program.exe", "file.txt", "--Name=Aleks Seovic", "--Framework=Spring.NET" });
            vs.ArgumentPrefix = "--";
            vs.ValueSeparator = "=";

            // existing vars
            Assert.AreEqual("Spring.NET", vs.ResolveVariable("FRAMEWORK"));
            Assert.AreEqual("Spring.NET", vs.ResolveVariable("framework"));
            Assert.AreEqual("Aleks Seovic", vs.ResolveVariable("name"));
            Assert.AreEqual("Aleks Seovic", vs.ResolveVariable("NAME"));

            // non-existant variable
            Assert.IsNull(vs.ResolveVariable("dummy"));
        }

        [Test]
        [Explicit]
        public void TestLiveVariablesResolutionWithTestDriven()
        {
            CommandLineArgsVariableSource vs = new CommandLineArgsVariableSource();
            Assert.IsTrue(vs.ResolveVariable("AssemblyName").StartsWith("TestDriven.TestRunner.Server"));
        }
    }
}
