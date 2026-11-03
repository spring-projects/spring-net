using System.Collections;
using NUnit.Framework;

namespace Spring.Objects.Factory.Config;

[TestFixture]
public class DictionaryVariableSourceTests
{
    [Test]
    public void CanEnumerateDictionary()
    {
        DictionaryVariableSource dvs = new DictionaryVariableSource();

        dvs.Add("key1", "theValue");
        dvs.Add("key2", "theValue");

        foreach (KeyValuePair<string, string> dv in dvs)
        {
            Assert.That(dv.Value, Is.EqualTo("theValue"));
        }
    }

    [Test]
    public void CanResolveVariable_RespectsCaseInsensitivity()
    {
        DictionaryVariableSource caseInsensitive = new DictionaryVariableSource();
        caseInsensitive.Add("key1", "value1");

        Assert.That(caseInsensitive.CanResolveVariable("KEY1"), Is.True);
    }

    [Test]
    public void CanResolveVariable_RespectsCaseSensitivity()
    {
        DictionaryVariableSource caseSensitive = new DictionaryVariableSource(false);
        caseSensitive.Add("KEY1", "value1");

        Assert.That(caseSensitive.CanResolveVariable("key1"), Is.False);
    }

    [Test]
    public void Iniitialize_WithStringArray_FillsKeyValuesInPairs()
    {
        DictionaryVariableSource dvs = new DictionaryVariableSource(new string[] { "key1", "value1", "key2", "value2" });

        Assert.That(dvs.ResolveVariable("key1"), Is.EqualTo("value1"));
        Assert.That(dvs.ResolveVariable("key2"), Is.EqualTo("value2"));
    }

    [Test]
    public void Iniitialize_WithStringArray_ThrowsException_WhenOddNumberOfStringsProvided()
    {
        try
        {
            new DictionaryVariableSource(new string[] { "key1", "value1", "key2", "value2", "orphanedKey1" });
            Assert.Fail("Expected ArgumentOutOfRangeException not thrown.");
        }
        catch (ArgumentOutOfRangeException)
        {
        }
    }

    [Test]
    public void Initialize_WithCaseSensitiveFlag_AddsCaseSensitiveKeys()
    {
        DictionaryVariableSource dvs = new DictionaryVariableSource(false);
        dvs.Add("key1", "lowercasevalue");
        dvs.Add("KEY1", "uppercasevalue");

        Assert.That(dvs.ResolveVariable("key1"), Is.EqualTo("lowercasevalue"));
        Assert.That(dvs.ResolveVariable("KEY1"), Is.EqualTo("uppercasevalue"));
    }

    [Test]
    public void Initialize_WithDictionaryConstructor_AddsCaseInsensitiveKeys()
    {
        IDictionary dict = new Hashtable();
        dict.Add("key1", "value1");
        dict.Add("KEY2", "value2");

        DictionaryVariableSource dvs = new DictionaryVariableSource(dict);

        Assert.That(dvs.ResolveVariable("KEY1"), Is.EqualTo("value1"));
        Assert.That(dvs.ResolveVariable("key2"), Is.EqualTo("value2"));
    }

    [Test]
    public void Initialize_WithDictionaryConstructor_AddsKeys()
    {
        IDictionary dict = new Hashtable();
        dict.Add("key1", "value1");
        dict.Add("key2", "value2");

        DictionaryVariableSource dvs = new DictionaryVariableSource(dict);

        Assert.That(dvs.ResolveVariable("key1"), Is.EqualTo("value1"));
        Assert.That(dvs.ResolveVariable("key2"), Is.EqualTo("value2"));
    }

    [Test]
    public void Initialize_WithDictionaryConstructorAndCaseSensitiveFlag_AddsCaseSensitiveKeys()
    {
        IDictionary dict = new Hashtable();
        dict.Add("key1", "lowecasevalue");
        dict.Add("KEY1", "uppercasevalue");

        DictionaryVariableSource dvs = new DictionaryVariableSource(dict, false);

        Assert.That(dvs.ResolveVariable("key1"), Is.EqualTo("lowecasevalue"));
        Assert.That(dvs.ResolveVariable("KEY1"), Is.EqualTo("uppercasevalue"));
    }

    [Test]
    public void Initialize_WithEmptyConstructor_AddsCaseInsensitiveKeys()
    {
        DictionaryVariableSource dvs = new DictionaryVariableSource();
        dvs.Add("key1", "value1");
        dvs.Add("KEY2", "value2");

        Assert.That(dvs.ResolveVariable("KEY1"), Is.EqualTo("value1"));
        Assert.That(dvs.ResolveVariable("key2"), Is.EqualTo("value2"));
    }

    [Test]
    public void Requesting_KeyNotFound_ThrowsException()
    {
        const string THE_KEY = "key-found";

        DictionaryVariableSource dvs = new DictionaryVariableSource();
        dvs.Add(THE_KEY, "value-found");

        try
        {
            dvs.ResolveVariable("not" + THE_KEY);
            Assert.Fail("Expected ArgumentException not thrown.");
        }
        catch (ArgumentException)
        {
        }
    }
}
