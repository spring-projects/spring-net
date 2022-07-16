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

using System.Runtime.Serialization;

namespace Spring.Objects.Factory
{
	/// <summary>
	/// Exception thrown when an object depends on other objects or simple properties
	/// that were not specified in the object factory definition, although dependency
	/// checking was enabled.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	[Serializable]
	public class UnsatisfiedDependencyException : ObjectCreationException
	{
		/// <summary>
		/// Creates a new instance of the UnsatisfiedDependencyException class.
		/// </summary>
		public UnsatisfiedDependencyException()
		{
		}

		/// <summary>
		/// Creates a new instance of the UnsatisfiedDependencyException class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public UnsatisfiedDependencyException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Creates a new instance of the UnsatisfiedDependencyException class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception that is being wrapped.
		/// </param>
		public UnsatisfiedDependencyException(string message, Exception rootCause)
			: base(message, rootCause)
		{
		}

		/// <summary>
		/// Creates a new instance of the UnsatisfiedDependencyException class.
		/// </summary>
		/// <param name="resourceDescription">
		/// The description of the resource associated with the object.
		/// </param>
		/// <param name="name">
		/// The name of the object that has the unsatisfied dependency.
		/// </param>
		/// <param name="argumentIndex">
		/// The constructor argument index at which the dependency is
		/// unsatisfied.
		/// </param>
		/// <param name="argumentType">
		/// The <see cref="System.Type"/> of the constructor argument at
		/// which the dependency is unsatisfied.
		/// </param>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public UnsatisfiedDependencyException(
			string resourceDescription,
			string name,
			int argumentIndex,
			Type argumentType,
			string message)
			: base(
				resourceDescription,
				name,
				string.Format(
					"Unsatisfied dependency expressed through constructor argument with index {0} of type [{1}] : {2}",
					argumentIndex,
					argumentType,
					message))
		{
		}

		/// <summary>
		/// Creates a new instance of the UnsatisfiedDependencyException class.
		/// </summary>
		/// <param name="resourceDescription">
		/// The description of the resource associated with the object.
		/// </param>
		/// <param name="name">
		/// The name of the object that has the unsatisfied dependency.
		/// </param>
		/// <param name="propertyName">
		/// The name identifying the property on which the dependency is
		/// unsatisfied.
		/// </param>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public UnsatisfiedDependencyException(
			string resourceDescription,
			string name,
			string propertyName,
			string message)
			: base(
				resourceDescription,
				name,
				string.Format(
					"Unsatisfied dependency expressed through object property '{0}': {1}",
					propertyName,
					message))
		{
		}

		/// <summary>
		/// Creates a new instance of the UnsatisfiedDependencyException class.
		/// </summary>
		/// <param name="info">
		/// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="System.Runtime.Serialization.StreamingContext"/>
		/// that contains contextual information about the source or destination.
		/// </param>
		protected UnsatisfiedDependencyException(
			SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
