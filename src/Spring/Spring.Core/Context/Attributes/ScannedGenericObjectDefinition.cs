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
using Spring.Objects;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Stereotype;
using Spring.Objects.Factory.Attributes;

namespace Spring.Context.Attributes
{
    /// <summary>
    /// A GenericObjectDefinition that provides attribute driven propulation
    /// of properties like LazyInit, Scope or Qualifier
    /// </summary>
    public class ScannedGenericObjectDefinition : GenericObjectDefinition
    {
        /// <summary>
        /// Name provided by the Component Attribute
        /// </summary>
        private string _componentName;

        /// <summary>
        /// Creates a GenericObjectDefinition that applies the default values provided
        /// in the XML Spring config document. Additionally parses the specific class
        /// attributesthat allows the definition of LazyInit, Scope or Qualifier
        /// </summary>
        /// <param name="typeOfObject">Type of scanned component</param>
        /// <param name="defaults">Defualts provided in Spring Config document</param>
        public ScannedGenericObjectDefinition(Type typeOfObject, DocumentDefaultsDefinition defaults)
        {
            ObjectType = typeOfObject;

            ParseName();
            ApplyDefaults(defaults);
            ParseScopeAttribute();
            ParseLazyAttribute();
            ParseQualifierAttribute();
        }

        private void ParseName()
        {
            var attr = Attribute.GetCustomAttribute(ObjectType, typeof (ComponentAttribute), true) as ComponentAttribute;
            if (attr != null && !string.IsNullOrEmpty(attr.Name))
                _componentName = attr.Name;
        }

        private void ApplyDefaults(DocumentDefaultsDefinition defaults)
        {
            if (defaults == null)
                return;

            bool lazyInit = false;
            bool.TryParse(defaults.LazyInit, out lazyInit);
            IsLazyInit = lazyInit;
        }

        private void ParseScopeAttribute()
        {
            var attr = Attribute.GetCustomAttribute(ObjectType, typeof(ScopeAttribute), true) as ScopeAttribute;
            if (attr != null)
                Scope = attr.ObjectScope.ToString().ToLower();
        }

        private void ParseLazyAttribute()
        {
            var attr = Attribute.GetCustomAttribute(ObjectType, typeof(LazyAttribute), true) as LazyAttribute;
            if (attr != null)
                IsLazyInit = attr.LazyInitialize;
        }

        private void ParseQualifierAttribute()
        {
            var attr = Attribute.GetCustomAttribute(ObjectType, typeof(QualifierAttribute), true) as QualifierAttribute;
            if (attr != null)
            {
                var qualifier = new AutowireCandidateQualifier(attr.GetType());

                if (!string.IsNullOrEmpty(attr.Value))
                    qualifier.SetAttribute(AutowireCandidateQualifier.VALUE_KEY, attr.Value);

                ParseQualifierProperties(attr, qualifier);

                AddQualifier(qualifier);
            }
        }

        private void ParseQualifierProperties(QualifierAttribute attr, AutowireCandidateQualifier qualifier)
        {
            foreach (var property in attr.GetType().GetProperties())
            {
                if (!property.Name.Equals("TypeId") && !property.Name.Equals("Value"))
                {
                    object value = property.GetValue(attr, null);
                    if (value != null)
                    {
                        var attribute = new ObjectMetadataAttribute(property.Name, value);
                        qualifier.AddMetadataAttribute(attribute);
                    }
                }
            }
        }

        /// <summary>
        /// Provides the name of the object scanned
        /// </summary>
        /// <returns>return the provided attribute name of the full object type name</returns>
        public string ComponentName
        {
            get
            {
                return _componentName;
            }
        }
    }
}
