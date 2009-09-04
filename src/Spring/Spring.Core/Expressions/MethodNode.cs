#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using Spring.Expressions.Processors;
using Spring.Util;
using Spring.Reflection.Dynamic;

namespace Spring.Expressions
{
    /// <summary>
    /// Represents parsed method node in the navigation expression.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class MethodNode : NodeWithArguments
    {
        private const BindingFlags BINDING_FLAGS
            = BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Instance | BindingFlags.Static
            | BindingFlags.IgnoreCase;

        private static readonly IDictionary collectionProcessorMap = new Hashtable();
        private static readonly IDictionary extensionMethodProcessorMap = new Hashtable();

        private bool initialized = false;
        private bool cachedIsParamArray = false;
        private Type paramArrayType;
        private int argumentCount;
        private SafeMethod cachedInstanceMethod;
        private int cachedInstanceMethodHash;

        /// <summary>
        /// Static constructor. Initializes a map of special collection processor methods.
        /// </summary>
        static MethodNode()
        {
            collectionProcessorMap.Add("count", new CountAggregator());
            collectionProcessorMap.Add("sum", new SumAggregator());
            collectionProcessorMap.Add("max", new MaxAggregator());
            collectionProcessorMap.Add("min", new MinAggregator());
            collectionProcessorMap.Add("average", new AverageAggregator());
            collectionProcessorMap.Add("sort", new SortProcessor());
            collectionProcessorMap.Add("orderBy", new OrderByProcessor());
            collectionProcessorMap.Add("distinct", new DistinctProcessor());
            collectionProcessorMap.Add("nonNull", new NonNullProcessor());
            collectionProcessorMap.Add("reverse", new ReverseProcessor());
            collectionProcessorMap.Add("convert", new ConversionProcessor());

            extensionMethodProcessorMap.Add("date", new DateConversionProcessor());
        }

        /// <summary>
        /// Create a new instance
        /// </summary>
        public MethodNode()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected MethodNode(SerializationInfo info, StreamingContext context)
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
            string methodName = this.getText();
            object[] argValues = ResolveArguments(evalContext);

            // resolve method, if necessary
            lock (this)
            {
                // check if it is a collection and the methodname denotes a collection processor
                if ((context == null || context is ICollection))
                {
                    ICollectionProcessor localCollectionProcessor;
                    // predefined collection processor?
                    localCollectionProcessor = (ICollectionProcessor) collectionProcessorMap[methodName];

                    // user-defined collection processor?
                    if (localCollectionProcessor == null && evalContext.Variables != null)
                    {
                        localCollectionProcessor = evalContext.Variables[methodName] as ICollectionProcessor;
                    }

                    if (localCollectionProcessor != null)
                    {
                        return localCollectionProcessor.Process((ICollection) context, argValues);
                    }
                }

                // try extension methods
                IMethodCallProcessor methodCallProcessor = (IMethodCallProcessor)extensionMethodProcessorMap[methodName];
                {
                    // user-defined extension method processor?
                    if (methodCallProcessor == null && evalContext.Variables != null)
                    {
                        methodCallProcessor = evalContext.Variables[methodName] as IMethodCallProcessor;
                    }

                    if (methodCallProcessor != null)
                    {
                        return methodCallProcessor.Process(context, argValues);
                    }
                }

                // try instance method
                if (context != null)
                {
                    // calculate checksum, if the cached method matches the current context
                    if (initialized)
                    {
                        int calculatedHash = CalculateMethodHash(context.GetType(), argValues);
                        initialized = (calculatedHash == cachedInstanceMethodHash);
                    }

                    if (!initialized)
                    {
                        Initialize(methodName, argValues, context);
                        initialized = true;
                    }

                    if (cachedInstanceMethod != null)
                    {
                        object[] paramValues = (cachedIsParamArray)
                                                ? ReflectionUtils.PackageParamArray(argValues, argumentCount, paramArrayType)
                                                : argValues;
                        return cachedInstanceMethod.Invoke(context, paramValues);
                    }
                }
            }
            
            throw new ArgumentException(string.Format("Method '{0}' with the specified number and types of arguments does not exist.", methodName));
        }

