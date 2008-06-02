#region License

/*
 * Copyright © 2002-2006 the original author or authors.
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
    /// Callback delegate to set any necessary parameters or other
    /// properties on the command object.  The CommandType and
    /// CommandText properties will have already been supplied
    /// </summary>
    /// <param name="dbCommand">The database command object.</param>
    /// <author>Mark Pollack</author>
    public delegate void CommandSetterDelegate(IDbCommand dbCommand);

}
