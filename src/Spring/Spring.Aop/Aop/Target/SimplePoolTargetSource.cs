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

using Spring.Objects.Factory;
using Spring.Pool;
using Spring.Pool.Support;

#endregion

namespace Spring.Aop.Target
{
	/// <summary>
	/// Pooling target source implementation based on the
	/// <see cref="Spring.Pool.Support.SimplePool"/>
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Federico Spinazzi</author>
    [Serializable]
    public class SimplePoolTargetSource : AbstractPoolingTargetSource, IPoolableObjectFactory
	{
		private IObjectPool objectPool;

		/// <summary>
		/// Returns the target object (acquired from the pool).
		/// </summary>
		/// <returns>
		/// The target object (acquired from the pool).
		/// </returns>
		/// <exception cref="System.Exception">
		/// If unable to obtain the target object.
		/// </exception>
		public override object GetTarget()
		{
			return this.objectPool.BorrowObject();
		}

		/// <summary>
		/// Creates the pool.
		/// </summary>
		/// <param name="factory">
		/// The owning <see cref="Spring.Objects.Factory.IObjectFactory"/>, in
		/// case one needs collaborators from it (normally one's own properties
		/// are sufficient).
		/// </param>
		/// <seealso cref="Spring.Objects.Factory.IObjectFactoryAware.ObjectFactory"/>
		protected override void CreatePool(IObjectFactory factory)
		{
			#region Instrumentation

			if(logger.IsDebugEnabled)
			{
				logger.Debug("Creating object pool.");
			}

			#endregion

			this.objectPool = CreateObjectPool();
		}

		/// <summary>
		/// Creates a new instance of an appropriate
		/// <see cref="Spring.Pool.IObjectPool"/> implementation.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Subclasses can, of course, override this method if they want to
		/// return a different <see cref="Spring.Pool.IObjectPool"/> implementation.
		/// </p>
		/// </remarks>
		/// <returns>
		/// An empty <see cref="Spring.Pool.IObjectPool"/>.
		/// </returns>
		protected virtual IObjectPool CreateObjectPool()
		{
			return new SimplePool(this, MaxSize);
		}

		/// <summary>
		/// Releases the target object (returns it to the pool).
		/// </summary>
		/// <param name="target">The target object to release (return to the pool).</param>
		/// <exception cref="System.Exception">
		/// In the case that the <paramref name="target"/> could not be released.
		/// </exception>
		public override void ReleaseTarget(object target)
		{
			this.objectPool.ReturnObject(target);
		}

		/// <summary>
		/// The number of active object instances in this pool.
		/// </summary>
		public override int Active
		{
			get { return this.objectPool.NumActive; }
		}

		/// <summary>
		/// The number of free object instances in this pool.
		/// </summary>
		public override int Free
		{
			get { return this.objectPool.NumIdle; }
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or
		/// resetting unmanaged resources.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Disposes of the pool.
		/// </p>
		/// </remarks>
		public override void Dispose()
		{
			#region Instrumentation

			if(logger.IsDebugEnabled)
			{
				logger.Debug("Closing pool...");
			}

			#endregion

			this.objectPool.Close();
		}

		/// <summary>
		/// Creates an instance that can be returned by the pool.
		/// </summary>
		/// <returns>
		/// An instance that can be returned by the pool.
		/// </returns>
		/// <seealso cref="Pool.IPoolableObjectFactory.MakeObject"/>
		public virtual Object MakeObject()
		{
			return NewPrototypeInstance();
		}

		/// <summary>
		/// Destroys an instance no longer needed by the pool.
		/// </summary>
		/// <param name="obj">The instance to be destroyed.</param>
		/// <seealso cref="Pool.IPoolableObjectFactory.DestroyObject"/>
		public virtual void DestroyObject(object obj)
		{
			if (obj is IDisposable)
			{
				((IDisposable) obj).Dispose();
			}
		}

		/// <summary>
		/// Ensures that the instance is safe to be returned by the pool.
		/// Returns false if this object should be destroyed.
		/// </summary>
		/// <param name="obj">The instance to validate.</param>
		/// <returns>
		/// <see langword="false"/> if this object is not valid and
		/// should be dropped from the pool, otherwise <see langword="true"/>.
		/// </returns>
		/// <seealso cref="IPoolableObjectFactory.ValidateObject(object)"/>
		public virtual bool ValidateObject(object obj)
		{
			return true;
		}

		/// <summary>
		/// Reinitialize an instance to be returned by the pool.
		/// </summary>
		/// <param name="obj">The instance to be activated.</param>
		/// <seealso cref="IPoolableObjectFactory.ActivateObject(object)"/>
		public virtual void ActivateObject(object obj)
		{
		}

		/// <summary>
		/// Passivates the object.
		/// </summary>
		/// <param name="obj">The instance returned to the pool.</param>
		/// <seealso cref="Pool.IPoolableObjectFactory.PassivateObject"/>
		public virtual void PassivateObject(object obj)
		{
		}
	}
}
