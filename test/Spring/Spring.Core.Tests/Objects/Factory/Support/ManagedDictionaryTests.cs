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
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

using Spring.Objects.Factory.Config;

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Integration tests for ManagedDictionary
    /// </summary>
    /// <author>Erich Eichinger</author>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class ManagedDictionaryTests
    {
        [Test]
        public void MergeSunnyDay()
        {
            ManagedDictionary parent = new ManagedDictionary();
            parent.Add("one", "one");
            parent.Add("two", "two");
            ManagedDictionary child = new ManagedDictionary();
            child.Add("three", "three");
            child.MergeEnabled = true;
            IDictionary mergedList = (IDictionary)child.Merge(parent);
            Assert.AreEqual(3, mergedList.Count);
        }

        [Test]
        public void MergeWithNullParent()
        {
            ManagedDictionary child = new ManagedDictionary();
            child.MergeEnabled = true;
            Assert.AreSame(child, child.Merge(null));
        }

        [Test]
        public void MergeNotAllowedWhenMergeNotEnabled()
        {
            ManagedDictionary child = new ManagedDictionary();
            Assert.Throws<InvalidOperationException>(() => child.Merge(null), "Not allowed to merge when the 'MergeEnabled' property is set to 'false'");
        }

        [Test]
        public void MergeWithNonCompatibleParentType()
        {
            ManagedDictionary child = new ManagedDictionary();
            child.MergeEnabled = true;
            Assert.Throws<InvalidOperationException>(() => child.Merge("hello"));
        }

        [Test]
        public void MergeEmptyChild()
        {
            ManagedDictionary parent = new ManagedDictionary();
            parent.Add("one", "one");
            parent.Add("two", "two");
            ManagedDictionary child = new ManagedDictionary();
            child.MergeEnabled = true;
            IDictionary mergedMap = (IDictionary)child.Merge(parent);
            Assert.AreEqual(2, mergedMap.Count);
        }

        [Test]
        public void MergeChildValueOverrideTheParents()
        {
            ManagedDictionary parent = new ManagedDictionary();
            parent.Add("one", "one");
            parent.Add("two", "two");
            ManagedDictionary child = new ManagedDictionary();
            child.Add("one", "fork");
            child.MergeEnabled = true;
            IDictionary mergedMap = (IDictionary)child.Merge(parent);
            Assert.AreEqual(2, mergedMap.Count);
            Assert.AreEqual("fork", mergedMap["one"]);
        }

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
                                                               delegate(string name, IObjectDefinition definition, string argumentName, object element)
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
                                                              delegate(string name, IObjectDefinition definition, string argumentName, object element)
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
        
        [Test]
        public void ResolvesMergedGenericType()
        {
            ManagedDictionary parent = new ManagedDictionary();
            parent.Add("one", 1);
            parent.Add("two", 2);
            parent.KeyTypeName = "string";
            parent.ValueTypeName = "int";
            
            ManagedDictionary child = new ManagedDictionary();
            child.MergeEnabled = true;
            child.Add("one", -1);
            child.Add("three", 3);
             
            ManagedDictionary merged = (ManagedDictionary) child.Merge(parent);

            IDictionary resolved = (IDictionary) merged.Resolve("somename", new RootObjectDefinition(typeof(object)), "prop",
                (name, definition, argumentName, element) => element);
            
            Assert.IsInstanceOf<IDictionary<string,int>>(resolved);
            Assert.AreEqual(3, resolved.Count);
            Assert.AreEqual(typeof(int), resolved["two"].GetType());
            Assert.AreEqual(-1, resolved["one"]);
        }
    }
}
