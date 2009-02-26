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

namespace Spring.Calculator.Domain
{
    /// <summary>
    /// Encapsulates the result of an integer division
    /// </summary>
    [Serializable]
	public class DivisionResult
	{
		private int _quotient = 0;
		private int _rest = 0;

		/// <summary>
		/// Gets or sets the quotient of the division.
		/// </summary>
		public int Quotient
		{
			get { return _quotient; }
			set { _quotient = value; }
		}

		/// <summary>
		/// Gets or sets the rest of the division.
		/// </summary>
		public int Rest
		{
			get { return _rest; }
			set { _rest = value; }
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DivisionResult"/> class.
		/// </summary>
		public DivisionResult() {}

		/// <summary>
		/// Rrturns a string represantation of a division's result.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Format("Quotient: '{0}'; Rest: '{1}'", _quotient, _rest);
		}
	}
}
