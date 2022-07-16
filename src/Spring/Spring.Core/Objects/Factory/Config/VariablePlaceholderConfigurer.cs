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

using System.Collections;
using System.Globalization;
using Common.Logging;
using Spring.Collections;
using Spring.Core;
using Spring.Util;

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
    public class VariablePlaceholderConfigurer : IObjectFactoryPostProcessor, IPriorityOrdered
    {
        /// <summary>
        /// The default placeholder prefix.
        /// </summary>
        public static readonly string DefaultPlaceholderPrefix = "${";

        /// <summary>
        /// The default placeholder suffix.
        /// </summary>
        public static readonly string DefaultPlaceholderSuffix = "}";

        private int order = Int32.MaxValue; // default: same as non-Ordered

        private bool includeAncestors;
        private bool ignoreUnresolvablePlaceholders;
        private string placeholderPrefix = DefaultPlaceholderPrefix;
        private string placeholderSuffix = DefaultPlaceholderSuffix;

        private IList variableSourceList = new ArrayList();

        /// <summary>
        /// Create a new instance without any variable sources
        /// </summary>
        public VariablePlaceholderConfigurer()
        {}

        /// <summary>
        /// Create a new instance and initialize with the given variable source
        /// </summary>
        /// <param name="variableSource"></param>
        public VariablePlaceholderConfigurer(IVariableSource variableSource)
        {
            this.VariableSource = variableSource;
        }

        /// <summary>
        /// Create a new instance and initialize with the given list of variable sources
        /// </summary>
        public VariablePlaceholderConfigurer(IList variableSources)
        {
            this.VariableSources = variableSources;
        }

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
                variableSourceList.Add( value );
            }
        }

        /// <summary>
        /// The placeholder prefix (the default is <c>${</c>).
        /// </summary>
        /// <seealso cref="DefaultPlaceholderPrefix"/>
        public string PlaceholderPrefix
        {
            set { placeholderPrefix = value; }
        }

        /// <summary>
        /// The placeholder suffix (the default is <c>}</c>)
        /// </summary>
        /// <seealso cref="DefaultPlaceholderSuffix"/>
        public string PlaceholderSuffix
        {
            set { placeholderSuffix = value; }
        }

        /// <summary>
        /// Indicates whether unresolved placeholders should be ignored.
        /// </summary>
        public bool IgnoreUnresolvablePlaceholders
        {
            set { ignoreUnresolvablePlaceholders = value; }
        }

        public bool IncludeAncestors
        {
            set { includeAncestors = value;  }
        }

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
        public void PostProcessObjectFactory( IConfigurableListableObjectFactory factory )
        {
            if (CollectionUtils.IsEmpty(variableSourceList))
            {
                throw new ArgumentException("No VariableSources configured");
            }

            ICollection filtered = CollectionUtils.FindValuesOfType(this.variableSourceList, typeof (IVariableSource));
            if (filtered.Count != this.variableSourceList.Count)
            {
                throw new ArgumentException("'VariableSources' must contain IVariableSource elements only", "VariableSources");
            }

            try
            {
                ProcessProperties( factory );
            }
            catch (Exception ex)
            {
                if (typeof( ObjectsException ).IsInstanceOfType( ex ))
                {
                    throw;
                }
                else
                {
                    throw new ObjectsException(
                        "Errored while postprocessing an object factory.", ex );
                }
            }
        }

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
        protected virtual void ProcessProperties( IConfigurableListableObjectFactory factory )
        {
            CompositeVariableSource compositeVariableSource = new CompositeVariableSource(variableSourceList);
            TextProcessor tp = new TextProcessor(this, compositeVariableSource);
            ObjectDefinitionVisitor visitor = new ObjectDefinitionVisitor(new ObjectDefinitionVisitor.ResolveHandler(tp.ParseAndResolveVariables));

            var objectDefinitionNames = factory.GetObjectDefinitionNames(includeAncestors);
            for (int i = 0; i < objectDefinitionNames.Count; ++i)
            {
                string name = objectDefinitionNames[i];
                IObjectDefinition definition = factory.GetObjectDefinition( name, includeAncestors );

                if (definition == null)
                    continue;

                try
                {
                    visitor.VisitObjectDefinition( definition );
                }
                catch (ObjectDefinitionStoreException ex)
                {
                    throw new ObjectDefinitionStoreException(
                        definition.ResourceDescription, name, ex.Message );
                }
            }
        }

        private class TextProcessor
        {
            private readonly ILog logger = LogManager.GetLogger(typeof(TextProcessor));
            private readonly VariablePlaceholderConfigurer owner;
            private readonly IVariableSource variableSource;

            public TextProcessor(VariablePlaceholderConfigurer owner, IVariableSource variableSource)
            {
                this.owner = owner;
                this.variableSource = variableSource;
            }

            public string ParseAndResolveVariables(string rawStringValue)
            {
                return ParseAndResolveVariables(rawStringValue, new HashedSet());
            }

            private string ParseAndResolveVariables(string strVal, ISet visitedPlaceholders)
            {
                if (strVal == null)
                {
                    return null;
                }

                int startIndex = strVal.IndexOf(owner.placeholderPrefix);
                while (startIndex != -1)
                {
                    int endIndex = strVal.IndexOf(
                        owner.placeholderSuffix, startIndex + owner.placeholderPrefix.Length);
                    if (endIndex != -1)
                    {
                        int pos = startIndex + owner.placeholderPrefix.Length;
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

                        if (variableSource.CanResolveVariable(placeholder))
                        {
                            string resolvedValue = variableSource.ResolveVariable(placeholder);
                            resolvedValue = ParseAndResolveVariables(resolvedValue, visitedPlaceholders);

                            if (logger.IsDebugEnabled)
                            {
                                logger.Debug(string.Format(
                                                 CultureInfo.InvariantCulture,
                                                 "Resolving placeholder '{0}' to '{1}'.", placeholder, resolvedValue));
                            }

                            if (resolvedValue == null
                                && startIndex == 0
                                && strVal.Length <= endIndex + owner.placeholderSuffix.Length)
                            {
                                return null;
                            }
                            strVal = strVal.Substring(0, startIndex) + resolvedValue + strVal.Substring(endIndex + owner.placeholderSuffix.Length);
                            startIndex = strVal.IndexOf(owner.placeholderPrefix, startIndex + (resolvedValue == null ? 0 : resolvedValue.Length));
                        }
                        else if (owner.ignoreUnresolvablePlaceholders)
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
        }

        private class CompositeVariableSource : IVariableSource
        {
            private readonly IList variableSourceList;

            public CompositeVariableSource( IList variableSourceList )
            {
                this.variableSourceList = variableSourceList;
            }

            public string ResolveVariable( string variableName )
            {
                foreach (IVariableSource variableSource in variableSourceList)
                {
                    if (!variableSource.CanResolveVariable(variableName)) continue;

                    return variableSource.ResolveVariable( variableName );
                }
                throw new ArgumentException(string.Format("cannot resolve variable '{0}'", variableName));
            }

            public bool CanResolveVariable(string variableName)
            {
                foreach (IVariableSource variableSource in variableSourceList)
                {
                    if (variableSource.CanResolveVariable(variableName))
                        return true;
                }
                return false;
            }
        }
    }
}
