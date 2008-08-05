#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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

using Spring.Context.Support;
using Spring.ServiceModel;

#endregion

namespace Spring.WcfQuickStart
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                // Using Spring's IoC container
                ContextRegistry.GetContext(); // Force Spring to load configuration
                Console.Out.WriteLine("Server listening...");
                Console.Out.WriteLine("--- Press <return> to quit ---");
                Console.ReadLine();

                // Programmatically (You need to comment 'calculatorServiceHost' object definition in App.config)
                //using (SpringServiceHost serviceHost = new SpringServiceHost("calculator"))
                //{
                //    serviceHost.Open();

                //    Console.Out.WriteLine("Server listening...");
                //    Console.Out.WriteLine("--- Press <return> to quit ---");
                //    Console.ReadLine();
                //}
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e);
                Console.Out.WriteLine("--- Press <return> to quit ---");
                Console.ReadLine();
            }
        }
    }
}
