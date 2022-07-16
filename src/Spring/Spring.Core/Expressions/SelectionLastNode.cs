#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

using System.Collections;
using System.Runtime.Serialization;

namespace Spring.Expressions
{
    /// <summary>
    /// Represents parsed selection node in the navigation expression.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class SelectionLastNode : BaseNode
    {
        /// <summary>
        /// Create a new instance
        /// </summary>
        public SelectionLastNode():base()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected SelectionLastNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Returns the last context item that matches selection expression.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            IList list = context as IList;

            if (list == null)
            {
                throw new ArgumentException(
                    "Selection can only be used on an instance of the type that implements IList.");
            }

            using (evalContext.SwitchThisContext())
            {
                BaseNode expression = (BaseNode) this.getFirstChild();
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    object listItem = list[i];
                    evalContext.ThisContext = listItem;
                    bool isMatch = (bool)GetValue(expression, listItem, evalContext );
                    if (isMatch)
                    {
                        return listItem;
                    }
                }
            }
            return null;
        }
    }
}
