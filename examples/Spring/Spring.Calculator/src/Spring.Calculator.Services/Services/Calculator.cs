#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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
    public class Calculator : ICalculator
    {
        // SimpleCalculator prints out this value to proof, that they are running within the same AppDomain.
        public static volatile int instanceCount;

        static Calculator()
        {
            instanceCount = 0;
//            Common.Logging.ILog log = Common.Logging.LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);
//            log.Debug(
//                string.Format("Initialized Type in AppDomain {0}, ConfigFile={1}, BaseDirectory={2}, CurrentDirectory={3}, ExecutingAssembly={4}, EntryAssembly={5}"
//                , AppDomain.CurrentDomain.GetHashCode()
//                , AppDomain.CurrentDomain.SetupInformation.ConfigurationFile
//                , AppDomain.CurrentDomain.BaseDirectory
//                , Environment.CurrentDirectory
//                , Assembly.GetExecutingAssembly().Location
//                , Assembly.GetEntryAssembly().Location
//                ));
        }

		#region ICalculator Members

//        protected readonly Common.Logging.ILog Log = Common.Logging.LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

	    public Calculator()
	    {
//            Common.Logging.Log.Debug(string.Format("Initialized Instance in AppDomain {0}, #{0} at {1}", AppDomain.CurrentDomain.GetHashCode(), instanceCount, new System.Diagnostics.StackTrace()));
            instanceCount++;
        }

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