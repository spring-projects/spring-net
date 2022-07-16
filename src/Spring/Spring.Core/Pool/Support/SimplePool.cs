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

#region Imports

using System.Collections;
using Spring.Util;
using Spring.Threading;

#endregion

namespace Spring.Pool.Support
{
	/// <summary>
	/// A simple pool implementation
	/// </summary>
	/// <remarks>
	/// <p>
	/// Based on the implementation found in Concurrent Programming in Java,
	/// 2nd ed., by <a href="http://gee.cs.oswego.edu/dl/">Doug Lea</a>.
	/// </p>
	/// </remarks>
	/// <author>Doug Lea</author>
	/// <author>Federico Spinazzi</author>
	/// <author>Mark Pollack</author>
	/// <author>Zbynek Vyskovsky, kvr@centrum.cz</author>
	public class SimplePool : IObjectPool
	{
		private readonly IPoolableObjectFactory factory;
		private bool closed;
		private IList free = new ArrayList();
		private IList busy = new ArrayList(); // linear search !!

		/// <summary>
		/// Set of permits
		/// </summary>
		internal readonly Semaphore available;

		/// <summary>
		/// Creates a new instance of the <see cref="Spring.Pool.Support.SimplePool"/>
		/// class.
		/// </summary>
		/// <param name="factory">
		/// The factory used to instantiate and manage the lifecycle of pooled objects.
		/// </param>
		/// <param name="initialSize">The initial size of the pool.</param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="factory"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// If the supplied <paramref name="initialSize"/> is less than or equal to zero.
		/// </exception>
		public SimplePool(IPoolableObjectFactory factory, int initialSize)
            : this(factory, initialSize, initialSize)
		{
		}

        /// <summary>
		/// Creates a new instance of the <see cref="Spring.Pool.Support.SimplePool"/>
		/// class.
		/// </summary>
		/// <param name="factory">
		/// The factory used to instantiate and manage the lifecycle of pooled objects.
		/// </param>
		/// <param name="maxSize">The maximum size of the pool.</param>
		/// <param name="initialSize">The initial size of the pool.</param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="factory"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// If the supplied <paramref name="maxSize"/> is less than or equal to zero.
		/// </exception>
		public SimplePool(IPoolableObjectFactory factory, int maxSize, int initialSize)
		{
			AssertUtils.ArgumentNotNull(factory, "factory");
			this.available = new Semaphore(maxSize);
			this.factory = factory;
			InitItems(initialSize);
		}

		/// <summary>
		/// Obtain an instance from the pool.
		/// </summary>
		/// <exception cref="Spring.Pool.PoolException">
		/// In case the pool is unusable.
		/// </exception>
		/// <seealso cref="Spring.Pool.IPoolableObjectFactory.ActivateObject"/>
		/// <seealso cref="Spring.Pool.IObjectPool.BorrowObject"/>
		public object BorrowObject()
		{
			available.Acquire();
			return DoBorrow();
		}

		/// <summary>
		/// Return an instance to the pool.
		/// </summary>
		/// <param name="target">The instance to be returned to the pool.</param>
		/// <seealso cref="Spring.Pool.IPoolableObjectFactory.PassivateObject"/>
		/// <seealso cref="Spring.Pool.IObjectPool.ReturnObject"/>
		public void ReturnObject(object target)
		{
			if (DoReturn(target))
			{
				available.Release();
			}
		}

		/// <summary>
		/// Create an object using the factory set by
		/// the <see cref="PoolableObjectFactory"/> property
		/// or other implementation dependent mechanism
		/// and place it into the pool.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This implementation <b>always</b> throws a
		/// <see cref="System.NotSupportedException"/>.
		/// </p>
		/// </remarks>
		/// <exception cref="NotSupportedException">
		/// If the implementation does not support the operation.
		/// </exception>
		public void AddObject()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Synchronized borrow logic.
		/// </summary>
		/// <seealso cref="Spring.Pool.Support.SimplePool.BorrowObject"/>
		protected object DoBorrow()
		{
			lock (this)
			{
				while (free.Count > 0)
				{
					int i = free.Count - 1;
					object o = free[i];
					free.RemoveAt(i);
					factory.ActivateObject(o);
					if (factory.ValidateObject(o))
					{
						busy.Add(o);
						return o;
					}
				}
				if (!closed)
				{
					object o = factory.MakeObject();
					busy.Add(o);
					return o;
				}
				else
				{
					throw new PoolException("Pool was closed and is unusable.");
				}
			}
		}

		/// <summary>
		/// Synchronized release logic.
		/// </summary>
		/// <param name="target">
		/// The object to release to the pool.
		/// </param>
		/// <returns>
		/// <see langword="false"/> if the object was not a busy one.
		/// </returns>
		protected bool DoReturn(object target)
		{
			lock (this)
			{
				if (busy.Contains(target))
				{
					busy.Remove(target);
					factory.PassivateObject(target);
					free.Add(target);
					return true;
				}
				return false;
			}
		}

		/// <summary>
		/// Instantiates the supplied number of instances and adds
		/// them to the pool.
		/// </summary>
		/// <param name="initialInstances">
		/// The initial number of objects to build.
		/// </param>
		/// <exception cref="System.ArgumentException">
		/// If the supplied number of <paramref name="initialInstances"/> is
		/// less than or equal to zero.
		/// </exception>
		protected void InitItems(int initialInstances)
		{
			if(initialInstances <= 0)
			{
				throw new ArgumentException("Cannot pool a negative number of instances.", "initialInstances");
			}
			for (int i = 0; i < initialInstances; ++i)
			{
				free.Add(factory.MakeObject());
			}
		}

		/// <summary>
		/// Close the pool and free any resources associated with it.
		/// </summary>
		public void Close()
		{
			lock (this)
			{
				for (IEnumerator e = busy.GetEnumerator();
				     e.MoveNext();
				     e = busy.GetEnumerator())
				{
					ReturnObject(e.Current);
				}
				foreach (object o in free)
				{
					factory.DestroyObject(o);
				}
				MakeNotUsable();
			}
		}

		/// <summary>
		/// Clear objects sitting idle in the pool, releasing any
		/// associated resources.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This implementation <b>always</b> throws a
		/// <see cref="System.NotSupportedException"/>.
		/// </p>
		/// </remarks>
		/// <exception cref="NotSupportedException">
		/// If the implementation does not support the operation.
		/// </exception>
		public void Clear()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Change the state of the pool to unusable.
		/// </summary>
		private void MakeNotUsable()
		{
			free = busy = new ArrayList();
			closed = true;
		}

		/// <summary>
		/// Gets the number of instances currently borrowed from the pool.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// If the implementation does not support the operation.
		/// </exception>
		/// <seealso cref="Spring.Pool.IObjectPool.NumActive"/>
		public int NumActive
		{
			get { return this.busy.Count; }
		}

		/// <summary>
		/// Gets the number of instances currently idle in the pool.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// If the implementation does not support the operation.
		/// </exception>
		/// <seealso cref="Spring.Pool.IObjectPool.NumIdle"/>
		public int NumIdle
		{
			get { return this.free.Count; }
		}

		/// <summary>
		/// Set the factory used to create new instances.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This implementation <b>always</b> throws a
		/// <see cref="System.NotSupportedException"/>.
		/// </p>
		/// </remarks>
		/// <exception cref="NotSupportedException">
		/// If the implementation does not support the operation.
		/// </exception>
		public IPoolableObjectFactory PoolableObjectFactory
		{
			set { throw new NotSupportedException(); }
		}
	}
}
