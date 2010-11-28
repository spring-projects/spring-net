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
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Spring.Util
{
    // TODO : Check .NET 3.5 class
    // TODO : Back to original Java behavior Expand(params string[]) method ?

    /// <summary>
    /// Represents a URI template. An URI template is a URI-like String that contained variables 
    /// marked of in braces {}, which can be expanded to produce a URI.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Bruno Baia (.NET)</author>
    public class UriTemplate
    {
        /** Captures URI template variable names. */
        private static Regex VARIABLENAMES_REGEX = new Regex(@"\{([^/]+?)\}", RegexOptions.Compiled);
        //private static Regex VARIABLENAMES_REGEX = new Regex(@"\{[^{}]+\}", RegexOptions.Compiled);
        
        /** Replaces template variables in the URI template. */
	    private static string VARIABLEVALUE_PATTERN = "(?<{0}>.*)";
        
        private const string BRACE_LEFT = "{";
        private const string BRACE_RIGHT = "}";

        private string uriTemplate;
        private string[] variableNames;
        private Regex matchRegex;

        /// <summary>
        /// Gets the names of the variables in the template, in order.
        /// </summary>
        public string[] VariableNames
        {
            get { return this.variableNames; }
        }

        /// <summary>
        /// Creates a new instance of <see cref="UriTemplate"/> with the given URI String.
        /// </summary>
        /// <param name="uriTemplate">The URI template string.</param>
        public UriTemplate(string uriTemplate)
        {
            this.uriTemplate = uriTemplate;
            Parser parser = new Parser(uriTemplate);
            this.variableNames = parser.GetVariableNames();
            this.matchRegex = parser.GetMatchRegex();
        }

        /// <summary>
        /// Given the dictionary of variables, expands this template into a full URI. 
        /// The dictionary keys represent variable names, the dicitonary values variable values. 
        /// The order of variables is not significant.
        /// </summary>
        /// <example>
        /// <code>
        /// UriTemplate template = new UriTemplate("http://example.com/hotels/{hotel}/bookings/{booking}");
        /// IDictionary&lt;string, string&gt; uriVariables = new Dictionary&lt;String, String&gt;();
        /// uriVariables.Add("booking", "42");
        /// uriVariables.Add("hotel", "1");
        /// Console.Out.WriteLine(template.Expand(uriVariables));
        /// </code>
        /// will print: <blockquote>http://example.com/hotels/1/bookings/42</blockquote>
        /// </example>
        /// <param name="uriVariables">The dictionary of URI variables.</param>
        /// <returns>The expanded URI</returns>
        public Uri Expand(IDictionary<string, string> uriVariables)
        {
            if (uriVariables.Count != this.variableNames.Length)
            {
                throw new ArgumentException(String.Format(
                        "Invalid amount of variables values in '{0}': expected {1}; got {2}",
                        this.uriTemplate, this.variableNames.Length, uriVariables.Count));
            }

            string uri = this.uriTemplate;
            foreach (string variableName in this.variableNames)
            {
                if (!uriVariables.ContainsKey(variableName))
                {
                    throw new ArgumentException(String.Format(
                        "'uriVariables' dictionary has no value for '{0}'",
                        variableName));
                }
                uri = Replace(uri, variableName, uriVariables[variableName]);
            }

            return new Uri(uri, UriKind.RelativeOrAbsolute);

            //string[] uriVariableValues = new String[this.variableNames.Length];
            //for (int i = 0; i < this.variableNames.Length; i++)
            //{
            //    string variableName = this.variableNames[i];
            //    if (!uriVariables.ContainsKey(variableName))
            //    {
            //        throw new ArgumentException(String.Format(
            //            "'uriVariables' dictionary has no value for '{0}'",
            //            variableName));
            //    }
            //    uriVariableValues[i] = uriVariables[variableName];
            //}
            //return Expand(uriVariableValues);
        }

        /// <summary>
        /// Given an array of variables, expands this template into a full URI. 
        /// The array represent variable values. The order of variables is significant.
        /// </summary>
        /// <example>
        /// <code>
        /// UriTemplate template = new UriTemplate("http://example.com/hotels/{hotel}/bookings/{booking}");
        /// Console.Out.WriteLine(template.Expand("1", "42"));
        /// </code>
        /// will print: <blockquote>http://example.com/hotels/1/bookings/42</blockquote>
        /// </example>
        /// <param name="uriVariableValues">The array of URI variables.</param>
        /// <returns>The expanded URI</returns>
        public Uri Expand(params string[] uriVariableValues)
        {
            if (uriVariableValues.Length != this.variableNames.Length)
            {
                throw new ArgumentException(String.Format(
                        "Invalid amount of variables values in '{0}': expected {1}; got {2}",
                        this.uriTemplate, this.variableNames.Length, uriVariableValues.Length));
            }

            string uri = this.uriTemplate;
            for (int i = 0; i < this.variableNames.Length; i++)
            {
                uri = Replace(uri, this.variableNames[i], uriVariableValues[i]);
            }

            return new Uri(uri, UriKind.RelativeOrAbsolute);
        }

        /// <summary>
        /// Indicates whether the given URI matches this template.
        /// </summary>
        /// <param name="uri">The URI to match to.</param>
        /// <returns><see langword="true"/> if it matches; otherwise <see langword="false"/></returns>
        public bool Matches(string uri)
        {
            if (uri == null)
            {
                return false;
            }
            return this.matchRegex.IsMatch(uri);
        }

        /// <summary>
        /// Match the given URI to a dictionary of variable values. Keys in the returned map are variable names, 
        /// values are variable values, as occurred in the given URI
        /// </summary>
        /// <example>
        /// <code>
        /// UriTemplate template = new UriTemplate("http://example.com/hotels/{hotel}/bookings/{booking}");
        /// Console.Out.WriteLine(template.Match("http://example.com/hotels/1/bookings/42"));
        /// </code>
        /// will print: <blockquote>{hotel=1, booking=42}</blockquote>
        /// </example>
        /// <param name="uri">The URI to match to.</param>
        /// <returns>A dictionary of variable values.</returns>
        public IDictionary<string, string> Match(string uri)
        {
            AssertUtils.ArgumentNotNull(uri, "uri");

            IDictionary<string, string> result = new Dictionary<string, string>();
            Match match = this.matchRegex.Match(uri);
            for (int i = 1; i < match.Groups.Count; i++ )
            {
                result.Add(this.matchRegex.GroupNameFromNumber(i), match.Groups[i].Value);
            }
            return result;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object."/>
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object."/>.
        /// </returns>
        public override string ToString()
        {
            return this.uriTemplate;
        }

        //private static string[] GetVariableNames(string uriTemplate)
        //{
        //    List<string> variableNames = new List<string>();
        //    foreach (Match match in VARIABLENAMES_REGEX.Matches(uriTemplate))
        //    {
        //        string token = match.Value;
        //        token = token.Substring(1, token.Length - 2);

        //        if (!variableNames.Contains(token))
        //        {
        //            variableNames.Add(token);
        //        }
        //    }

        //    return variableNames.ToArray();
        //}

        private static string Replace(string uriTemplate, string token, string value)
        {
            string quotedToken = BRACE_LEFT + token + BRACE_RIGHT;
            return uriTemplate.Replace(quotedToken, value);
        }

        /**
	     * Static inner class to parse uri template strings into a matching regular expression.
	     */
	    private class Parser 
        {
		    private List<String> variableNames = new List<String>();
		    private StringBuilder patternBuilder = new StringBuilder();

		    public Parser(string uriTemplate) 
            {
			    AssertUtils.ArgumentNotNull(uriTemplate, "'uriTemplate' must not be null");

                int index = 0;
                this.patternBuilder.Append("^");
                foreach (Match match in VARIABLENAMES_REGEX.Matches(uriTemplate))
                {
                    string variableName = match.Groups[1].Value;
                    if (!variableNames.Contains(variableName))
                    {
                        variableNames.Add(variableName);
                    }

                    this.patternBuilder.Append(Escape(uriTemplate, index, match.Index - index));
                    this.patternBuilder.Append(String.Format(VARIABLEVALUE_PATTERN, variableName));
                    index = match.Index + match.Length;
                }
                this.patternBuilder.Append(Escape(uriTemplate, index, uriTemplate.Length - index));
                this.patternBuilder.Append("$");
		    }

            private static string Escape(String fullPath, int start, int end)
            {
                if (start == end)
                {
                    return "";
                }
                return Regex.Escape(fullPath.Substring(start, end));
            }

            public string[] GetVariableNames() 
            {
			    return this.variableNames.ToArray();
		    }

            public Regex GetMatchRegex() 
            {
			    return new Regex(this.patternBuilder.ToString(), RegexOptions.Compiled);
		    }
	    }
    }
}
