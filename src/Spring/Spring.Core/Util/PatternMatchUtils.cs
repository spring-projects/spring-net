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
using System.Reflection;
using System.Text.RegularExpressions;
using Spring.Objects.Factory;

#endregion

namespace Spring.Util
{
    /// <summary> Utility methods for simple pattern matching, in particular for
    /// Spring's typical "xxx*", "*xxx" and "*xxx*" pattern styles.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack</author>
    public abstract class PatternMatchUtils
    {
        /// <summary> Match a String against the given pattern, supporting the following simple
        /// pattern styles: "xxx*", "*xxx" and "*xxx*" matches, as well as direct equality.
        /// </summary>
        /// <param name="pattern">the pattern to match against
        /// </param>
        /// <param name="str">the String to match
        /// </param>
        /// <returns> whether the String matches the given pattern
        /// </returns>
        public static bool SimpleMatch(System.String pattern, System.String str)
        {
            if (ObjectUtils.NullSafeEquals(pattern, str) || "*".Equals(pattern))
            {
                return true;
            }
            if (pattern == null || str == null)
            {
                return false;
            }
            if (pattern.StartsWith("*") && pattern.EndsWith("*") &&
                str.IndexOf(pattern.Substring(1, (pattern.Length - 1) - (1))) != -1)
            {
                return true;
            }
            if (pattern.StartsWith("*") && str.EndsWith(pattern.Substring(1, (pattern.Length) - (1))))
            {
                return true;
            }
            if (pattern.EndsWith("*") && str.StartsWith(pattern.Substring(0, (pattern.Length - 1) - (0))))
            {
                return true;
            }
            return false;
        }

        /// <summary> Match a String against the given patterns, supporting the following simple
        /// pattern styles: "xxx*", "*xxx" and "*xxx*" matches, as well as direct equality.
        /// </summary>
        /// <param name="patterns">the patterns to match against
        /// </param>
        /// <param name="str">the String to match
        /// </param>
        /// <returns> whether the String matches any of the given patterns
        /// </returns>
        public static bool SimpleMatch(System.String[] patterns, System.String str)
        {
            if (patterns != null)
            {
                for (int i = 0; i < patterns.Length; i++)
                {
                    
                    if (SimpleMatch(patterns[i], str))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Signature of callbacks that may be used for matching object names.
        /// </summary>
        /// <param name="objectName">the object name to check.</param>
        /// <param name="namePattern">the pattern to match <paramref name="objectName"/> against.</param>
        /// <returns>true, if the <paramref name="objectName"/> matches <paramref name="namePattern"/></returns>
        /// <see cref="IsObjectNameMatch"/>
        public delegate bool ObjectNameMatchPredicate(string objectName, string namePattern);

        /// <summary>
        /// Convenience method that may be used by derived classes. Iterates over the list of <paramref name="objectNamePatterns"/> to match <paramref name="objectName"/> against.
        /// </summary>
        /// <param name="objType">the object's type. Must not be <c>null</c>.</param>
        /// <param name="objectName">the name of the object Must not be <c>null</c>.</param>
        /// <param name="objectNamePatterns">the list of patterns, that <paramref name="objectName"/> shall be matched against. Must not be <c>null</c>.</param>
        /// <param name="isMatchPredicate">
        /// the <see cref="ObjectNameMatchPredicate"/> used for 
        /// matching <paramref name="objectName"/> against each pattern in <paramref name="objectNamePatterns"/>. Must not be <c>null</c>.
        /// </param>
        /// <param name="factoryObjectPrefix">the prefix to be used for dereferencing factory object names.</param>
        /// <returns>
        /// If <paramref name="objectNamePatterns"/> is <c>null</c>, will always return <c>true</c>, otherwise
        /// if <paramref name="objectName"/> matches any of the patterns specified in <paramref name="objectNamePatterns"/>. 
        /// </returns>
        public static bool IsObjectNameMatch(Type objType, string objectName, IList objectNamePatterns, ObjectNameMatchPredicate isMatchPredicate, string factoryObjectPrefix)
        {
            AssertUtils.ArgumentNotNull(objType, "objType");
            AssertUtils.ArgumentNotNull(objectName, "objectName");
            AssertUtils.ArgumentNotNull(objectNamePatterns, "objectNamePatterns");
            AssertUtils.ArgumentNotNull(isMatchPredicate, "isMatchPredicate");
            AssertUtils.ArgumentNotNull(factoryObjectPrefix, "factoryObjectPrefix");

            for (int i = 0; i < objectNamePatterns.Count; i++)
            {
                string mappedName = (string)objectNamePatterns[i];
                if (typeof( IFactoryObject ).IsAssignableFrom( objType ))
                {
                    if (!objectName.StartsWith( factoryObjectPrefix ))
                    {
                        continue;
                    }
                    mappedName = mappedName.Substring( factoryObjectPrefix.Length );
                }
                if (isMatchPredicate( objectName, mappedName ))
                {
                    return true;
                }
            }
            return false;
        }
    }
}