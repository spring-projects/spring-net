using System;
using System.Collections;
using System.Text;
using NUnit.Framework;

namespace Spring.Objects.Factory.Config
{
    [TestFixture]
    public class DictionaryVariableSourceTests
    {
        [Test]
        public void CanEnumerateDictionary()
        {
            DictionaryVariableSource dvs = new DictionaryVariableSource();

            dvs.Add("key1", "theValue");
            dvs.Add("key2", "theValue");

            foreach (DictionaryEntry dv in dvs)
            {
                Assert.AreEqual("theValue", dv.Value);
            }

        }

        [Test]
        public void CanResolveVariable_RespectsCaseInsensitivity()
        {
            DictionaryVariableSource caseInsensitive = new DictionaryVariableSource();
            caseInsensitive.Add("key1", "value1");

            Assert.True(caseInsensitive.CanResolveVariable("KEY1"));
        }

        [Test]
        public void CanResolveVariable_RespectsCaseSensitivity()
        {
            DictionaryVariableSource caseSensitive = new DictionaryVariableSource(false);
            caseSensitive.Add("KEY1", "value1");

            Assert.False(caseSensitive.CanResolveVariable("key1"));
        }

        [Test]
        public void Iniitialize_WithStringArray_FillsKeyValuesInPairs()
        {
            DictionaryVariableSource dvs = new DictionaryVariableSource(new string[] { "key1", "value1", "key2", "value2" });

            Assert.AreEqual("value1", dvs.ResolveVariable("key1"));
            Assert.AreEqual("value2", dvs.ResolveVariable("key2"));
        }

        [Test]
        public void Iniitialize_WithStringArray_ThrowsException_WhenOddNumberOfStringsProvided()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DictionaryVariableSource(new string[] { "key1", "value1", "key2", "value2", "orphanedKey1" }));
        }

        [Test]
        public void Initialize_WithCaseSensitiveFlag_AddsCaseSensitiveKeys()
        {
            DictionaryVariableSource dvs = new DictionaryVariableSource(false);
            dvs.Add("key1", "lowercasevalue");
            dvs.Add("KEY1", "uppercasevalue");

            Assert.AreEqual("lowercasevalue", dvs.ResolveVariable("key1"));
            Assert.AreEqual("uppercasevalue", dvs.ResolveVariable("KEY1"));
        }

        [Test]
        public void Initialize_WithDictionaryConstructor_AddsCaseInsensitiveKeys()
        {
            IDictionary dict = new Hashtable() { { "key1", "value1" }, { "KEY2", "value2" } };

            DictionaryVariableSource dvs = new DictionaryVariableSource(dict);

            Assert.AreEqual("value1", dvs.ResolveVariable("KEY1"));
            Assert.AreEqual("value2", dvs.ResolveVariable("key2"));
        }

        [Test]
        public void Initialize_WithDictionaryConstructor_AddsKeys()
        {
            IDictionary dict = new Hashtable() { { "key1", "value1" }, { "key2", "value2" } };

            DictionaryVariableSource dvs = new DictionaryVariableSource(dict);

            Assert.AreEqual("value1", dvs.ResolveVariable("key1"));
            Assert.AreEqual("value2", dvs.ResolveVariable("key2"));
        }

        [Test]
        public void Initialize_WithDictionaryConstructorAndCaseSensitiveFlag_AddsCaseSensitiveKeys()
        {
            IDictionary dict = new Hashtable() { { "key1", "lowecasevalue" }, { "KEY1", "uppercasevalue" } };

            DictionaryVariableSource dvs = new DictionaryVariableSource(dict, false);

            Assert.AreEqual("lowecasevalue", dvs.ResolveVariable("key1"));
            Assert.AreEqual("uppercasevalue", dvs.ResolveVariable("KEY1"));
        }

        [Test]
        public void Initialize_WithEmptyConstructor_AddsCaseInsensitiveKeys()
        {
            DictionaryVariableSource dvs = new DictionaryVariableSource();
            dvs.Add("key1", "value1");
            dvs.Add("KEY2", "value2");

            Assert.AreEqual("value1", dvs.ResolveVariable("KEY1"));
            Assert.AreEqual("value2", dvs.ResolveVariable("key2"));
        }

        [Test]
        public void Initialize_WithInlineDictionarySyntax()
        {
            DictionaryVariableSource dvs = new DictionaryVariableSource() { { "key1", "value1" }, { "key2", "value2" } };

            Assert.AreEqual("value1", dvs.ResolveVariable("key1"));
            Assert.AreEqual("value2", dvs.ResolveVariable("key2"));

        }

        [Test]
        public void Requesting_KeyNotFound_ThrowsException()
        {
            DictionaryVariableSource dvs = new DictionaryVariableSource();
            dvs.Add("key-found", "value-found");

            Assert.Throws<ArgumentException>(() => dvs.ResolveVariable("key-not-found"));
        }
    }
}
