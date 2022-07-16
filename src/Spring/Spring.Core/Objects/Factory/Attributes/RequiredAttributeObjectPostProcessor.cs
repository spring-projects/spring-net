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

using System.Reflection;
using System.Text;

using Spring.Collections;
using Spring.Objects.Factory.Config;
using Spring.Util;

namespace Spring.Objects.Factory.Attributes
{
    /// <summary>
    ///  A <see cref="IObjectPostProcessor"/> implementation that enforces required properties to have been configured.
    /// Required properties are detected through an attribute, by default, Spring's <see cref="RequiredAttribute"/>
    /// attribute.
    /// </summary>
    /// <remarks>
    /// <para>The motivation for the existence of this IObjectPostProcessor is to allow
    /// developers to annotate the setter properties of their own classes with an
    /// arbitrary attribute to indicate that the container must check
    /// for the configuration of a dependency injected value. This neatly pushes
    /// responsibility for such checking onto the container (where it arguably belongs),
    /// and obviates the need (<b>in part</b>) for a developer to code a method that
    /// simply checks that all required properties have actually been set.
    /// </para>
    /// <para>Please note that an 'init' method may still need to implemented (and may
    /// still be desirable), because all that this class does is enforce that a
    /// 'required' property has actually been configured with a value. It does
    /// <b>not</b> check anything else... In particular, it does not check that a
    /// configured value is not <code>null</code>.
    /// </para>
    /// </remarks>
    /// <author>Rob Harrop</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class RequiredAttributeObjectPostProcessor : InstantiationAwareObjectPostProcessorAdapter
    {
        private Type requiredAttributeType = typeof (RequiredAttribute);

        /// <summary>
        /// Cache for validated object names, skipping re-validation for the same object
        /// </summary>
        private ISet validatedObjectNames = new SynchronizedSet(new HashedSet());

        /// <summary>
        /// Sets the type of the required attribute, to be used on a property setter
        /// </summary>
        /// <remarks>
        /// The default required attribute type is the Spring-provided <see cref="RequiredAttribute"/> attribute.
        /// This setter property exists so that developers can provide their own
        /// (non-Spring-specific) annotation type to indicate that a property value is required.
        /// </remarks>
        /// <value>The type of the required attribute.</value>
        public Type RequiredAttributeType
        {
            set
            {
                AssertUtils.ArgumentNotNull(value, "RequiredAttributeType", "RequiredAttributeType property must not be null");
                if (!typeof(Attribute).IsAssignableFrom(value))
                {
                    throw new ArgumentException(
                        string.Format("[{0}] does not inherit from System.Attribute (it must).", value.FullName));
                }
                requiredAttributeType = value;
            }
            get
            {
                return requiredAttributeType;
            }
        }


        /// <summary>
        /// Post-process the given property values before the factory applies them
        /// to the given object.  Checks for the attribute specified by this PostProcessor's RequiredAttributeType.
        /// </summary>
        /// <param name="pvs">The property values that the factory is about to apply (never <code>null</code>).</param>
        /// <param name="pis">The relevant property infos for the target object (with ignored
        /// dependency types - which the factory handles specifically - already filtered out)</param>
        /// <param name="objectInstance">The object instance created, but whose properties have not yet
        /// been set.</param>
        /// <param name="objectName">Name of the object.</param>
        /// <returns>
        /// The actual property values to apply to the given object (can be the
        /// passed-in PropertyValues instances or null to skip property population.
        /// </returns>
        /// <exception cref="ObjectInitializationException">If a required property value has not been specified
        /// in the configuration metadata.</exception>
        public override IPropertyValues PostProcessPropertyValues(IPropertyValues pvs, IList<PropertyInfo> pis, object objectInstance, string objectName)
        {
            if (!validatedObjectNames.Contains(objectName))
            {
                List<string> invalidProperties = new List<string>();

                foreach (PropertyInfo pi in pis)
                {
                    if (IsRequiredProperty(pi) && !pvs.Contains(pi.Name))
                    {
                        invalidProperties.Add(pi.Name);
                    }
                }
                if (invalidProperties.Count != 0)
                {
                    throw new ObjectInitializationException(
                        BuildExceptionMessage(invalidProperties, objectName));
                }
                validatedObjectNames.Add(objectName);
            }
            return pvs;
        }

        /// <summary>
        /// Determines whether the supplied property is required to have a value, that is to be dependency injected.
        /// </summary>
        /// <remarks>
        /// This implementation looks for the existence of a "required" attribute on the supplied PropertyInfo and that
        /// the property has a setter method.
        /// </remarks>
        /// <param name="pi">The target PropertyInfo</param>
        /// <returns>
        /// 	<c>true</c> if the supplied property has been marked as being required;; otherwise, <c>false</c> if
        /// not or if the supplied property does not have a setter method
        /// </returns>
        protected virtual bool IsRequiredProperty(PropertyInfo pi)
        {
            return (pi.GetSetMethod() != null && pi.GetCustomAttributes(RequiredAttributeType, true).Length > 0);
        }

        /// <summary>
        /// Builds an exception message for the given list of invalid properties.
        /// </summary>
        /// <param name="invalidProperties">The list of names of invalid properties.</param>
        /// <param name="objectName">Name of the object.</param>
        /// <returns>The exception message</returns>
        private string BuildExceptionMessage(IList<string> invalidProperties, ICloneable objectName)
        {
            int size = invalidProperties.Count;
            StringBuilder sb = new StringBuilder();
            sb.Append(size == 1 ? "Property" : "Properties");
            for (int i=0; i < size; i++)
            {
                string propName = invalidProperties[i];
                if (i > 0)
                {
                    if (i == (size -1 ))
                    {
                        sb.Append(" and");
                    }
                    else
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(" '").Append(propName).Append("'");
            }
            sb.Append(" required for object '").Append(objectName).Append("'");
            return sb.ToString();

        }
    }
}
