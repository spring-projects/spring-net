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
using System.Collections;
using NUnit.Framework;
using Spring.Util;

#endregion

namespace Spring.Web.Support
{
	/// <summary>
	/// Unit tests for the Result class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class ResultTests
	{
		private const string ExpectedTargetPageName = "Foo.aspx";

		[Test]
		public void DoesItDefaultToTheDefaultResultMode()
		{
			Result result = new Result();
			Assert.AreEqual(Result.DefaultResultMode, result.Mode, "Not defaulting to the default ResultMode.");
		}

		[Test]
		public void DefaultsToTransferMode()
		{
			Assert.AreEqual(ResultMode.Transfer, Result.DefaultResultMode,
							"Hey! Be sure to change the documentation since you've " +
							"obviously gone and changed the default transfer mode.");
		}

		[Test]
		public void InstantiationBailsWithNullResultString()
		{
            Assert.Throws<ArgumentNullException>(() => new Result(null));
		}

		[Test]
		public void InstantiationBailsWithEmptyResultString()
		{
            Assert.Throws<ArgumentNullException>(() => new Result(string.Empty));
		}

		[Test]
		public void InstantiationBailsWithAllWhitespaceResultString()
		{
            Assert.Throws<ArgumentNullException>(() => new Result("    \n\t   "));
		}

		[Test]
		public void WithNoResultModeAndNoParameters()
		{
			Result result = new Result(ExpectedTargetPageName);
			Assert.AreEqual(Result.DefaultResultMode, result.Mode, "Not defaulting to the default ResultMode.");
			Assert.IsNull(result.Parameters,
						  "No parameters were passed but the Parameters property appears to be set to a non-null value anyway.");
			Assert.AreEqual(ExpectedTargetPageName, result.TargetPage,
							"The TargetPage property is not being correctly extracted " +
							"from the result string passed into the ctor.");
		}

		[Test]
		public void WithWhitespacePageAndNoResultModeAndNoParameters()
		{
			Result result = new Result("   " + ExpectedTargetPageName + "\n");
			Assert.AreEqual(Result.DefaultResultMode, result.Mode, "Not defaulting to the default ResultMode.");
			Assert.IsNull(result.Parameters,
						  "No parameters were passed but the Parameters property appears to be set to a non-null value anyway.");
			Assert.AreEqual(ExpectedTargetPageName, result.TargetPage,
							"The TargetPage property is not being correctly extracted " +
							"from the result string passed into the ctor.");
		}

		[Test]
		public void WithTransferResultModeButNoParameters()
		{
			Result result = new Result("transfer:" + ExpectedTargetPageName);
			Assert.AreEqual(ResultMode.Transfer, result.Mode, "Not extracting the correct ResultMode " +
															  "from the result string passed into the ctor.");
			Assert.AreEqual(true, result.PreserveForm, "ResultMode.Transfer must result in PreserveForm being 'true'");
			Assert.IsNull(result.Parameters,
						  "No parameters were passed but the Parameters property appears to be set to a non-null value anyway.");
			Assert.AreEqual(ExpectedTargetPageName, result.TargetPage,
							"The TargetPage property is not being correctly extracted " +
							"from the result string passed into the ctor.");
		}

		[Test]
		public void WithTransferNoPreserveResultMode()
		{
			Result result = new Result( "transfernopreserve:" + ExpectedTargetPageName );
			Assert.AreEqual( ResultMode.TransferNoPreserve,result.Mode,"Not extracting the correct ResultMode " +
															  "from the result string passed into the ctor." );
			Assert.AreEqual( false,result.PreserveForm,"ResultMode.TransferNoPreserver must result in PreserveForm being 'false'" );
		}

		[Test]
		public void WithWhitespacedRedirectResultMode()
		{
			Result result = new Result("    redirect:" + ExpectedTargetPageName);
			Assert.AreEqual(ResultMode.Redirect, result.Mode, "Not extracting the correct ResultMode " +
															  "from the result string passed into the ctor.");
			Assert.IsNull(result.Parameters,
						  "No parameters were passed but the Parameters property appears to be set to a non-null value anyway.");
			Assert.AreEqual(ExpectedTargetPageName, result.TargetPage,
							"The TargetPage property is not being correctly extracted " +
							"from the result string passed into the ctor.");
		}

		[Test]
		public void WithRedirectResultMode()
		{
			Result result = new Result("redirect:" + ExpectedTargetPageName);
			Assert.AreEqual(ResultMode.Redirect, result.Mode, "Not extracting the correct ResultMode " +
															  "from the result string passed into the ctor.");
			Assert.IsTrue(result.EndResponse);
			Assert.IsNull(result.Parameters,
						  "No parameters were passed but the Parameters property appears to be set to a non-null value anyway.");
			Assert.AreEqual(ExpectedTargetPageName, result.TargetPage,
							"The TargetPage property is not being correctly extracted " +
							"from the result string passed into the ctor.");
		}

		[Test]
		public void WithRedirectNoAbortResultMode()
		{
			Result result = new Result("redirectnoabort:" + ExpectedTargetPageName);
			Assert.AreEqual(ResultMode.RedirectNoAbort, result.Mode, "Not extracting the correct ResultMode " +
															  "from the result string passed into the ctor.");
			Assert.IsFalse(result.EndResponse);
			Assert.IsNull(result.Parameters,
						  "No parameters were passed but the Parameters property appears to be set to a non-null value anyway.");
			Assert.AreEqual(ExpectedTargetPageName, result.TargetPage,
							"The TargetPage property is not being correctly extracted " +
							"from the result string passed into the ctor.");
		}

		[Test]
		public void WithRubbishResultMode()
		{
            // note the incorrect spelling of the result mode prefix...
            Assert.Throws<ArgumentOutOfRangeException>(() => new Result("redirct:" + ExpectedTargetPageName));
		}

		[Test]
		public void WithResultModeAndEmptyParameterDelimiterOnly()
		{
			Result result = new Result("transfer:" + ExpectedTargetPageName + "?");
			Assert.AreEqual(ResultMode.Transfer, result.Mode, "Not extracting the correct ResultMode " +
															  "from the result string passed into the ctor.");
			Assert.IsNull(result.Parameters,
						  "No parameters were passed (just the parameter delimiter) but the " +
						  "Parameters property appears to be set to a non-null value anyway.");
			Assert.AreEqual(ExpectedTargetPageName, result.TargetPage,
							"The TargetPage property is not being correctly extracted " +
							"from the result string passed into the ctor.");
		}

		[Test]
		public void WithResultModeAndWhitespaceParameterDelimiterOnly()
		{
			Result result = new Result("redirect:" + ExpectedTargetPageName + "?     ");
			Assert.AreEqual(ResultMode.Redirect, result.Mode, "Not extracting the correct ResultMode " +
															  "from the result string passed into the ctor.");
			Assert.IsNull(result.Parameters,
						  "No parameters were passed (just the parameter delimiter) but the " +
						  "Parameters property appears to be set to a non-null value anyway.");
			Assert.AreEqual(ExpectedTargetPageName, result.TargetPage,
							"The TargetPage property is not being correctly extracted " +
							"from the result string passed into the ctor.");
		}

		[Test]
		public void WithNoResultModeAndWhitespaceParameterDelimiterOnly()
		{
			Result result = new Result(ExpectedTargetPageName + "?     ");
			Assert.AreEqual(Result.DefaultResultMode, result.Mode, "Not defaulting to the default ResultMode.");
			Assert.IsNull(result.Parameters,
						  "No parameters were passed (just the parameter delimiter) but the " +
						  "Parameters property appears to be set to a non-null value anyway.");
			Assert.AreEqual(ExpectedTargetPageName, result.TargetPage,
							"The TargetPage property is not being correctly extracted " +
							"from the result string passed into the ctor.");
		}

		[Test]
		public void WithNoResultModeAndOneParameter()
		{
			const string key = "id";
			const string value = "Nina Persson";
			Result result = new Result(ExpectedTargetPageName + "?" + key + "=" + value);
			Assert.AreEqual(Result.DefaultResultMode, result.Mode, "Not defaulting to the default ResultMode.");
			Assert.IsNotNull(result.Parameters,
							 "Parameters were passed but the Parameters property appears to be null (incorrectly).");
			Assert.AreEqual(1, result.Parameters.Count,
							"Wrong number of query parameters parsed (only supplied one key-value pair).");
			Assert.IsTrue(result.Parameters.Contains(key), "The 'id' parameter is not being correctly extracted " +
														   "from the result string passed into the ctor.");
			Assert.AreEqual(value, result.Parameters[key]);
			Assert.AreEqual(ExpectedTargetPageName, result.TargetPage,
							"The TargetPage property is not being correctly extracted " +
							"from the result string passed into the ctor.");
		}

		[Test]
		public void WithTransferResultModeAndOneParameter()
		{
			const string key = "id";
			const string value = "Nina Persson";
			Result result = new Result("transfer:" + ExpectedTargetPageName + "?" + key + "=" + value);
			Assert.AreEqual(ResultMode.Transfer, result.Mode, "Not extracting the correct ResultMode " +
															  "from the result string passed into the ctor.");
			Assert.IsNotNull(result.Parameters,
							 "Parameters were passed but the Parameters property appears to be null (incorrectly).");
			Assert.AreEqual(1, result.Parameters.Count,
							"Wrong number of query parameters parsed (only supplied one key-value pair).");
			Assert.IsTrue(result.Parameters.Contains(key), "The 'id' parameter is not being correctly extracted " +
														   "from the result string passed into the ctor.");
			Assert.AreEqual(value, result.Parameters[key]);
			Assert.AreEqual(ExpectedTargetPageName, result.TargetPage,
							"The TargetPage property is not being correctly extracted " +
							"from the result string passed into the ctor.");
		}

		[Test]
		[Ignore("How does one escape ampersands then?")]
		public void ParametersHavingEmbeddedCharactersThatHaveToBeEscaped()
		{
			const string key = "id";
			const string value = "Nina Persson & The Cardigans"; // notice the use of the embedded '&'...
			Result result = new Result(ExpectedTargetPageName + "?" + key + "=" + value);
			Assert.AreEqual(Result.DefaultResultMode, result.Mode, "Not defaulting to the default ResultMode.");
			Assert.IsNotNull(result.Parameters,
							 "Parameters were passed but the Parameters property appears to be null (incorrectly).");
//            Assert.AreEqual(1, result.Parameters.Count,
//                            "Wrong number of query parameters parsed (only supplied one key-value pair).");
//            Assert.IsTrue(result.Parameters.Contains(key), "The 'id' parameter is not being correctly extracted " +
//                                                           "from the result string passed into the ctor.");
//            Assert.AreEqual(value, result.Parameters[key]);
//            Assert.AreEqual(ExpectedTargetPageName, result.TargetPage,
//                            "The TargetPage property is not being correctly extracted " +
//                            "from the result string passed into the ctor.");
		}

		[Test]
		public void WithTransferResultModeAndTwoParametersSplitUsingAmpersand()
		{
			const string pKey1 = "id";
			const string pValue1 = "Nina Persson";
			const string pKey2 = "verdict";
			const string pValue2 = "schwing";
			Result result =
				new Result(ExpectedTargetPageName + "?" + pKey1 + "=" + pValue1 + "&" + pKey2 + "=" + pValue2);
			Assert.AreEqual(ResultMode.Transfer, result.Mode, "Not extracting the correct ResultMode " +
															  "from the result string passed into the ctor.");
			Assert.IsNotNull(result.Parameters,
							 "Parameters were passed but the Parameters property appears to be null (incorrectly).");
			Assert.AreEqual(2, result.Parameters.Count,
							"Wrong number of query parameters parsed (supplied two key-value pairs).");
			Assert.IsTrue(result.Parameters.Contains(pKey1), "The 'id' parameter is not being correctly extracted " +
															 "from the result string passed into the ctor.");
			Assert.AreEqual(pValue1, result.Parameters[pKey1]);
			Assert.IsTrue(result.Parameters.Contains(pKey2),
						  "The 'verdict' parameter is not being correctly extracted " +
						  "from the result string passed into the ctor.");
			Assert.AreEqual(pValue2, result.Parameters[pKey2]);
			Assert.AreEqual(ExpectedTargetPageName, result.TargetPage,
							"The TargetPage property is not being correctly extracted " +
							"from the result string passed into the ctor.");
		}

		[Test]
		public void WithWhitespacedPageAndTransferResultModeAndTwoParametersUsingAmpersands()
		{
			const string pKey1 = "id";
			const string pValue1 = "Nina Persson";
			const string pKey2 = "verdict";
			const string pValue2 = "schwing";
			Result result =
				new Result("\n" + ExpectedTargetPageName + "   ?" + pKey1 + "=" + pValue1 + "&" + pKey2 + "=" + pValue2);
			Assert.AreEqual(ResultMode.Transfer, result.Mode, "Not extracting the correct ResultMode " +
															  "from the result string passed into the ctor.");
			Assert.IsNotNull(result.Parameters,
							 "Parameters were passed but the Parameters property appears to be null (incorrectly).");
			Assert.AreEqual(2, result.Parameters.Count,
							"Wrong number of query parameters parsed (supplied two key-value pairs).");
			Assert.IsTrue(result.Parameters.Contains(pKey1), "The 'id' parameter is not being correctly extracted " +
															 "from the result string passed into the ctor.");
			Assert.AreEqual(pValue1, result.Parameters[pKey1]);
			Assert.IsTrue(result.Parameters.Contains(pKey2),
						  "The 'verdict' parameter is not being correctly extracted " +
						  "from the result string passed into the ctor.");
			Assert.AreEqual(pValue2, result.Parameters[pKey2]);
			Assert.AreEqual(ExpectedTargetPageName, result.TargetPage,
							"The TargetPage property is not being correctly extracted " +
							"from the result string passed into the ctor.");
		}

		[Test]
		public void WithWhitespacedPageAndTransferResultModeAndTwoParametersUsingCommas()
		{
			const string pKey1 = "id";
			const string pValue1 = "Nina Persson";
			const string pKey2 = "verdict";
			const string pValue2 = "schwing";
			Result result =
				new Result("\n" + ExpectedTargetPageName + "   ?" + pKey1 + "=" + pValue1 + "," + pKey2 + "=" + pValue2);
			Assert.AreEqual(ResultMode.Transfer, result.Mode, "Not extracting the correct ResultMode " +
															  "from the result string passed into the ctor.");
			Assert.IsNotNull(result.Parameters,
							 "Parameters were passed but the Parameters property appears to be null (incorrectly).");
			Assert.AreEqual(2, result.Parameters.Count,
							"Wrong number of query parameters parsed (supplied two key-value pairs).");
			Assert.IsTrue(result.Parameters.Contains(pKey1), "The 'id' parameter is not being correctly extracted " +
															 "from the result string passed into the ctor.");
			Assert.AreEqual(pValue1, result.Parameters[pKey1]);
			Assert.IsTrue(result.Parameters.Contains(pKey2),
						  "The 'verdict' parameter is not being correctly extracted " +
						  "from the result string passed into the ctor.");
			Assert.AreEqual(pValue2, result.Parameters[pKey2]);
			Assert.AreEqual(ExpectedTargetPageName, result.TargetPage,
							"The TargetPage property is not being correctly extracted " +
							"from the result string passed into the ctor.");
		}

		[Test]
		public void TargetPageMayBeRuntimeExpression()
		{
			Result result = new Result("%{['var1']}");
			string resultUri = result.GetRedirectUri( MakeDictionary("var1", "val1") );
			Assert.AreEqual( "val1", resultUri );
		}

		[Test]
		public void ParameterKeysAndValuesMayBeRuntimeExpression()
		{
			Result result = new Result("redirect:%{['var1']}?%{['var2']}=%{['var3']}");
			string resultUri = result.GetRedirectUri( MakeDictionary("var1", "val1", "var2", "val2", "var3", "val3") );
			Assert.AreEqual( "val1?val2=val3", resultUri );
		}

		private IDictionary MakeDictionary( params object[] args)
		{
			AssertUtils.IsTrue(args.Length % 2 == 0);
			Hashtable ht= new Hashtable();
			for(int i=0;i<args.Length;i+=2)
			{
				ht[args[i]] = args[i+1];
			}
			return ht;
		}
	}
}