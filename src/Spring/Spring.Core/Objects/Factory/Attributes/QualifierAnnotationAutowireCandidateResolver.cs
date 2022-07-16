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

using System.Runtime.Serialization;

using Spring.Core;
using Spring.Core.TypeConversion;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Util;

namespace Spring.Objects.Factory.Attributes
{
    /// <summary>
    /// <see cref="IAutowireCandidateResolver"/> implementation that matches bean definition qualifier
    /// against <see cref="QualifierAttribute"/> on the field or parameter to be autowired.
    /// Also supports suggested expression values through a <see cref="ValueAttribute"/> attribute.
    /// </summary>
    [Serializable]
    public class QualifierAnnotationAutowireCandidateResolver : IAutowireCandidateResolver, IObjectFactoryAware, ISerializable
    {
        private IObjectFactory _objectFactory;
        private readonly HashSet<Type> _qualifierTypes = new HashSet<Type>();

        private Type _valueAttributeType = typeof(ValueAttribute);

        public IObjectFactory ObjectFactory
        {
            set { _objectFactory = value; }
        }

        public Type ValueAttributeType
        {
            set { _valueAttributeType = value; }
        }

        /// <summary>
        /// Create a new QualifierAnnotationAutowireCandidateResolver
        /// for Spring's standard <see cref="QualifierAttribute"/> attribute.
        /// </summary>
        public QualifierAnnotationAutowireCandidateResolver()
        {
            _qualifierTypes.Add(typeof(QualifierAttribute));
        }

        /// <summary>
        /// Create a new QualifierAnnotationAutowireCandidateResolver
        /// for the given qualifier attribute type.
        /// </summary>
        /// <param name="qualifierType">the qualifier attribute to look for</param>
        public QualifierAnnotationAutowireCandidateResolver(Type qualifierType)
        {
            AssertUtils.ArgumentNotNull(qualifierType, "'qualifierType' must not be null");
            _qualifierTypes.Add(qualifierType);
        }

        /// <summary>
        /// Create a new QualifierAnnotationAutowireCandidateResolver
        /// for the given qualifier attribute types.
        /// </summary>
        /// <param name="qualifierTypes">the qualifier annotations to look for</param>
        public QualifierAnnotationAutowireCandidateResolver(Collections.Generic.ISet<Type> qualifierTypes)
        {
            AssertUtils.ArgumentNotNull(qualifierTypes, "'qualifierTypes' must not be null");
            foreach (var type in qualifierTypes)
            {
                if (!_qualifierTypes.Contains(type))
                    _qualifierTypes.Add(type);
            }
        }

