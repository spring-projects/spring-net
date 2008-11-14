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

using Spring.Calculator.Interfaces;

#endregion

namespace Spring.Calculator.Services
{
    /// <summary>
    /// An advanced calculator service.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class AdvancedCalculator : Calculator, IAdvancedCalculator
    {
        #region Fields

        private int memoryStore = 0;

        #endregion

        #region Constructor(s) / Destructor

        public AdvancedCalculator()
            : this(0)
        { }

        public AdvancedCalculator(int initialMemory)
        {
            memoryStore = initialMemory;
        }

        #endregion

        #region IAdvancedCalculator Members

        public int GetMemory()
        {            
//            Log.Debug(string.Format("GetMemory: IsInTransaction={0}, at {1}", System.EnterpriseServices.ContextUtil.IsInTransaction, new System.Diagnostics.StackTrace()));
            return memoryStore;
        }

        public void SetMemory(int memoryValue)
        {
            memoryStore = memoryValue;
        }

        public void MemoryClear()
        {
            memoryStore = 0;
        }

        public void MemoryAdd(int num)
        {
            memoryStore += num;
        }

        #endregion
    }
}