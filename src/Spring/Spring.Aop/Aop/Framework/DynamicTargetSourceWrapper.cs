#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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

using System;
using Spring.Util;

#endregion

namespace Spring.Aop.Framework
{
	/// <summary>
	/// Decorates a target source with the <see cref="System.IDisposable"/>
	/// interface.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This implementation will release the target object when said object
	/// is disposed.
	/// </p>
	/// </remarks>
	/// <author>Aleksandar Seovic</author>
	[Serializable]
    public sealed class DynamicTargetSourceWrapper : ITargetSourceWrapper
	{
		private ITargetSource targetSource;
		private object target;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Framework.DynamicTargetSourceWrapper"/>
		/// class.
		/// </summary>
		/// <param name="targetSource">
		/// The target object that proxy methods will be delegated to.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="targetSource"/> is
		/// <see langword="null"/>.
		/// </exception>
		internal DynamicTargetSourceWrapper(ITargetSource targetSource)
		{
			AssertUtils.ArgumentNotNull(targetSource, "targetSource");
			this.targetSource = targetSource;
		}

		/// <summary>
		/// Returns the target object that proxy methods will be delegated to.
		/// </summary>
		/// <returns>The target object.</returns>
		public object GetTarget()
		{
            if (this.target == null)
            {
                this.target = targetSource.GetTarget();
            }

			return this.target;
		}
		
		/// <summary>
		/// Releases the dynamic target when this object is disposed.
		/// </summary>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if (disposing && this.target != null)
			{
				this.targetSource.ReleaseTarget(this.target);
				this.target = null;
			}
		}
	}
}
