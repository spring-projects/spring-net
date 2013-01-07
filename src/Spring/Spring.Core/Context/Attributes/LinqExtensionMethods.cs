#region License

/*
 * Copyright © 2010-2011 the original author or authors.
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

namespace Spring.Context.Attributes
{
    /// <summary>
    /// Limited extension methods reproducing the small subset of LINQ that is needed in the code; required b/c the project targets .NET 2.0 where LINQ is not available.
    /// </summary>
    internal static class LinqExtensionMethods
    {
        public static int Count<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null) throw new ArgumentNullException("source");

            int counter = 0;
            foreach (TSource obj in source)
            {
                counter++;
            }

            return counter;
        }

        internal static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value) where TSource : class
        {
            if (source == null) throw new ArgumentNullException("source");
            
            foreach (TSource obj in source)
            {
                if (obj == value)
                {
                    return true;
                }
            }

            return false;
        }

        internal static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null) throw new ArgumentNullException("source");

            IList<TSource> results = new List<TSource>();

            foreach (TSource obj in source)
            {
                results.Add(obj);
            }

            return results;
        }


        internal static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source,
                                                            Predicate<TSource> predicate)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (predicate == null) throw new ArgumentNullException("predicate");

            IList<TSource> matching = new List<TSource>();

            foreach (TSource obj in source)
            {
                if (predicate(obj))
                {
                    matching.Add(obj);
                }
            }

            return matching;
        }


        internal static bool Any<TSource>(this IEnumerable<TSource> source, Predicate<TSource> predicate)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (predicate == null) throw new ArgumentNullException("predicate");

            foreach (TSource obj in source)
            {
                if (predicate(obj))
                {
                    return true;
                }
            }

            return false;
        }
    }
}