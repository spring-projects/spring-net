#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using System.Data;

#endregion

namespace Spring.Objects.Factory.Xml
{
	/// <summary>
    /// Test class for Spring.NET's ability to create objects using static
    /// factory methods, rather than constructors.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Rick Evans</author>
    public class InstanceFactory 
    {
        private string _factoryObjectProperty;
	
        public string FactoryObjectProperty 
        {
            get 
            {
                return _factoryObjectProperty;

            }
            set 
            {
                _factoryObjectProperty = value;
            }
        }
	
        public FactoryMethods DefaultInstance() 
        {
            TestObject to = new TestObject();
            to.Name = FactoryObjectProperty;
            return FactoryMethods.NewInstance (to);
        }

        /// <summary>
        /// Note that overloaded methods are supported.
        /// </summary>
        public FactoryMethods NewInstance (TestObject to) 
        {
            return FactoryMethods.NewInstance (to);
        }
	
        public FactoryMethods NewInstance (TestObject to, int num, string name) 
        {
            return FactoryMethods.NewInstance (to, name, num);
        }

        public FactoryMethods CreateInstance()
        {
            return FactoryMethods.NewInstance(new TestObject(), "DefaultCtor", 0);
        }

        public FactoryMethods CreateInstance(DataRow dr)
        {
            return FactoryMethods.NewInstance(new TestObject(), "DataRowCtor", 0);
        }

        public FactoryMethods CreateInstance(IDataRecord dr)
        {
            return FactoryMethods.NewInstance(new TestObject(), "DataRecordCtor", 0);
        }

	}
}
