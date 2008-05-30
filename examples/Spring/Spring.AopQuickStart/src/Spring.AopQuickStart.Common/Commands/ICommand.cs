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

#region Imports


#endregion

namespace Spring.AopQuickStart.Commands
{
	/// <summary>
	/// A simple interface for Command pattern style usage.
    /// </summary>
	/// <remarks>
	/// <p>
	/// Does not strictly adhere to the common conventions of the
	/// Command patten, due to the fact that the remit of this interface
	/// (and of it's attendant implementations) is to serve as the
	/// basis for illustrating AOP advice.
	/// </p>
	/// </remarks>
    /// <author>Rick Evans</author>
    /// <version>$Id: ICommand.cs,v 1.1 2006/11/26 18:58:00 bbaia Exp $</version>
	public interface ICommand 
    {
        bool IsUndoCapable { get; }

		void Execute();

        void UnExecute();
	}
}
