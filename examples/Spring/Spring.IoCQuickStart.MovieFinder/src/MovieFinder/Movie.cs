#region License

/*
 * Copyright 2002-2006 the original author or authors.
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

namespace Spring.IocQuickStart.MovieFinder
{
	/// <summary>
	/// An object that describes a movie.
	/// </summary>
	public class Movie
	{
        private string _title;
        private string _director;

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.IocQuickStart.MovieFinder.Movie"/> class.
        /// </summary>
        /// <param name="title">The title of the movie.</param>
        /// <param name="director">The director of the movie.</param>
		public Movie (string title, string director)
		{
            _title = title;
            _director = director;
		}

        /// <summary>
        /// Property Title (string).
        /// </summary>
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }

        /// <summary>
        /// Property Director (string).
        /// </summary>
        public string Director
        {
            get
            {
                return _director;
            }
            set
            {
                _director = value;
            }
        }
	}
}
