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

using Spring.Util;

#endregion

namespace Spring.Core.IO
{
	/// <summary>
	/// <see cref="IResource"/> adapter implementation for a
	/// <see cref="System.IO.Stream"/>.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Should only be used if no other <see cref="Spring.Core.IO.IResource"/>
	/// implementation is applicable.
	/// </p>
	/// <p>
	/// In contrast to other <see cref="Spring.Core.IO.IResource"/>
	/// implementations, this is an adapter for an <i>already opened</i>
	/// resource - the <see cref="Spring.Core.IO.InputStreamResource.IsOpen"/>
	/// therefore always returns <see langword="true"/>. Do not use this class
	/// if you need to keep the resource descriptor somewhere, or if you need
	/// to read a stream multiple times.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	public class InputStreamResource : AbstractResource
	{
		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Core.IO.InputStreamResource"/> class.
		/// </summary>
		/// <param name="inputStream">
		/// The input <see cref="System.IO.Stream"/> to use.
		/// </param>
		/// <param name="description">
		/// Where the input <see cref="System.IO.Stream"/> comes from.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="inputStream"/> is
		/// <see langword="null"/>.
		/// </exception>
		public InputStreamResource(Stream inputStream, string description)
		{
			AssertUtils.ArgumentNotNull(inputStream, "inputStream");

			_inputStream = inputStream;
			_description = description == null ? string.Empty : description;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The input <see cref="System.IO.Stream"/> to use.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">
		/// If the underlying <see cref="System.IO.Stream"/> has already
		/// been read.
		/// </exception>
		public override Stream InputStream
		{
			get
			{
				if (_inputStream == null)
				{
					throw new InvalidOperationException(
						"InputStream has already been read - " +
						"do not use InputStreamResource if a stream " +
						"needs to be read multiple times");
				}
				Stream result = _inputStream;
				_inputStream = null;
				return result;
			}
		}

		/// <summary>
		/// Returns a description for this resource.
		/// </summary>
		/// <value>
		/// A description for this resource.
		/// </value>
		/// <seealso cref="Spring.Core.IO.IResource.Description"/>
		public override string Description
		{
			get { return _description; }
		}

        /// <summary>
        /// This implementation always returns true
        /// </summary>
		public override bool IsOpen
		{
			get { return true; }
		}



        /// <summary>
        /// This implemementation always returns true
        /// </summary>
		public override bool Exists
		{
			get { return true; }
		}

		#endregion

		#region Fields

		private Stream _inputStream;
		private string _description;

		#endregion
	}
}
