#region License

/*
 * Copyright 2004 the original author or authors.
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
using System.IO;

using NUnit.Framework;

#endregion

namespace Spring.Core.IO
{
	/// <summary>
	/// Unit tests for the InputStreamResource class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class InputStreamResourceTests
    {
        [Test]
        public void Instantiation () 
        {
            FileInfo file = null;
            Stream stream = null;
            try 
            {
                file = new FileInfo ("Instantiation");
                stream = file.Create ();
                InputStreamResource res = new InputStreamResource (stream, "A temporary resource.");
                Assert.IsTrue (res.IsOpen);
                Assert.IsTrue (res.Exists);
                Assert.IsNotNull (res.InputStream);
            } 
            finally 
            {
                try 
                {
                    if (stream != null) 
                    {
                        stream.Close ();
                    }
                    if (file != null
                        && file.Exists) 
                    {
                        file.Delete ();
                    }
                } 
                catch {}
            }
        }

        [Test]
        public void InstantiationWithNull ()
        {
            Assert.Throws<ArgumentNullException>(() => new InputStreamResource (null, "A null resource."));
        }

        [Test]
        public void ReadStreamMultipleTimes () 
        {
            FileInfo file = null;
            Stream stream = null;
            try 
            {
                file = new FileInfo ("ReadStreamMultipleTimes");
                stream = file.Create ();
                // attempting to read this stream twice is an error...
                InputStreamResource res = new InputStreamResource (stream, "A temporary resource.");
                Stream streamOne = res.InputStream;
                Stream streamTwo;
                Assert.Throws<InvalidOperationException>(() => streamTwo = res.InputStream); // should bail here
            } 
            finally 
            {
                try 
                {
                    if (stream != null) 
                    {
                        stream.Close ();
                    }
                    if (file != null
                        && file.Exists) 
                    {
                        file.Delete ();
                    }
                } 
                catch {}
            }
        }
	}
}
