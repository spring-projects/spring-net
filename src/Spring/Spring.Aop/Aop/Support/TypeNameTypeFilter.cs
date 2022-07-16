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

using Spring.Util;

#endregion

namespace Spring.Aop.Support
{
	/// <summary>
	/// Simple <see cref="Spring.Aop.ITypeFilter"/> implementation that matches
	/// a given <see cref="Type"/>'s <see cref="Type.FullName"/> against <see cref="TypeNamePatterns"/>.
	/// For a list of supported pattern syntax see <see cref="PatternMatchUtils.SimpleMatch(string[],string)"/>.
	/// </summary>
    /// <author>Erich Eichinger</author>
    /// <seealso cref="PatternMatchUtils.SimpleMatch(string[],string)"/>
    public class TypeNameTypeFilter : ITypeFilter
    {
        private string[] _typeNamePatterns;

        ///<summary>
        /// Returns the list of type name patterns for this filter.
        ///</summary>
        /// <seealso cref="PatternMatchUtils.SimpleMatch(string[],string)"/>
        public string[] TypeNamePatterns
        {
            get { return _typeNamePatterns; }
        }

        ///<summary>
        ///Creates a new instance of <see cref="TypeNameTypeFilter"/> using a list of given <paramref name="patterns"/>.
        ///</summary>
        ///<param name="patterns">the list patterns to match typenames against. Must not be <c>null</c>.</param>
        /// <seealso cref="PatternMatchUtils.SimpleMatch(string[],string)"/>
        public TypeNameTypeFilter(string[] patterns)
        {
            AssertUtils.ArgumentNotNull(patterns, "patterns");
            _typeNamePatterns = patterns;
        }

	    /// <summary>
	    /// Does the supplied type's <see cref="Type.FullName"/> match any of the <see cref="TypeNamePatterns"/>?
	    /// </summary>
	    /// <param name="type">
	    /// The candidate <see cref="System.Type"/>.
	    /// </param>
	    /// <returns>
	    /// <see langword="true"/> if the <paramref name="type"/> matches any of the <see cref="TypeNamePatterns"/>.
	    /// </returns>
	    public bool Matches(Type type)
        {
            return PatternMatchUtils.SimpleMatch(_typeNamePatterns, type.FullName);
        }
    }
}
