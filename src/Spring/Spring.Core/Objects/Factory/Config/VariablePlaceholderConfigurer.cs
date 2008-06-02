#region License

/*
 * Copyright 2002-2007 the original author or authors.
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
using System.Globalization;
using Common.Logging;
using Spring.Collections;
using Spring.Core;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Resolves placeholder values in one or more object definitions
    /// </summary>
    /// <remarks>
    /// The placeholder syntax follows the NAnt style: <c>${...}</c>.
    /// Placeholders values are resolved against a list of 
    /// <see cref="IVariableSource"/>s.  In case of multiple definitions
    /// for the same property placeholder name, the first one in the 
    /// list is used.
    /// <para>Variable substitution is performed on simple property values, 
    /// lists, dictionaries, sets, constructor
    /// values, object type name, and object names in
    /// runtime object references (
    /// <see cref="Spring.Objects.Factory.Config.RuntimeObjectReference"/>).</para>
    /// <para>Furthermore, placeholder values can also cross-reference other
    /// placeholders, in the manner of the following example where the
    /// <c>rootPath</c> property is cross-referenced by the <c>subPath</c>
    /// property.
    /// </para>
    /// <example>
    /// <code escaped="true">
    /// <name-values>
    ///		<add key="rootPath" value="myrootdir"/>
    ///		<add key="subPath" value="${rootPath}/subdir"/>
    /// </name-values>
    /// </code>
    /// </example>
    /// <para>If a configurer cannot resolve a placeholder, and the value of the
    /// <see cref="Spring.Objects.Factory.Config.PropertyPlaceholderConfigurer.IgnoreUnresolvablePlaceholders"/>
    /// property is currently set to <see langword="false"/>, an
    /// <see cref="Spring.Objects.Factory.ObjectDefinitionStoreException"/>
    /// will be thrown. </para>
    /// </remarks>
    /// <author>Mark Pollack</author>
    public class VariablePlaceholderConfigurer : IObjectFactoryPostProcessor, IOrdered
    {
        #region Fields
        private int order = Int32.MaxValue; // default: same as non-Ordered

        private bool ignoreUnresolvablePlaceholders;

        private IList variableSourceList;
        #endregion

        #region Properties

        /// <summary>
        /// Sets the list of <see cref="IVariableSource"/>s that will be used to resolve placeholder names.
        /// </summary>
        /// <value>A list of <see cref="IVariableSource"/>s.</value>
        public IList VariableSources
        {
            set { variableSourceList = value; }
        }

        /// <summary>
        /// Sets <see cref="IVariableSource"/> that will be used to resolve placeholder names.
        /// </summary>
        /// <value>A <see cref="IVariableSource"/> instance.</value>
        public IVariableSource VariableSource
        {
            set
            {
                variableSourceList = new ArrayList();
                variableSourceList.Add(value);
            }
        }

        /// <summary>
        /// Indicates whether unresolved placeholders should be ignored.
        /// </summary>
        public bool IgnoreUnresolvablePlaceholders
        {
            set { ignoreUnresolvablePlaceholders = value; }
        }

        #endregion

        #region IObjectFactoryPostProcessor Members

        /// <summary>
        /// Modify the application context's internal object factory after its
        /// standard initialization.
        /// </summary>
        /// <param name="factory">The object factory used by the application context.</param>
        /// <remarks>
        /// <p>
        /// All object definitions will have been loaded, but no objects will have
        /// been instantiated yet. This allows for overriding or adding properties
        /// even to eager-initializing objects.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        public void PostProcessObjectFactory(IConfigurableListableObjectFactory factory)
        {
            try
            {
                ProcessProperties(factory);
            }
            catch (Exception ex)
            {
                if (typeof (ObjectsException).IsInstanceOfType(ex))
                {
                    throw;
                }
                else
                {
                    throw new ObjectsException(
                        "Errored while postprocessing an object factory.", ex);
                }
            }
        }

        #endregion

        #region IOrdered Members

        /// <summary>
        /// Return the order value of this object, where a higher value means greater in
        /// terms of sorting.
        /// </summary>
        /// <returns>The order value.</returns>
        /// <seealso cref="Spring.Core.IOrdered.Order"/>
        public int Order
        {
            get { return order; }
            set { order = value; }
        }

        #endregion

        /// <summary>
        /// Apply the property replacement using the specified <see cref="IVariableSource"/>s for all
        /// object in the supplied
        /// <see cref="Spring.Objects.Factory.Config.IConfigurableListableObjectFactory"/>.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="Spring.Objects.Factory.Config.IConfigurableListableObjectFactory"/>
        /// used by the application context.
        /// </param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If an error occured.
        /// </exception>
        protected virtual void ProcessProperties(IConfigurableListableObjectFactory factory)
        {
            IVariableSource compositeVariableSource =
                new PlaceholderResolvingCompositeVariableSource(variableSourceList, ignoreUnresolvablePlaceholders);
            ObjectDefinitionVisitor visitor = new ObjectDefinitionVisitor(compositeVariableSource);

            string[] objectDefinitionNames = factory.GetObjectDefinitionNames();
            for (int i = 0; i < objectDefinitionNames.Length; ++i)
            {
                string name = objectDefinitionNames[i];
                IObjectDefinition definition = factory.GetObjectDefinition(name);
                try
                {
                    visitor.VisitObjectDefinition(definition);
                }
                catch (ObjectDefinitionStoreException ex)
                {
                    throw new ObjectDefinitionStoreException(
                        definition.ResourceDescription, name, ex.Message);
                }
            }
        }
    }

    #region Helper class
    internal class PlaceholderResolvingCompositeVariableSource : IVariableSource
    {
        private string placeholderPrefix = "${";
        private string placeholderSuffix = "}";
        private bool ignoreUnresolvablePlaceholders;

        private ILog logger = LogManager.GetLogger(typeof (PlaceholderResolvingCompositeVariableSource));

        private IList variableSourceList;

        public PlaceholderResolvingCompositeVariableSource(IList variableSourceList, bool ignoreUnresolvablePlaceholders)
        {
            this.variableSourceList = variableSourceList;
            this.ignoreUnresolvablePlaceholders = ignoreUnresolvablePlaceholders;
        }

        #region IVariableSource Members

        public string ResolveVariable(string rawStringValue)
        {
            return ParseAndResolveVariable(rawStringValue, new HashedSet());
        }


        //TODO handle resolved values at are not string - identify this case as only 1 placeholder present?

        private string ParseAndResolveVariable(string strVal, ISet visitedPlaceholders)
        {
            int startIndex = strVal.IndexOf(placeholderPrefix);
            while (startIndex != -1)
            {
                int endIndex = strVal.IndexOf(
                    placeholderSuffix, startIndex + placeholderPrefix.Length);
                if (endIndex != -1)
                {
                    int pos = startIndex + placeholderPrefix.Length;
                    string placeholder = strVal.Substring(pos, endIndex - pos);
                    if (visitedPlaceholders.Contains(placeholder))
                    {
                        throw new ObjectDefinitionStoreException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Circular placeholder reference '{0}' detected. ",
                                placeholder));
                    }
                    visitedPlaceholders.Add(placeholder);
                    string resolvedValue = ResolvePlaceholderVariable(placeholder);
                    if (resolvedValue != null)
                    {
                        resolvedValue = ParseAndResolveVariable(resolvedValue, visitedPlaceholders);

                        #region Instrumentation

                        if (logger.IsDebugEnabled)
                        {
                            logger.Debug(string.Format(
                                             CultureInfo.InvariantCulture,
                                             "Resolving placeholder '{0}' to '{1}'.", placeholder, resolvedValue));
                        }

                        #endregion

                        strVal = strVal.Substring(0, startIndex) + resolvedValue + strVal.Substring(endIndex + 1);
                        startIndex = strVal.IndexOf(placeholderPrefix, startIndex + resolvedValue.Length);
                    }
                    else if (ignoreUnresolvablePlaceholders)
                    {
                        // simply return the unprocessed value...
                        return strVal;
                    }
                    else
                    {
                        throw new ObjectDefinitionStoreException(string.Format(
                                                                     CultureInfo.InvariantCulture,
                                                                     "Could not resolve placeholder '{0}'.", placeholder));
                    }
                    visitedPlaceholders.Remove(placeholder);
                }
                else
                {
                    startIndex = -1;
                }
            }
            return strVal;
        }

        private string ResolvePlaceholderVariable(string variableName)
        {
            foreach (IVariableSource variableSource in variableSourceList)
            {
                //TODO handle resolved values at are not strings?

                object resolvedValue = variableSource.ResolveVariable(variableName);
                if (resolvedValue is string)
                {
                }
                if (resolvedValue != null)
                {
                    if (resolvedValue is string)
                    {
                        return resolvedValue as string;
                    }
                    else
                    {
                        logger.Warn("Placeholder " + variableSource + " resolved to object type [" + resolvedValue.GetType() + "].  Only string type currently supported");
                    }
                }
            }
            return null;
        }

        #endregion
    }

    #endregion
}