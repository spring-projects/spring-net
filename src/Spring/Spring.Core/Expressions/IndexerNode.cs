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
using System.Reflection;
using System.Runtime.Serialization;
using Spring.Core;
using Spring.Util;
using Spring.Reflection.Dynamic;

namespace Spring.Expressions
{
    /// <summary>
    /// Represents parsed indexer node in the navigation expression.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class IndexerNode : NodeWithArguments
    {
        private const BindingFlags BINDING_FLAGS
            = BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Instance | BindingFlags.Static
            | BindingFlags.IgnoreCase;

        private SafeProperty indexer;

        /// <summary>
        /// Create a new instance
        /// </summary>
        public IndexerNode()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected IndexerNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Returns node's value for the given context.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            if (context == null)
            {
                throw new NullValueInNestedPathException("Cannot retrieve the value of the indexer because the context for its resolution is null.");
            }

            try
            {
                if (context is Array)
                {
                    return GetArrayValue( (Array) context, evalContext );
                }
                else if (context is IList)
                {
                    return GetListValue( (IList) context, evalContext );
                }
                else if (context is IDictionary)
                {
                    return GetDictionaryValue( (IDictionary) context, evalContext );
                }
                else if (context is string)
                {
                    return GetCharacter( (string) context, evalContext );
                }
                else
                {
                    return GetGenericIndexer( context, evalContext );
                }
            }
            catch (TargetInvocationException e)
            {
                throw new InvalidPropertyException(evalContext.RootContextType, this.ToString(), "Getter for indexer threw an exception.", e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new InvalidPropertyException( evalContext.RootContextType,this.ToString(),"Illegal attempt to get value for the indexer.",e );
            }
            catch (IndexOutOfRangeException e)
            {
                throw new InvalidPropertyException( evalContext.RootContextType,this.ToString(),"Index out of range.",e );
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw new InvalidPropertyException( evalContext.RootContextType,this.ToString(),"Argument out of range.",e );
            }
            catch (InvalidCastException e)
            {
                throw new InvalidPropertyException( evalContext.RootContextType,this.ToString(),"Invalid index type.",e );
            }
            catch (ArgumentException e)
            {
                throw new InvalidPropertyException( evalContext.RootContextType,this.ToString(),"Invalid argument.",e );
            }
        }

        /// <summary>
        /// Sets node's value for the given context.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <param name="newValue">New value for this node.</param>
        protected override void Set(object context, EvaluationContext evalContext, object newValue)
        {
            if (context == null)
            {
                throw new NullValueInNestedPathException("Cannot set the value of the indexer because the context for its resolution is null.");
            }

            try
            {
                if (context is Array)
                {
                    SetArrayValue( (Array) context, evalContext,newValue );
                }
                else if (context is IList)
                {
                    SetListValue( (IList) context, evalContext,newValue );
                }
                else if (context is IDictionary)
                {
                    SetDictionaryValue( (IDictionary) context, evalContext,newValue );
                }
                else
                {
                    SetGenericIndexer( context, evalContext,newValue );
                }
            }
            catch (TargetInvocationException e)
            {
                throw new InvalidPropertyException( evalContext.RootContextType,this.ToString(),"Setter for indexer threw an exception.",e );
            }
            catch (UnauthorizedAccessException e)
            {
                throw new InvalidPropertyException( evalContext.RootContextType,this.ToString(),"Illegal attempt to set value for the indexer.",e );
            }
            catch (IndexOutOfRangeException e)
            {
                throw new InvalidPropertyException( evalContext.RootContextType,this.ToString(),"Index out of range.",e );
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw new InvalidPropertyException( evalContext.RootContextType,this.ToString(),"Argument out of range.",e );
            }
            catch (InvalidCastException e)
            {
                throw new InvalidPropertyException( evalContext.RootContextType,this.ToString(),"Invalid index type.",e );
            }
            catch (ArgumentException e)
            {
                throw new InvalidPropertyException( evalContext.RootContextType,this.ToString(),"Invalid argument.",e );
            }

        }

        /// <summary>
        /// Utility method that is needed by ObjectWrapper and AbstractAutowireCapableObjectFactory.
        /// </summary>
        /// <param name="context">Context to resolve property against.</param>
        /// <param name="variables">Expression variables map.</param>
        /// <returns>PropertyInfo for this node.</returns>
        internal PropertyInfo GetPropertyInfo(object context, IDictionary<string, object> variables)
        {
            lock (this)
            {
                EvaluationContext evalContext = new EvaluationContext(context, variables);
                InitializeIndexerProperty(context, evalContext);

                return indexer.PropertyInfo;
            }
        }

        private object GetArrayValue(Array array, EvaluationContext evalContext)
        {
            int argCount = array.Rank;
            AssertArgumentCount(argCount);

            Int32[] indices = new Int32[argCount];
            for (int i = 0; i < argCount; i++)
            {
                indices[i] = (Int32) ResolveArgument(i, evalContext);
            }
            return array.GetValue(indices);
        }

        private object GetListValue(IList list, EvaluationContext evalContext)
        {
            AssertArgumentCount(1);
            return list[(int) ResolveArgument(0, evalContext)];
        }

        private object GetDictionaryValue(IDictionary dictionary, EvaluationContext evalContext)
        {
            AssertArgumentCount(1);
            return dictionary[ResolveArgument( 0,evalContext )];
        }

        private object GetCharacter(string character, EvaluationContext evalContext)
        {
            AssertArgumentCount(1);
            return character[(int)ResolveArgument( 0,evalContext )];
        }

        private object GetGenericIndexer(object context, EvaluationContext evalContext)
        {
            object[] indices = InitializeIndexerProperty( context, evalContext );
            return indexer.GetValue(context, indices);
        }

        private void SetArrayValue(Array array, EvaluationContext evalContext,object newValue)
        {
            int argCount = array.Rank;
            AssertArgumentCount(argCount);

            Int32[] indices = new Int32[argCount];
            for (int i = 0; i < argCount; i++)
            {
                indices[i] = (Int32) ResolveArgument(i, evalContext);
            }
            array.SetValue(newValue, indices);
        }

        private void SetListValue(IList list, EvaluationContext evalContext,object newValue)
        {
            AssertArgumentCount(1);
            list[(int) ResolveArgument(0, evalContext)] = newValue;
        }

        private void SetDictionaryValue(IDictionary dictionary, EvaluationContext evalContext,object newValue)
        {
            AssertArgumentCount(1);
            dictionary[ResolveArgument( 0,evalContext )] = newValue;
        }

        private void SetGenericIndexer(object context, EvaluationContext evalContext,object newValue)
        {
            object[] indices = InitializeIndexerProperty( context, evalContext );
            indexer.SetValue( context, newValue, indices );
        }

        private object[] InitializeIndexerProperty(object context, EvaluationContext evalContext)
        {
            object[] indices = ResolveArguments( evalContext );

            if (indexer == null)
            {
                lock (this)
                {
                    if (indexer == null)
                    {
                        Type contextType = context.GetType();
                        Type[] argTypes = ReflectionUtils.GetTypes(indices);
                        string defaultMember = "Item";
                        object[] atts = contextType.GetCustomAttributes(typeof(DefaultMemberAttribute), true);
                        if (atts != null && atts.Length > 0)
                        {
                            defaultMember = ((DefaultMemberAttribute) atts[0]).MemberName;
                        }
                        PropertyInfo indexerProperty = contextType.GetProperty(defaultMember, BINDING_FLAGS, null, null, argTypes, null);
                        if (indexerProperty == null)
                        {
                            throw new ArgumentException("Indexer property with specified number and types of arguments does not exist.");
                        }

                        indexer = new SafeProperty(indexerProperty);
                    }
                }
            }

            return indices;
        }
    }
}
