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

using System.Reflection;
using Spring.Core;
using Spring.Util;

namespace Spring.Objects.Events.Support
{
	/// <summary>
	/// Utility class to aid in the manipulation of events and delegates.
	/// </summary>
	/// <author>Griffin Caprio</author>
	public sealed class EventManipulationUtils
	{
		/// <summary>
		/// Returns a new instance of the requested <see cref="Delegate"/>.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Often used to wire subscribers to event publishers.
		/// </p>
		/// </remarks>
		/// <param name="delegateType">
		/// The <see cref="System.Type"/> of delegate to create.
		/// </param>
		/// <param name="targetSubscriber">
		/// The target subscriber object that contains the delegate implementation.
		/// </param>
		/// <param name="targetSubscriberDelegateMethod">
		/// <see cref="MethodInfo"/> referencing the delegate method on the subscriber.
		/// </param>
		/// <returns>
		/// A delegate handler that can be added to an events list of handlers, or called directly.
		/// </returns>
		public static Delegate GetHandlerDelegate(
			Type delegateType, object targetSubscriber, MethodInfo targetSubscriberDelegateMethod)
		{
			return Delegate.CreateDelegate(
				delegateType, targetSubscriber, targetSubscriberDelegateMethod.Name);
		}

		/// <summary>
		/// Queries the input type for a <see cref="MethodInfo" /> signature matching the input
		/// <see cref="MethodInfo"/> signature.
		/// </summary>
		/// <remarks>
		/// Typically used to query a potential subscriber to see if they implement an event handler.
		/// </remarks>
		/// <param name="invoke"><see cref="MethodInfo"/> to match against</param>
		/// <param name="subscriberType"><see cref="Type"/> to query</param>
		/// <returns>
		/// <see cref="MethodInfo"/> matching input <see cref="MethodInfo"/>
		/// signature, or <see langword="null"/> if there is no match.
		/// </returns>
		public static MethodInfo GetMethodInfoMatchingSignature(
			MethodInfo invoke, Type subscriberType)
		{
            ParameterInfo[] parameters = invoke.GetParameters();

			ComposedCriteria criteria = new ComposedCriteria();
			criteria.Add(new MethodReturnTypeCriteria(invoke.ReturnType));
            criteria.Add(new MethodParametersCountCriteria(parameters.Length));
			criteria.Add(new MethodParametersCriteria(ReflectionUtils.GetParameterTypes(parameters)));

			MemberInfo[] methods = subscriberType.FindMembers(
				MemberTypes.Method, ReflectionUtils.AllMembersCaseInsensitiveFlags,
				new MemberFilter(new CriteriaMemberFilter().FilterMemberByCriteria),
				criteria);
			if (methods != null
				&& methods.Length > 0)
			{
				return methods[0] as MethodInfo;
			}
			return null;
		}

		#region Constructor (s) / Destructor

		// CLOVER:OFF

		/// <summary>
		/// Creates a new instance of the EventManipulationUtilities class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is a utility class, and as such has no publicly visible constructors.
		/// </p>
		/// </remarks>
		private EventManipulationUtils()
		{
		}

		// CLOVER:ON

		#endregion
	}
}
