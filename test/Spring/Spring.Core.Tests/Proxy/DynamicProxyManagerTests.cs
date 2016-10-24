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

#region Imports

using System;
using System.Reflection.Emit;
using System.Threading;
using NUnit.Framework;

#endregion

namespace Spring.Proxy
{
    /// <summary>
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class DynamicProxyManagerTests
    {
        #region WorkerThread Class

        public class WorkerThread
        {
            private Exception _exception;
            private Thread _thread;
            private WaitCallback _callback;
            private object _arg;

            public WorkerThread(WaitCallback callback,object arg)
            {
                this._arg = arg;
                this._callback = callback;
                _thread = new Thread( new ThreadStart( Run ) );
            }

            public void Start()
            {
                _thread.Start();
            }

            public void Join()
            {
                _thread.Join();
            }

            public Exception Exception
            {
                get { return this._exception; }
            }

            private void Run()
            {
                try
                {
                    _callback( _arg );
                }
                catch(Exception ex)
                {
                    _exception = ex;
                }
            }
        }

        #endregion WorkerThread Class

        [Test]
        public void CreateTypeBuilderMustNotBeCalledTwiceWithSameArguments()
        {
            TypeBuilder tb1 = DynamicProxyManager.CreateTypeBuilder("testtypename", null);
            
            try
            {
                TypeBuilder tb2 = DynamicProxyManager.CreateTypeBuilder("testtypename", typeof(AbstractProxyTypeBuilder));
                Assert.Fail("Did not throw expected ArgumentException.");
            }
            catch (ArgumentException)
            {
            }
            
        }

        [Test]
        public void CreateTypeBuilderIsThreadSafe()
        {
            const int WORKERS = 20;

            WorkerThread[] workers = new WorkerThread[WORKERS];
            for(int i=0;i<WORKERS;i++)
            {
                workers[i] = new WorkerThread(new WaitCallback(CallCreateTypeBuilder),  i);
                workers[i].Start();
            }

            for(int i=0;i<WORKERS;i++)
            {
                workers[i].Join();
                Assert.AreEqual(null, workers[i].Exception);
            }
        }

        private static void CallCreateTypeBuilder(object arg)
        {
            int nr = (int) arg;

            for(int i=0;i<100;i++)
            {
                DynamicProxyManager.CreateTypeBuilder( "TestType" + nr + "_" + i,typeof( AbstractProxyTypeBuilder ) );
                Thread.Sleep(0);
            }
        }
    }
}