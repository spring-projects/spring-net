#region License

/*
 * Copyright 2002-2011 the original author or authors.
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
using System.IO;

namespace Spring.Util
{
	/// <summary>
    /// Utility methods for IO handling.
	/// </summary>
	/// <author>Bruno Baia</author>
	internal sealed class IoUtils
	{
        /// <summary>
        /// Copies one stream into another. 
        /// </summary>
        public static void CopyStream(Stream source, Stream destination)
        {
#if NET_4_0
            source.CopyTo(destination);
#else
            int bytesCount;
            byte[] buffer = new byte[0x1000];
            while ((bytesCount = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                destination.Write(buffer, 0, bytesCount);
            }
#endif
        }
	}
}