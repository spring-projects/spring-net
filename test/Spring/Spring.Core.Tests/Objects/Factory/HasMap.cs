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

#region Imports

using System;
using System.Collections;
using System.Collections.Specialized;

using Spring.Collections;

#endregion

namespace Spring.Objects.Factory {

	/// <summary>
	/// Object exposing a map. Used for object factory tests.
	/// </summary>
    public class HasMap {

        #region Properties        
        public IDictionary Map
        {
            get
            {
                return _map;
            }
            set
            {
                _map = value;
            }
        }

        public Set Set
        {
            get
            {
                return _set;
            }
            set
            {
                _set = value;
            }
        }

        public NameValueCollection Props
        {
            get
            {
                return _props;
            }
            set
            {
                _props = value;
            }
        }

        public object [] ObjectArray
        {
            get
            {
                return _objectArray;
            }
            set
            {
                _objectArray = value;
            }
        }

        virtual public Type [] ClassArray
        {
            get
            {
                return _classArray;
            }
            set
            {
                _classArray = value;
            }
        }

        virtual public int [] IntegerArray
        {
            get
            {
                return _intArray;
            }
            set
            {
                _intArray = value;
            }
        }
        #endregion

        #region Fields
        private IDictionary _map;
        private Set _set;
        private NameValueCollection _props;
        private object [] _objectArray;
        private Type [] _classArray;
        private int [] _intArray;
        #endregion
	}
}
