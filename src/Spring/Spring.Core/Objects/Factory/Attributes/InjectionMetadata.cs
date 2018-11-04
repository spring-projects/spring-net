#region License

/*
 * Copyright © 2010-2011 the original author or authors.
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

using System;
using System.Collections.Generic;
using System.Reflection;
using Common.Logging;
using Spring.Objects.Factory.Support;

namespace Spring.Objects.Factory.Attributes
{
	/// <summary>
	/// Internal class for managing injection metadata.
	/// Not intended for direct use in applications.
	/// </summary>
	internal class InjectionMetadata
	{
		private static readonly ILog Logger = LogManager.GetLogger<InjectionMetadata>();

		private readonly Type targetType;
		private readonly List<InjectedElement> _injectedElements;

		public InjectionMetadata(Type targetType, List<InjectedElement> elements)
		{
			this.targetType = targetType;
			_injectedElements = elements;

			if (Logger.IsDebugEnabled)
			{
				for (var i = 0; i < elements.Count; i++)
				{
					var element = elements[i];
					Logger.Debug($"Found injected element on class [{targetType.Name}]: {element}");
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="objectDefinition"></param>
		public void CheckConfigMembers(RootObjectDefinition objectDefinition)
		{
		}

		/// <summary>
		/// Inject values for members into object instance
		/// </summary>
		public void Inject(
			AutowiredAttributeObjectPostProcessor processor, 
			object instance, 
			string objectName, 
			IPropertyValues pvs)
		{
			bool debugEnabled = Logger.IsDebugEnabled;
			for (var i = 0; i < _injectedElements.Count; i++)
			{
				var element = _injectedElements[i];
				if (debugEnabled)
				{
					Logger.Debug($"Processing injected method of bean '{objectName}': {element}");
				}
				element.Inject(processor, instance, objectName, pvs);
			}
		}

		/// <summary>
		/// Represents an element that needs to be injected
		/// </summary>
		internal abstract class InjectedElement
		{
			/// <summary>
			/// Instantiates a new inject element
			/// </summary>
			/// <param name="member"></param>
			protected InjectedElement(MemberInfo member)
			{
				Member = member;
			}

			/// <summary>
			/// The Property, field, method or constructor info.
			/// </summary>
			public MemberInfo Member { get; }

			/// <summary>
			/// Executed to inject value to associated member info
			/// </summary>
			public abstract void Inject(
				AutowiredAttributeObjectPostProcessor processor,
				object target,
				string requestingObjectName,
				IPropertyValues pvs);
		}

		public static bool NeedsRefresh(InjectionMetadata metadata, Type type)
		{
			return metadata == null || metadata.targetType != type;
		}
	}
}
