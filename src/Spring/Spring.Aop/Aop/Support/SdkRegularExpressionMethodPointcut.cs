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

using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Common.Logging;
using Spring.Util;

namespace Spring.Aop.Support
{
	/// <summary>
	/// Regular expression based pointcut object.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Uses the regular expression classes from the .NET Base Class Library.
	/// </p>
	/// <p>
	/// The regular expressions must be a match. For example, the
	/// <code>.*Get*</code> pattern will match <c>Com.Mycom.Foo.GetBar()</c>, and
	/// <code>Get.*</code> will not.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Simon White (.NET)</author>
	[Serializable]
	public class SdkRegularExpressionMethodPointcut : AbstractRegularExpressionMethodPointcut
	{
        [NonSerialized]
		private static readonly ILog _logger = LogManager.GetLogger(typeof(SdkRegularExpressionMethodPointcut));

		private Regex[] _compiledPatterns = new Regex[0];
        private RegexOptions _defaultOptions = RegexOptions.None;

	    /// <summary>
		/// Creates a new instance of the
		/// <see cref="SdkRegularExpressionMethodPointcut"/> class.
		/// </summary>
		public SdkRegularExpressionMethodPointcut()
		{
		}

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="SdkRegularExpressionMethodPointcut"/> class,
        /// using the supplied pattern or <paramref name="patterns"/>.
        /// </summary>
        /// <param name="patterns">
        /// The intial pattern value(s) to be matched against.
        /// </param>
        public SdkRegularExpressionMethodPointcut(params string[] patterns)
        {
            Patterns = patterns;
        }

	    /// <inheritdoc />
	    protected SdkRegularExpressionMethodPointcut(SerializationInfo info, StreamingContext context)
	        : base(info, context)
	    {
	    }

	    /// <summary>
	    /// Gets or sets default options that should be used by
	    /// regular expressions that don't have options explicitly set.
	    /// </summary>
	    /// <value>
	    /// Default options that should be used by regular expressions
	    /// that don't have options explicitly set.
	    /// </value>
	    public RegexOptions DefaultOptions
	    {
	        get { return _defaultOptions; }
	        set
	        {
	            _defaultOptions = value;
	            InitPatternRepresentation(Patterns);
	        }
	    }

	    /// <summary>
	    /// Initializes the regular expression pointcuts.
	    /// </summary>
	    /// <remarks>
	    /// <p>
	    /// Can be invoked multiple times.
	    /// </p>
	    /// <p>
	    /// This method will be invoked from the
	    /// <see cref="AbstractRegularExpressionMethodPointcut.Patterns"/> property,
	    /// and also on deserialization.
	    /// </p>
	    /// </remarks>
	    /// <param name="patterns">
	    /// The patterns to initialize.
	    /// </param>
	    /// <exception cref="System.ArgumentException">
	    /// In the case of an invalid pattern.
	    /// </exception>
	    /// <exception cref="System.ArgumentNullException">
	    /// If the supplied <paramref name="patterns"/> is <see langword="null"/>.
	    /// </exception>
	    protected override void InitPatternRepresentation(object[] patterns)
	    {
	        AssertUtils.ArgumentNotNull(patterns, "patterns");

            if (patterns.Length > 0)
            {
                _compiledPatterns = new Regex[patterns.Length];
                for (int i = 0; i < patterns.Length; i++)
                {
                    if (patterns[i] == null)
                    {
                        throw new ArgumentNullException(
                            "Null is not a valid value for an element of the Patterns property.");
                    }
                    else if (patterns[i] is Regex)
                    {
                        _compiledPatterns[i] = (Regex)patterns[i];
                    }
                    else if (patterns[i] is string)
                    {
                        _compiledPatterns[i] = new Regex((string)patterns[i], DefaultOptions);
                    }
                    else
                    {
                        throw new ArgumentException(
                            "You can only specify a string value or an instance of a Regex class " +
                            "as an element of the 'Patterns' property.");
                    }
                }
            }
	    }

	    /// <summary>
	    /// Does the pattern at the supplied <paramref name="patternIndex"/>
	    /// match this <paramref name="pattern"/>?
	    /// </summary>
	    /// <param name="pattern">The pattern to match</param>
	    /// <param name="patternIndex">The index of pattern.</param>
	    /// <returns>
	    /// <see langword="true"/> if there is a match.
	    /// </returns>
	    protected override bool Matches(string pattern, int patternIndex)
	    {
	        Match match = _compiledPatterns[patternIndex].Match(pattern);
	        bool matched = match.Success;

	        if (_logger.IsDebugEnabled)
	        {
	            _logger.Debug("Candidate is: '" + pattern + "'; pattern is '" +
	                          _compiledPatterns[patternIndex].ToString() + "'; matched=" + matched);
	        }

	        return matched;
	    }
	}
}
