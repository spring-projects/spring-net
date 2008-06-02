#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using System.Text;
using System.Web;
using Spring.Expressions;
using Spring.Util;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// Represents the ASPX result page that an operation maps to.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Parameters can reference variables accessible from the page using the
    /// standard NAnt style property notation, namely <c>${varName}</c>.
    /// </p>
    /// <p>
    /// The <c>result</c> <see cref="System.String"/> that is passed as the sole
    /// parameter to the parameterized constructor
    /// (<see cref="Spring.Web.Support.Result(string)"/>) must adhere to the
    /// following format:
    /// </p>
    /// <code>
    /// [&lt;mode&gt;:]&lt;targetPage&gt;[?param1,param2,...,paramN]
    /// </code>
    /// <p>
    /// Only the <c>targetPage</c> is mandatory.
    /// </p>
    /// </remarks>
    /// <example>
    /// <p>
    /// Examples of valid <see cref="System.String"/> values that can be passed
    /// to the the parameterized constructor
    /// (<see cref="Spring.Web.Support.Result(string)"/>) include:
    /// </p>
    /// <p>
    /// <list type="bullet">
    /// <item><description>Login.aspx</description></item>
    /// <item><description>~/Login.aspx</description></item>
    /// <item><description>redirect:~/Login.aspx</description></item>
    /// <item><description>transfer:~/Login.aspx</description></item>
    /// <item><description>transfer:Services/Register.aspx</description></item>
    /// <item><description>Login.aspx?username=springboy,password=7623AAjoe</description></item>
    /// <item><description>redirect:Login.aspx?username=springboy,password=7623AAjoe</description></item>
    /// </list>
    /// </p>
    /// </example>
    /// <author>Aleksandar Seovic</author>
    /// <author>Matan Shapira</author>
    [Serializable]
    public class Result
    {
        #region Constants

        /// <summary>
        /// The default <see cref="Spring.Web.Support.ResultMode"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Currently defaults to <see cref="Spring.Web.Support.ResultMode.Transfer"/>.
        /// </p>
        /// </remarks>
        /// <seealso cref="Spring.Web.Support.ResultMode"/>
        public const ResultMode DefaultResultMode = ResultMode.Transfer;

        #endregion

        #region Fields

        private ResultMode mode = DefaultResultMode;
        private string targetPage;
        private IDictionary parameters;

        #endregion

        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.Web.Support.Result"/> class.
        /// </summary>
        public Result()
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.Web.Support.Result"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// See both the class documentation (<see cref="Spring.Web.Support.Result"/>)
        /// and the reference documentation for the Spring.Web library for a
        /// discussion and examples of what values the supplied <paramref name="result"/>
        /// can have.
        /// </p>
        /// </remarks>
        /// <param name="result">The result descriptor.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="result"/> is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </exception>
        public Result(string result)
        {
            AssertUtils.ArgumentHasText(result, "result");
            result = ExtractAndSetResultMode(result);
            int indexOfQueryStringDelimiter = result.IndexOf('?');
            if (indexOfQueryStringDelimiter > 0)
            {
                ParseParameters(result.Substring(indexOfQueryStringDelimiter + 1));
                result = result.Substring(0, indexOfQueryStringDelimiter);
            }
            targetPage = result.Trim();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The <see cref="Spring.Web.Support.ResultMode"/>. Defines which
        /// method will be used to navigate to the
        /// <see cref="Spring.Web.Support.Result.TargetPage"/>.
        /// </summary>
        public ResultMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        /// <summary>
        /// The target page.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is the (relative) path to the target page. 
        /// </p>
        /// </remarks>
        /// <example>
        /// <p>
        /// Examples of valid values would be:
        /// </p>
        /// <p>
        /// <list type="bullet">
        /// <item><description>Login.aspx</description></item>
        /// <item><description>~/Login.aspx</description></item>
        /// <item><description>~/B2B/SignUp.aspx</description></item>
        /// <item><description>B2B/Foo/FooServices.aspx</description></item>
        /// </list>
        /// </p>
        /// </example>
        public string TargetPage
        {
            get { return targetPage; }
            set { targetPage = value; }
        }

        /// <summary>
        /// The parameters thar are to be passed to the
        /// <see cref="Spring.Web.Support.Result.TargetPage"/> upon
        /// navigation.
        /// </summary>
        public IDictionary Parameters
        {
            get { return parameters; }
            set { parameters = value; }
        }

        /// <summary>
        /// Indicates, if <see cref="HttpServerUtility.Transfer(string,bool)"/> should be called with preserverForm='true' | 'false'. Only relevant for ResultMode.TransferXXXX modes.
        /// </summary>
        public bool PreserveForm
        {
            get { return (Mode == ResultMode.Transfer); }
        }

        #endregion

        /// <summary>
        /// Navigates to the <see cref="Spring.Web.Support.Result.TargetPage"/>
        /// defined by this result.
        /// </summary>
        /// <param name="page">
        /// The context object for parameter resolution. This is typically
        /// a <see cref="System.Web.UI.Page"/>.
        /// </param>
        public virtual void Navigate(object page)
        {
            switch (Mode)
            {
                case ResultMode.Redirect:
                    DoRedirect(page);
                    break;
                case ResultMode.Transfer:
                case ResultMode.TransferNoPreserve:
                    DoTransfer(page);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(string.Format("Unknown ResultMode {0}", Mode));
            }
        }

        /// <summary>
        /// Performs a server-side transfer to the
        /// <see cref="Spring.Web.Support.Result.TargetPage"/> defined by this result.
        /// </summary>
        /// <param name="page">
        /// The context object for parameter resolution. This is typically
        /// a <see cref="System.Web.UI.Page"/>.
        /// </param>
        /// <seealso cref="System.Web.HttpServerUtility.Transfer(string,bool)"/>
        protected virtual void DoTransfer(object page)
        {
            HttpContext ctx = HttpContext.Current;
            SetTransferParameters(ctx.Items, page);
            ctx.Server.Transfer(TargetPage, PreserveForm);
        }

        /// <summary>
        /// Resolves transfer parameters and stores them into <see cref="IDictionary" /> instance.
        /// </summary>
        /// <param name="contextDictionary"></param>
        /// <param name="page"></param>
        protected void SetTransferParameters(IDictionary contextDictionary, object page)
        {
            if (this.parameters != null && this.parameters.Count > 0)
            {
                foreach (DictionaryEntry entry in this.parameters)
                {
                    string value = entry.Value.ToString();
                    if (IsRuntimeExpression(value))
                    {
                        contextDictionary[entry.Key] = ResolveRuntimeExpression(page, value);
                    }
                    else
                    {
                        contextDictionary[entry.Key] = entry.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Performs a redirect to the
        /// <see cref="Spring.Web.Support.Result.TargetPage"/> defined by this
        /// result.
        /// </summary>
        /// <param name="page">
        /// The context object for parameter resolution. This is typically
        /// a <see cref="System.Web.UI.Page"/>.
        /// </param>
        /// <seealso cref="System.Web.HttpResponse.Redirect(string)"/>
        protected virtual void DoRedirect(object page)
        {
            HttpContext.Current.Response.Redirect(GetRedirectUri(page));
        }

        /// <summary>
        /// Returns a redirect url string that points to the 
        /// <see cref="Spring.Web.Support.Result.TargetPage"/> defined by this
        /// result.
        /// </summary>
        /// <param name="page">
        /// A redirect url string.
        /// </param>
        public string GetRedirectUri(object page)
        {
            StringBuilder url = new StringBuilder(256);
            url.Append(TargetPage);
            if (parameters != null && parameters.Count > 0)
            {
                char separator = '?';
                foreach (DictionaryEntry entry in parameters)
                {
                    url.Append(separator);
                    url.Append(entry.Key).Append('=');
                    string value = entry.Value.ToString();
                    if (IsRuntimeExpression(value))
                    {
                        url.Append(HttpContext.Current.Server.UrlEncode(
                                    ResolveRuntimeExpression(page, value).ToString()));
                    }
                    else
                    {
                        url.Append(HttpContext.Current.Server.UrlEncode(value));
                    }
                    separator = '&';
                }
            }
            return url.ToString();
        }

        private static bool IsRuntimeExpression(string value)
        {
            // allow for 2 alternative prefixes (SPRNET-864)
            return (value.StartsWith("${") || value.StartsWith("%{")) && value.EndsWith("}");
        }

        private static object ResolveRuntimeExpression(object page, string value)
        {
            return ExpressionEvaluator.GetValue(page, value.Substring(2, value.Length - 3));
        }

        /// <summary>
        /// Extracts and sets this instance's <see cref="Spring.Web.Support.Result.Mode"/>
        /// property from the supplied <paramref name="result"/> descriptor.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The supplied <paramref name="result"/> descriptor is typically
        /// something like <c>/Foo.aspx</c> or
        /// <c>redirect:http://www.springframework.net/</c>.
        /// </p>
        /// </remarks>
        /// <param name="result">
        /// The result descriptor.
        /// </param>
        /// <returns>
        /// The supplied <paramref name="result"/> without the result mode
        /// prefix (if any).
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If the supplied <paramref name="result"/> starts with an illegal
        /// result mode (see <see cref="Spring.Web.Support.ResultMode"/>).
        /// </exception>
        private string ExtractAndSetResultMode(string result)
        {
            int indexOfResultModeDelimiter = result.IndexOf(':');
            if (indexOfResultModeDelimiter > 0)
            {
                try
                {
                    Mode = (ResultMode)Enum.Parse(typeof(ResultMode),
                                                   result.Substring(0, indexOfResultModeDelimiter), true);
                    return result.Substring(indexOfResultModeDelimiter + 1);
                }
                catch
                {
                    throw new ArgumentOutOfRangeException("result", result, "Illegal result mode.");
                }
            }
            return result;
        }

        /// <summary>
        /// Parses query parameters from the supplied <paramref name="queryString"/>.
        /// </summary>
        /// <param name="queryString">
        /// The query string (may be <cref lang="null"/>).
        /// </param>
        private void ParseParameters(string queryString)
        {
            if (StringUtils.HasText(queryString))
            {
                this.parameters = new Hashtable();
                string[] nameValuePairs = queryString.Split("&,".ToCharArray());
                foreach (string pair in nameValuePairs)
                {
                    int n = pair.IndexOf('=');
                    if (n > 0)
                    {
                        string name = pair.Substring(0, n);
                        string val = pair.Substring(n + 1).Trim();
                        this.parameters[name] = val;
                    }
                }
            }
        }
    }
}