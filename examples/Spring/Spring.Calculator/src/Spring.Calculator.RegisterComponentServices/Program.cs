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

using Spring.Context.Support;

#endregion

namespace Spring.Calculator.RegisterComponentServices
{
	class Program
	{
		[STAThread]
		static void Main()
		{
			try
			{
				Console.Out.WriteLine("Registering Component Services...");

				ContextRegistry.GetContext(); 

				Console.Out.WriteLine("Component Services registered.");
			}
			catch (Exception e)
			{
				Console.Out.WriteLine(e);
			}
			finally
			{
				Console.Out.WriteLine("--- Press <return> to quit ---");
				Console.ReadLine();
			}
		}
	}
}
