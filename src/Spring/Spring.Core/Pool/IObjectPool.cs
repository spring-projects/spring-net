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

namespace Spring.Pool
{
	/// <summary>
	/// A simple pooling interface for managing and monitoring a pool
	/// of objects.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Based on the Jakarta Commons Pool API.
	/// </p>
	/// </remarks>
	/// <author>Federico Spinazzi</author>
	/// <seealso cref="Spring.Pool.IPoolableObjectFactory"/>
	public interface IObjectPool
	{
		/// <summary>
		/// Obtain an instance from the pool.
		/// </summary>
		/// <remarks>
		/// <p>
		/// By contract, clients <b>must</b> return the borrowed
		/// instance using <see cref="Spring.Pool.IObjectPool.ReturnObject"/>
		/// or a related method as defined in an implementation or
		/// sub-interface.
		/// </p>
		/// </remarks>
		/// <returns>An instance from the pool.</returns>
		/// <exception cref="Spring.Pool.PoolException">
		/// In case the pool is unusable.
		/// </exception>
		/// <seealso cref="Spring.Pool.IPoolableObjectFactory.ActivateObject"/>
		object BorrowObject();

		/// <summary>
		/// Return an instance to the pool.
		/// </summary>
		/// <remarks>
		/// <p>
		/// By contract, the object <b>must</b> have been obtained using
		/// <see cref="Spring.Pool.IObjectPool.BorrowObject"/>
		/// or a related method as defined in an implementation or sub-interface.
		/// </p>
		/// </remarks>
		/// <param name="target">The instance to be returned to the pool.</param>
		/// <seealso cref="Spring.Pool.IPoolableObjectFactory.PassivateObject"/>
		void ReturnObject(object target);

        /// <summary>
        /// Create an object using the factory set by
        /// the <see cref="PoolableObjectFactory"/> property
        /// or other implementation dependent mechanism
        /// and place it into the pool.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an optional operation. AddObject is useful for "pre-loading" a
        /// pool with idle objects.
        /// </p>
        /// </remarks>
        /// <exception cref="NotSupportedException">
        /// If the implementation does not support the operation.
        /// </exception>
        void AddObject();

        /// <summary>
        /// Close the pool and free any resources associated with it.
        /// </summary>
        void Close();

        /// <summary>
        /// Clear objects sitting idle in the pool, releasing any
        /// associated resources.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an optional operation.
        /// </p>
        /// </remarks>
        /// <exception cref="NotSupportedException">
        /// If the implementation does not support the operation.
        /// </exception>
        void Clear();

		/// <summary>
		/// Gets the number of instances currently borrowed from the pool.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is an optional operation.
		/// </p>
		/// </remarks>
        /// <exception cref="NotSupportedException">
        /// If the implementation does not support the operation.
        /// </exception>
        int NumActive { get; }

        /// <summary>
		/// Gets the number of instances currently idle in the pool.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is an optional operation.
		/// </p>
		/// <p>
		/// This may be considered an <i>approximation</i> of the number of objects
		/// that can be borrowed without creating any new instances.
		/// </p>
		/// </remarks>
        /// <exception cref="NotSupportedException">
        /// If the implementation does not support the operation.
        /// </exception>
        int NumIdle { get; }

        /// <summary>
        /// Set the factory used to create new instances.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is an optional operation.
		/// </p>
		/// </remarks>
        /// <exception cref="NotSupportedException">
        /// If the implementation does not support the operation.
        /// </exception>
        IPoolableObjectFactory PoolableObjectFactory { set; }
    }
}
