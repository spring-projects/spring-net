#region License

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

#endregion

#region Imports

using System.Collections;
using System.Text;
using System.Web;
using Spring.Collections;
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
    public class Result : IResult
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
        /// <exception cref="ArgumentOutOfRangeException">if the result mode is unknown.</exception>
        public Result( string result )
        {
            AssertUtils.ArgumentHasText( result, "result" );

            result = ExtractAndSetResultMode( result );

            int indexOfQueryStringDelimiter = result.IndexOf( '?' );
            if (indexOfQueryStringDelimiter > 0)
            {
                ParseParameters( result.Substring( indexOfQueryStringDelimiter + 1 ) );
                result = result.Substring( 0, indexOfQueryStringDelimiter );
            }
            targetPage = result.Trim();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.Web.Support.Result"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// See both the class documentation (<see cref="Spring.Web.Support.Result"/>)
        /// and the reference documentation for the Spring.Web library for a
        /// discussion and examples of what values the supplied <paramref name="resultText"/>
        /// can have.
        /// </p>
        /// </remarks>
        /// <param name="resultMode">The desired result mode. May be null to use default mode.</param>
        /// <param name="resultText">The result descriptor (without resultMode prefix!).</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="resultText"/> is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">if the result mode is unknown.</exception>
        public Result( string resultMode, string resultText )
        {
            AssertUtils.ArgumentHasText( resultText, "resultText" );

            this.SetResultMode( resultMode );

            string result = resultText;
            int indexOfQueryStringDelimiter = result.IndexOf( '?' );
            if (indexOfQueryStringDelimiter > 0)
            {
                ParseParameters( result.Substring( indexOfQueryStringDelimiter + 1 ) );
                result = result.Substring( 0, indexOfQueryStringDelimiter );
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
        /// Indicates, if <see cref="HttpServerUtility.Transfer(string,bool)"/> should be called with preserveForm='true' | 'false'. Only relevant for ResultMode.TransferXXXX modes.
        /// </summary>
        public bool PreserveForm
        {
            get { return (Mode == ResultMode.Transfer); }
        }

        /// <summary>
        /// Indicates, if <see cref="HttpResponse.Redirect(string,bool)"/> should be called with endResponse='true' | 'false'. Only relevant for ResultMode.RedirectXXXX modes.
        /// </summary>
        public bool EndResponse
        {
            get { return (Mode == ResultMode.Redirect); }
        }

        #endregion

        /// <summary>
        /// Navigates to the <see cref="Spring.Web.Support.Result.TargetPage"/>
        /// defined by this result.
        /// </summary>
        /// <param name="context">
        /// The context object for parameter resolution. This is typically
        /// a <see cref="System.Web.UI.Page"/>.
        /// </param>
        public virtual void Navigate( object context )
        {
            switch (Mode)
            {
                case ResultMode.Redirect:
                case ResultMode.RedirectNoAbort:
                    DoRedirect( context );
                    break;
                case ResultMode.Transfer:
                case ResultMode.TransferNoPreserve:
                    DoTransfer( context );
                    break;
                default:
                    throw new ArgumentOutOfRangeException( string.Format( "Unknown ResultMode {0}", Mode ) );
            }
        }

        /// <summary>
        /// Performs a server-side transfer to the
        /// <see cref="Spring.Web.Support.Result.TargetPage"/> defined by this result.
        /// </summary>
        /// <param name="context">
        /// The context object for parameter resolution. This is typically
        /// a <see cref="System.Web.UI.Page"/>.
        /// </param>
        /// <seealso cref="System.Web.HttpServerUtility.Transfer(string,bool)"/>
        protected virtual void DoTransfer( object context )
        {
            HttpContext ctx = HttpContext.Current;
            SetTransferParameters( ctx.Items, context );
            ctx.Server.Transfer( GetResolvedTargetPage( context ), PreserveForm );
        }

        /// <summary>
        /// Resolves transfer parameters and stores them into <see cref="IDictionary" /> instance.
        /// </summary>
        /// <param name="contextDictionary"></param>
        /// <param name="context"></param>
        protected void SetTransferParameters( IDictionary contextDictionary, object context )
        {
            if (this.parameters != null && this.parameters.Count > 0)
            {
                foreach (DictionaryEntry entry in this.parameters)
                {
                    string value = entry.Value.ToString();
                    if (IsSpELRuntimeExpression( value ))
                    {
                        contextDictionary[entry.Key] = ResolveValueIfNecessary( context, value );
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
        /// <param name="context">
        /// The context object for parameter resolution. This is typically
        /// a <see cref="System.Web.UI.Page"/>.
        /// </param>
        /// <seealso cref="System.Web.HttpResponse.Redirect(string)"/>
        protected virtual void DoRedirect( object context )
        {
            HttpContext.Current.Response.Redirect( GetRedirectUri( context ), EndResponse );
        }

        /// <summary>
        /// Returns a redirect url string that points to the
        /// <see cref="Spring.Web.Support.Result.TargetPage"/> defined by this
        /// result.
        /// </summary>
        /// <param name="context">
        /// A redirect url string.
        /// </param>
        public virtual string GetRedirectUri( object context )
        {
            string path = GetResolvedTargetPage( context );

            IDictionary resolvedParameters = null;
            if (this.Parameters != null && this.Parameters.Count > 0)
            {
                resolvedParameters = new CaseInsensitiveHashtable();
                foreach (DictionaryEntry entry in this.Parameters)
                {
                    object key = ResolveValueIfNecessary( context, entry.Key.ToString() );
                    object value = ResolveValueIfNecessary( context, entry.Value.ToString() );
                    resolvedParameters[key] = value;
                }
            }

            return BuildUrl( path, resolvedParameters );
        }

        /// <summary>
        /// Construct the actual url to be executed or returned.
        /// </summary>
        /// <param name="resolvedPath">the already evaluated <see cref="TargetPage"/></param>
        /// <param name="resolvedParameters">the already evaluated parameters.</param>
        /// <returns>the url to be returned by <see cref="GetRedirectUri"/></returns>
        protected virtual string BuildUrl( string resolvedPath, IDictionary resolvedParameters )
        {
            StringBuilder url = new StringBuilder( 256 );
            url.Append( resolvedPath );
            if (resolvedParameters != null && resolvedParameters.Count > 0)
            {
                char separator = '?';
                foreach (DictionaryEntry entry in resolvedParameters)
                {
                    url.Append( separator );
                    url = BuildUrlParameter( url, entry.Key.ToString(), entry.Value.ToString() );
                    separator = '&';
                }
            }
            return url.ToString();
        }

        /// <summary>
        /// Append the url parameter to the url being constructed.
        /// </summary>
        /// <param name="url">the <see cref="StringBuilder"/> containing the url constructed so far.</param>
        /// <param name="key">the parameter key</param>
        /// <param name="value">the parameter value</param>
        /// <returns>the <see cref="StringBuilder"/> to use for further url construction.</returns>
        protected virtual StringBuilder BuildUrlParameter( StringBuilder url, string key, string value )
        {
            url.Append( WebUtils.UrlEncode( key ) )
               .Append( '=' )
               .Append( WebUtils.UrlEncode( value ) );

            return url;
        }

        /// <summary>
        /// Evaluates <paramref name="value"/> within <paramref name="context"/> and returns the evaluation result.
        /// </summary>
        /// <param name="context">the context to be used for evaluation.</param>
        /// <param name="value">the string that might need evaluation</param>
        /// <returns>the evaluation result. Unodified <paramref name="value"/> if no evalution occured.</returns>
        protected virtual object ResolveValueIfNecessary( object context, string value )
        {
            return ResolveSpELRuntimeExpressionIfNecessary( context, value );
        }

        /// <summary>
        /// Checks, if value is a SpEL expression <c>${expression}</c> or <c>%{expression}</c>.
        /// </summary>
        private static bool IsSpELRuntimeExpression( string value )
        {
            // allow for 2 alternative prefixes (SPRNET-864)
            return (value.StartsWith( "${" ) || value.StartsWith( "%{" )) && value.EndsWith( "}" );
        }

        /// <summary>
        /// If <paramref name="value"/> is a SpEL expression (<c>${expression}</c> or <c>%{expression}</c>), evaluates
        /// the value against <paramref name="context"/>.
        /// </summary>
        protected static object ResolveSpELRuntimeExpressionIfNecessary( object context, string value )
        {
            AssertUtils.ArgumentNotNull(value, "value");

            if (IsSpELRuntimeExpression( value ))
            {
                return ExpressionEvaluator.GetValue( context, value.Substring( 2, value.Length - 3 ) );
            }
            return value;
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
        private string ExtractAndSetResultMode( string result )
        {
            int indexOfResultModeDelimiter = result.IndexOf( ':' );
            if (indexOfResultModeDelimiter > 0)
            {
                string resultMode = result.Substring( 0, indexOfResultModeDelimiter );
                SetResultMode( resultMode );
                return result.Substring( indexOfResultModeDelimiter + 1 );
            }
            return result;
        }

        /// <summary>
        /// Set the actual <see cref="Mode"/> from the parsed <paramref name="resultMode"/> string.
        /// </summary>
        /// <param name="resultMode">the parsed result mode</param>
        protected virtual void SetResultMode( string resultMode )
        {
            try
            {
                if (StringUtils.HasText( resultMode ))
                {
                    Mode = (ResultMode)Enum.Parse( typeof( ResultMode ), resultMode, true );
                }
            }
            catch
            {
                throw new ArgumentOutOfRangeException( "resultMode", resultMode, "Illegal result mode." );
            }
        }

        /// <summary>
        /// Resolves dynamic expression contained in <see cref="TargetPage"/> if any by calling <see cref="ResolveValueIfNecessary"/>.
        /// </summary>
        /// <param name="context">the context to be used for evaluating the expression</param>
        /// <returns>the evaluated expression</returns>
        protected string GetResolvedTargetPage( object context )
        {
            return ResolveValueIfNecessary( context, TargetPage ).ToString();
        }

        /// <summary>
        /// Parses query parameters from the supplied <paramref name="queryString"/>.
        /// </summary>
        /// <param name="queryString">
        /// The query string (may be <cref lang="null"/>).
        /// </param>
        private void ParseParameters( string queryString )
        {
            if (StringUtils.HasText( queryString ))
            {
                this.parameters = new CaseInsensitiveHashtable();
                string[] nameValuePairs = queryString.Split( "&,".ToCharArray() );
                foreach (string pair in nameValuePairs)
                {
                    int n = pair.IndexOf( '=' );
                    if (n > 0)
                    {
                        string name = pair.Substring( 0, n );
                        string val = pair.Substring( n + 1 ).Trim();
                        this.parameters[name] = val;
                    }
                }
            }
        }
    }
}
