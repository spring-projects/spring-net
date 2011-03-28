#region License
/*
* Copyright © 2002-2011 the original author or authors.
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

namespace Spring.Threading
{
    /// <summary>
    /// Acquire/Release protocol, base of many concurrency utilities.
    /// </summary>
    /// 
    /// <remarks>
    /// <p><see cref="ISync"/> objects isolate waiting and notification for particular logical 
    /// states, resource availability, events, and the like that are shared 
    /// across multiple threads.</p>
    /// 
    /// <p>Use of <see cref="ISync"/>s sometimes (but by no means always) adds 
    /// flexibility and efficiency compared to the use of plain 
    /// .Net monitor methods and locking, and are sometimes (but by no means 
    /// always) simpler to program with.</p>
    /// 
    /// <p>Used for implementation of a <see cref="Spring.Pool.Support.SimplePool"/></p>
    /// </remarks>
    /// 
    /// <author>Doug Lea</author>
    /// <author>Federico Spinazzi (.Net)</author>
    public interface ISync
	{
        /// <summary>  Wait (possibly forever) until successful passage.
        /// Fail only upon interuption. Interruptions always result in
        /// `clean' failures. On failure,  you can be sure that it has not 
        /// been acquired, and that no 
        /// corresponding release should be performed. Conversely,
        /// a normal return guarantees that the acquire was successful.
        /// 
        /// </summary>
        void Acquire();

        /// <summary> Potentially enable others to pass.
        /// <p>
        /// Because release does not raise exceptions, 
        /// it can be used in `finally' clauses without requiring extra
        /// embedded try/catch blocks. But keep in mind that
        /// as with any java method, implementations may 
        /// still throw unchecked exceptions such as Error or NullPointerException
        /// when faced with uncontinuable errors. However, these should normally
        /// only be caught by higher-level error handlers.
        /// </p>
        /// </summary>
        void Release ();
        
        /// <summary>
        /// Wait at most msecs to pass; report whether passed. 
        /// <p>
        /// The method has best-effort semantics:
        /// The msecs bound cannot
        /// be guaranteed to be a precise upper bound on wait time in Java.
        /// Implementations generally can only attempt to return as soon as possible
        /// after the specified bound. Also, timers in Java do not stop during garbage
        /// collection, so timeouts can occur just because a GC intervened.
        /// So, msecs arguments should be used in
        /// a coarse-grained manner. Further,
        /// implementations cannot always guarantee that this method
        /// will return at all without blocking indefinitely when used in
        /// unintended ways. For example, deadlocks may be encountered
        /// when called in an unintended context.
        /// </p>
        /// </summary>
        /// <param name="msecs">the number of milleseconds to wait
        /// An argument less than or equal to zero means not to wait at all. 
        /// However, this may still require
        /// access to a synchronization lock, which can impose unbounded
        /// delay if there is a lot of contention among threads.
        /// </param>
        /// <returns>true if acquired</returns>
        bool Attempt(long msecs);
    }
}
