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

#endregion

namespace Spring.Aop.Target
{
	/// <summary>
	/// <see cref="Spring.Aop.ITargetSource"/> implementation that creates a
	/// new instance of the target object for each request.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Can only be used in an object factory.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Spinazzi Federico (.NET)</author>
	public sealed class PrototypeTargetSource : AbstractPrototypeTargetSource
	{
		/// <summary>
		/// Returns the target object.
		/// </summary>
		/// <returns>The target object.</returns>
		/// <exception cref="System.Exception">
		/// If unable to obtain the target object.
		/// </exception>
		public override Object GetTarget()
		{
			return NewPrototypeInstance();
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
		public override void ReleaseTarget(object target)
		{
		}

		/// <summary>
		/// Is the target source static?
		/// </summary>
		/// <value>
		/// <see langword="false"/> because this target source is never static.
		/// </value>
		public override bool IsStatic
		{
			get { return false; }
		}
	}
}
