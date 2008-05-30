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

using Spring.AopQuickStart.Attributes;

#endregion

namespace Spring.AopQuickStart.Commands
{
    /// <summary>
    /// Another simple implementation of the
    /// <see cref="Spring.Examples.AopQuickStart.ICommand"/> interface.
    /// </summary>
    /// <author>Bruno Baia</author>
    /// <version>$Id: AnotherServiceCommand.cs,v 1.1 2006/12/02 13:30:02 bbaia Exp $</version>
    public class AnotherServiceCommand : ICommand
    {
        #region ICommand Members

        public bool IsUndoCapable
        {
            get { return true; }
        }

#if NET_2_0
        [ConsoleLogging(Color = ConsoleColor.Red)]
#else
        [ConsoleLogging]
#endif
        public void Execute()
        {
            Console.Out.WriteLine("--- AnotherServiceCommand implementation : Execute()... ---");
        }

        public void UnExecute()
        {
            Console.Out.WriteLine("--- AnotherServiceCommand implementation : UnExecute()... ---");

            // Uncomment to see IThrowAdvice implementation in action
            //throw new Exception("The method or operation is not supported.");
        }

        #endregion
    }
}