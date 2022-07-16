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

using Spring.Util;

#endregion

namespace Spring.Aop.Target
{
	/// <summary>
	/// <see cref="Spring.Aop.ITargetSource"/> implementation that caches a
	/// local target object, but allows the target to be swapped while the
	/// application is running
	/// </summary>
	/// <remarks>
	/// <p>
	/// If configuring an object of this class in a Spring IoC container,
	/// use constructor injection to supply the intial target.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.Net)</author>
    [Serializable]
    public class HotSwappableTargetSource : ITargetSource
	{
		private object _target;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Target.HotSwappableTargetSource"/> with the initial target.
		/// </summary>
		/// <param name="initialTarget">
		/// The initial target. May be <see langword="null"/>.
		/// </param>
		public HotSwappableTargetSource(object initialTarget)
		{
			_target = initialTarget;
		}

		/// <summary>
		/// The <see cref="System.Type"/> of the target object.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Can return <see langword="null"/>.
		/// </p>
		/// </remarks>
		public virtual Type TargetType
		{
			get
			{
                lock(this)
                {
                    return _target.GetType();
                }
			}
		}

		/// <summary>
		/// Is the target source static?
		/// </summary>
		/// <value>
		/// <see langword="true"/> if the target source is static.
		/// </value>
		public virtual bool IsStatic
		{
			get { return false; }
		}

		/// <summary>
		/// Returns the target object.
		/// </summary>
		/// <returns>The target object.</returns>
		/// <exception cref="System.Exception">
		/// If unable to obtain the target object.
		/// </exception>
		public object GetTarget()
		{
			// synchronization around something that takes so little time is fine...
			lock (this)
			{
				return _target;
			}
		}

		/// <summary>
		/// Releases the target object.
		/// </summary>
		/// <remarks>
		/// <p>
		/// No-op implementation.
		/// </p>
		/// </remarks>
		/// <param name="target">The target object to release.</param>
		public virtual void ReleaseTarget(Object target)
		{
		}

		/// <summary>
		/// Swap the target, returning the old target.
		/// </summary>
		/// <param name="newTarget">The new target.</param>
		/// <returns>The old target.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// If the new target is <see langword="null"/>.
		/// </exception>
		public virtual object Swap(object newTarget)
		{
			AssertUtils.ArgumentNotNull(newTarget, "newTarget", "Cannot swap to null.");
			lock (this)
			{
				// TODO: type checks
				object old = _target;
				_target = newTarget;
				return old;
			}
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/>
		/// is equal to the current <see cref="System.Object"/>.  
		/// </summary>
		/// <remarks>
		/// <p>
		/// Two invoker interceptors are equal if they have the same target or
		/// if the targets are equal.
		/// </p>
		/// </remarks>
		/// <param name="other">The target source to compare with.</param>
		/// <returns>
		/// <see langword="true"/> if this instance is equal to the
		/// specified <see cref="System.Object"/>.
		/// </returns>
		public override bool Equals(object other)
		{
			HotSwappableTargetSource otherTargetSource = other as HotSwappableTargetSource;
			if (other == null)
			{
				return false;
			}
			return otherTargetSource._target == _target || otherTargetSource._target.Equals(_target);
		}

		/// <summary>
		/// Serves as a hash function for a particular type, suitable for use
		/// in hashing algorithms and data structures like a hash table.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="System.Object"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return base.GetHashCode() + 13 *
				(_target == null ? 0 : _target.GetHashCode());
		}
	}
}
