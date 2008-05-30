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

using Spring.Calculator.Interfaces;

#endregion

namespace Spring.Calculator.ClientApp
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

                Console.WriteLine("Get Calculator...");
				IAdvancedCalculator firstCalc = (IAdvancedCalculator) ctx.GetObject("calculatorService");
                Console.WriteLine("Divide(11, 2) : " + firstCalc.Divide(11, 2));
                Console.WriteLine("Memory = " + firstCalc.GetMemory());
				firstCalc.MemoryAdd(2);
                Console.WriteLine("Memory + 2 = " + firstCalc.GetMemory());

                Console.WriteLine("Get Calculator...");
				IAdvancedCalculator secondCalc = (IAdvancedCalculator) ctx.GetObject("calculatorService");
                Console.WriteLine("Memory = " + secondCalc.GetMemory());
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