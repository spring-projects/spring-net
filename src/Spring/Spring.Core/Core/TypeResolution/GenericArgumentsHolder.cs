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

#if NET_2_0

#region Imports

using System.Collections;
using System.Text.RegularExpressions;
using Spring.Util;

#endregion

namespace Spring.Core.TypeResolution
{
    /// <summary>
    /// Holder for the generic arguments when using type parameters.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Type parameters can be applied to classes, interfaces, 
    /// structures, methods, delegates, etc...
    /// </p>
    /// </remarks>
    public class GenericArgumentsHolder
    {
        #region Constants

        private static readonly Regex ClrPattern = new Regex(
              "^"
              + @"(?'name'\w[\w\d\.]+)"
              + @"`\d+\s*\["
              + @"(?'args'(?>[^\[\]]+|\[(?<DEPTH>)|\](?<-DEPTH>))*(?(DEPTH)(?!)))"
              + @"\]"
              + @"(?'remainder'.*)"
              + @"$"
              , RegexOptions.CultureInvariant | RegexOptions.Compiled
            );

        private static readonly Regex CSharpPattern = new Regex(
              "^"
              + @"(?'name'\w[\w\d\.]+)"
              + @"<"
              + @"(?'args'.*)"
              + @">"
              + @"(?'remainder'.*)"
              + @"$"
              , RegexOptions.CultureInvariant | RegexOptions.Compiled
            );

        private static Regex GenericArgumentListPattern = new Regex(
            @",("
            + @"(\[(?>[^\[\]]+|\[(?<DEPTH>)|\](?<-DEPTH>))*(?(DEPTH)(?!))\])" // capture anything between matching brackets
            + @"|"
            + @"([^,\[\]]*)" // alternatively capture any string that doesn't contain brackets and commas
            + @")+"
            );

        /// <summary>
        /// The generic arguments prefix.
        /// </summary>
        public const char GenericArgumentsQuotePrefix = '[';

        /// <summary>
        /// The generic arguments suffix.
        /// </summary>
        public const char GenericArgumentsQuoteSuffix = ']';

        /// <summary>
        /// The generic arguments prefix.
        /// </summary>
        public const char GenericArgumentsPrefix = '<';

        /// <summary>
        /// The generic arguments suffix.
        /// </summary>
        public const char GenericArgumentsSuffix = '>';

        /// <summary>
        /// The character that separates a list of generic arguments.
        /// </summary>
        public const char GenericArgumentsSeparator = ',';

        #endregion

        #region Fields

        private string unresolvedGenericTypeName;
        private string unresolvedGenericMethodName;
        private string[] unresolvedGenericArguments;
        private string arrayDeclaration;

