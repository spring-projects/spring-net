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

    /**
     * Represents a URI template. An URI template is a URI-like String that contained variables marked of in braces
     * (<code>{</code>, <code>}</code>), which can be expanded to produce a URI. <p>See {@link #expand(Map)},
     * {@link #expand(Object[])}, and {@link #match(String)} for example usages.
     *
     * @author Arjen Poutsma
     * @author Juergen Hoeller
     * @since 3.0
     * @see <a href="http://bitworking.org/projects/URI-Templates/">URI Templates</a>
     */
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

        public string[] VariableNames
        {
            get { return this.variableNames; }
        }

        public UriTemplate(string uriTemplate)
        {
            this.uriTemplate = uriTemplate;
            Parser parser = new Parser(uriTemplate);
            this.variableNames = parser.GetVariableNames();
            this.matchRegex = parser.GetMatchRegex();
        }

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

        /**
         * Indicate whether the given URI matches this template.
         * @param uri the URI to match to
         * @return <code>true</code> if it matches; <code>false</code> otherwise
         */
        public bool Matches(string uri)
        {
            if (uri == null)
            {
                return false;
            }
            return this.matchRegex.IsMatch(uri);
        }

        /**
         * Match the given URI to a map of variable values. Keys in the returned map are variable names, values are variable
         * values, as occurred in the given URI. <p>Example: <pre class="code"> UriTemplate template = new
         * UriTemplate("http://example.com/hotels/{hotel}/bookings/{booking}"); System.out.println(template.match("http://example.com/hotels/1/bookings/42"));
         * </pre> will print: <blockquote><code>{hotel=1, booking=42}</code></blockquote>
         * @param uri the URI to match to
         * @return a map of variable values
         */
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
			    AssertUtils.ArgumentHasText(uriTemplate, "'uriTemplate' must not be null");

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