        private int CalculateMethodHash(Type contextType, object[] argValues)
        {
            int hash = contextType.GetHashCode();
            for (int i = 0; i < argValues.Length; i++)
            {
                object arg = argValues[i];
                if (arg != null)
                    hash += s_primes[i] * arg.GetType().GetHashCode();
            }
            return hash;
        }

        private void Initialize(string methodName, object[] argValues, object context)
        {
            Type contextType = (context is Type ? context as Type : context.GetType());

            // check the context type first
            MethodInfo mi = GetBestMethod(contextType, methodName, BINDING_FLAGS, argValues);

            // if not found, probe the Type's type          
            if (mi == null)
            {
                mi = GetBestMethod(typeof(Type), methodName, BINDING_FLAGS, argValues);
            }

            if (mi == null)
            {
                return;
            }
            else
            {
                ParameterInfo[] parameters = mi.GetParameters();
                if (parameters.Length > 0)
                {
                    ParameterInfo lastParameter = parameters[parameters.Length - 1];
                    cachedIsParamArray = lastParameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
                    if (cachedIsParamArray)
                    {
                        paramArrayType = lastParameter.ParameterType.GetElementType();
                        argumentCount = parameters.Length;
                    }
                }

                cachedInstanceMethod = new SafeMethod(mi);
                cachedInstanceMethodHash = CalculateMethodHash(contextType, argValues);
            }
        }

        /// <summary>
        /// Gets the best method given the name, argument values, for a given type.
        /// </summary>
        /// <param name="type">The type on which to search for the method.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <param name="argValues">The arg values.</param>
        /// <returns>Best matching method or null if none found.</returns>
        public static MethodInfo GetBestMethod(Type type, string methodName, BindingFlags bindingFlags, object[] argValues)
        {
            MethodInfo mi = null;
            try
            {
                mi = type.GetMethod(methodName, bindingFlags | BindingFlags.FlattenHierarchy);
            }
            catch (AmbiguousMatchException)
            {

                MethodInfo[] overloads = GetCandidateMethods(type, methodName, bindingFlags, argValues.Length);
                if (overloads.Length > 0)
                {
                    mi = ReflectionUtils.GetMethodByArgumentValues(overloads, argValues);
                }
            }
            return mi;
        }



        private static MethodInfo[] GetCandidateMethods(Type type, string methodName, BindingFlags bindingFlags, int argCount)
        {
            MethodInfo[] methods = type.GetMethods(bindingFlags | BindingFlags.FlattenHierarchy);
            ArrayList matches = new ArrayList();

            foreach (MethodInfo method in methods)
            {
                if (method.Name == methodName)
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == argCount)
                    {
                        matches.Add(method);
                    }
                    else if (parameters.Length > 0)
                    {
                        ParameterInfo lastParameter = parameters[parameters.Length - 1];
                        if (lastParameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
                        {
                            matches.Add(method);
                        }
                    }
                }
            }

            return (MethodInfo[])matches.ToArray(typeof(MethodInfo));
        }

        // used to calculate signature hash while caring for arg positions
        private static readonly int[] s_primes =
            {
                17, 19, 23, 29
                , 31, 37, 41, 43, 47, 53, 59, 61, 67, 71
                , 73, 79, 83, 89, 97, 101, 103, 107, 109, 113
                , 127, 131, 137, 139, 149, 151, 157, 163, 167, 173
                , 179, 181, 191, 193, 197, 199, 211, 223, 227, 229
                , 233, 239, 241, 251, 257, 263, 269, 271, 277, 281
                , 283, 293, 307, 311, 313, 317, 331, 337, 347, 349
                , 353, 359, 367, 373, 379, 383, 389, 397, 401, 409
                , 419, 421, 431, 433, 439, 443, 449, 457, 461, 463
                , 467, 479, 487, 491, 499, 503, 509, 521, 523, 541
                , 547, 557, 563, 569, 571, 577, 587, 593, 599, 601
                , 607, 613, 617, 619, 631, 641, 643, 647, 653, 659
                , 661, 673, 677, 683, 691, 701, 709, 719, 727, 733
            };
    }
}
