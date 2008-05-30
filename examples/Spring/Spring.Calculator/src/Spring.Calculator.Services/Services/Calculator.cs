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

using Spring.Calculator.Domain;
using Spring.Calculator.Interfaces;

#endregion

namespace Spring.Calculator.Services
{
	/// <summary>
	/// A calculator service.
	/// </summary>
    /// <author>Bruno Baia</author>
	/// <version>$Id: Calculator.cs,v 1.1 2006/10/29 18:39:13 bbaia Exp $</version>
    public class Calculator : ICalculator
    {
		#region ICalculator Members

        public int Add(int n1, int n2)
        {
            return n1 + n2;
        }

        public int Substract(int n1, int n2)
        {
            return n1 - n2;
        }

        public DivisionResult Divide(int n1, int n2)
        {
            DivisionResult result = new DivisionResult();
            result.Quotient = n1 / n2;
            result.Rest = n1 % n2;
            return result;
        }

        public int Multiply(int n1, int n2)
        {
            return n1 * n2;
        }

		#endregion
    }
}