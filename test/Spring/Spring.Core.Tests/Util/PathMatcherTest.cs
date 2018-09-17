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
using System;
using System.IO;
using NUnit.Framework;

namespace Spring.Util
{
    [TestFixture]
	public class PathMatcherTest
	{
	    private static readonly string dir = Path.Combine("Data", "PathMatcher");

        [Test]
        public void TestFilesInDataPathMatcher ()
        {
            DirectoryInfo info = new DirectoryInfo(dir);
            foreach (FileInfo file in info.GetFiles("*.test"))
            {
                ProcessFile (file);
            }
            
        }

       [Test, Explicit]
       public void ThisIsATestForDebuggingPurposes ()
       {
           ProcessFile(new FileInfo(dir + "/Examples.test"));
       }

        private static void ProcessFile (FileInfo file)
        {
            bool trueness = true;
            using (StreamReader r = file.OpenText())
            {
                string pattern = null;
                string line;
                int nline = 0;

                bool expectingPattern = false;
                while ((line = r.ReadLine()) != null)
                {
                    nline++;
                    switch (line)
                    {
                        case "":
                            if (expectingPattern)
                            {
                                pattern = line;
                                expectingPattern = false;
                                continue;
                            }
                            break;
                        case "--match--":
                        case "# <match>":
                            trueness = true;
                            break;
                        case "--dont-match--":
                        case "# <dont.match>":
                            trueness = false;
                            break;
                        case "--pattern--":
                        case "# <pattern>":
                            expectingPattern = true;
                            nline++;
                            break;
                        default:
                            if (line.StartsWith("#"))
                            {
                                continue;
                            }
                            if (expectingPattern)
                            {
                                pattern = line;
                                expectingPattern = false;
                                continue;
                            }
                            AssertMatches(file, nline, trueness, line, pattern);
                            AssertMatches(file, nline, trueness, line.Replace("/", "\\"), pattern);
                            break;
                    }
                }
            }
        }

        private static void AssertMatches (FileInfo file, int nline, bool trueness, string path, string pattern)
        {
                Assert.AreEqual(trueness, 
                    PathMatcher.Match(pattern, path), 
                    String.Format("\ntest file '{0}', line {1}:\npath '{2}' does {3} match '{4}' (translated to {5})", 
                    file.Name, nline, path, trueness ? "not" : "", pattern, PathMatcher.BuildRegex(pattern))) ;
        }
	}
}
