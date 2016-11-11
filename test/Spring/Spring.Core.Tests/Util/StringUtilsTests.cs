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

using System;
using System.Collections.Generic;

using NUnit.Framework;

#endregion

namespace Spring.Util
{
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
			string[] res1 = new string[] {" a", "b", "", "  c  ", "d", ":e  "};
			string[] res2 = new string[] {" a", "b", "  c  ", "d", ":e  "};
			string[] res3 = new string[] {"a", "b", "", "c", "d", ":e"};
			string[] res4 = new string[] {"a", "b", "c", "d", ":e"};

			Assert.AreEqual(0, StringUtils.Split(null, null, false, false).Length);
			Assert.AreEqual(testString, StringUtils.Split(testString, null, false, false)[0]);
			Assert.IsTrue(ArrayUtils.AreEqual(res1, res = StringUtils.Split(testString, delim, false, false)), "Received '" + String.Join(",", res) + "'");
			Assert.IsTrue(ArrayUtils.AreEqual(res2, res = StringUtils.Split(testString, delim, false, true)), "Received '" + String.Join(",", res) + "'");
			Assert.IsTrue(ArrayUtils.AreEqual(res3, res = StringUtils.Split(testString, delim, true, false)), "Received '" + String.Join(",", res) + "'");
			Assert.IsTrue(ArrayUtils.AreEqual(res4, res = StringUtils.Split(testString, delim, true, true)), "Received '" + String.Join(",", res) + "'");

