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

namespace Spring.Pool
{
	/// <summary>
	/// Defines lifecycle methods for objects that are to be used in an
	/// <see cref="Spring.Pool.IObjectPool"/> implementation.
	/// </summary>
	/// <remarks>
	/// <p>
	/// The following methods summarize the contract between an
	/// <see cref="Spring.Pool.IObjectPool"/> and an
	/// an <see cref="Spring.Pool.IPoolableObjectFactory"/>.
	/// </p>
	/// <list type="number">
	///   <item>
	///		<see cref="Spring.Pool.IPoolableObjectFactory.MakeObject"/>
	///		is called whenever a new instance is needed.
	///   </item>
	///   <item>
	///		<see cref="Spring.Pool.IPoolableObjectFactory.ActivateObject"/>
	///		is invoked on every instance before it is returned from
	///		the pool.
	///   </item>
	///   <item>
	///		<see cref="Spring.Pool.IPoolableObjectFactory.PassivateObject"/>
	///		is invoked on every instance when it is returned to the pool.
	///   </item>
	///   <item>
	///		<see cref="Spring.Pool.IPoolableObjectFactory.DestroyObject"/>
	///		is invoked on every instance when it is being dropped from the
	///		pool (see
	///		<see cref="Spring.Pool.IPoolableObjectFactory.ValidateObject"/>
	///   </item>
	/// </list>
	/// <p>
	/// Based on the Jakarta Commons Pool API.
	/// </p>
	/// </remarks>
	/// <author>Federico Spinazzi</author>
	/// <seealso cref="Spring.Pool.IObjectPool"/>
	public interface IPoolableObjectFactory
	{
		/// <summary>
		/// Creates an instance that can be returned by the pool.
		/// </summary>
		/// <returns>
		/// An instance that can be returned by the pool.
		/// </returns>
		object MakeObject();

		/// <summary>
		/// Destroys an instance no longer needed by the pool.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Invoked on every instance when it is being "dropped" 
		/// from the pool (whether due to the return value from a call to the
		/// <see cref="Spring.Pool.IPoolableObjectFactory.ValidateObject"/>
		/// method, or for reasons specific to the pool implementation.)
		/// </p>
		/// </remarks>
		/// <param name="obj">The instance to be destroyed.</param>
		void DestroyObject(object obj);

		/// <summary>
		/// Ensures that the instance is safe to be returned by the pool. 
		/// Returns false if this object should be destroyed. 
		/// </summary>
		/// <remarks>
		/// <p>
		/// Invoked in an implementation-specific fashion to determine if an 
		/// instance is still valid to be returned by the pool. 
		/// It will only be invoked on an "activated" instance.
		/// </p>
		/// </remarks>
		/// <param name="obj">The instance to validate.</param>
		/// <returns>
		/// <see langword="false"/> if this object is not valid and
		/// should be dropped from the pool, otherwise <see langword="true"/>.
		/// </returns>
		bool ValidateObject(object obj);

		/// <summary>
		/// Reinitialize an instance to be returned by the pool. 
		/// </summary>
		/// <remarks>
		/// <p>
		/// Invoked on every instance before it is returned from the pool.
		/// </p>
		/// </remarks>
		/// <param name="obj">The instance to be activated.</param>
		void ActivateObject(object obj);

		/// <summary>
		/// Uninitialize an instance to be returned to the pool.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Invoked on every instance when it is returned to the pool.
		/// </p>
		/// </remarks>
		/// <param name="obj">The instance returned to the pool.</param>
		void PassivateObject(object obj);
	}
}