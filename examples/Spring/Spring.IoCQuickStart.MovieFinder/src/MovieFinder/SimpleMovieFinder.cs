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

#region Imports

using System.Collections;

#endregion

namespace Spring.IocQuickStart.MovieFinder
{
	/// <summary>
	/// Simple in-memory storage implementation of the
    /// <see cref="Spring.IocQuickStart.MovieFinder.IMovieFinder"/> interface.
	/// </summary>
	public class SimpleMovieFinder : IMovieFinder
	{
        private ArrayList _list = new ArrayList();

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.IocQuickStart.MovieFinder.SimpleMovieFinder"/> class.
        /// </summary>
        public SimpleMovieFinder()
        {
            InitList();
        }

        /// <summary>
        /// Add a movie to the list.
        /// </summary>
        /// <param name="m">The movie.</param>
        public void AddMovie (Movie m)
        {
            _list.Add(m);
        }

        /// <summary>
        /// Finds all of the movies stored in this
        /// <see cref="Spring.IocQuickStart.MovieFinder.IMovieFinder"/> implementation.
        /// </summary>
        /// <returns>
        /// All of the movies stored in this
        /// <see cref="Spring.IocQuickStart.MovieFinder.IMovieFinder"/> implementation.
        /// </returns>
        public IList FindAll()
        {
            return new ArrayList (_list);
        }

        /// <summary>
        /// Initialises the in-mememory list of stored movies.
        /// </summary>
        private void InitList ()
        {
            _list.Add (new Movie ("La vita e bella", "Roberto Benigni"));
        }
	}
}
