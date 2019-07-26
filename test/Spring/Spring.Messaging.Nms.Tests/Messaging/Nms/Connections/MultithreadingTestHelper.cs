using System;
using System.Diagnostics;
using System.Threading;

namespace Spring.Messaging.Nms.Connections
{
    public class MultithreadingTestHelper
    {
        [DebuggerStepThrough]
        public static TestThreadHandler RunOnSeparateThread(Action action)
        {
            Exception exception = null;
            var thread1 = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    exception = e;
                }
            });
            thread1.Start();

            return new TestThreadHandler(() =>
            {
                thread1.Join();
                if (exception != null)
                {
                    throw exception;
                }
            });
        }

        public class TestThreadHandler
        {
            private readonly Action _waitAction;

            public TestThreadHandler(Action waitAction)
            {
                _waitAction = waitAction;
            }

            [DebuggerStepThrough]
            public void Wait()
            {
                _waitAction();
            }
        }
    }
}