        #endregion

        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the GenericArgumentsHolder class.
        /// </summary>
        /// <param name="value">
        /// The string value to parse looking for a generic definition
        /// and retrieving its generic arguments.
        /// </param>
        public GenericArgumentsHolder(string value)
        {
            ParseGenericTypeDeclaration(value);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The (unresolved) generic type name portion 
        /// of the original value when parsing a generic type.
        /// </summary>
        public string GenericTypeName
        {
            get { return unresolvedGenericTypeName; }
        }

        /// <summary>
        /// The (unresolved) generic method name portion 
        /// of the original value when parsing a generic method.
        /// </summary>
        public string GenericMethodName
        {
            get { return unresolvedGenericMethodName; }
        }

        /// <summary>
        /// Is the string value contains generic arguments ?
        /// </summary>
        /// <remarks>
        /// <p>
        /// A generic argument can be a type parameter or a type argument.
        /// </p>
        /// </remarks>
        public bool ContainsGenericArguments
        {
            get
            {
                return (unresolvedGenericArguments != null &&
                    unresolvedGenericArguments.Length > 0);
            }
        }

        /// <summary>
        /// Is generic arguments only contains type parameters ?
        /// </summary>
        public bool IsGenericDefinition
        {
            get
            {
                if (unresolvedGenericArguments == null)
                    return false;

                foreach (string arg in unresolvedGenericArguments)
                {
                    if (arg.Length > 0)
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Returns the array declaration portion of the definition, e.g. "[,]"
        /// </summary>
        /// <returns></returns>
        public string GetArrayDeclaration()
        {
            return arrayDeclaration;    
        }

        /// <summary>
        /// Is this an array type definition?
        /// </summary>
        public bool IsArrayDeclaration
        {
            get { return arrayDeclaration != null; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns an array of unresolved generic arguments types.
        /// </summary>
        /// <remarks>
        /// <p>
        /// A empty string represents a type parameter that 
        /// did not have been substituted by a specific type.
        /// </p>
        /// </remarks>
        /// <returns>
        /// An array of strings that represents the unresolved generic 
        /// arguments types or an empty array if not generic.
        /// </returns>
        public string[] GetGenericArguments()
        {
            if (unresolvedGenericArguments == null)
                return StringUtils.EmptyStrings;

            return unresolvedGenericArguments;
        }

        private void ParseGenericTypeDeclaration(string originalString)
        {
            if (originalString.IndexOf('[') == -1 && originalString.IndexOf('<') == -1)
            {
                // nothing to do
                unresolvedGenericTypeName = originalString;
                unresolvedGenericMethodName = originalString;
                return;
            }

            originalString = originalString.Trim();

            bool isClrStyleNotation = originalString.IndexOf('`') > -1;

            Match m = (isClrStyleNotation)
                          ? ClrPattern.Match(originalString)
                          : CSharpPattern.Match(originalString);

            if (m == null || !m.Success)
            {
                unresolvedGenericTypeName = originalString;
                unresolvedGenericMethodName = originalString;
                return;
            }

            Group g = m.Groups["args"];
            unresolvedGenericArguments = ParseGenericArgumentList(g.Value);

            string name = m.Groups["name"].Value;
            string remainder = m.Groups["remainder"].Value.Trim();

            // check, if we're dealing with an array type declaration
            if (remainder.Length > 0 && remainder.IndexOf('[') > -1)
            {
                string[] remainderParts = StringUtils.Split(remainder, ",", false, false, "[]");
                string arrayPart = remainderParts[0].Trim();
                if (arrayPart[0] == '[' && arrayPart[arrayPart.Length-1] == ']')
                {
                    arrayDeclaration = arrayPart;
                    remainder = ", " + string.Join(",", remainderParts, 1, remainderParts.Length - 1);
                }               
            }
            
            unresolvedGenericMethodName = name + remainder;
            unresolvedGenericTypeName = name + "`" + unresolvedGenericArguments.Length + remainder;



            //            char lBoundary = isClrStyleNotation ? '[' : GenericArgumentsPrefix;
            //            char rBoundary = isClrStyleNotation ? ']' : GenericArgumentsSuffix;
            //
            //            int argsStartIndex = originalString.IndexOf(lBoundary);
            //            if (argsStartIndex < 0)
            //            {
            //                unresolvedGenericTypeName = originalString;
            //                unresolvedGenericMethodName = originalString;
            //            }
            //            else
            //            {
            //                int argsEndIndex = originalString.LastIndexOf(rBoundary);
            //                if (argsEndIndex != -1)
            //                {
            //                    unresolvedGenericMethodName = originalString.Remove(
            //                        argsStartIndex, argsEndIndex - argsStartIndex + 1);
            //
            //                    unresolvedGenericArguments = ParseGenericArgumentList(originalString.Substring(
            //                        argsStartIndex + 1, argsEndIndex - argsStartIndex - 1));
            //
            //                    unresolvedGenericTypeName = originalString.Replace(
            //                        originalString.Substring(argsStartIndex, argsEndIndex - argsStartIndex + 1),
            //                        "`" + unresolvedGenericArguments.Length);
            //                }
            //            }
        }

        private static string[] ParseGenericArgumentList(string originalArgs)
        {
            string[] args = StringUtils.Split(originalArgs, ",", true, false, "[]<>"     );
            // remove quotes if necessary
            for(int i=0;i<args.Length;i++)
            {
                string arg = args[i];
                if (arg.Length > 1 && arg[0] == '[')
                {
                    args[i] = arg.Substring(1, arg.Length - 2);
                }
            }
            return args;
//            ArrayList args = new ArrayList();
//            originalArgs = originalArgs.Trim();
//            if (originalArgs.Length == 0)
//            {
//                return new string[0];
//            }
//
//            if (originalArgs.IndexOf(',') == -1)
//            {
//                return new string[] { originalArgs };
//            }
//
//            // pattern assumes "(,<argname>)+"
//            originalArgs = "," + originalArgs;
//
//            foreach (Match match in GenericArgumentListPattern.Matches(originalArgs))
//            {
//                string arg = match.Groups["args"].Value.Trim(' ', '\t', '[', ']');
//                args.Add(arg);
//            }

            //            Match m = GenericArgumentListPattern.Match(originalArgs);
            //            if (m != null && m.Success)
            //            {
            //                Group g = m.Groups[0];
            //                foreach (Capture capture in g.Captures)
            //                {
            //                    string arg = capture.Value.Trim(' ', '\t', '[', ']');
            //                    args.Add(arg);
            //                }
            //            }
            //            int index = 0;
            //            int cursor = originalArgs.IndexOf(GenericArgumentsSeparator, index);
            //            while (cursor != -1)
            //            {
            //                string arg = originalArgs.Substring(index, cursor - index);
            //                if (arg.Split(GenericArgumentsPrefix).Length ==
            //                    arg.Split(GenericArgumentsSuffix).Length)
            //                {
            //                    args.Add(arg.Trim());
            //                    index = cursor + 1;
            //                }
            //                cursor = originalArgs.IndexOf(GenericArgumentsSeparator, cursor + 1);
            //            }
            //            args.Add(originalArgs.Substring(index, originalArgs.Length - index).Trim());

//            return (string[])args.ToArray(typeof(string));
        }
        #endregion
    }
}

#endif
