#region License

/*
 * Copyright © 2010-2011 the original author or authors.
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

namespace Spring.Objects.Factory.Attributes
{
    /// <summary>
    /// Defines a method that will be called prior to the destruction of the object instance
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PreDestroyAttribute : Attribute
    {
        private int _order;


        /// <summary>
        /// Initializes a new instance of the PreDestroy attribute with order = 1
        /// </summary>
        public PreDestroyAttribute()
        {
            _order = int.MaxValue;
        }

        /// <summary>
        /// Initializes a new instance of the PreDestroy attribute with defined order
        /// </summary>
        /// <param name="order">Order in which the PostContruct method is called</param>
        public PreDestroyAttribute(int order)
        {
            _order = order;
        }


        /// <summary>
        /// Defined the order in which the PreDestroy methods are called
        /// </summary>
        public int Order
        {
            get { return _order; }
            set { _order = value; }
        }
    }
}
