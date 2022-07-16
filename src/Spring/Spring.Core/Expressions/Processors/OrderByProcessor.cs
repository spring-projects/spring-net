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

using System.Collections;
using Spring.Util;

namespace Spring.Expressions.Processors
{
    /// <summary>
    /// Implementation of the 'order by' processor.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <author>Erich Eichinger</author>
    public class OrderByProcessor : ICollectionProcessor
    {
        #region Comparer Helper Implementations

        private class SimpleExpressionComparer : IComparer
        {
            private readonly IExpression _expression;

            public SimpleExpressionComparer(IExpression expression)
            {
                _expression = expression;
            }

            public int Compare(object x, object y)
            {
                x = _expression.GetValue(x);
                y = _expression.GetValue(y);

                if (x==y) return 0;

                if (x != null) return ((IComparable) x).CompareTo(y);

                return ((IComparable)y).CompareTo(x)*-1;
            }
        }

        private class LambdaComparer : IComparer
        {
            private readonly Dictionary<string, object> _variables;
            private readonly IExpression _fn;

            public LambdaComparer(LambdaExpressionNode lambdaExpression)
            {
                FunctionNode functionNode = new FunctionNode();
                functionNode.Text = "compare";
                VariableNode x = new VariableNode();
                x.Text = "x";
                VariableNode y = new VariableNode();
                y.Text = "y";

                functionNode.addChild(x);
                functionNode.addChild(y);

                _fn = functionNode;
                _variables = new Dictionary<string, object>();
                _variables.Add( "compare", lambdaExpression );
            }

            public int Compare(object x, object y)
            {
                _variables["x"] = x;
                _variables["y"] = y;
                return (int) _fn.GetValue(null, _variables);
            }
        }

        private class DelegateComparer : IComparer
        {
            private readonly Delegate _fnCompare;

            public DelegateComparer(Delegate fnCompare)
            {
                _fnCompare = fnCompare;
            }

            public int Compare(object x, object y)
            {
                return (int)_fnCompare.DynamicInvoke(new object[] { x, y });
            }
        }

        #endregion

        /// <summary>
        /// Sorts the source collection using custom sort criteria.
        /// </summary>
        /// <remarks>
        /// Please note that your compare function needs to take care about
        /// proper conversion of types to be comparable!
        /// </remarks>
        /// <param name="source">
        /// The source collection to sort.
        /// </param>
        /// <param name="args">
        /// Sort criteria to use.
        /// </param>
        /// <returns>
        /// A sorted array containing collection elements.
        /// </returns>
        public object Process(ICollection source, object[] args)
        {
            if (source == null || source.Count == 0)
            {
                return source;
            }

            if (args == null || args.Length != 1)
            {
                throw new ArgumentException("compare expression is a required argument for orderBy");
            }

            object arg = args[0];
            IComparer comparer = null;
            if (arg is string)
            {
                IExpression expCompare = Expression.Parse((string) arg);
                comparer = new SimpleExpressionComparer(expCompare);
            }
            else if (arg is IComparer)
            {
                comparer = (IComparer) arg;
            }
            else if (arg is LambdaExpressionNode)
            {
                LambdaExpressionNode fnCompare = (LambdaExpressionNode)arg;
                if (fnCompare.ArgumentNames.Length != 2)
                {
                    throw new ArgumentException("compare function must accept 2 arguments");
                }
                comparer = new LambdaComparer(fnCompare);
            }
            else if (arg is Delegate)
            {
                comparer = new DelegateComparer((Delegate) arg);
            }

            AssertUtils.ArgumentNotNull(comparer, "comparer", "orderBy(comparer) argument 'comparer' does not evaluate to a supported type");

            ArrayList list = new ArrayList(source);
            list.Sort(comparer);
            return list;
        }
    }
}
