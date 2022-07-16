//// <fragments>
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

using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace Spring.Util
{
	/// <summary>
	/// Support matching of file system paths in a manner similar to that of the
	/// <see href="http://nant.sourceforge.net">NAnt</see> <c>FileSet</c>.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Any (back)slashes are converted to forward slashes.
	/// </p>
	/// </remarks>
	/// <example>
	/// <code>
	/// // true
	/// PathMatcher.Match("c:/*.bat", @"c:\autoexec.bat");
	/// PathMatcher.Match("c:\fo*\*.bat", @"c:/foobar/autoexec.bat");
	/// PathMatcher.Match("c:\fo?\*.bat", @"c:/foo/autoexec.bat");
	/// // false
	/// PathMatcher.Match("c:\fo?\*.bat", @"c:/fo/autoexec.bat");
	/// </code>
	/// </example>
	/// <author>Federico Spinazzi</author>
	public sealed class PathMatcher
	{
		private const string AllFilesInThisDirectory = "*.*";

		/// <summary>
		/// Determines if a given path matches a <c>NAnt</c>-like pattern.
		/// </summary>
		/// <param name="pattern">
		/// A forward or back-slashed fileset-like pattern.
		/// </param>
		/// <param name="path">A forward or back-slashed full path.</param>
		/// <param name="ignoreCase">should the match consider the case</param>
		/// <returns>
		/// <see langword="true"/> if the path is matched by the pattern;
		/// otherwise <see langword="false"/>.
		/// </returns>
                //// <fragment name="match-method-nocase">
		public static bool Match(string pattern, string path, bool ignoreCase)
	        //// </fragment>
		{
            return Match(ToLower(pattern), ToLower(path));
		}

	    /// <summary>
		/// Determines if a given path matches a <c>NAnt</c>-like pattern.
		/// </summary>
		/// <param name="pattern">
		/// A forward or back-slashed fileset-like pattern.
		/// </param>
		/// <param name="path">A forward or back-slashed full path.</param>
		/// <returns>
		/// <see langword="true"/> if the path is matched by the pattern;
		/// otherwise <see langword="false"/>.
		/// </returns>
                //// <fragment name="match-method">
		public static bool Match(string pattern, string path)
	        //// </fragment>
		{
			pattern = ForwardifySlashes(pattern);
			path = ForwardifySlashes(path);

			if (MatchAll(pattern))
				return true;

			String regex = BuildRegex(pattern);
			return Regex.Match(path, regex).Success;
		}

		/// <summary>
		/// Replaces back(slashes) with forward slashes.
		/// </summary>
		/// <param name="path">
		/// The path or the pattern to modify.
		/// </param>
		/// <returns>A forward-slashed string.</returns>
		public static string ForwardifySlashes(string path)
		{
			return path.Replace("\\", "/");
		}

		/// <summary>
		/// Helper method to convert a <c>NAnt</c>-like pattern into the
		/// appropriate pattern for a regular expression.
		/// </summary>
		/// <param name="pattern">The <c>NAnt</c>-like pattern.</param>
		/// <returns>A regex-compatible pattern.</returns>
		public static String BuildRegex(string pattern)
		{
			if (AllFilesInThisDirectory.Equals(pattern))
			{
				return "^[^/]*$";
			}
			string[] parts = pattern.Split('/');
			int indexOfLastSplittedPart = parts.Length - 1;
			int indexOfTheCurrentSplittedPart = 0;
			StringBuilder regex = new StringBuilder();
			foreach (string currentSplittedPart in parts)
			{
				bool currentPartIsTheLast = indexOfTheCurrentSplittedPart == indexOfLastSplittedPart;
				string partToAdd = currentSplittedPart;
				switch (currentSplittedPart)
				{
					case "**":
						partToAdd = TranslateDoubleAsterisk(currentPartIsTheLast);
						break;
					default:
						partToAdd = TranslateDot(partToAdd);
						partToAdd = TranslateAsterisk(partToAdd);
						partToAdd = TranslateQuestionMark(partToAdd);
						partToAdd = TranslateLiteral(partToAdd, currentPartIsTheLast);
						break;
				}
				regex.Append(partToAdd);
				indexOfTheCurrentSplittedPart++;
			}
			return regex.ToString();
		}

        private static string ToLower(string pattern)
        {
            return pattern == null ? pattern : pattern.ToLower();
        }

        private static string TranslateLiteral(string part, bool isLastPart)
		{
			if (isLastPart)
			{
				return part + "$";
			}
			else
			{
				return part + "/?";
			}
		}

		private static string TranslateDoubleAsterisk(bool isLastPart)
		{
			string slashTerminated = "(.*/?(?<=/))";
			if (isLastPart)
				slashTerminated = String.Format("($|(?<=/){0}*)", slashTerminated);
			return slashTerminated;
		}

		private static string TranslateQuestionMark(string thisPart)
		{
			return thisPart.Replace("?", "[^./]");
		}

		private static string TranslateAsterisk(string thisPart)
		{
			return thisPart.Replace("*", "[^/]*");
		}

		private static string TranslateDot(string thisPart)
		{
			return thisPart.Replace(".", @"\.");
		}

		private static bool MatchAll(string pattern)
		{
			if (AllFilesInThisDirectory.Equals(pattern))
			{
				return false;
			}

			foreach (char c in pattern)
			{
				if (c != '*' && c != '/' && c != '.')
				{
					return false;
				}
			}
			return true;
		}

		#region Constructor (s) / Destructor

		// CLOVER:OFF

		/// <summary>
		/// Creates a new instance of the <see cref="Spring.Util.PathMatcher"/> class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is a utility class, and as such exposes no public constructors.
		/// </p>
		/// </remarks>
		private PathMatcher()
		{
		}

		// CLOVER:ON

		#endregion
	}
}
//// </fragments>
