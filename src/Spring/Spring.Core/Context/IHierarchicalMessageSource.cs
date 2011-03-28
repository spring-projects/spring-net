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

namespace Spring.Context
{
	/// <summary>
	/// Sub-interface of <see cref="Spring.Context.IMessageSource"/> to be
	/// implemented by objects that can resolve messages hierarchically.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Mark Pollack (.NET)</author>
	/// <seealso cref="Spring.Context.IMessageSource"/>
	public interface IHierarchicalMessageSource : IMessageSource
	{
		/// <summary>
		/// The parent message source used to try and resolve messages that
		/// this object can't resolve.
		/// </summary>
		/// <remarks>
		/// <p>
		/// If the value of this property is <see langword="null"/> then no
		/// further resolution is possible.
		/// </p>
		/// </remarks>
		IMessageSource ParentMessageSource { get; set; }
	}
}