        protected QualifierAnnotationAutowireCandidateResolver(SerializationInfo info, StreamingContext context)
        {
            _objectFactory = (IObjectFactory) info.GetValue("objectFactory", typeof(IObjectFactory));
            var type = info.GetString("valueAttributeType");
            _valueAttributeType = type != null ? Type.GetType(type) : null;

            _qualifierTypes = new HashSet<Type>();
            var typeNames = (List<string>) info.GetValue("qualifierTypeNames", typeof(List<string>));
            foreach (var typeName in typeNames)
            {
                _qualifierTypes.Add(Type.GetType(typeName));
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("objectFactory", _objectFactory);
            info.AddValue("valueAttributeType", _valueAttributeType?.AssemblyQualifiedName);

            var qualifierTypeNames = new List<string>();
            foreach (var qualifierType in _qualifierTypes)
            {
                qualifierTypeNames.Add(qualifierType?.AssemblyQualifiedName);
            }
            info.AddValue("qualifierTypeNames", qualifierTypeNames);
        }

        /// <summary>
        /// Register the given type to be used as a qualifier when autowiring.
        /// <p>This identifies qualifier annotations for direct use (on fields,
        /// method parameters and constructor parameters) as well as meta
        /// annotations that in turn identify actual qualifier annotations.</p>
        /// <p>This implementation only supports annotations as qualifier types.
        /// The default is Spring's <see cref="QualifierAttribute"/> attribute which serves
        /// as a qualifier for direct use and also as a meta attribute.</p>
        /// </summary>
        /// <param name="qualifierType">the attribute type to register</param>
        public void AddQualifierType(Type qualifierType)
        {
            _qualifierTypes.Add(qualifierType);
        }

        /// <summary>
        /// Determine whether the provided object definition is an autowire candidate.
        /// <p>To be considered a candidate the object's <em>autowire-candidate</em>
        /// attribute must not have been set to 'false'. Also, if an attribute on
        /// the field or parameter to be autowired is recognized by this bean factory
        /// as a <em>qualifier</em>, the object must 'match' against the attribute as
        /// well as any attributes it may contain. The bean definition must contain
        /// the same qualifier or match by meta attributes. A "value" attribute will
        /// fallback to match against the bean name or an alias if a qualifier or
        /// attribute does not match.</p>
        /// </summary>
        public bool IsAutowireCandidate(ObjectDefinitionHolder odHolder, DependencyDescriptor descriptor)
        {
            if (!odHolder.ObjectDefinition.IsAutowireCandidate)
            {
                // if explicitly false, do not proceed with qualifier check
                return false;
            }

            if (descriptor == null)
            {
                // no qualification necessaryodHolder
                return true;
            }

            bool match = CheckQualifiers(odHolder, descriptor.Attributes);
            if (match)
            {
                MethodParameter methodParam = descriptor.MethodParameter;
                if (methodParam != null)
                {
                    var method = methodParam.MethodInfo;
                    if (method == null || method.ReturnType == typeof(void))
                    {
                        match = CheckQualifiers(odHolder, methodParam.MethodAttributes);
                    }
                }
            }

            return match;
        }

        /// <summary>
        /// Match the given qualifier annotations against the candidate bean definition.
        /// </summary>
        protected bool CheckQualifiers(ObjectDefinitionHolder odHolder, Attribute[] annotationsToSearch)
        {
            if (annotationsToSearch == null || annotationsToSearch.Length == 0)
            {
                return true;
            }

            foreach (var attribute in annotationsToSearch)
            {
                if (IsQualifier(attribute.GetType()))
                {
                    if (!CheckQualifier(odHolder, attribute))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks whether the given attribute type is a recognized qualifier type.
        /// </summary>
        protected bool IsQualifier(Type attributeType)
        {
            foreach (Type qualifierType in _qualifierTypes)
            {
                if (IsSubTypeOf(attributeType, qualifierType))
                    return true;
            }

            return false;
        }

        private bool IsSubTypeOf(Type actual, Type requested)
        {
            do
            {
                if (actual == requested)
                    return true;

                actual = actual.BaseType;
            } while (actual != typeof(Object));

            return false;
        }

        /// <summary>
        /// Match the given qualifier attribute against the candidate bean definition.
        /// </summary>
        protected bool CheckQualifier(ObjectDefinitionHolder odHolder, Attribute attribute)
        {
            Type type = attribute.GetType();
            RootObjectDefinition od = (RootObjectDefinition) odHolder.ObjectDefinition;
            AutowireCandidateQualifier qualifier = od.GetQualifier(type.FullName);
            if (qualifier == null)
            {
                qualifier = od.GetQualifier(type.Name);
            }

            if (qualifier == null)
            {
                Attribute targetAttribute = null;
                // TODO: Get the resolved factory method
                //if (od.GetResolvedFactoryMethod() != null) {
                //    targetAttribute = Attribute.GetCustomAttribute(od.GetResolvedFactoryMethod(), type);
                //}
                if (targetAttribute == null)
                {
                    // look for matching attribute on the target class
                    if (_objectFactory != null)
                    {
                        Type objectType = od.ObjectType;
                        if (objectType != null)
                        {
                            targetAttribute = Attribute.GetCustomAttribute(objectType, type);
                        }
                    }

                    if (targetAttribute == null && od.ObjectType != null)
                    {
                        targetAttribute = Attribute.GetCustomAttribute(od.ObjectType, type);
                    }
                }

                if (targetAttribute != null && targetAttribute.Equals(attribute))
                {
                    return true;
                }
            }

            IDictionary<string, object> attributes = AttributeUtils.GetAttributeProperties(attribute);
            if (attributes.Count == 0 && qualifier == null)
            {
                // if no attributes, the qualifier must be present
                return false;
            }

            foreach (var entry in attributes)
            {
                string propertyName = entry.Key;
                object expectedValue = entry.Value;
                object actualValue = null;
                // check qualifier first
                if (qualifier != null)
                {
                    actualValue = qualifier.GetAttribute(propertyName);
                }

                if (actualValue == null)
                {
                    // fall back on bean definition attribute
                    actualValue = od.GetAttribute(propertyName);
                }

                if (actualValue == null && propertyName.Equals(AutowireCandidateQualifier.VALUE_KEY) &&
                    expectedValue is string && odHolder.MatchesName((string) expectedValue))
                {
                    // fall back on bean name (or alias) match
                    continue;
                }

                if (actualValue == null && qualifier != null)
                {
                    // fall back on default, but only if the qualifier is present
                    actualValue = AttributeUtils.GetDefaultValue(attribute, propertyName);
                }

                if (actualValue != null)
                {
                    actualValue = TypeConversionUtils.ConvertValueIfNecessary(expectedValue.GetType(), actualValue, null);
                }

                if (!expectedValue.Equals(actualValue))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determine whether the given dependency carries a value attribute.
        /// </summary>
        public Object GetSuggestedValue(DependencyDescriptor descriptor)
        {
            Object value = FindValue(descriptor.Attributes);
            if (value == null)
            {
                MethodParameter methodParam = descriptor.MethodParameter;
                if (methodParam != null)
                {
                    value = FindValue(methodParam.MethodAttributes);
                }
            }

            return value;
        }

        /// <summary>
        /// Determine a suggested value from any of the given candidate annotations.
        /// </summary>
        protected Object FindValue(Attribute[] annotationsToSearch)
        {
            foreach (var attribute in annotationsToSearch)
            {
                if (_valueAttributeType == attribute.GetType())
                {
                    Object value = ((ValueAttribute) attribute).Expression;
                    if (value == null)
                    {
                        throw new InvalidOperationException("Value attribute must have a value attribute");
                    }

                    return value;
                }
            }

            return null;
        }
    }
}
