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

using System;
using System.Collections;

namespace Spring.Objects
{
    public interface ITestObject
    {
        event EventHandler Click;

        /// <summary>
        /// Public method to programmatically raise the <event>Click</event> event
        /// while testing.
        /// </summary>
        void OnClick ();

        int Age
        {
            get;
			set;			
        }

        ICollection Friends
        { 
            get;
        }

        INestedTestObject Doctor
        {
            get;
        }
		
        INestedTestObject Lawyer
        {
            get;
        }

        string Name
        {
            get;            
            set;
        }

        ITestObject Spouse
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the exception method call count.
        /// </summary>
        /// <value>The exception method call count.</value>
        int ExceptionMethodCallCount
        {
            get;
        }

        void Exceptional (Exception t);

        int ExceptionalWithReturnValue(Exception t);
		
        object ReturnsThis ();
        string GetDescription();
    }
}
