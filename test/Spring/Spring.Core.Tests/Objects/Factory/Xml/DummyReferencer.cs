#region License

/*
 * Copyright 2004 the original author or authors.
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



#endregion

namespace Spring.Objects.Factory.Xml {

	/// <summary>
	/// Summary description for DummyReferencer.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
	public class DummyReferencer 
    {

        public ITestObject TestObject1
        {
            get
            {
                return testObject1;
            }
            set
            {
                this.testObject1 = value;
            }
        }

        public ITestObject TestObject2
        {
            get
            {
                return testObject2;
            }
            set
            {
                this.testObject2 = value;
            }
        }
		
        private ITestObject testObject1;
        private ITestObject testObject2;
	}
}
