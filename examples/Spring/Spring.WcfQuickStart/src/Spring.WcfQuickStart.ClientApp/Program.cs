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

using Spring.Context;
using Spring.Context.Support;

using Spring.WcfQuickStart;

#endregion

namespace Spring.WcfQuickStart.ClientApp
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Pause();

                IApplicationContext ctx = ContextRegistry.GetContext();
                foreach (ICalculator calculator in ctx.GetObjectsOfType(typeof(ICalculator)).Values)
                {
                    try
                    {
                        Console.WriteLine(calculator.GetName());
                        Console.WriteLine("Add(1, 1) : " + calculator.Add(1, 1));
                        Console.WriteLine("Divide(11, 2) : " + calculator.Divide(11, 2));
                        Console.WriteLine("Multiply(2, 5) : " + calculator.Multiply(2, 5));
                        Console.WriteLine("Subtract(7, 4) : " + calculator.Subtract(7, 4));
                        Console.WriteLine("Power(2, 8) : " + calculator.Power(new BinaryOperationArgs(2,8)).Result);
                        Console.WriteLine();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Pause();
            }
        }

        public static void Pause()
        {
            Console.Write("--- Press <return> to continue ---");
            Console.ReadLine();
        }
    }
}