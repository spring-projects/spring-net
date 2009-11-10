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
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;

#endregion

namespace Spring
{
    /// <summary>
    /// Tests the various exception classes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Shamelessly lifted from the NAnt test suite.
    /// </para>
    /// </remarks>
    public abstract class CompilerOptionsTests
    {
        #region Tests

        [Test]
        public void TestBuildCompliance()
        {
            ProcessAssembly(AssemblyToCheck);
        }

        #endregion

        private void ProcessAssembly(Assembly assembly)
        {
            object[] attributes = assembly.GetCustomAttributes(typeof (DebuggableAttribute), false);

#if DEBUG && TRACE
            if (attributes.Length == 0)
            {
                Assert.Fail("No DebugAttributes found in debug build");
            }
            ProcessDebugBuild(attributes);
#endif
#if TRACE && !DEBUG
            if (attributes.Length == 0)
            {
                // This was build with VS.NET Release mode and no DebugAttributes were added.  
            }
            else
            {
                // Command line build still adds some debug attributes...
                ProcessReleaseBuild(attributes);
            }
#endif
        }

        private void ProcessReleaseBuild(object[] attributes)
        {
            foreach (Attribute attribute in attributes)
            {
                if (attribute is DebuggableAttribute)
                {
                    DebuggableAttribute debuggableAttribute = attribute as DebuggableAttribute;
                    if (debuggableAttribute.IsJITOptimizerDisabled)
                    {
                        Assert.Fail("IsJITOptimizerDisabled should be set to false for Release builds.");
                    }
                    if (debuggableAttribute.IsJITTrackingEnabled)
                    {
                        Assert.Fail("IsJITTrackingEnabled should be set to false for Release builds.");
                    }
                }
            }
        }


        private void ProcessDebugBuild(object[] attributes)
        {
            foreach (Attribute attribute in attributes)
            {
                if (attribute is DebuggableAttribute)
                {
                    DebuggableAttribute debuggableAttribute = attribute as DebuggableAttribute;
                    if (debuggableAttribute.IsJITOptimizerDisabled == false)
                    {
                        Assert.Fail("IsJITOptimizerDisabled should be set to true for Debug builds.");
                    }
                    if (debuggableAttribute.IsJITTrackingEnabled == false)
                    {
                        Assert.Fail("IsJITTrackingEnabled should be set to true for Debug builds.");
                    }
                }
            }
        }



        #region Properties

        /// <summary>
        /// Specify the assembly whose metadata will be checked for a given release mode (debug/release).
        /// </summary>
		protected Assembly AssemblyToCheck
		{
			get { return _assemblyToCheck; }
			set { _assemblyToCheck = value; }
		}

		#endregion

		private Assembly _assemblyToCheck = null;


    }
}