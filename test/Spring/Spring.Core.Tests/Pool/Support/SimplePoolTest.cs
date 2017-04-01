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

using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

using FakeItEasy;

using NUnit.Framework;
using Spring.Pool.Support;
using Spring.Threading;

namespace Spring.Pool
{
	#region Inner Class : Helper

	public class Helper
	{
		private Latch latch;
		private IObjectPool objectPool;
		private ISync sync;

		public bool gone = false;
		public object gotFromPool;

		public Helper(ISync sync, Latch latch)
		{
			Init(latch, sync, null);
		}

		public Helper(Latch latch, ISync sync, IObjectPool objectPool)
		{
			Init(latch, sync, objectPool);
		}

		public void Init(Latch latch, ISync sync, IObjectPool objectPool)
		{
			this.sync = sync;
			this.latch = latch;
			this.objectPool = objectPool;
		}

		public void Go()
		{
			latch.Release();
			sync.Acquire();
			gone = true;
			sync.Release();
		}

		public void UsePool()
		{
			gotFromPool = objectPool.BorrowObject();
			latch.Release();
		}
	}

	#endregion

	/// <summary>
	/// Unit tests for the SimplePool class.
	/// </summary>
	[TestFixture]
	public sealed class SimplePoolTests
	{
		#region Inner Class : MyFactory (IPoolableObjectFactory implementation)

	    private sealed class MyFactory : IPoolableObjectFactory
		{
			public object MakeObject()
			{
				return new object();
			}

            public void DestroyObject(object o)
			{
			}

            public bool ValidateObject(object o)
			{
				return true;
			}

			public void ActivateObject(object o)
			{
			}

            public void PassivateObject(object o)
			{
			}
		}

        #endregion

        private IPoolableObjectFactory factory;
		private SimplePool pool;

		[SetUp]
		public void SetUp()
		{
            factory = A.Fake<IPoolableObjectFactory>();
		    A.CallTo(() => factory.MakeObject()).Returns(new object());

            pool = new SimplePool(factory, 1);
		}


		[Test]
		public void InstantiateWithNullPoolableObjectFactory()
		{
            Assert.Throws<ArgumentNullException>(() => new SimplePool(null, 10));
		}

		[Test]
		public void InstantiateSpecifyingZeroPooledItems()
		{
            Assert.Throws<ArgumentException>(() => new SimplePool(factory, 0));
		}

		[Test]
		public void InstantiateSpecifyingNegativePooledItems()
		{
            Assert.Throws<ArgumentException>(() => new SimplePool(factory, -10000));
		}

		[Test]
		public void ActivateOnObjectOnBorrow()
		{
            A.CallTo(() => factory.ValidateObject(null)).WithAnyArguments().Returns(true);

			Assert.AreEqual(0, pool.NumActive, "active wrong");
			Assert.AreEqual(1, pool.NumIdle, "idle wrong");
			pool.BorrowObject();
			Assert.AreEqual(1, pool.NumActive, "active wrong");
			Assert.AreEqual(0, pool.NumIdle, "idle wrong");

            A.CallTo(() => factory.ActivateObject(null)).WithAnyArguments().MustHaveHappened();
        }

        [Test]
		public void PassivateBusyObjectsBeforeClose()
		{
            A.CallTo(() => factory.ValidateObject(A<object>._)).WithAnyArguments().Returns(true);

			object o = pool.BorrowObject();
            pool.Close();
            A.CallTo(() => factory.PassivateObject(o)).MustHaveHappened();
        }

        [Test]
        [Ignore("No longer expected behavior per SPRNET-1582 being resolved.")]
		public void NoMoreUsableAfterClose()
		{
            pool.Close();
            Assert.Throws<PoolException>(() => pool.BorrowObject());

            A.CallTo(() => factory.PassivateObject(null)).WithAnyArguments().MustHaveHappened();
        }

        [Test]
        [Ignore("No longer expected behavior per SPRNET-1582 being resolved.")]
        public void ThrowsExceptionWhenOutOfItemsBecauseFailedValidation()
		{
		    object o = new object();
		    A.CallTo(() => factory.MakeObject()).Returns(o);
			A.CallTo(() => factory.ValidateObject(o)).Returns(false);

			pool = new SimplePool(factory, 1);
            Assert.Throws<PoolException>(() => pool.BorrowObject());
        }

