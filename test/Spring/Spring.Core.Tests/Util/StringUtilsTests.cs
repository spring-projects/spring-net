/*
 * Copyright � 2002-2011 the original author or authors.
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

using NUnit.Framework;

namespace Spring.Util;

/// <summary>
/// Unit tests for the StringUtils class.
/// </summary>
/// <author>Aleksandar Seovic</author>
/// <author>Rick Evans</author>
/// <author>Griffin Caprio</author>
[TestFixture]
public sealed class StringUtilsTests
{
    [Test]
    public void SplitTests()
    {
        string testString = " a,b,,  c  ,d\n:e  ";
        string delim = ",\n";
        string[] res;
        string[] res1 = new string[] { " a", "b", "", "  c  ", "d", ":e  " };
        string[] res2 = new string[] { " a", "b", "  c  ", "d", ":e  " };
        string[] res3 = new string[] { "a", "b", "", "c", "d", ":e" };
        string[] res4 = new string[] { "a", "b", "c", "d", ":e" };

        Assert.That(StringUtils.Split(null, null, false, false).Length, Is.EqualTo(0));
        Assert.That(StringUtils.Split(testString, null, false, false)[0], Is.EqualTo(testString));
        Assert.That(ArrayUtils.AreEqual(res1, res = StringUtils.Split(testString, delim, false, false)), Is.True, "Received '" + String.Join(",", res) + "'");
        Assert.That(ArrayUtils.AreEqual(res2, res = StringUtils.Split(testString, delim, false, true)), Is.True, "Received '" + String.Join(",", res) + "'");
        Assert.That(ArrayUtils.AreEqual(res3, res = StringUtils.Split(testString, delim, true, false)), Is.True, "Received '" + String.Join(",", res) + "'");
        Assert.That(ArrayUtils.AreEqual(res4, res = StringUtils.Split(testString, delim, true, true)), Is.True, "Received '" + String.Join(",", res) + "'");

        Assert.That(ArrayUtils.AreEqual(new string[] { "one" }, res = StringUtils.Split("one", delim, true, true)), Is.True, "Received '" + String.Join(",", res) + "'");
    }

    [Test]
    public void SplitWithQuotedStrings()
    {
        string[] expectedParts = new string[] { "a", "[test,<,>>[]]]", "asdf<,<,>,>aa" };
        string testString = string.Join(",", expectedParts);
        string[] result = StringUtils.Split(testString, ",", false, false, "[]<>");
        Assert.That(result, Is.EqualTo(expectedParts));
    }

    [Test]
    public void SplitAddsEmptyStrings()
    {
        string testString = ",";
        string[] result = StringUtils.Split(testString, ",", true, false, null);
        Assert.That(result, Is.EqualTo(new string[] { "", "" }));
    }

    [Test]
    public void SplitAcceptsWhitespaceDelimiters()
    {
        string testString = "a\nb\tc d";
        string[] result = StringUtils.Split(testString, "\n\t ", true, false, null);
        Assert.That(result, Is.EqualTo(new string[] { "a", "b", "c", "d" }));
    }

    [Test]
    public void HasLengthTests()
    {
        Assert.That(StringUtils.HasLength(null), Is.False);
        Assert.That(StringUtils.HasLength(""), Is.False);
        Assert.That(StringUtils.HasLength(" "), Is.True);
        Assert.That(StringUtils.HasLength("Hello"), Is.True);
    }

    [Test]
    public void HasTextTests()
    {
        Assert.That(StringUtils.HasText(null), Is.False);
        Assert.That(StringUtils.HasText(""), Is.False);
        Assert.That(StringUtils.HasText(" "), Is.False);
        Assert.That(StringUtils.HasText("12345"), Is.True);
        Assert.That(StringUtils.HasText("  12345  "), Is.True);
    }

    [Test]
    public void IsNullOrEmptyTests()
    {
        Assert.That(StringUtils.IsNullOrEmpty(null), Is.True);
        Assert.That(StringUtils.IsNullOrEmpty(""), Is.True);
        Assert.That(StringUtils.IsNullOrEmpty(" "), Is.True);
        Assert.That(StringUtils.IsNullOrEmpty("12345"), Is.False);
        Assert.That(StringUtils.IsNullOrEmpty("  12345  "), Is.False);
    }

    [Test]
    public void CollectionToDelimitedString()
    {
        Foo[] arr = new Foo[] { new Foo("Foo"), new Foo("Bar") };
        Assert.That(
            StringUtils.CollectionToCommaDelimitedString(arr), Is.EqualTo(":Foo,:Bar"));

        Assert.That(StringUtils.CollectionToCommaDelimitedString<object>(null), Is.EqualTo("null"));
    }

    [Test]
    public void ArrayToDelimitedString()
    {
        Foo[] arr = new Foo[] { new Foo("Foo"), new Foo("Bar") };
        Assert.That(
            StringUtils.CollectionToCommaDelimitedString(arr), Is.EqualTo(":Foo,:Bar"));
        Assert.That(StringUtils.CollectionToCommaDelimitedString<object>(null), Is.EqualTo("null"));
    }

    [Test]
    public void DelimitedListToStringArray()
    {
        string[] expected = new string[] { "Foo", "", "Bar" };
        string input = "Foo,,Bar";
        string[] actual = StringUtils.DelimitedListToStringArray(input, ",");
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Length, Is.EqualTo(expected.Length));
        for (int i = 0; i < actual.Length; ++i)
        {
            Assert.That(actual[i], Is.EqualTo(expected[i]));
        }
    }

    [Test]
    public void DelimitedListToStringArrayWithEmptyStringDelimiter()
    {
        string input = "Foo,,Bar";
        string[] expected = new string[] { input };
        string[] actual = StringUtils.DelimitedListToStringArray(input, string.Empty);
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Length, Is.EqualTo(expected.Length));
        for (int i = 0; i < actual.Length; ++i)
        {
            Assert.That(actual[i], Is.EqualTo(expected[i]));
        }
    }

    [Test]
    public void DelimitedListToStringArrayWithGuff()
    {
        string[] expected = new string[] { };
        string[] actual = StringUtils.DelimitedListToStringArray(null, null);
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Length, Is.EqualTo(expected.Length));

        string aString = "HungerHurtsButStarvingWorks...";
        expected = new string[] { aString };
        actual = StringUtils.DelimitedListToStringArray(aString, null);
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Length, Is.EqualTo(expected.Length));
        Assert.That(actual[0], Is.EqualTo(aString));
    }

    [Test]
    public void StripFirstAndLastCharacterWithNull()
    {
        string actual = StringUtils.StripFirstAndLastCharacter(null);
        Assert.That(actual, Is.Not.Null, "StringUtils.StripFirstAndLastCharacter(null) should return String.Empty.");
        Assert.That(actual, Is.Empty, "StringUtils.StripFirstAndLastCharacter(null) should return String.Empty.");
    }

    [Test]
    public void StripFirstAndLastCharacterWithEmptyString()
    {
        string actual = StringUtils.StripFirstAndLastCharacter(String.Empty);
        Assert.That(actual, Is.Not.Null, "StringUtils.StripFirstAndLastCharacter(String.Empty) should return String.Empty.");
        Assert.That(actual, Is.Empty, "StringUtils.StripFirstAndLastCharacter(String.Empty) should return String.Empty.");
    }

    [Test]
    public void StripFirstAndLastCharacterWithReallyShortStrings()
    {
        string actual = StringUtils.StripFirstAndLastCharacter("B");
        Assert.That(actual, Is.Not.Null, "StringUtils.StripFirstAndLastCharacter(\"B\") should return String.Empty.");
        Assert.That(actual, Is.Empty, "StringUtils.StripFirstAndLastCharacter(\"B\") should return String.Empty.");

        actual = StringUtils.StripFirstAndLastCharacter("BF");
        Assert.That(actual, Is.Not.Null, "StringUtils.StripFirstAndLastCharacter(\"BF\") should return String.Empty.");
        Assert.That(actual, Is.Empty, "StringUtils.StripFirstAndLastCharacter(\"BF\") should return String.Empty.");
    }

    [Test]
    public void StripFirstAndLastCharacterWorksLikeACharm()
    {
        string actual = StringUtils.StripFirstAndLastCharacter("There are 17 steps to winding up a bird chronicle correctly");
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Is.EqualTo("here are 17 steps to winding up a bird chronicle correctl"));
    }

    [Test]
    public void GetAntExpressionsWithNull()
    {
        IList<string> actual = StringUtils.GetAntExpressions(null);
        Assert.That(actual, Is.Not.Null);
        string[] expected = new string[] { };
        Assert.That(ArrayUtils.AreEqual(expected, new List<string>(actual).ToArray()), Is.True);
    }

    [Test]
    public void GetAntExpressionsWithEmptyString()
    {
        IList<string> actual = StringUtils.GetAntExpressions(String.Empty);
        Assert.That(actual, Is.Not.Null);
        string[] expected = new string[] { };
        Assert.That(ArrayUtils.AreEqual(expected, new List<string>(actual).ToArray()), Is.True);
    }

    [Test]
    public void GetAntExpressionsWithAStringThatDoesntHaveAnyExpressions()
    {
        IList<string> actual = StringUtils.GetAntExpressions("I could really go a cup of tea right now... in fact I think I'll go get one.");
        Assert.That(actual, Is.Not.Null);
        string[] expected = new string[] { };
        Assert.That(ArrayUtils.AreEqual(expected, new List<string>(actual).ToArray()), Is.True);
    }

    [Test]
    public void GetAntExpressionsWithAValidExpression()
    {
        IList<string> actual = StringUtils.GetAntExpressions("${slurp}. Ah! That is one good cup of tea. That agent Cooper and his coffee... he sure was missing out on a good thing.");
        CheckGetAntExpressions(actual, "slurp");
    }

    [Test]
    public void GetAntExpressionsWithANestedExpression()
    {
        IList<string> actual = StringUtils.GetAntExpressions("And yeah, I've never been a fan of the doughnut... ${blechh${shudder}}");
        CheckGetAntExpressions(actual, "blechh${shudder");
    }

    [Test]
    public void GetAntExpressionsWithACoupleOfDuplicatedValidExpressions()
    {
        IList<string> actual = StringUtils.GetAntExpressions("${sigh}. Laura Palmer though... man, that sure was a tragedy. ${sigh}");
        CheckGetAntExpressions(actual, "sigh");
    }

    [Test]
    public void GetAntExpressionsWithACoupleOfUniqueValidExpressions()
    {
        IList<string> actual = StringUtils.GetAntExpressions("${Mmm}. Has there been any good telly since then... ${thinks}");
        CheckGetAntExpressions(actual, "Mmm", "thinks");
    }

    [Test]
    public void GetAntExpressionsWithMalformedExpression()
    {
        IList<string> actual = StringUtils.GetAntExpressions("Mmm... just what counts as ${a malformed{ expression?");
        CheckGetAntExpressions(actual, new string[] { });
    }

    private static void CheckGetAntExpressions(IList<string> actual, params string[] expected)
    {
        Assert.That(actual, Is.Not.Null);
        Assert.That(ArrayUtils.AreEqual(
            expected,
            new List<string>(actual).ToArray()), Is.True);
    }

    [Test]
    public void GetAntExpressionsIgnoresEmptyExpression()
    {
        Assert.Throws<FormatException>(() => StringUtils.GetAntExpressions("This is an empty expression ${}..."));
    }

    [Test]
    public void SetAntExpressionWithNullText()
    {
        string expected = String.Empty;
        string actual = StringUtils.SetAntExpression(null, "foo", "bar");
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void SetAntExpressionWithEmptyStringExpression()
    {
        string expected = String.Empty;
        string actual = StringUtils.SetAntExpression("     ", "foo", "bar");
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void SetAntExpressionWithMultipleEvaluations()
    {
        string expected = "Hey, do you want to hear the most annoying sound in the world? Mehhh! Mehhh!";
        string actual = StringUtils.SetAntExpression("Hey, do you want to hear the most annoying sound in the world? ${foo}! ${foo}!", "foo", "Mehhh");
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void SetAntExpressionWithNullReplacementValue()
    {
        string expected = "That John Denver... he's full of .";
        string actual = StringUtils.SetAntExpression("That John Denver... he's full of ${bleep}.", "bleep", null);
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void FullSurroundWithNulls()
    {
        Assert.That(StringUtils.Surround(null, "Hi", null), Is.EqualTo("Hi"));
    }

    [Test]
    public void FullSurroundWithEmptyStrings()
    {
        Assert.That(StringUtils.Surround(string.Empty, "Hi", string.Empty), Is.EqualTo("Hi"));
    }

    [Test]
    public void FullSurround()
    {
        Assert.That(StringUtils.Surround("[", "Hi", "]"), Is.EqualTo("[Hi]"));
    }

    [Test]
    public void FullSurroundNullWithEmptyStrings()
    {
        Assert.That(StringUtils.Surround(string.Empty, null, string.Empty), Is.Empty);
    }

    [Test]
    public void FullSurroundEmptyStringWithEmptyStrings()
    {
        Assert.That(StringUtils.Surround(string.Empty, string.Empty, string.Empty), Is.Empty);
    }

    [Test]
    public void FullSurroundNullWithNulls()
    {
        Assert.That(StringUtils.Surround(null, null, null), Is.Empty);
    }

    [Test]
    public void SurroundWithEmptyStrings()
    {
        Assert.That(StringUtils.Surround(string.Empty, "Hi"), Is.EqualTo("Hi"));
    }

    [Test]
    public void Surround()
    {
        Assert.That(StringUtils.Surround("-", "Hi"), Is.EqualTo("-Hi-"));
    }

    [Test]
    public void SurroundNullWithEmptyStrings()
    {
        Assert.That(StringUtils.Surround(string.Empty, null), Is.Empty);
    }

    [Test]
    public void SurroundEmptyStringWithEmptyStrings()
    {
        Assert.That(StringUtils.Surround(string.Empty, string.Empty), Is.Empty);
    }

    [Test]
    public void SurroundNullWithNulls()
    {
        Assert.That(StringUtils.Surround(null, null), Is.Empty);
    }

    [Test]
    public void FullSurroundObjectWithNulls()
    {
        Assert.That(StringUtils.Surround(null, new Foo("Hi"), null), Is.EqualTo(":Hi"));
    }

    [Test]
    public void FullSurroundObjectWithEmptyStrings()
    {
        Assert.That(StringUtils.Surround(string.Empty, new Foo("Hi"), string.Empty), Is.EqualTo(":Hi"));
    }

    [Test]
    public void FullSurroundObject()
    {
        Assert.That(StringUtils.Surround("[", new Foo("Hi"), "]"), Is.EqualTo("[:Hi]"));
    }

    [Test]
    public void SurroundObjectWithEmptyStrings()
    {
        Assert.That(StringUtils.Surround(string.Empty, new Foo("Hi"), string.Empty), Is.EqualTo(":Hi"));
    }

    [Test]
    public void SurroundObject()
    {
        Assert.That(StringUtils.Surround("[", new Foo("Hi"), "]"), Is.EqualTo("[:Hi]"));
    }

    [Test]
    public void ConvertEscapedCharactersNoEscapedCharacters()
    {
        string inputString = "foo bar is a funny term";
        Assert.That(StringUtils.ConvertEscapedCharacters(inputString), Is.EqualTo(inputString));
    }

    [Test]
    public void ConvertEscapedCharactersAll()
    {
        string inputString = "newline\\n tab\\t return\\r";
        Assert.That(StringUtils.ConvertEscapedCharacters(inputString), Is.EqualTo("newline\n tab\t return\r"));
    }

    [Test]
    public void ConvertUnsupportedEscapedCharacters()
    {
        string inputString = "what is this\\g";
        Assert.That(StringUtils.ConvertEscapedCharacters(inputString), Is.EqualTo(inputString));
    }

    private sealed class Foo
    {
        public Foo(string bar)
        {
            this.bar = bar;
        }

        public override string ToString()
        {
            return ":" + bar;
        }

        private string bar;
    }
}
