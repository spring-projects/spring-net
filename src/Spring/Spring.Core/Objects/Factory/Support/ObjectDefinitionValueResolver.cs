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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Remoting;
using Common.Logging;
using Spring.Core.TypeConversion;
using Spring.Expressions;
using Spring.Objects.Factory.Config;

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Helper class for use in object factory implementations,
    /// resolving values contained in object definition objects
    /// into the actual values applied to the target object instance.
    /// </summary>
    /// <remarks>
    /// Used by <see cref="AbstractAutowireCapableObjectFactory"/>.
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class ObjectDefinitionValueResolver
    {
        private readonly ILog log;

        private readonly AbstractObjectFactory objectFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDefinitionValueResolver"/> class.
        /// </summary>
        /// <param name="objectFactory">The object factory.</param>
        public ObjectDefinitionValueResolver(AbstractObjectFactory objectFactory)
        {
            this.log = LogManager.GetLogger(this.GetType());

            this.objectFactory = objectFactory;
        }

        /// <summary>
        /// Given a property value, return a value, resolving any references to other
        /// objects in the factory if necessary.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The value could be :
        /// <list type="bullet">
        /// <item>
        /// <p>
        /// An <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/>,
        /// which leads to the creation of a corresponding new object instance.
        /// Singleton flags and names of such "inner objects" are always ignored: inner objects
        /// are anonymous prototypes.
        /// </p>
        /// </item>
        /// <item>
        /// <p>
        /// A <see cref="Spring.Objects.Factory.Config.RuntimeObjectReference"/>, which must
        /// be resolved.
        /// </p>
        /// </item>
        /// <item>
        /// <p>
        /// An <see cref="Spring.Objects.Factory.Config.IManagedCollection"/>. This is a
        /// special placeholder collection that may contain
        /// <see cref="Spring.Objects.Factory.Config.RuntimeObjectReference"/>s or
        /// collections that will need to be resolved.
        /// </p>
        /// </item>
        /// <item>
        /// <p>
        /// An ordinary object or <see langword="null"/>, in which case it's left alone.
        /// </p>
        /// </item>
        /// </list>
        /// </p>
        /// </remarks>
        /// <param name="name">
        /// The name of the object that is having the value of one of its properties resolved.
        /// </param>
        /// <param name="definition">
        /// The definition of the named object.
        /// </param>
        /// <param name="argumentName">
        /// The name of the property the value of which is being resolved.
        /// </param>
        /// <param name="argumentValue">
        /// The value of the property that is being resolved.
        /// </param>
        public virtual object ResolveValueIfNecessary(string name, IObjectDefinition definition, string argumentName, object argumentValue)
        {
            object resolvedValue = null;
            resolvedValue = ResolvePropertyValue(name, definition, argumentName, argumentValue);
            return resolvedValue;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="name">
        /// The name of the object that is having the value of one of its properties resolved.
        /// </param>
        /// <param name="definition">
        /// The definition of the named object.
        /// </param>
        /// <param name="argumentName">
        /// The name of the property the value of which is being resolved.
        /// </param>
        /// <param name="argumentValue">
        /// The value of the property that is being resolved.
        /// </param>
        private object ResolvePropertyValue(string name, IObjectDefinition definition, string argumentName, object argumentValue)
        {
            object resolvedValue = null;
            
            // we must check the argument value to see whether it requires a runtime
            // reference to another object to be resolved.
            // if it does, we'll attempt to instantiate the object and set the reference.
            if (RemotingServices.IsTransparentProxy(argumentValue))
            {
                resolvedValue = argumentValue;
            }
            else if (argumentValue is ICustomValueReferenceHolder)
            {
                resolvedValue = ((ICustomValueReferenceHolder) argumentValue).Resolve(objectFactory, name, definition, argumentName, argumentValue);
            }
            else if (argumentValue is ObjectDefinitionHolder)
            {
                // contains an IObjectDefinition with name and aliases...
                ObjectDefinitionHolder holder = (ObjectDefinitionHolder)argumentValue;
                resolvedValue = ResolveInnerObjectDefinition(name, holder.ObjectName, argumentName, holder.ObjectDefinition, definition.IsSingleton);
            }
            else if (argumentValue is IObjectDefinition)
            {
                // resolve plain IObjectDefinition, without contained name: use dummy name... 
                IObjectDefinition def = (IObjectDefinition)argumentValue;
                resolvedValue = ResolveInnerObjectDefinition(name, "(inner object)", argumentName, def, definition.IsSingleton);

            }
            else if (argumentValue is RuntimeObjectReference)
            {
                RuntimeObjectReference roref = (RuntimeObjectReference)argumentValue;
                resolvedValue = ResolveReference(definition, name, argumentName, roref);
            }
            else if (argumentValue is ExpressionHolder)
            {
                ExpressionHolder expHolder = (ExpressionHolder)argumentValue;
                object context = null;
                IDictionary<string, object> variables = null;

                if (expHolder.Properties != null)
                {
                    PropertyValue contextProperty = expHolder.Properties.GetPropertyValue("Context");
                    context = contextProperty == null
                                  ? null
                                  : ResolveValueIfNecessary(name, definition, "Context",
                                                            contextProperty.Value);
                    PropertyValue variablesProperty = expHolder.Properties.GetPropertyValue("Variables");
                    object vars = (variablesProperty == null
                                       ? null
                                       : ResolveValueIfNecessary(name, definition, "Variables",
                                                                 variablesProperty.Value));
                    if (vars is IDictionary<string, object>)
                    {
                        variables = (IDictionary<string, object>)vars;
                    }
                    if (vars is IDictionary)
                    {
                        IDictionary temp = (IDictionary) vars;
                        variables = new Dictionary<string, object>(temp.Count);
                        foreach (DictionaryEntry entry in temp)
                        {
                            variables.Add((string) entry.Key, entry.Value);
                        }
                    }
                    else
                    {
                        if (vars != null) throw new ArgumentException("'Variables' must resolve to an IDictionary");
                    }
                }

                if (variables == null) variables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                // add 'this' objectfactory reference to variables
                variables.Add(Expression.ReservedVariableNames.CurrentObjectFactory, objectFactory);

                resolvedValue = expHolder.Expression.GetValue(context, variables);
            }
            else if (argumentValue is IManagedCollection)
            {
                resolvedValue =
                    ((IManagedCollection)argumentValue).Resolve(name, definition, argumentName, ResolveValueIfNecessary);
            }
            else if (argumentValue is TypedStringValue)
            {
                TypedStringValue tsv = (TypedStringValue)argumentValue;
                try
                {
                    Type resolvedTargetType = ResolveTargetType(tsv);
                    if (resolvedTargetType != null)
                    {
                        resolvedValue = TypeConversionUtils.ConvertValueIfNecessary(tsv.TargetType, tsv.Value, null);
                    }
                    else
                    {
                        resolvedValue = tsv.Value;
                    }
                }
                catch (Exception ex)
                {
                    throw new ObjectCreationException(definition.ResourceDescription, name,
                                                      "Error converted typed String value for " + argumentName, ex);
                }

            }
            else
            {
                // no need to resolve value...
                resolvedValue = argumentValue;
            }
            return resolvedValue;
        }

        /// <summary>
        /// Resolve the target type of the passed <see cref="TypedStringValue"/>.
        /// </summary>
        /// <param name="value">The <see cref="TypedStringValue"/> who's target type is to be resolved</param>
        /// <returns>The resolved target type, if any. <see lang="null" /> otherwise.</returns>
        protected virtual Type ResolveTargetType(TypedStringValue value)
        {
            if (value.HasTargetType) 
            {
			    return value.TargetType;
            }
            return value.ResolveTargetType();
        }

        /// <summary>
        /// Resolves an inner object definition.
        /// </summary>
        /// <param name="name">
        /// The name of the object that surrounds this inner object definition.
        /// </param>
        /// <param name="innerObjectName">
        /// The name of the inner object definition... note: this is a synthetic
        /// name assigned by the factory (since it makes no sense for inner object
        /// definitions to have names).
        /// </param>
        /// <param name="argumentName">
        /// The name of the property the value of which is being resolved.
        /// </param>
        /// <param name="definition">
        /// The definition of the inner object that is to be resolved.
        /// </param>
        /// <param name="singletonOwner">
        /// <see langword="true"/> if the owner of the property is a singleton.
        /// </param>
        /// <returns>
        /// The resolved object as defined by the inner object definition.
        /// </returns>
        protected virtual object ResolveInnerObjectDefinition(string name, string innerObjectName, string argumentName, IObjectDefinition definition,
                                                      bool singletonOwner)
        {
            RootObjectDefinition mod = objectFactory.GetMergedObjectDefinition(innerObjectName, definition);

            // Check given bean name whether it is unique. If not already unique,
            // add counter - increasing the counter until the name is unique.
            String actualInnerObjectName = innerObjectName;
            if (mod.IsSingleton)
            {
                actualInnerObjectName = AdaptInnerObjectName(innerObjectName);
            }


            mod.IsSingleton = singletonOwner;
            object instance;
            object result;
            try
            {
                //SPRNET-986 ObjectUtils.EmptyObjects -> null
                instance = objectFactory.InstantiateObject(actualInnerObjectName, mod, null, false, false);
                result = objectFactory.GetObjectForInstance(instance, actualInnerObjectName, actualInnerObjectName, mod);
            }
            catch (ObjectsException ex)
            {
                throw ObjectCreationException.GetObjectCreationException(ex, name, argumentName, definition.ResourceDescription, innerObjectName);
            }
            if (singletonOwner && instance is IDisposable)
            {
                // keep a reference to the inner object instance, to be able to destroy
                // it on factory shutdown...
                objectFactory.DisposableInnerObjects.Add(instance);
            }
            return result;
        }

        /// <summary>
        /// Checks the given bean name whether it is unique. If not already unique,
        /// a counter is added, increasing the counter until the name is unique.
        /// </summary>
        /// <param name="innerObjectName">Original Name of the inner object.</param>
        /// <returns>The Adapted name for the inner object</returns>
        private string AdaptInnerObjectName(string innerObjectName)
        {
            string actualInnerObjectName = innerObjectName;
            int counter = 0;
            while (this.objectFactory.IsObjectNameInUse(actualInnerObjectName))
            {
                counter++;
                actualInnerObjectName = innerObjectName + ObjectFactoryUtils.GeneratedObjectNameSeparator + counter;
            }
            return actualInnerObjectName;
        }

        /// <summary>
        /// Resolve a reference to another object in the factory.
        /// </summary>
        /// <param name="name">
        /// The name of the object that is having the value of one of its properties resolved.
        /// </param>
        /// <param name="definition">
        /// The definition of the named object.
        /// </param>
        /// <param name="argumentName">
        /// The name of the property the value of which is being resolved.
        /// </param>
        /// <param name="reference">
        /// The runtime reference containing the value of the property.
        /// </param>
        /// <returns>A reference to another object in the factory.</returns>
        protected virtual object ResolveReference(IObjectDefinition definition, string name, string argumentName, RuntimeObjectReference reference)
        {
            #region Instrumentation
            if (log.IsDebugEnabled)
            {
                log.Debug(
                        string.Format(CultureInfo.InvariantCulture, "Resolving reference from property '{0}' in object '{1}' to object '{2}'.",
                                      argumentName, name, reference.ObjectName));
            }
            #endregion

            try
            {
                if (reference.IsToParent)
                {
                    if (null == objectFactory.ParentObjectFactory)
                    {
                        throw new ObjectCreationException(definition.ResourceDescription, name,
                                                          string.Format(
                                                              "Can't resolve reference to '{0}' in parent factory: " + "no parent factory available.",
                                                              reference.ObjectName));
                    }
                    return objectFactory.ParentObjectFactory.GetObject(reference.ObjectName);
                }
                return objectFactory.GetObject(reference.ObjectName);
            }
            catch (ObjectsException ex)
            {
                throw ObjectCreationException.GetObjectCreationException(ex, name, argumentName, definition.ResourceDescription, reference.ObjectName);
            }
        }


    }
}