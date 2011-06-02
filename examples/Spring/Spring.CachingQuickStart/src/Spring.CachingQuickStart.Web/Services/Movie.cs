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

namespace Spring.CachingQuickStart.Services
{
    /// <summary>
    /// An object that describes a movie.
    /// </summary>
    public class Movie
    {
        public int ID { get; set; }
        public string Title { get; set; }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.CachingQuickStart.Services.Movie"/> class.
        /// </summary>
        /// <param name="id">The ID of the movie.</param>
        /// <param name="title">The title of the movie.</param>
        public Movie(int id, string title)
        {
            this.ID = id;
            this.Title = title;
        }

        public override string ToString()
        {
            return String.Format(
                "Movie[ID='{0}'; Title='{1}']",
                this.ID, this.Title);
        }
    }
}