		[Test]
		public void PassivateObjectOnReturn()
		{
            A.CallTo(() => factory.ValidateObject(null)).WithAnyArguments().Returns(true);

			pool.ReturnObject(pool.BorrowObject());
            A.CallTo(() => factory.PassivateObject(null)).WithAnyArguments().MustHaveHappened();
        }

        [Test]
		public void DestroyObjectOnClose()
		{
            A.CallTo(() => factory.ValidateObject(null)).WithAnyArguments().Returns(true);

			pool.BorrowObject();
			pool.Close();

            A.CallTo(() => factory.DestroyObject(null)).WithAnyArguments().MustHaveHappened();
        }

        [Test]
		public void WaitOnBorrowWhenExausted()
		{
			int n = 100;
			object[] objects = new object[n];
			pool = new SimplePool(new MyFactory(), n);
			for (int i = 0; i < n; i++)
			{
				objects[i] = pool.BorrowObject();
			}
			Latch latch = new Latch();
			ISync sync = new Latch();
			Helper helper = new Helper(latch, sync, pool);
			Thread thread = new Thread(new ThreadStart(helper.UsePool));
			thread.Start();
			Assert.AreEqual(n, pool.NumActive);
			Assert.AreEqual(0, pool.NumIdle);
			object released = objects[n - 1];
			pool.ReturnObject(released);
			latch.Acquire();
			Assert.AreEqual(n, pool.NumActive);
			Assert.AreEqual(0, pool.NumIdle);
			Assert.IsNotNull(helper.gotFromPool, "helper did not get from pool");
			Assert.AreSame(released, helper.gotFromPool, "got unexpected object");
		}
	}

	[TestFixture]
	public sealed class StessSimplePool
	{
		public sealed class QueryPerformance
		{
			public interface IQueried
			{
				void Run();
			}

			[DllImport("KERNEL32")]
			private static extern bool QueryPerformanceCounter(ref long lpPerformanceCount);

			[DllImport("KERNEL32")]
			private static extern bool QueryPerformanceFrequency(ref long lpFrequency);

			public static float Query(IQueried queried)
			{
				long frequency = 0;
				QueryPerformanceFrequency(ref frequency);

				long startTime = 0;
				QueryPerformanceCounter(ref startTime);

				queried.Run();

				long endTime = 0;
				QueryPerformanceCounter(ref endTime);

				float elapsed = (float) (endTime - startTime)/frequency;
				//            Console.WriteLine("{0:0000.000}ms ", elapsed);
				//            Console.ReadLine();
				return elapsed;
			}
		}

		private sealed class Pooled : IPoolableObjectFactory, QueryPerformance.IQueried
		{
			private int creationTime;
			private int executionTime;

			public Pooled(int creationTime, int executionTime)
			{
				this.creationTime = creationTime;
				this.executionTime = executionTime;
				Thread.Sleep(creationTime);
			}

			public void Run()
			{
				Thread.Sleep(executionTime);
			}

			public object MakeObject()
			{
				return new Pooled(creationTime, executionTime);
			}

			public void DestroyObject(object o)
			{
			}

			public bool ValidateObject(object o)
			{
				return true;
			}

			public void ActivateObject(object o)
			{
			}

			public void PassivateObject(object o)
			{
			}
		}

		private sealed class Client : QueryPerformance.IQueried
		{
			private int repeat;
			private static int nRun = 0, created = 0;
			private int id;
			private ISync run;
			private SimplePool pool;
			public bool runned;

			public Client(int id, ISync run, SimplePool pool, int repeat)
			{
				this.run = run;
				this.pool = pool;
				this.id = id;
				lock (this.GetType())
				{
					created++;
					//Console.Out.WriteLine("#{0} created (created/run: {1}/{2})...", id, created, nRun);
				}
				this.repeat = repeat;
			}

