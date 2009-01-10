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
using System.Collections.Generic;
using NUnit.Framework;

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class ManagedDictionaryTests
    {
#if NET_2_0
        [Test]
        public void ResolvesGenericTypeNames()
        {
            ManagedDictionary dict = new ManagedDictionary();
            dict.Add("key", "value");
            dict.KeyTypeName = "string";
            
            dict.ValueTypeName = "System.Collections.Generic.List<[string]>";
            dict.Resolve("somename", new RootObjectDefinition(typeof (object)), "prop", 
                    delegate(string name, RootObjectDefinition definition, string argumentName, object element)
                            {
                                return new List<string>();
                            }
                );
        }
#endif
    }
}