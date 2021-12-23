#region License
// /*
//  * Copyright 2022 the original author or authors.
//  *
//  * Licensed under the Apache License, Version 2.0 (the "License");
//  * you may not use this file except in compliance with the License.
//  * You may obtain a copy of the License at
//  *
//  *      http://www.apache.org/licenses/LICENSE-2.0
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS,
//  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  * See the License for the specific language governing permissions and
//  * limitations under the License.
//  */
#endregion

namespace Spring.Messaging.Nms.Support
{
    /// <summary>
    /// Lock that can be used in sync and async code
    /// its non reentrant but there is a parameter acquireLock that can be passed from outer context to tell that lock is already taken for current execution flow
    /// </summary>
    public class SemaphoreSlimLock
    {
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public IDisposable Lock(bool acquireLock = true)
        {
            if (acquireLock)
            {
                _semaphoreSlim.Wait();
                return new DisposableLock(_semaphoreSlim);
            }
            else
            {
                return new DisposableLock(null);
            }
        }
        
        public async Task<IDisposable> LockAsync(bool acquireLock = true)
        {
            if (acquireLock)
            {
                await _semaphoreSlim.WaitAsync().Awaiter();
                return new DisposableLock(_semaphoreSlim);
            }
            else
            {
                return new DisposableLock(null);
            }
        }


        private class DisposableLock : IDisposable
        {
            private readonly SemaphoreSlim _semaphoreToUnlock;

            public DisposableLock(SemaphoreSlim semaphoreSlim)
            {
                this._semaphoreToUnlock = semaphoreSlim;
            }
            
            public void Dispose()
            {
                _semaphoreToUnlock?.Release();
            }
        }
        
    }
}