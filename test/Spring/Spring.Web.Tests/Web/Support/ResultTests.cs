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

using System.Collections;
using NUnit.Framework;
using Spring.Util;

namespace Spring.Web.Support;

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
        Assert.That(result.Mode, Is.EqualTo(Result.DefaultResultMode), "Not defaulting to the default ResultMode.");
    }

    [Test]
    public void DefaultsToTransferMode()
    {
        Assert.That(Result.DefaultResultMode, Is.EqualTo(ResultMode.Transfer),
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
        Assert.That(result.Mode, Is.EqualTo(Result.DefaultResultMode), "Not defaulting to the default ResultMode.");
        Assert.That(result.Parameters,
            Is.Null,
            "No parameters were passed but the Parameters property appears to be set to a non-null value anyway.");
        Assert.That(result.TargetPage, Is.EqualTo(ExpectedTargetPageName),
            "The TargetPage property is not being correctly extracted " +
            "from the result string passed into the ctor.");
    }

    [Test]
    public void WithWhitespacePageAndNoResultModeAndNoParameters()
    {
        Result result = new Result("   " + ExpectedTargetPageName + "\n");
        Assert.That(result.Mode, Is.EqualTo(Result.DefaultResultMode), "Not defaulting to the default ResultMode.");
        Assert.That(result.Parameters,
            Is.Null,
            "No parameters were passed but the Parameters property appears to be set to a non-null value anyway.");
        Assert.That(result.TargetPage, Is.EqualTo(ExpectedTargetPageName),
            "The TargetPage property is not being correctly extracted " +
            "from the result string passed into the ctor.");
    }

    [Test]
    public void WithTransferResultModeButNoParameters()
    {
        Result result = new Result("transfer:" + ExpectedTargetPageName);
        Assert.That(result.Mode, Is.EqualTo(ResultMode.Transfer), "Not extracting the correct ResultMode " +
                                                          "from the result string passed into the ctor.");
        Assert.That(result.PreserveForm, Is.EqualTo(true), "ResultMode.Transfer must result in PreserveForm being 'true'");
        Assert.That(result.Parameters,
            Is.Null,
            "No parameters were passed but the Parameters property appears to be set to a non-null value anyway.");
        Assert.That(result.TargetPage, Is.EqualTo(ExpectedTargetPageName),
            "The TargetPage property is not being correctly extracted " +
            "from the result string passed into the ctor.");
    }

    [Test]
    public void WithTransferNoPreserveResultMode()
    {
        Result result = new Result("transfernopreserve:" + ExpectedTargetPageName);
        Assert.That(result.Mode, Is.EqualTo(ResultMode.TransferNoPreserve), "Not extracting the correct ResultMode " +
                                                                    "from the result string passed into the ctor.");
        Assert.That(result.PreserveForm, Is.EqualTo(false), "ResultMode.TransferNoPreserver must result in PreserveForm being 'false'");
    }

    [Test]
    public void WithWhitespacedRedirectResultMode()
    {
        Result result = new Result("    redirect:" + ExpectedTargetPageName);
        Assert.That(result.Mode, Is.EqualTo(ResultMode.Redirect), "Not extracting the correct ResultMode " +
                                                          "from the result string passed into the ctor.");
        Assert.That(result.Parameters,
            Is.Null,
            "No parameters were passed but the Parameters property appears to be set to a non-null value anyway.");
        Assert.That(result.TargetPage, Is.EqualTo(ExpectedTargetPageName),
            "The TargetPage property is not being correctly extracted " +
            "from the result string passed into the ctor.");
    }

    [Test]
    public void WithRedirectResultMode()
    {
        Result result = new Result("redirect:" + ExpectedTargetPageName);
        Assert.That(result.Mode, Is.EqualTo(ResultMode.Redirect), "Not extracting the correct ResultMode " +
                                                          "from the result string passed into the ctor.");
        Assert.That(result.EndResponse, Is.True);
        Assert.That(result.Parameters,
            Is.Null,
            "No parameters were passed but the Parameters property appears to be set to a non-null value anyway.");
        Assert.That(result.TargetPage, Is.EqualTo(ExpectedTargetPageName),
            "The TargetPage property is not being correctly extracted " +
            "from the result string passed into the ctor.");
    }

    [Test]
    public void WithRedirectNoAbortResultMode()
    {
        Result result = new Result("redirectnoabort:" + ExpectedTargetPageName);
        Assert.That(result.Mode, Is.EqualTo(ResultMode.RedirectNoAbort), "Not extracting the correct ResultMode " +
                                                                 "from the result string passed into the ctor.");
        Assert.That(result.EndResponse, Is.False);
        Assert.That(result.Parameters,
            Is.Null,
            "No parameters were passed but the Parameters property appears to be set to a non-null value anyway.");
        Assert.That(result.TargetPage, Is.EqualTo(ExpectedTargetPageName),
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
        Assert.That(result.Mode, Is.EqualTo(ResultMode.Transfer), "Not extracting the correct ResultMode " +
                                                          "from the result string passed into the ctor.");
        Assert.That(result.Parameters,
            Is.Null,
            "No parameters were passed (just the parameter delimiter) but the " +
            "Parameters property appears to be set to a non-null value anyway.");
        Assert.That(result.TargetPage, Is.EqualTo(ExpectedTargetPageName),
            "The TargetPage property is not being correctly extracted " +
            "from the result string passed into the ctor.");
    }

    [Test]
    public void WithResultModeAndWhitespaceParameterDelimiterOnly()
    {
        Result result = new Result("redirect:" + ExpectedTargetPageName + "?     ");
        Assert.That(result.Mode, Is.EqualTo(ResultMode.Redirect), "Not extracting the correct ResultMode " +
                                                          "from the result string passed into the ctor.");
        Assert.That(result.Parameters,
            Is.Null,
            "No parameters were passed (just the parameter delimiter) but the " +
            "Parameters property appears to be set to a non-null value anyway.");
        Assert.That(result.TargetPage, Is.EqualTo(ExpectedTargetPageName),
            "The TargetPage property is not being correctly extracted " +
            "from the result string passed into the ctor.");
    }

    [Test]
    public void WithNoResultModeAndWhitespaceParameterDelimiterOnly()
    {
        Result result = new Result(ExpectedTargetPageName + "?     ");
        Assert.That(result.Mode, Is.EqualTo(Result.DefaultResultMode), "Not defaulting to the default ResultMode.");
        Assert.That(result.Parameters,
            Is.Null,
            "No parameters were passed (just the parameter delimiter) but the " +
            "Parameters property appears to be set to a non-null value anyway.");
        Assert.That(result.TargetPage, Is.EqualTo(ExpectedTargetPageName),
            "The TargetPage property is not being correctly extracted " +
            "from the result string passed into the ctor.");
    }

    [Test]
    public void WithNoResultModeAndOneParameter()
    {
        const string key = "id";
        const string value = "Nina Persson";
        Result result = new Result(ExpectedTargetPageName + "?" + key + "=" + value);
        Assert.That(result.Mode, Is.EqualTo(Result.DefaultResultMode), "Not defaulting to the default ResultMode.");
        Assert.That(result.Parameters,
            Is.Not.Null,
            "Parameters were passed but the Parameters property appears to be null (incorrectly).");
        Assert.That(result.Parameters.Count, Is.EqualTo(1),
            "Wrong number of query parameters parsed (only supplied one key-value pair).");
        Assert.That(result.Parameters.Contains(key), Is.True, "The 'id' parameter is not being correctly extracted " +
                                                       "from the result string passed into the ctor.");
        Assert.That(result.Parameters[key], Is.EqualTo(value));
        Assert.That(result.TargetPage, Is.EqualTo(ExpectedTargetPageName),
            "The TargetPage property is not being correctly extracted " +
            "from the result string passed into the ctor.");
    }

    [Test]
    public void WithTransferResultModeAndOneParameter()
    {
        const string key = "id";
        const string value = "Nina Persson";
        Result result = new Result("transfer:" + ExpectedTargetPageName + "?" + key + "=" + value);
        Assert.That(result.Mode, Is.EqualTo(ResultMode.Transfer), "Not extracting the correct ResultMode " +
                                                          "from the result string passed into the ctor.");
        Assert.That(result.Parameters,
            Is.Not.Null,
            "Parameters were passed but the Parameters property appears to be null (incorrectly).");
        Assert.That(result.Parameters.Count, Is.EqualTo(1),
            "Wrong number of query parameters parsed (only supplied one key-value pair).");
        Assert.That(result.Parameters.Contains(key), Is.True, "The 'id' parameter is not being correctly extracted " +
                                                       "from the result string passed into the ctor.");
        Assert.That(result.Parameters[key], Is.EqualTo(value));
        Assert.That(result.TargetPage, Is.EqualTo(ExpectedTargetPageName),
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
        Assert.That(result.Mode, Is.EqualTo(Result.DefaultResultMode), "Not defaulting to the default ResultMode.");
        Assert.That(result.Parameters,
            Is.Not.Null,
            "Parameters were passed but the Parameters property appears to be null (incorrectly).");
//            Assert.That(result.Parameters.Count, Is.EqualTo(1), //                            "Wrong number of query parameters parsed (only supplied one key-value pair).");
//            Assert.That(result.Parameters.Contains(key), Is.True, "The 'id' parameter is not being correctly extracted " +
//                                                           "from the result string passed into the ctor.");
//            Assert.That(result.Parameters[key], Is.EqualTo(value));
//            Assert.That(result.TargetPage, Is.EqualTo(ExpectedTargetPageName), //                            "The TargetPage property is not being correctly extracted " +
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
        Assert.That(result.Mode, Is.EqualTo(ResultMode.Transfer), "Not extracting the correct ResultMode " +
                                                          "from the result string passed into the ctor.");
        Assert.That(result.Parameters,
            Is.Not.Null,
            "Parameters were passed but the Parameters property appears to be null (incorrectly).");
        Assert.That(result.Parameters.Count, Is.EqualTo(2),
            "Wrong number of query parameters parsed (supplied two key-value pairs).");
        Assert.That(result.Parameters.Contains(pKey1), Is.True, "The 'id' parameter is not being correctly extracted " +
                                                         "from the result string passed into the ctor.");
        Assert.That(result.Parameters[pKey1], Is.EqualTo(pValue1));
        Assert.That(result.Parameters.Contains(pKey2),
            Is.True,
            "The 'verdict' parameter is not being correctly extracted " +
            "from the result string passed into the ctor.");
        Assert.That(result.Parameters[pKey2], Is.EqualTo(pValue2));
        Assert.That(result.TargetPage, Is.EqualTo(ExpectedTargetPageName),
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
        Assert.That(result.Mode, Is.EqualTo(ResultMode.Transfer), "Not extracting the correct ResultMode " +
                                                          "from the result string passed into the ctor.");
        Assert.That(result.Parameters,
            Is.Not.Null,
            "Parameters were passed but the Parameters property appears to be null (incorrectly).");
        Assert.That(result.Parameters.Count, Is.EqualTo(2),
            "Wrong number of query parameters parsed (supplied two key-value pairs).");
        Assert.That(result.Parameters.Contains(pKey1), Is.True, "The 'id' parameter is not being correctly extracted " +
                                                         "from the result string passed into the ctor.");
        Assert.That(result.Parameters[pKey1], Is.EqualTo(pValue1));
        Assert.That(result.Parameters.Contains(pKey2),
            Is.True,
            "The 'verdict' parameter is not being correctly extracted " +
            "from the result string passed into the ctor.");
        Assert.That(result.Parameters[pKey2], Is.EqualTo(pValue2));
        Assert.That(result.TargetPage, Is.EqualTo(ExpectedTargetPageName),
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
        Assert.That(result.Mode, Is.EqualTo(ResultMode.Transfer), "Not extracting the correct ResultMode " +
                                                          "from the result string passed into the ctor.");
        Assert.That(result.Parameters,
            Is.Not.Null,
            "Parameters were passed but the Parameters property appears to be null (incorrectly).");
        Assert.That(result.Parameters.Count, Is.EqualTo(2),
            "Wrong number of query parameters parsed (supplied two key-value pairs).");
        Assert.That(result.Parameters.Contains(pKey1), Is.True, "The 'id' parameter is not being correctly extracted " +
                                                         "from the result string passed into the ctor.");
        Assert.That(result.Parameters[pKey1], Is.EqualTo(pValue1));
        Assert.That(result.Parameters.Contains(pKey2),
            Is.True,
            "The 'verdict' parameter is not being correctly extracted " +
            "from the result string passed into the ctor.");
        Assert.That(result.Parameters[pKey2], Is.EqualTo(pValue2));
        Assert.That(result.TargetPage, Is.EqualTo(ExpectedTargetPageName),
            "The TargetPage property is not being correctly extracted " +
            "from the result string passed into the ctor.");
    }

    [Test]
    public void TargetPageMayBeRuntimeExpression()
    {
        Result result = new Result("%{['var1']}");
        string resultUri = result.GetRedirectUri(MakeDictionary("var1", "val1"));
        Assert.That(resultUri, Is.EqualTo("val1"));
    }

    [Test]
    public void ParameterKeysAndValuesMayBeRuntimeExpression()
    {
        Result result = new Result("redirect:%{['var1']}?%{['var2']}=%{['var3']}");
        string resultUri = result.GetRedirectUri(MakeDictionary("var1", "val1", "var2", "val2", "var3", "val3"));
        Assert.That(resultUri, Is.EqualTo("val1?val2=val3"));
    }

    private IDictionary MakeDictionary(params object[] args)
    {
        AssertUtils.IsTrue(args.Length % 2 == 0);
        Hashtable ht = new Hashtable();
        for (int i = 0; i < args.Length; i += 2)
        {
            ht[args[i]] = args[i + 1];
        }

        return ht;
    }
}
