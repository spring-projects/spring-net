#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

namespace Spring.Web.UI
{
	/// <summary>
	/// Specifies that page should be treated as a dialog, meaning that after processing
	/// is over user should return to the referring page.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Pages marked with this attribute will have "close" result predefined.
	/// </p>
	/// <p>
	/// Developers should call SetResult("close") from the event handler
	/// in order to return control back to the calling page.
	/// </p>
	/// </remarks>
    /// <author>Aleksandar Seovic</author>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class DialogAttribute : Attribute
	{
	}
}
