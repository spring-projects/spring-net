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

using Spring.Context;
using Spring.Context.Support;

using Spring.AopQuickStart.Commands;

#endregion

namespace Spring.AopQuickStart
{
    /// <summary>
    /// A simple application that demonstrates the AOP functionality of Spring.NET.
    /// </summary>
    /// <version>$Id: Program.cs,v 1.2 2006/12/02 13:30:00 bbaia Exp $</version>
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main(string[] args)
        {
            try
            {
                // Create AOP proxy using Spring.NET IoC container.
                IApplicationContext ctx = ContextRegistry.GetContext();
                ICommand command = (ICommand)ctx["myServiceCommand"];

                command.Execute();
                if (command.IsUndoCapable)
                {
                    command.UnExecute();
                }
            }
            catch (Exception ex)
            {
				Console.Out.WriteLine();
                Console.Out.WriteLine(ex);
            }
            finally
            {
				Console.Out.WriteLine();
                Console.Out.WriteLine("--- hit <return> to quit ---");
                Console.ReadLine();
            }
        }
    }
}
