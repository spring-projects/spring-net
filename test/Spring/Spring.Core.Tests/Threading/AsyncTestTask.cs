#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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
using System.Threading;

#endregion

namespace Spring.Threading
{
    /// <summary>
    /// Base class for async test operations
    /// </summary>
    /// <author>Erich Eichinger</author>
    /// <version>$Id: AsyncTestTask.cs,v 1.2 2008/02/02 10:24:38 oakinger Exp $</version>
    public abstract class AsyncTestTask
    {
        private Exception exception;
        private Thread t;
        private readonly int iterations;
        private int iterationno;

        public AsyncTestTask(int iterations)
        {
            this.iterations = iterations;
            t = new Thread(new ThreadStart(Run));
            t.IsBackground = true;
        }

        public abstract void DoExecute();

        public AsyncTestTask Start()
        {
            t.Start();
            return this;
        }

        public void Run()
        {
            int loop = 0;
            try
            {
                iterationno = 0;
                for(loop = 0; loop < iterations; loop++)
                {
                    DoExecute();
                    Thread.Sleep(0);
                }
            }
            catch(Exception ex)
            {
                iterationno = loop;
                exception = ex;
            }
        }

        public void AssertNoException()
        {
            t.Join();
            if(exception != null)
            {
                throw new Exception("Exception occured in testtask on iteration " + iterationno, exception);
            }
        }
    }
}