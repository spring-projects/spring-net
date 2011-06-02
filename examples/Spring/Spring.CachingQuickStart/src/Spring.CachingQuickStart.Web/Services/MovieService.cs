#region License

/*
 * Copyright 2002-2011 the original author or authors.
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
using System.Collections.Generic;

using Spring.Caching;

namespace Spring.CachingQuickStart.Services
{
    /// <summary>
    /// Basic implementation of the IMovieService interface.
    /// </summary>
    public class MovieService : IMovieService
    {
        private IDictionary<int, Movie> movies;

        public MovieService()
        {
            this.movies = new Dictionary<int, Movie>();
            this.movies.Add(1, new Movie(1, "La vita e bella"));
            this.movies.Add(2, new Movie(2, "Blue Velvet"));
        }

        [CacheResult("DefaultCache", "'Movie-' + #id")]
        public Movie GetById(int id)
        {
            if (this.movies.ContainsKey(id))
            {
                return this.movies[id];
            }
            throw new ApplicationException(String.Format("Movie (ID='{0}') does not exist.", id));
        }

        [CacheResult("DefaultCache", "'AllMovies'", TimeToLive = "2m")]
        [CacheResultItems("DefaultCache", "'Movie-' + ID")]
        public IEnumerable<Movie> FindAll()
        {
            return movies.Values;
        }

        [InvalidateCache("DefaultCache", Keys = "'AllMovies'")]
        public void Save(
            [CacheParameter("DefaultCache", "'Movie-' + ID")]Movie movie)
        {
            if (this.movies.ContainsKey(movie.ID))
            {
                this.movies[movie.ID] = movie;
            }
            else
            {
                this.movies.Add(movie.ID, movie);
            }
        }

        [InvalidateCache("DefaultCache", Keys = "'Movie-' + #movie.ID")]
        [InvalidateCache("DefaultCache", Keys = "'AllMovies'")]
        public void Delete(Movie movie)
        {
            if (this.movies.ContainsKey(movie.ID))
            {
                this.movies.Remove(movie.ID);
            }
        }
    }
}
