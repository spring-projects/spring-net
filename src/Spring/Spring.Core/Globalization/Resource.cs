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

using Spring.Expressions;

namespace Spring.Globalization
{
	/// <summary>
	/// Holds mapping between control property and it's value
	/// as read from the resource file.
	/// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class Resource
    {
        private IExpression target;
        private object resourceValue;

        /// <summary>
        /// Creates instance of resource mapper.
        /// </summary>
        /// <param name="target">Target property.</param>
        /// <param name="resourceValue">Resource value.</param>
        public Resource(IExpression target, object resourceValue)
        {
            this.target = target;
            this.resourceValue = resourceValue;
        }

        /// <summary>
        /// Gets parsed target property expression. See <see cref="Spring.Expressions.IExpression"/>
        /// for more information on object navigation expressions.
        /// </summary>
        public IExpression Target
        {
            get { return target; }
        }

        /// <summary>
        /// Value of the resource that target property should be set to.
        /// </summary>
        public object Value
        {
            get { return resourceValue; }
        }
    }
}
