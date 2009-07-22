#region License

/*
 * Copyright 2002-2009 the original author or authors.
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
using System.Collections;
using NUnit.Framework;
#if NET_2_0
using System.Collections.Generic;
#endif

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class ManagedDictionaryTests
    {
#if NET_2_0
        internal class InternalType
        {
            public string Value = "OK";
        }

        [Test]
        public void ResolvesInternalGenericTypes()
        {
            ManagedDictionary dict2 = new ManagedDictionary();
            dict2.Add("1", "stringValue");
            dict2.KeyTypeName = "int";
            dict2.ValueTypeName = typeof(InternalType).FullName;

            IDictionary resolved = (IDictionary) dict2.Resolve("other", new RootObjectDefinition(typeof (object)), "prop",
                                                               delegate(string name, RootObjectDefinition definition, string argumentName, object element)
                                                                   {
                                                                       if ("stringValue".Equals(element))
                                                                       {
                                                                           return new InternalType();
                                                                       }
                                                                       return element;
                                                                   }
                                                     );
            Assert.AreEqual( typeof(InternalType), resolved[1].GetType() );
        }

        [Test]
        public void ResolvesGenericTypeNames()
        {
            ManagedDictionary dict = new ManagedDictionary();
            dict.Add("key", "value");
            dict.KeyTypeName = "string";

            dict.ValueTypeName = "System.Collections.Generic.List<[string]>";
            IDictionary resolved = (IDictionary) dict.Resolve("somename", new RootObjectDefinition(typeof(object)), "prop",
                                                              delegate(string name, RootObjectDefinition definition, string argumentName, object element)
                                                                  {
                                                                      if ("value".Equals(element))
                                                                      {
                                                                          return new List<string>();
                                                                      }
                                                                      return element;
                                                                  }
                                                     );
            Assert.AreEqual(1, resolved.Count);
            Assert.AreEqual(typeof(List<string>), resolved["key"].GetType());
        }
#endif
    }
}