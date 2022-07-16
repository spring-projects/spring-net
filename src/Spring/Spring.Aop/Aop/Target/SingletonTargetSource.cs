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

using System.Runtime.Serialization;

using Spring.Util;

namespace Spring.Aop.Target
{
	/// <summary>
	/// <see cref="Spring.Aop.ITargetSource"/> implementation that holds a local
	/// object.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This is the default implementation of the
	/// <see cref="Spring.Aop.ITargetSource"/> interface used by the AOP
	/// framework. There should be no need to create objects of this class in
	/// application code.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
    [Serializable]
    public sealed class SingletonTargetSource : ITargetSource, ISerializable
	{
		private readonly object target;
	    private readonly Type targetType;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Target.SingletonTargetSource"/>
		/// for the specified target object.
		/// </summary>
		/// <param name="target">The target object to expose.</param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="target"/> is
		/// <see langword="null"/>.
		/// </exception>
		public SingletonTargetSource(object target)
            :this(target, target != null ? target.GetType() : null)
		{}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Target.SingletonTargetSource"/>
		/// for the specified target object.
		/// </summary>
		/// <param name="target">The target object to expose.</param>
		/// <param name="targetType">The type of <paramref name="target"/> to expose.</param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="target"/> is
		/// <see langword="null"/>.
		/// </exception>
		public SingletonTargetSource(object target, Type targetType)
		{
			AssertUtils.ArgumentNotNull(target, "target");
			AssertUtils.ArgumentNotNull(targetType, "targetType");
			this.target = target;
		    this.targetType = targetType;
		}

	    /// <inheritdoc />
	    private SingletonTargetSource(SerializationInfo info, StreamingContext context)
	    {
	        target = info.GetValue("Target", typeof(object));
	        var type = info.GetString("TargetTypeName");
	        targetType = type != null ? Type.GetType(type) : null;
	    }

	    /// <summary>
		/// The <see cref="System.Type"/> of the target object.
		/// </summary>
		public Type TargetType
		{
			get { return targetType; }
		}

		/// <summary>
		/// Is the target source static?
		/// </summary>
		/// <value>
		/// <see langword="true"/> because this target source is always static.
		/// </value>
		public bool IsStatic
		{
			get { return true; }
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
			return target;
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
		public void ReleaseTarget(object target)
		{

		}

	    /// <inheritdoc />
	    public void GetObjectData(SerializationInfo info, StreamingContext context)
	    {
	        info.AddValue("TargetTypeName", targetType?.AssemblyQualifiedName);
	        info.AddValue("Target", target);
	    }

	    /// <summary>
		/// Returns a stringified representation of this target source.
		/// </summary>
		/// <returns>
		/// A stringified representation of this target source.
		/// </returns>
		public override string ToString()
		{
			return "Singleton target source (not dynamic): target=[" + target + "]";
		}

	    /// <summary>
	    /// Determines whether the specified <see cref="System.Object"/>
	    /// is equal to the current <see cref="System.Object"/>.
	    /// </summary>
	    /// <param name="other">The target source to compare with.</param>
	    /// <returns>
	    /// <see langword="true"/> if this instance is equal to the
	    /// specified <see cref="System.Object"/>.
	    /// </returns>
	    public override bool Equals(object other)
	    {
	        if (this == other)
	        {
	            return true;
	        }
	        SingletonTargetSource b = other as SingletonTargetSource;
	        if (b == null)
	        {
	            return false;
	        }
	        return target.Equals(b.target);
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
				(target == null ? 0 : target.GetHashCode());
		}
	}
}