			Assert.IsTrue(ArrayUtils.AreEqual(new string[] {"one"}, res = StringUtils.Split("one", delim, true, true)), "Received '" + String.Join(",", res) + "'");
		}

        [Test]
        public void SplitWithQuotedStrings()
        {
            string[] expectedParts = new string[] { "a", "[test,<,>>[]]]", "asdf<,<,>,>aa" };
            string testString = string.Join(",", expectedParts);
            string[] result = StringUtils.Split(testString, ",", false, false, "[]<>");
            Assert.AreEqual( expectedParts, result );
        }

        [Test]
        public void SplitAddsEmptyStrings()
        {
            string testString = ",";
            string[] result = StringUtils.Split(testString, ",", true, false, null);
            Assert.AreEqual( new string[] { "", "" }, result );
        }

        [Test]
        public void SplitAcceptsWhitespaceDelimiters()
        {
            string testString = "a\nb\tc d";
            string[] result = StringUtils.Split(testString, "\n\t ", true, false, null);
            Assert.AreEqual( new string[] { "a", "b", "c", "d" }, result );
        }

		[Test]
		public void HasLengthTests()
		{
			Assert.IsFalse(StringUtils.HasLength(null));
			Assert.IsFalse(StringUtils.HasLength(""));
			Assert.IsTrue(StringUtils.HasLength(" "));
			Assert.IsTrue(StringUtils.HasLength("Hello"));
		}

		[Test]
		public void HasTextTests()
		{
			Assert.IsFalse(StringUtils.HasText(null));
			Assert.IsFalse(StringUtils.HasText(""));
			Assert.IsFalse(StringUtils.HasText(" "));
			Assert.IsTrue(StringUtils.HasText("12345"));
			Assert.IsTrue(StringUtils.HasText("  12345  "));
		}

		[Test]
		public void IsNullOrEmptyTests()
		{
			Assert.IsTrue(StringUtils.IsNullOrEmpty(null));
			Assert.IsTrue(StringUtils.IsNullOrEmpty(""));
			Assert.IsTrue(StringUtils.IsNullOrEmpty(" "));
			Assert.IsFalse(StringUtils.IsNullOrEmpty("12345"));
			Assert.IsFalse(StringUtils.IsNullOrEmpty("  12345  "));
		}

		[Test]
		public void CollectionToDelimitedString()
		{
			Foo[] arr = new Foo[] {new Foo("Foo"), new Foo("Bar")};
			Assert.AreEqual(
				":Foo,:Bar", StringUtils.CollectionToCommaDelimitedString(arr));

			Assert.AreEqual("null", StringUtils.CollectionToCommaDelimitedString<object>(null));
		}

		[Test]
		public void ArrayToDelimitedString()
		{
			Foo[] arr = new Foo[] {new Foo("Foo"), new Foo("Bar")};
			Assert.AreEqual(
				":Foo,:Bar", StringUtils.CollectionToCommaDelimitedString(arr));
			Assert.AreEqual("null", StringUtils.CollectionToCommaDelimitedString<object>(null));
		}

		[Test]
		public void DelimitedListToStringArray()
		{
			string[] expected = new string[] {"Foo", "", "Bar"};
			string input = "Foo,,Bar";
			string[] actual = StringUtils.DelimitedListToStringArray(input, ",");
			Assert.IsNotNull(actual);
			Assert.AreEqual(expected.Length, actual.Length);
			for (int i = 0; i < actual.Length; ++i)
			{
				Assert.AreEqual(expected[i], actual[i]);
			}
		}

		[Test]
		public void DelimitedListToStringArrayWithEmptyStringDelimiter()
		{
			string input = "Foo,,Bar";
			string[] expected = new string[] {input};
			string[] actual = StringUtils.DelimitedListToStringArray(input, string.Empty);
			Assert.IsNotNull(actual);
			Assert.AreEqual(expected.Length, actual.Length);
			for (int i = 0; i < actual.Length; ++i)
			{
				Assert.AreEqual(expected[i], actual[i]);
			}
		}

		[Test]
		public void DelimitedListToStringArrayWithGuff()
		{
			string[] expected = new string[] {};
			string[] actual = StringUtils.DelimitedListToStringArray(null, null);
			Assert.IsNotNull(actual);
			Assert.AreEqual(expected.Length, actual.Length);

			string aString = "HungerHurtsButStarvingWorks...";
			expected = new string[] {aString};
			actual = StringUtils.DelimitedListToStringArray(aString, null);
			Assert.IsNotNull(actual);
			Assert.AreEqual(expected.Length, actual.Length);
			Assert.AreEqual(aString, actual[0]);
		}

		[Test]
		public void StripFirstAndLastCharacterWithNull()
		{
			string actual = StringUtils.StripFirstAndLastCharacter(null);
			Assert.IsNotNull(actual, "StringUtils.StripFirstAndLastCharacter(null) should return String.Empty.");
			Assert.AreEqual(String.Empty, actual, "StringUtils.StripFirstAndLastCharacter(null) should return String.Empty.");
		}

		[Test]
		public void StripFirstAndLastCharacterWithEmptyString()
		{
			string actual = StringUtils.StripFirstAndLastCharacter(String.Empty);
			Assert.IsNotNull(actual, "StringUtils.StripFirstAndLastCharacter(String.Empty) should return String.Empty.");
			Assert.AreEqual(String.Empty, actual, "StringUtils.StripFirstAndLastCharacter(String.Empty) should return String.Empty.");
		}

		[Test]
		public void StripFirstAndLastCharacterWithReallyShortStrings()
		{
			string actual = StringUtils.StripFirstAndLastCharacter("B");
			Assert.IsNotNull(actual, "StringUtils.StripFirstAndLastCharacter(\"B\") should return String.Empty.");
			Assert.AreEqual(String.Empty, actual, "StringUtils.StripFirstAndLastCharacter(\"B\") should return String.Empty.");

			actual = StringUtils.StripFirstAndLastCharacter("BF");
			Assert.IsNotNull(actual, "StringUtils.StripFirstAndLastCharacter(\"BF\") should return String.Empty.");
			Assert.AreEqual(String.Empty, actual, "StringUtils.StripFirstAndLastCharacter(\"BF\") should return String.Empty.");
		}

		[Test]
		public void StripFirstAndLastCharacterWorksLikeACharm()
		{
			string actual = StringUtils.StripFirstAndLastCharacter("There are 17 steps to winding up a bird chronicle correctly");
			Assert.IsNotNull(actual);
			Assert.AreEqual("here are 17 steps to winding up a bird chronicle correctl", actual);
		}

		[Test]
		public void GetAntExpressionsWithNull()
		{
			IList<string> actual = StringUtils.GetAntExpressions(null);
			Assert.IsNotNull(actual);
			string[] expected = new string[] {};
			Assert.IsTrue(ArrayUtils.AreEqual(expected, new List<string>(actual).ToArray()));
		}

		[Test]
		public void GetAntExpressionsWithEmptyString()
		{
			IList<string> actual = StringUtils.GetAntExpressions(String.Empty);
			Assert.IsNotNull(actual);
			string[] expected = new string[] {};
			Assert.IsTrue(ArrayUtils.AreEqual(expected, new List<string>(actual).ToArray()));
		}

		[Test]
		public void GetAntExpressionsWithAStringThatDoesntHaveAnyExpressions()
		{
			IList<string> actual = StringUtils.GetAntExpressions("I could really go a cup of tea right now... in fact I think I'll go get one.");
			Assert.IsNotNull(actual);
			string[] expected = new string[] {};
			Assert.IsTrue(ArrayUtils.AreEqual(expected, new List<string>(actual).ToArray()));
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
			CheckGetAntExpressions(actual, new string[] {});
		}

		private static void CheckGetAntExpressions(IList<string> actual, params string[] expected)
		{
			Assert.IsNotNull(actual);
			Assert.IsTrue(ArrayUtils.AreEqual(
				expected,
				new List<string>(actual).ToArray()));
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
			Assert.IsNotNull(actual);
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void SetAntExpressionWithEmptyStringExpression()
		{
			string expected = String.Empty;
			string actual = StringUtils.SetAntExpression("     ", "foo", "bar");
			Assert.IsNotNull(actual);
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void SetAntExpressionWithMultipleEvaluations()
		{
			string expected = "Hey, do you want to hear the most annoying sound in the world? Mehhh! Mehhh!";
			string actual = StringUtils.SetAntExpression("Hey, do you want to hear the most annoying sound in the world? ${foo}! ${foo}!", "foo", "Mehhh");
			Assert.IsNotNull(actual);
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void SetAntExpressionWithNullReplacementValue()
		{
			string expected = "That John Denver... he's full of .";
			string actual = StringUtils.SetAntExpression("That John Denver... he's full of ${bleep}.", "bleep", null);
			Assert.IsNotNull(actual);
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void FullSurroundWithNulls()
		{
			Assert.AreEqual("Hi", StringUtils.Surround(null, "Hi", null));
		}

		[Test]
		public void FullSurroundWithEmptyStrings()
		{
			Assert.AreEqual("Hi", StringUtils.Surround(string.Empty, "Hi", string.Empty));
		}

		[Test]
		public void FullSurround()
		{
			Assert.AreEqual("[Hi]", StringUtils.Surround("[", "Hi", "]"));
		}

		[Test]
		public void FullSurroundNullWithEmptyStrings()
		{
			Assert.AreEqual(string.Empty, StringUtils.Surround(string.Empty, null, string.Empty));
		}

		[Test]
		public void FullSurroundEmptyStringWithEmptyStrings()
		{
			Assert.AreEqual(string.Empty, StringUtils.Surround(string.Empty, string.Empty, string.Empty));
		}

		[Test]
		public void FullSurroundNullWithNulls()
		{
			Assert.AreEqual(string.Empty, StringUtils.Surround(null, null, null));
		}

		[Test]
		public void SurroundWithEmptyStrings()
		{
			Assert.AreEqual("Hi", StringUtils.Surround(string.Empty, "Hi"));
		}

		[Test]
		public void Surround()
		{
			Assert.AreEqual("-Hi-", StringUtils.Surround("-", "Hi"));
		}

		[Test]
		public void SurroundNullWithEmptyStrings()
		{
			Assert.AreEqual(string.Empty, StringUtils.Surround(string.Empty, null));
		}

		[Test]
		public void SurroundEmptyStringWithEmptyStrings()
		{
			Assert.AreEqual(string.Empty, StringUtils.Surround(string.Empty, string.Empty));
		}

		[Test]
		public void SurroundNullWithNulls()
		{
			Assert.AreEqual(string.Empty, StringUtils.Surround(null, null));
		}

		[Test]
		public void FullSurroundObjectWithNulls()
		{
			Assert.AreEqual(":Hi", StringUtils.Surround(null, new Foo("Hi"), null));
		}

		[Test]
		public void FullSurroundObjectWithEmptyStrings()
		{
			Assert.AreEqual(":Hi", StringUtils.Surround(string.Empty, new Foo("Hi"), string.Empty));
		}

		[Test]
		public void FullSurroundObject()
		{
			Assert.AreEqual("[:Hi]", StringUtils.Surround("[", new Foo("Hi"), "]"));
		}

		[Test]
		public void SurroundObjectWithEmptyStrings()
		{
			Assert.AreEqual(":Hi", StringUtils.Surround(string.Empty, new Foo("Hi"), string.Empty));
		}

		[Test]
		public void SurroundObject()
		{
			Assert.AreEqual("[:Hi]", StringUtils.Surround("[", new Foo("Hi"), "]"));
		}

		[Test]
		public void ConvertEscapedCharactersNoEscapedCharacters()
		{
			string inputString = "foo bar is a funny term";
			Assert.AreEqual(inputString, StringUtils.ConvertEscapedCharacters(inputString));
		}

		[Test]
		public void ConvertEscapedCharactersAll()
		{
			string inputString = "newline\\n tab\\t return\\r";
			Assert.AreEqual("newline\n tab\t return\r", StringUtils.ConvertEscapedCharacters(inputString));
		}

		[Test]
		public void ConvertUnsupportedEscapedCharacters()
		{
			string inputString = "what is this\\g";
			Assert.AreEqual(inputString, StringUtils.ConvertEscapedCharacters(inputString));
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
}