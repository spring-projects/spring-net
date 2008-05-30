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

using System.IO;
using System.Collections;

#endregion

namespace Spring.IocQuickStart.MovieFinder
{
	/// <summary>
    /// An <see cref="Spring.IocQuickStart.MovieFinder.IMovieFinder"/> implementation
	/// that uses a colon delimited text file as it's backing data store.
    /// </summary>
    public class ColonDelimitedMovieFinder : IMovieFinder
    {
        private static readonly char [] Delimeter = new char [] {':'};

        private FileInfo _movieFile;
        private IList _movies;

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.IocQuickStart.MovieFinder.ColonDelimitedMovieFinder"/> class.
        /// </summary>
        /// <param name="file">
        /// The file containing colon delimited movies.
        /// </param>
        public ColonDelimitedMovieFinder (FileInfo file)
        {
            MovieFile = file;
        }

        /// <summary>
        /// The file containing colon delimited movies.
        /// </summary>
        /// <value>
        /// A <see cref="System.IO.FileInfo"/> pointing to the file containing
        /// colon delimited movies.
        /// </value>
        public FileInfo MovieFile 
        {
            get 
            {
                return _movieFile;
            }
            set 
            {
                _movieFile = value;
                if (_movieFile != null && _movieFile.Exists) 
                {
                    InitList ();
                }
            }
        }

        /// <summary>
        /// Finds all of the movies stored in this
        /// <see cref="Spring.IocQuickStart.MovieFinder.IMovieFinder"/> implementation.
        /// </summary>
        /// <returns>
        /// All of the movies stored in this
        /// <see cref="Spring.IocQuickStart.MovieFinder.IMovieFinder"/> implementation.
        /// </returns>
        public IList FindAll ()
        {
            return _movies;
        }

        /// <summary>
        /// Loads all of the movies in the file referred to by <c>MovieFile</c>.
        /// </summary>
        private void InitList ()
        {
            _movies = new ArrayList ();
            using (StreamReader reader = MovieFile.OpenText ()) 
            {
                string line = null;
                while ((line = reader.ReadLine ()) != null) 
                {
                    string [] tuple = line.Split (Delimeter);
                    Movie movie = new Movie (tuple [0], tuple [1]);
                    _movies.Add (movie);
                }
            }
        }
    }
}
