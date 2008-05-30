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
using Spring.Aop.Framework;

#endregion

namespace Spring.AopQuickStart
{
    /// <summary>
    /// A simple application that uses an introduction.
    /// </summary>
    /// <version>$Id: Program.cs,v 1.4 2007/07/26 00:50:11 bbaia Exp $</version>

    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main(string[] args)
        {
            try
            {
                Contact contact = GetProxy();
                IIsModified modificationWatcher = (IIsModified)contact;

                contact.FirstName = "Aleksandar";
                if (modificationWatcher.IsModified)
                    Console.WriteLine("Should not flag as modification");
                
                contact.FirstName = "Goran";

                if (modificationWatcher.IsModified)
                    Console.WriteLine("Should flag as modification");
               
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
        private static Contact GetProxy()
        {
            Contact target = new Contact();
            target.FirstName = "Aleksandar";
            target.LastName = "Seovic";

            ProxyFactory pf = new ProxyFactory(target);
            pf.AddAdvisor(new ModificationAdvisor(target.GetType()));
            pf.AddIntroduction(new IsModifiedAdvisor());
            pf.ProxyTargetType = true;

            return (Contact)pf.GetProxy();
        }
    }
}
