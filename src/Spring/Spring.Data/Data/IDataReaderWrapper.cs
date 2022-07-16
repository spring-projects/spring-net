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

using System.Data;

namespace Spring.Data
{
    /// <summary>
    /// Custom data reader implementations often delegate to an underlying
    /// instance.  This interface captures that relationship for reuse in
    /// the framework.
    /// </summary>
    /// <remarks>Implementations will typically add behavior to standard IDataReader methods,
    /// for example, by providing default values for DbNull values.
    /// See <see cref="Spring.Data.Support.NullMappingDataReader"/> as an example.
    ///  </remarks>
    /// <author>Mark Pollack (.NET)</author>
    public interface IDataReaderWrapper : IDataReader
    {
        /// <summary>
        /// The underlying reader implementation to delegate to for accessing data
        /// from a returned result sets.
        /// </summary>
        /// <value>The wrapped reader.</value>
        IDataReader WrappedReader
        {
            get;
            set;
        }

    }
}