			public void Run()
			{
				lock (this.GetType())
				{
					//Console.Out.WriteLine("#{0} running (created/run: {1}/{2})...", id, created, nRun);
					nRun++;
				}
				for (int i = 0; i < repeat; i++)
				{
					QueryPerformance.IQueried queried = pool.BorrowObject() as QueryPerformance.IQueried;
					queried.Run();
					pool.ReturnObject(queried);
					runned = true;
				}
				run.Release();
			}
		}

		private sealed class Job : QueryPerformance.IQueried
		{
			private int repeat;
			private IList clients = new ArrayList();
			private int poolSize;
			private int creationTime;
			private int clientSize;
			private ISync run;
			private int executionTime;

			public Job(int size, int creationTime, int clientSize, ISync start, int executionTime, int repeat)
			{
				this.poolSize = size;
				this.creationTime = creationTime;
				this.clientSize = clientSize;
				this.run = start;
				this.executionTime = executionTime;
				this.repeat = repeat;
			}

			public void Run()
			{
				SimplePool pool = new SimplePool(new Pooled(creationTime, executionTime), poolSize);
				for (int i = 0; i < clientSize; i++)
				{
					Client client = new Client(i, run, pool, repeat);
					Thread thread = new Thread(new ThreadStart(client.Run));
					thread.Start();
					clients.Add(client);
				}
				run.Acquire();

				foreach (Client client in clients)
					Assert.IsTrue(client.runned, "\nclient " + clients.IndexOf(client) + " not runned");
			}

			public static string Do(int poolSize, int clientSize, int executionTime, int repeat, int creationTime)
			{
				ISync start = new Spring.Threading.Semaphore(-(clientSize - 1));
				Job job = new Job(poolSize, creationTime, clientSize, start, executionTime, repeat);
				float elapsed = QueryPerformance.Query(job);
				return String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5:0.000} ",
				                     creationTime, executionTime, poolSize, clientSize, repeat, elapsed);
			}
		}

		[Test, Ignore("a try for a stress")]
		public void ScalingAgainstIncreasingNumberOfThreads()
		{
			Console.Out.WriteLine("creationTime\texecutionTime\tpoolSize\tclientSize\trepeat\telapsed");

			Console.Out.WriteLine("ramp pools size");
			Console.Out.WriteLine(Job.Do(10, 1000, 100, 1, 1));
			Console.Out.WriteLine(Job.Do(100, 1000, 100, 1, 1));
			Console.Out.WriteLine(Job.Do(200, 1000, 100, 1, 1));
			Console.Out.WriteLine(Job.Do(500, 1000, 100, 1, 1));
			Console.Out.WriteLine(Job.Do(1000, 1000, 100, 1, 1));

			Console.Out.WriteLine("ramp client size");
			Console.Out.WriteLine(Job.Do(10, 10, 100, 1, 1));
			Console.Out.WriteLine(Job.Do(10, 100, 100, 1, 1));
			Console.Out.WriteLine(Job.Do(10, 200, 100, 1, 1));
			Console.Out.WriteLine(Job.Do(10, 1000, 100, 1, 1));

			Console.Out.WriteLine("ramp load");
			Console.Out.WriteLine(Job.Do(10, 10, 100, 1, 1));
			Console.Out.WriteLine(Job.Do(10, 10, 100, 5, 1));
			Console.Out.WriteLine(Job.Do(10, 10, 100, 10, 1));
			Console.Out.WriteLine(Job.Do(10, 10, 100, 50, 1));
			Console.Out.WriteLine(Job.Do(10, 10, 100, 100, 1));

			Console.Out.WriteLine("ramp client size - 2");
			Console.Out.WriteLine(Job.Do(10, 1, 100, 1, 1));
			Console.Out.WriteLine(Job.Do(10, 5, 100, 5, 1));
			Console.Out.WriteLine(Job.Do(10, 10, 100, 10, 1));
			Console.Out.WriteLine(Job.Do(10, 50, 100, 50, 1));
			Console.Out.WriteLine(Job.Do(10, 100, 100, 100, 1));
			Console.Out.WriteLine(Job.Do(10, 1000, 100, 100, 1));
		}
	}
}