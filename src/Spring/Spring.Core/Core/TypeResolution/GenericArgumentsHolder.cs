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
            ParseGenericArguments(value);
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

        private void ParseGenericArguments(string originalString)
        {
            int argsStartIndex
                = originalString.IndexOf(GenericArgumentsPrefix);
            if (argsStartIndex < 0)
            {
                unresolvedGenericTypeName = originalString;
                unresolvedGenericMethodName = originalString;
            }
            else
            {
                int argsEndIndex =
                    originalString.LastIndexOf(GenericArgumentsSuffix);
                if (argsEndIndex != -1)
                {
                    unresolvedGenericMethodName = originalString.Remove(
                        argsStartIndex, argsEndIndex - argsStartIndex + 1);

                    SplitGenericArguments(originalString.Substring(
                        argsStartIndex + 1, argsEndIndex - argsStartIndex - 1));

                    unresolvedGenericTypeName = originalString.Replace(
                        originalString.Substring(argsStartIndex, argsEndIndex - argsStartIndex + 1),
                        "`" + unresolvedGenericArguments.Length);
                }
            }
        }

        private void SplitGenericArguments(string originalArgs)
        {
            IList args = new ArrayList();

            int index = 0;
            int cursor = originalArgs.IndexOf(GenericArgumentsSeparator, index);
            while (cursor != -1)
            {
                string arg = originalArgs.Substring(index, cursor - index);
                if (arg.Split(GenericArgumentsPrefix).Length ==
                    arg.Split(GenericArgumentsSuffix).Length)
                {
                    args.Add(arg.Trim());
                    index = cursor + 1;
                }
                cursor = originalArgs.IndexOf(GenericArgumentsSeparator, cursor + 1);
            }
            args.Add(originalArgs.Substring(index, originalArgs.Length - index).Trim());

            unresolvedGenericArguments = new string[args.Count];
            args.CopyTo(unresolvedGenericArguments, 0);
        }

        #endregion
    }
}

#endif
