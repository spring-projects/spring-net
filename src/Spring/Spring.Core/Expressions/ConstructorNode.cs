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

using Spring.Core.TypeResolution;
using Spring.Util;
using Spring.Reflection.Dynamic;

namespace Spring.Expressions
{
    /// <summary>
    /// Represents parsed method node in the navigation expression.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class ConstructorNode : NodeWithArguments
    {
        private SafeConstructor constructor;
        private IDictionary namedArgs;
        private bool isParamArray = false;
        private Type paramArrayType;
        private int argumentCount;

        /// <summary>
        /// Create a new instance
        /// </summary>
        public ConstructorNode()
        {
        }

        /// <summary>
        /// Create a new instance
        /// </summary>
        public ConstructorNode(Type type)
            :base(type.FullName)
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected ConstructorNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Creates new instance of the type defined by this node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            object[] argValues = ResolveArguments(evalContext);
            IDictionary namedArgValues = ResolveNamedArguments(evalContext);

            if (constructor == null)
            {
                lock(this)
                {
                    if (constructor == null)
                    {
                        constructor = InitializeNode(argValues, namedArgValues);
                    }
                }
            }

            object[] paramValues = (isParamArray ? ReflectionUtils.PackageParamArray(argValues, argumentCount, paramArrayType) : argValues);
            object instance = constructor.Invoke(paramValues);
            if (namedArgValues != null)
            {
                SetNamedArguments(instance, namedArgValues);
            }
            
            return instance;
        }

        /// <summary>
        /// Determines the type of object that should be instantiated.
        /// </summary>
        /// <param name="typeName">
        /// The type name to resolve.
        /// </param>
        /// <returns>
        /// The type of object that should be instantiated.
        /// </returns>
        /// <exception cref="TypeLoadException">
        /// If the type cannot be resolved.
        /// </exception>
        protected virtual Type GetObjectType(string typeName)
        {
            return TypeResolutionUtils.ResolveType(typeName);
        }

        /// <summary>
        /// Initializes this node by caching necessary constructor and property info.
        /// </summary>
        /// <param name="argValues"></param>
        /// <param name="namedArgValues"></param>
        private SafeConstructor InitializeNode(object[] argValues, IDictionary namedArgValues)
        {
            SafeConstructor ctor = null;
            Type objectType = GetObjectType(this.getText().Trim());
                
            // cache constructor info
            ConstructorInfo ci = GetBestConstructor(objectType, argValues);
            if (ci == null)
            {
                throw new ArgumentException(
                    String.Format("Constructor for the type [{0}] with a specified " +
                                  "number and types of arguments does not exist.",
                                  objectType.FullName));
            }
            else 
            {
                ParameterInfo[] parameters = ci.GetParameters();
                if (parameters.Length > 0)
                {
                    ParameterInfo lastParameter = parameters[parameters.Length - 1];
                    isParamArray = lastParameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
                    if (isParamArray)
                    {
                        paramArrayType = lastParameter.ParameterType.GetElementType();
                        argumentCount = parameters.Length;
                    }
                }
                ctor = new SafeConstructor(ci);
            }
                
            // cache named args info
            if (namedArgValues != null)
            {
                namedArgs = new Hashtable(namedArgValues.Count);
                foreach (string name in namedArgValues.Keys)
                {
                    this.namedArgs[name] = Expression.ParseProperty(name);
                }
            }

            return ctor;
        }

        /// <summary>
        /// Sets the named arguments (properties).
        /// </summary>
        /// <param name="instance">Instance to set property values on.</param>
        /// <param name="namedArgValues">Argument (property) name to value mappings.</param>
        private void SetNamedArguments(object instance, IDictionary namedArgValues)
        {
            foreach (string name in namedArgValues.Keys)
            {
                IExpression property = (IExpression) namedArgs[name];
                property.SetValue(instance, namedArgValues[name]);
            }
        }

        private static ConstructorInfo GetBestConstructor(Type type, object[] argValues)
        {
            IList<ConstructorInfo> candidates = GetCandidateConstructors(type, argValues.Length);
            if (candidates.Count > 0)
            {
                return ReflectionUtils.GetConstructorByArgumentValues(candidates, argValues);
            }
            return null;
        }

        private static IList<ConstructorInfo> GetCandidateConstructors(Type type, int argCount)
        {
            ConstructorInfo[] ctors = type.GetConstructors(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            List<ConstructorInfo> matches = new List<ConstructorInfo>();

            foreach (ConstructorInfo ctor in ctors)
            {
                ParameterInfo[] parameters = ctor.GetParameters();
                if (parameters.Length == argCount)
                {
                    matches.Add(ctor);
                }
                else if (parameters.Length > 0)
                {
                    ParameterInfo lastParameter = parameters[parameters.Length - 1];
                    if (lastParameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
                    {
                        matches.Add(ctor);
                    }
                }
            }

            return matches;
        }

    }
}
