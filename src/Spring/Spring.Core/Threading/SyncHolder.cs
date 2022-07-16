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

namespace Spring.Threading
{
	/// <summary>
	/// Utility class to use an <see cref="ISync"/> with the
	/// C# <c>using () {}</c> idiom
	/// </summary>
	public class SyncHolder : IDisposable
	{
        ISync _sync;

        /// <summary>
        /// Creates a new <see cref="SyncHolder"/> trying to <see cref="ISync.Acquire"/> the given
        /// <see cref="ISync"/>
        /// </summary>
        /// <param name="sync">the <see cref="ISync"/> to be held</param>
	    public SyncHolder (ISync sync)
	    {
            Init (sync);
	    }

	    /// <summary>
        /// Creates a new <see cref="SyncHolder"/> trying to <see cref="ISync.Attempt"/> the given
        /// <see cref="ISync"/>
        /// </summary>
        /// <param name="sync">the <see cref="ISync"/> to be held</param>
        /// <param name="msecs">millisecond to try to acquire the lock</param>
	    public SyncHolder (ISync sync, long msecs)
	    {
            Init(new TimeoutSync(sync, msecs));
	    }

	    /// <summary>
        /// Releases the held <see cref="ISync"/>
        /// </summary>
	    public void Dispose ()
        {
	        _sync.Release();
        }

        /// <summary>
        /// initializes and acquire access to the <see cref="ISync"/>
        /// </summary>
        /// <param name="sync"></param>
        private void Init (ISync sync)
        {
            this._sync = sync;
            _sync.Acquire();
        }
    }
}
