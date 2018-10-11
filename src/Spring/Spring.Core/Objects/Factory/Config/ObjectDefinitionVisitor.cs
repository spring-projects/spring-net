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

#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using Spring.Collections;
using Spring.Util;

#endregion

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Visitor class for traversing <see cref="IObjectDefinition"/> objects, in particular
    /// the property values and constructor arguments contained in them resolving
    /// object metadata values.
    /// </summary>
    /// <remarks>
    /// Used by <see cref="PropertyPlaceholderConfigurer"/> and <see cref="VariablePlaceholderConfigurer"/>
    /// to parse all string values contained in a ObjectDefinition, resolving any placeholders found.
    /// </remarks>
    /// <author>Mark Pollack</author>
    public class ObjectDefinitionVisitor
    {
        public delegate string ResolveHandler(string rawStringValue);

        private readonly ResolveHandler resolveHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDefinitionVisitor"/> class.
        /// </summary>
        /// <param name="resolveHandler">The handler to be called for resolving variables contained in a string.</param>
        public ObjectDefinitionVisitor(ResolveHandler resolveHandler)
        {
            AssertUtils.ArgumentNotNull(resolveHandler, "ResovleHandler");
            this.resolveHandler = resolveHandler;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDefinitionVisitor"/> class
        /// for subclassing
        /// </summary>
        /// <remarks>Subclasses should override the <code>ResolveStringValue</code> method</remarks>
        protected ObjectDefinitionVisitor()
        {           
        }

        /// <summary>
        /// Traverse the given ObjectDefinition object and the MutablePropertyValues
        /// and ConstructorArgumentValues contained in them.
        /// </summary>
        /// <param name="definition">The object definition to traverse.</param>
        public virtual void VisitObjectDefinition(IObjectDefinition definition)
        {
            VisitObjectTypeName(definition);
            VisitPropertyValues(definition);

			ConstructorArgumentValues cas = definition.ConstructorArgumentValues;
            if (cas != null)
            {
                VisitIndexedArgumentValues(cas.IndexedArgumentValues);
                VisitNamedArgumentValues(cas.NamedArgumentValues);
                VisitGenericArgumentValues(cas.GenericArgumentValues);
            }
        }

        /// <summary>
        /// Visits the ObjectDefinition property ObjectTypeName, replacing string values using 
        /// the specified IVariableSource.
        /// </summary>
        /// <param name="objectDefinition">The object definition.</param>
        protected virtual void VisitObjectTypeName(IObjectDefinition objectDefinition)
        {
            string objectTypeName = objectDefinition.ObjectTypeName;
            if (objectTypeName != null)
            {
                string resolvedName = ResolveStringValue(objectTypeName);
                if (!objectTypeName.Equals(resolvedName))
                {
                    objectDefinition.ObjectTypeName = resolvedName;
                }
            }
        }


        /// <summary>
        /// Visits the property values of the ObjectDefinition, replacing string values
        /// using the specified IVariableSource.
        /// </summary>
        /// <param name="objectDefinition">The object definition.</param>
        protected virtual void VisitPropertyValues(IObjectDefinition objectDefinition)
        {
            MutablePropertyValues pvs = objectDefinition.PropertyValues;
            if (pvs != null)
            {
                for (int j = 0; j < pvs.PropertyValues.Count; j++)
                {
                    PropertyValue pv = pvs.PropertyValues[j];
                    object newVal = ResolveValue(pv.Value);
                    if (!ObjectUtils.NullSafeEquals(newVal, pv.Value))
                    {
                        pvs.Add(pv.Name, newVal);
                    }
                }
            }
        }

        /// <summary>
        /// Visits the indexed constructor argument values, replacing string values using the
        /// specified IVariableSource.
        /// </summary>
        /// <param name="ias">The indexed argument values.</param>
        protected virtual void VisitIndexedArgumentValues(IReadOnlyDictionary<int, ConstructorArgumentValues.ValueHolder> ias)
        {
            foreach (ConstructorArgumentValues.ValueHolder valueHolder in ias.Values)
            {
                ConfigureConstructorArgument(valueHolder);
            }
        }

        /// <summary>
        /// Visits the named constructor argument values, replacing string values using the
        /// specified IVariableSource.
        /// </summary>
        /// <param name="nav">The named argument values.</param>
        protected virtual void VisitNamedArgumentValues(IReadOnlyDictionary<string, object> nav)
        {
            foreach (ConstructorArgumentValues.ValueHolder valueHolder in nav.Values)
            {
                ConfigureConstructorArgument(valueHolder);
            }
        }

        /// <summary>
        /// Visits the generic constructor argument values, replacing string values using
        /// the specified IVariableSource.
        /// </summary>
        /// <param name="gav">The genreic argument values.</param>
        protected virtual void VisitGenericArgumentValues(IReadOnlyList<ConstructorArgumentValues.ValueHolder> gav)
        {
            for (var i = 0; i < gav.Count; i++)
            {
                ConfigureConstructorArgument(gav[i]);
            }
        }

        /// <summary>
        /// Configures the constructor argument ValueHolder.
        /// </summary>
        /// <param name="valueHolder">The vconstructor alue holder.</param>
        protected void ConfigureConstructorArgument(ConstructorArgumentValues.ValueHolder valueHolder)
        {
            object newVal = ResolveValue(valueHolder.Value);
            if (!ObjectUtils.NullSafeEquals(newVal, valueHolder.Value))
            {
                valueHolder.Value = newVal;
            }
        }

        /// <summary>
        /// Resolves the given value taken from an object definition according to its type
        /// </summary>
        /// <param name="value">the value to resolve</param>
        /// <returns>the resolved value</returns>
        protected virtual object ResolveValue(object value)
        {
            if (value is IObjectDefinition definition)
            {
                VisitObjectDefinition(definition);
            }
            else if (value is ObjectDefinitionHolder definitionHolder)
            {
                VisitObjectDefinition( definitionHolder.ObjectDefinition);
            }
            else if (value is RuntimeObjectReference ror)
            {
                //name has to be of string type.
                string newObjectName = ResolveStringValue(ror.ObjectName);
                if (!newObjectName.Equals(ror.ObjectName))
                {
                    return new RuntimeObjectReference(newObjectName);
                }
            }
            else if (value is ManagedList list)
            {
                VisitManagedList(list);
            }
            else if (value is ManagedSet set)
            {
                VisitManagedSet(set);
            }
            else if (value is ManagedDictionary dictionary)
            {
                VisitManagedDictionary(dictionary);
            }
            else if (value is NameValueCollection collection)
            {
                VisitNameValueCollection(collection);
            }
            else if (value is TypedStringValue typedStringValue)
            {
                String stringValue = typedStringValue.Value;
                if (stringValue != null)
                {
                    String visitedString = ResolveStringValue(stringValue);
                    typedStringValue.Value = visitedString;
                }
            }
            else if (value is string s)
            {
                return ResolveStringValue(s);
            }
            else if (value is ExpressionHolder holder)
            {
                string newExpressionString = ResolveStringValue(holder.ExpressionString);
                return new ExpressionHolder(newExpressionString);
            }
            return value;
        }

        /// <summary>
        /// Visits the ManagedList property ElementTypeName and 
        /// calls <see cref="ResolveValue"/> for list element.
        /// </summary>
        protected virtual void VisitManagedList(ManagedList listVal)
        {            
            string elementTypeName = listVal.ElementTypeName;
            if (elementTypeName != null)
            {
                string resolvedName = ResolveStringValue(elementTypeName);
                if (!elementTypeName.Equals(resolvedName))
                {
                    listVal.ElementTypeName = resolvedName;
                }
            }

            for (int i = 0; i < listVal.Count; ++i)
            {
                object oldValue = listVal[i];
                object newValue = ResolveValue(oldValue);
                if (!ObjectUtils.NullSafeEquals(newValue, oldValue))
                {
                    listVal[i] = newValue;
                }
            }
        }

        /// <summary>
        /// Visits the ManagedSet property ElementTypeName and 
        /// calls <see cref="ResolveValue"/> for list element.
        /// </summary>
        protected virtual void VisitManagedSet(ManagedSet setVal)
        {
            string elementTypeName = setVal.ElementTypeName;
            if (elementTypeName != null)
            {
                string resolvedName = ResolveStringValue(elementTypeName);
                if (!elementTypeName.Equals(resolvedName))
                {
                    setVal.ElementTypeName = resolvedName;
                }
            }

            ISet clone = (ISet)setVal.Clone();
            foreach (object oldValue in clone)
            {
                object newValue = ResolveValue(oldValue);
                if (!ObjectUtils.NullSafeEquals(newValue, oldValue))
                {
                    setVal.Remove(oldValue);
                    setVal.Add(newValue);
                }
            }
        }

        /// <summary>
        /// Visits the ManagedSet properties KeyTypeName and ValueTypeName and 
        /// calls <see cref="ResolveValue"/> for dictionary's value element.
        /// </summary>
        protected virtual void VisitManagedDictionary(ManagedDictionary dictVal)
        {
            string keyTypeName = dictVal.KeyTypeName;
            if (keyTypeName != null)
            {
                string resolvedName = ResolveStringValue(keyTypeName);
                if (!keyTypeName.Equals(resolvedName))
                {
                    dictVal.KeyTypeName = resolvedName;
                }
            }

            string valueTypeName = dictVal.ValueTypeName;
            if (valueTypeName != null)
            {
                string resolvedName = ResolveStringValue(valueTypeName);
                if (!valueTypeName.Equals(resolvedName))
                {
                    dictVal.ValueTypeName = resolvedName;
                }
            }

            Hashtable mods = new Hashtable();
            bool entriesModified = false;
            foreach (DictionaryEntry entry in dictVal)
            {
                /*

                object oldValue = entry.Value;
                object newValue = ResolveValue(oldValue);
                if (!ObjectUtils.NullSafeEquals(newValue, oldValue))
                {
                    mods[entry.Key] = newValue;
                }*/

                object key = entry.Key;                
                object newKey = ResolveValue(key);
                object oldValue = entry.Value;                
                object newValue = ResolveValue(oldValue);

                if (!ObjectUtils.NullSafeEquals(newValue, oldValue) || key != newKey)
                {
                    entriesModified = true;
                }
                mods[newKey] = newValue;

            }
            if (entriesModified)
            {
                dictVal.Clear();
                foreach (DictionaryEntry entry in mods)
                {
                    dictVal[entry.Key] = entry.Value;
                }
            }

        }

        /// <summary>
        /// Visits the elements of a NameValueCollection and calls
        /// <see cref="ResolveValue"/> for value of each element.
        /// </summary>
        protected virtual void VisitNameValueCollection(NameValueCollection collection)
        {
            foreach (string key in collection.AllKeys)
            {
                string oldValue = collection[key];
                string newValue = ResolveValue(oldValue) as string;
                if (!ObjectUtils.NullSafeEquals(newValue, oldValue))
                {
                    collection[key] = newValue;
                }
            }
        }

        /// <summary>
        /// calls the <see cref="ResolveHandler"/> to resolve any variables contained in the raw string.
        /// </summary>
        /// <param name="rawStringValue">the raw string value containing variable placeholders to be resolved</param>
        /// <exception cref="InvalidOperationException">If no <see cref="IVariableSource"/> has been configured.</exception>
        /// <returns>the resolved string, having variables being replaced, if any</returns>
        protected virtual string ResolveStringValue(string rawStringValue)
        {
            if (resolveHandler == null)
            {
                throw new InvalidOperationException("No resolveHandler specified - pass an instance " +
                                                    "into the constructor or override the 'ResolveStringValue' method");
            }

            return resolveHandler(rawStringValue);
        }
    }
}