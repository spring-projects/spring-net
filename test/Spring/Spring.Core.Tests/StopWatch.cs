#region License

/*
 * Copyright � 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
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

#endregion

namespace Spring
{
    /// <summary>
    /// A simple StopWatch implemetion for use in Performance Tests
    /// </summary>
    /// <example>
    /// The following example shows the usage:
    /// <code>
    /// StopWatch watch = new StopWatch();
    /// using (watch.Start("Duration: {0}"))
    /// {
    ///    // do some work...
    /// }
    /// </code>
    /// The format string passed to the start method is used to print the result message. 
    /// The example above will print <c>Duration: 0.123s</c>.
    /// </example>
    /// <author>Erich Eichinger</author>
    public class StopWatch
    {
        private DateTime _startTime;
        private TimeSpan _elapsed;

        private class Stopper : IDisposable
        {
            private readonly StopWatch _owner;
            private readonly string _format;

            public Stopper(StopWatch owner, string format)
            {
                _owner = owner;
                _format = format;
            }

            public void Dispose()
            {
                _owner.Stop(_format);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Starts the timer and returns and "handle" that must be disposed to stop the timer.
        /// </summary>
        /// <param name="outputFormat">the output format string, that is used to render the result message. Use '{0}' to print the elapsed timespan.</param>
        /// <returns>the handle to dispose for stopping the timer</returns>
        public IDisposable Start(string outputFormat)
        {
            Stopper stopper = new Stopper(this, outputFormat);
            _startTime = DateTime.Now;
            return stopper;
        }

        private void Stop(string outputFormat)
        {
            _elapsed = DateTime.Now.Subtract(_startTime);
            if (outputFormat != null)
            {
                Console.WriteLine(outputFormat, _elapsed);
            }
        }

        public DateTime StartTime
        {
            get { return _startTime; }
        }

        public TimeSpan Elapsed
        {
            get { return _elapsed; }
        }
    }
}