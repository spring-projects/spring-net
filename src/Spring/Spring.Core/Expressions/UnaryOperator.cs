#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

using System.Runtime.Serialization;

namespace Spring.Expressions
{
    /// <summary>
    /// Base class for unary operators.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    //[Serializable]
    public abstract class UnaryOperator : BaseNode
    {
        /// <summary>
        /// Create a new instance
        /// </summary>
        public UnaryOperator():base()
        {
        }

        /// <summary>
        /// Create a new instance
        /// </summary>
        public UnaryOperator(BaseNode operand)
        {
            this.addChild(operand);
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected UnaryOperator(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        
        /// <summary>
        /// Gets the operand.
        /// </summary>
        /// <value>The operand.</value>
        public BaseNode Operand
        {
            get { return (BaseNode) this.getFirstChild(); }
        }
    }
}