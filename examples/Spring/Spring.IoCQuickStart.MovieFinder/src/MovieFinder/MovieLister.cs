#region Licence

/*
 * Copyright 2002-2004 the original author or authors.
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

using System.Collections;
using Spring.Objects.Factory.Attributes;

namespace Spring.IocQuickStart.MovieFinder
{
	/// <summary>
	/// A class that provides a list of movies directed by a particular director.
    /// </summary>
	public class MovieLister
	{
        private IMovieFinder _movieFinder;

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.IocQuickStart.MovieFinder.MovieLister"/> class.
        /// </summary>
		public MovieLister ()
		{
		}

        /// <summary>
        /// Property MovieFinder (IMovieFinder).
        /// </summary>
        [Required]
        public IMovieFinder MovieFinder
        {
            get
            {
                return _movieFinder;
            }
            set
            {
                _movieFinder = value;
            }
        }

        /// <summary>
        /// Returns a list of those movies directed by the supplied
        /// <paramref name="director"/>
        /// </summary>
        /// <param name="director">
        /// The name of the director whose movies we are to return.
        /// </param>
        /// <returns>
        /// A list of those movies directed by the supplied
        /// <paramref name="director"/>.
        /// </returns>
        public Movie [] MoviesDirectedBy (string director)
        {
            IList allMovies = _movieFinder.FindAll ();
            IList movies = new ArrayList ();
            foreach (Movie m in allMovies)
            {
                if (director.Equals (m.Director))
                {
                    movies.Add (m);
                }
            }
            return (Movie []) ArrayList.Adapter (movies).ToArray (typeof (Movie));
        }
	}
}
