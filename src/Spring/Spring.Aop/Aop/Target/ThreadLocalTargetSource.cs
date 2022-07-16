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

using AopAlliance.Intercept;

using Spring.Aop.Support;
using Spring.Collections;
using Spring.Util;

#endregion

namespace Spring.Aop.Target
{
	/// <summary>
	/// <see cref="Spring.Aop.ITargetSource"/> implementation that uses a
	/// threading model in which every thread has its own copy of the target.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Alternative to an object pool.
	/// </p>
	/// <p>
	/// Application code is written as to a normal pool; callers can't assume
	/// they will be dealing with the same instance in invocations in different
	/// threads. However, state can be relied on during the operations of a
	/// single thread: for example, if one caller makes repeated calls on the
	/// AOP proxy.
	/// </p>
	/// <p>
	/// This class act both as an introduction and as an interceptor, so it
	/// should be added twice, once as an introduction and once as an
	/// interceptor.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Federico Spinazzi (.NET)</author>
	public sealed class ThreadLocalTargetSource : AbstractPrototypeTargetSource,
		IThreadLocalTargetSourceStats, IDisposable, IMethodInterceptor
	{
		#region Fields

		/// <summary>
		/// ThreadLocal holding the target associated with the current thread.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Unlike most thread local storage which is static, this variable is
		/// meant to be per thread per instance of this class.
		/// </p>
		/// </remarks>
		private LocalDataStoreSlot _targetInThread = Thread.AllocateDataSlot();

		/// <summary>
		/// The set of managed targets, enabling us to keep track of the
		/// targets we've created.
		/// </summary>
		private ISet _targetSet = new ListSet();

		private int _invocations;
		private int _hits;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the number of invocations of the <see cref="GetTarget()"/> and
		/// <see cref="Invoke(IMethodInvocation)"/> methods.
		/// </summary>
		/// <value>
		/// The number of invocations of the <see cref="GetTarget()"/> and
		/// <see cref="Invoke(IMethodInvocation)"/> methods.
		/// </value>
		public int Invocations
		{
			get { return _invocations; }
		}

		/// <summary>
		/// Gets the number of hits that were satisfied by a thread bound object.
		/// </summary>
		/// <value>
		/// The number of hits that were satisfied by a thread bound object.
		/// </value>
		public int Hits
		{
			get { return _hits; }
		}

		/// <summary>
		/// Gets the number of thread bound objects created.
		/// </summary>
		/// <value>The number of thread bound objects created.</value>
		public int Objects
		{
			get { return _targetSet.Count; }
		}

		private object ThreadBoundTarget
		{
			get { return Thread.GetData(_targetInThread); }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Returns the target object.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Tries to locate the target from thread local storage. If no target
		/// is found, a target will be obtained and bound to the thread.
		/// </p>
		/// </remarks>
		/// <returns>The target object.</returns>
		/// <exception cref="System.Exception">
		/// If unable to obtain the target object.
		/// </exception>
		/// <seealso cref="Spring.Aop.Target.AbstractPrototypeTargetSource.NewPrototypeInstance"/>
		public override Object GetTarget()
		{
			++_invocations;
			object target = ThreadBoundTarget;
			if (target == null)
			{
				#region Instrumentation

				if (logger.IsDebugEnabled)
				{
					logger.Debug(string.Format(
						"No target for apartment prototype '{0}' " +
						"found in thread: creating one and binding it to thread '#{1}'",
						TargetObjectName, Thread.CurrentThread.GetHashCode()));
				}

				#endregion

				target = NewPrototypeInstance();
				Thread.SetData(_targetInThread, target);
				_targetSet.Add(target);
			}
			else
			{
				++_hits;
			}
			return ThreadBoundTarget;
		}

		/// <summary>
		/// Return an introduction advisor mixin that allows the AOP proxy to be
		/// cast to an <see cref="IThreadLocalTargetSourceStats"/> reference.
		/// </summary>
		public IIntroductionAdvisor GetStatsMixin()
		{
			return new DefaultIntroductionAdvisor(this, typeof (IThreadLocalTargetSourceStats));
		}

		/// <summary>
		/// Cleans up this instance's thread storage, and disposes of any
		/// <see cref="System.IDisposable"/> targets as necessary.
		/// </summary>
		/// <seealso cref="System.IDisposable.Dispose"/>
		public void Dispose()
		{
			#region Instrumentation

			if (logger.IsDebugEnabled)
			{
				logger.Debug("Destroying ThreadLocal bindings");
			}

			#endregion

			foreach (object target in _targetSet)
			{
				if (target is IDisposable)
				{
					try
					{
						((IDisposable) target).Dispose();
					}
					catch (Exception ex)
					{
						#region Instrumentation

						if (logger.IsWarnEnabled)
						{
							logger.Warn(string.Format(
								"Thread-bound target of class '{0}' " +
								"threw exception from it's IDisposable.Dispose() method.",
								target.GetType()), ex);
						}

						#endregion
					}
				}
			}
			_targetSet.Clear();
			_targetInThread = null;
		}

		/// <summary>
		/// Increments Invocations and Hits statistics
		/// </summary>
		/// <param name="invocation">The method invocation joinpoint</param>
		/// <returns>The result of the call to IJoinpoint.Proceed(), might be intercepted by the interceptor.</returns>
		/// <exception cref="System.Exception">if the interceptors or the target-object throws an exception.</exception>
		public object Invoke(IMethodInvocation invocation)
		{
            if (ReflectionUtils.MethodIsOnOneOfTheseInterfaces(
				invocation.Method, new Type[] {typeof (IThreadLocalTargetSourceStats)}))
			{
				return invocation.Method.Invoke(this, invocation.Arguments);
			}
			return invocation.Method.Invoke(GetTarget(), invocation.Arguments);
		}

		#endregion
	}
}
