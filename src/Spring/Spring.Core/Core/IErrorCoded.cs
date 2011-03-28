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

#region Imports



#endregion

namespace Spring.Core
{
	/// <summary>
	/// Interface that can be implemented by exceptions etc that are error coded.
	/// </summary>
	/// <remarks>
	/// <p>
	/// The error code is a <see cref="System.String"/>, rather than a number, so it can
	/// be given user-readable values, such as "object.failureDescription".
	/// </p>
	/// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Aleksandar Seovic (.Net)</author>
	public interface IErrorCoded
	{
		/// <summary>
		/// Return the error code associated with this failure.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The GUI can render this anyway it pleases, allowing for I18n etc.
		/// </p>
		/// </remarks>
		/// <returns>
		/// The <see cref="System.String"/> error code associated with this failure,
		/// or the empty string instance if not error-coded.
		/// </returns>
		string ErrorCode
		{
			get;
		}
	}
}