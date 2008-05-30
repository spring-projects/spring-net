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

using System;

#endregion

namespace Spring.AopQuickStart.Commands
{
    /// <summary>
    /// Simple implementation of the
    /// <see cref="Spring.AopQuickStart.Commands.ICommand"/> interface.
    /// </summary>
    /// <author>Rick Evans</author>
    /// <version>$Id: ServiceCommand.cs,v 1.3 2007/02/06 17:40:52 aseovic Exp $</version>
    public class ServiceCommand : ICommand
    {
        #region ICommand Members

        public bool IsUndoCapable
        {
            get { return true; }
        }

        public void Execute()
        {
            Console.Out.WriteLine("--- ServiceCommand implementation : Execute()... ---");
        }

        public void UnExecute()
        {
            Console.Out.WriteLine("--- ServiceCommand implementation : UnExecute()... ---");

            // Uncomment to see IThrowAdvice implementation in action
            //throw new Exception("The method or operation is not supported.");
        }

        #endregion
    }
}