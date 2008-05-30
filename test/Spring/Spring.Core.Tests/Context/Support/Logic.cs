#region License

/*
 * Copyright 2002-2007 the original author or authors.
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

using Spring.Objects.Factory;

namespace Spring.Context.Support
{
    /// <author>Mark Pollack</author>
    /// <version>$Id: Logic.cs,v 1.1 2007/07/10 05:48:37 markpollack Exp $</version>
    public class Logic : IObjectNameAware
    {
        
        private string objectName;
        private Assembler assembler;


        public Assembler Assembler
        {
            set { assembler = value; }
        }

        #region IObjectNameAware Members

        public string ObjectName
        {
            set { objectName = value; }
        }

        #endregion
    }
}