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

using System.Threading;
using NUnit.Framework;

#endregion

namespace Spring.Threading
{
    /// <summary>
    ///
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class AsyncTestMethod : AsyncTestTask
    {
        public delegate object TestMethod(object[] args);

        private readonly TestMethod callback;
        private readonly object[] args;
        private object result;

        private class ThreadStartTestMethodAdapter
        {
            private readonly ThreadStart threadStart;

            public ThreadStartTestMethodAdapter(ThreadStart threadStart)
            {
                Assert.IsNotNull(threadStart);
                this.threadStart = threadStart;
            }

            public object Execute(object[] args)
            {
                threadStart();
                return null;
            }
        }

        public AsyncTestMethod(int iterations, ThreadStart callback, params object[] args)
            : this(iterations, new TestMethod(new ThreadStartTestMethodAdapter(callback).Execute), args)
        {
        }


        public AsyncTestMethod(int iterations, TestMethod callback, params object[] args) : base(iterations)
        {
            Assert.IsNotNull(callback);
            this.args = args;
            this.callback = callback;
        }

        public override void DoExecute()
        {
            result = callback(args);
        }

        public object Result
        {
            get { return result; }
        }
    }
}