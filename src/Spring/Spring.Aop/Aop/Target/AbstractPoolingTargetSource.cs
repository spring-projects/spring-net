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

using AopAlliance.Aop;
using Spring.Aop.Support;
using Spring.Objects;
using Spring.Objects.Factory;

#endregion

namespace Spring.Aop.Target
{
	/// <summary>
	/// Abstract superclass for pooling <see cref="Spring.Aop.ITargetSource"/>s.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Maintains a pool of target instances, acquiring and releasing a target
	/// object from the pool for each method invocation.
	/// </p>
	/// <p>
	/// This class is independent of pooling technology.
	/// </p>
	/// <p>
	/// Subclasses must implement the
	/// <see cref="Spring.Aop.Target.AbstractPoolingTargetSource.GetTarget"/> and
	/// <see cref="Spring.Aop.Target.AbstractPoolingTargetSource.ReleaseTarget"/>
	/// methods to work with their chosen pool. The
	/// <see cref="Spring.Aop.Target.AbstractPrototypeTargetSource.NewPrototypeInstance"/>
	/// method inherited from the
	/// <see cref="Spring.Aop.Target.AbstractPrototypeTargetSource"/> base class
	/// can be used to create objects to put in the pool. Subclasses must also
	/// implement some of the monitoring methods from the
	/// <see cref="Spring.Aop.Target.PoolingConfig"/> interface. This class
	/// provides the
	/// <see cref="Spring.Aop.Target.AbstractPoolingTargetSource.GetPoolingConfigMixin"/>
	/// method to return an <see cref="Spring.Aop.IIntroductionAdvisor"/>
	/// making these statistics available on proxied objects.
	/// </p>
	/// <p>
	/// This class implements the <see cref="System.IDisposable"/> interface in
	/// order to force subclasses to implement the
	/// <see cref="System.IDisposable.Dispose"/> method to cleanup and close
	/// down their pool.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Federico Spinazzi (.NET)</author>
    [Serializable]
    public abstract class AbstractPoolingTargetSource
		: AbstractPrototypeTargetSource, PoolingConfig, IDisposable, IAdvice
	{
		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Target.AbstractPoolingTargetSource"/>
		/// class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is an <see langword="abstract"/> class, and as such exposes no
		/// public constructors.
		/// </p>
		/// </remarks>
		protected AbstractPoolingTargetSource()
		{
		}

		#endregion

		/// <summary>
		/// Returns the target object (acquired from the pool).
		/// </summary>
		/// <returns>The target object (acquired from the pool).</returns>
		/// <exception cref="System.Exception">
		/// If unable to obtain the target object.
		/// </exception>
		public abstract override object GetTarget();

		/// <summary>
		/// Gets the <see cref="Spring.Aop.Target.PoolingConfig"/> mixin.
		/// </summary>
		/// <returns>
		/// An <see cref="Spring.Aop.IIntroductionAdvisor"/> exposing statistics
		/// about the pool maintained by this object.
		/// </returns>
		public DefaultIntroductionAdvisor GetPoolingConfigMixin()
		{
			return new DefaultIntroductionAdvisor(this, typeof (PoolingConfig));
		}

		/// <summary>
		/// The maximum number of object instances in this pool.
		/// </summary>
		public int MaxSize
		{
			get { return _maxSize; }
			set { _maxSize = value; }
		}

		/// <summary>
		/// The number of active object instances in this pool.
		/// </summary>
		public abstract int Active { get; }

		/// <summary>
		/// The number of free object instances in this pool.
		/// </summary>
		public abstract int Free { get; }

		/// <summary>
		/// The target factory that will be used to perform the lookup
		/// of the object referred to by the
		/// <see cref="Spring.Aop.Target.AbstractPrototypeTargetSource.TargetObjectName"/>
		/// property.
		/// </summary>
		/// <value>
		/// The owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// (will never be <see langword="null"/>).
		/// </value>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// In case of initialization errors.
		/// </exception>
		/// <seealso cref="Spring.Aop.Target.AbstractPrototypeTargetSource.ObjectFactory"/>
		public override IObjectFactory ObjectFactory
		{
			set
			{
				base.ObjectFactory = value;
				try
				{
					CreatePool(value);
				}
				catch (ObjectsException)
				{
					throw;
				}
				catch (Exception ex)
				{
					throw new ObjectInitializationException("Could not create instance pool.", ex);
				}
			}
		}

		/// <summary>
		/// Create the pool.
		/// </summary>
		/// <param name="factory">
		/// The owning <see cref="Spring.Objects.Factory.IObjectFactory"/>, in
		/// case one needs collaborators from it (normally one's own properties
		/// are sufficient).
		/// </param>
		/// <exception cref="System.Exception">
		/// In the case of errors encountered during the creation of the pool.
		/// </exception>
		protected abstract void CreatePool(IObjectFactory factory);

		/// <summary>
		/// Releases the target object (returns it to the pool).
		/// </summary>
		/// <param name="target">
		/// The target object to release (return to the pool).
		/// </param>
		/// <exception cref="System.Exception">
		/// In the case that the <paramref name="target"/> could not be released.
		/// </exception>
		public abstract override void ReleaseTarget(object target);

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or
		/// resetting unmanaged resources.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Disposes of the pool.
		/// </p>
		/// </remarks>
		public abstract void Dispose();

		private int _maxSize;
	}
}
