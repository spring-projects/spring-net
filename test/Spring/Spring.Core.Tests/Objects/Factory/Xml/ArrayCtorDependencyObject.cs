#region License

/*
 * Copyright 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// Class used to test array ctor autowiring
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    /// <author>Mark Pollack</author>
    public class ArrayCtorDependencyObject
    {
        private ITestObject spouse1;
        private ITestObject spouse2;


        public ArrayCtorDependencyObject(ITestObject[] spouses)
        {
            this.spouse1 = spouses[0];
            this.spouse2 = spouses[1];
        }

        public ITestObject Spouse1
        {
            get { return spouse1; }
        }

        public ITestObject Spouse2
        {
            get { return spouse2; }
        }
    }
}