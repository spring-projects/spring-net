/*
 * Copyright © 2002-2011 the original author or authors.
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

using System.ComponentModel;
using System.Reflection;
using System.Text;

using Common.Logging;
using Spring.Core;
using Spring.Expressions;
using Spring.Expressions.Parser.antlr;
using Spring.Util;
using StringUtils=Spring.Util.StringUtils;

namespace Spring.Objects
{
    /// <summary>
    /// Default implementation of the <see cref="Spring.Objects.IObjectWrapper"/>
    /// interface that should be sufficient for all normal uses.
    /// </summary>
    /// <remarks>
    /// <p>
    /// <see cref="Spring.Objects.ObjectWrapper"/> will convert
    /// <see cref="System.Collections.IList"/> and array
    /// values to the corresponding target arrays, if necessary. Custom
    /// <see cref="System.ComponentModel.TypeConverter"/>s that deal with
    /// <see cref="System.Collections.IList"/>s or arrays can be written against a
    /// comma delimited <see cref="System.String"/> as <see cref="System.String"/>
    /// arrays are converted in such a format if the array itself is not assignable.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller </author>
    /// <author>Jean-Pierre Pawlak</author>
    /// <author>Mark Pollack (.NET)</author>
    /// <author>Aleksandar Seovic(.NET)</author>
    [Serializable]
    public class ObjectWrapper : IObjectWrapper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ObjectWrapper));

        /// <summary>The wrapped object.</summary>
        private object wrappedObject;

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.Objects.ObjectWrapper"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The wrapped target instance will need to be set afterwards.
        /// </p>
        /// </remarks>
        /// <seealso cref="Spring.Objects.ObjectWrapper.WrappedInstance"/>
        public ObjectWrapper()
        {}

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.Objects.ObjectWrapper"/> class.
        /// </summary>
        /// <param name="instance">
        /// The object wrapped by this <see cref="Spring.Objects.ObjectWrapper"/>.
        /// </param>
        /// <exception cref="Spring.Objects.FatalObjectException">
        /// If the supplied <paramref name="instance"/> is <see lang="null"/>.
        /// </exception>
        public ObjectWrapper(object instance)
        {
            WrappedInstance = instance;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.Objects.ObjectWrapper"/> class,
        /// instantiating a new instance of the specified <see cref="System.Type"/> and using
        /// it as the <see cref="Spring.Objects.ObjectWrapper.WrappedInstance"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Please note that the <see cref="System.Type"/> passed as the
        /// <paramref name="type"/> argument must have a no-argument constructor.
        /// If it does not, an exception will be thrown when this class attempts
        /// to instantiate the supplied <paramref name="type"/> using it's
        /// (non-existent) constructor.
        /// </p>
        /// </remarks>
        /// <param name="type">
        /// The <see cref="System.Type"/> to instantiate and wrap.
        /// </param>
        /// <exception cref="Spring.Objects.FatalObjectException">
        /// If the <paramref name="type"/> is <see langword="null"/>, or if the
        /// invocation of the <paramref name="type"/>s default (no-arg) constructor
        /// fails (due to invalid arguments, insufficient permissions, etc).
        /// </exception>
        public ObjectWrapper(Type type) : this(true)
        {
            try
            {
                WrappedInstance = ObjectUtils.InstantiateType(type);
            } catch (FatalReflectionException e)
            {
                throw new FatalObjectException(e.Message, e);
            }
        }

        /// <summary>
        /// The object wrapped by this <see cref="Spring.Objects.ObjectWrapper"/>.
        /// </summary>
        /// <exception cref="Spring.Objects.FatalObjectException">
        /// If the object cannot be changed; or an attempt is made to set the
        /// value of this property to <see langword="null"/>.
        /// </exception>
        public object WrappedInstance
        {
            get => wrappedObject;
            set
            {
                if (value == null)
                {
                    throw new FatalObjectException("Wraped instance cannot be null.");
                }
                wrappedObject = value;
            }
        }

        /// <summary>
        /// Convenience method to return the <see cref="System.Type"/> of the wrapped object.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Do <b>not</b> use this (convenience) method prior to setting the
        /// <see cref="Spring.Objects.ObjectWrapper.WrappedInstance"/> property.
        /// </p>
        /// </remarks>
        /// <returns>
        /// The <see cref="System.Type"/> of the wrapped object.
        /// </returns>
        /// <exception cref="System.NullReferenceException">
        /// If the <see cref="Spring.Objects.ObjectWrapper.WrappedInstance"/> property
        /// is <see lang="null"/>.
        /// </exception>
        public Type WrappedType => WrappedInstance.GetType();

        /// <summary>Gets the value of a property.</summary>
        /// <param name="propertyName">
        /// The name of the property to get the value of.
        /// </param>
        /// <returns>The value of the property.</returns>
        /// <exception cref="Spring.Objects.FatalObjectException">
        /// If there is no such property, if the property isn't readable, or
        /// if getting the property value throws an exception.
        /// </exception>
        public virtual object GetPropertyValue(string propertyName)
        {
            try
            {
                IExpression propertyExpression = GetPropertyExpression(propertyName);
                return GetPropertyValue(propertyExpression);
            }
            catch (RecognitionException e)
            {
                throw new InvalidPropertyException("Failed to parse property name '" + propertyName + "'.", e);
            }
            catch (TokenStreamRecognitionException e)
            {
                throw new InvalidPropertyException("Failed to parse property name '" + propertyName + "'.", e);
            }
        }

        /// <summary>Gets the value of a property.</summary>
        /// <param name="propertyExpression">
        /// The property expression that should be used to retrieve the property value.
        /// </param>
        /// <returns>The value of the property.</returns>
        /// <exception cref="Spring.Objects.FatalObjectException">
        /// If there is no such property, if the property isn't readable, or
        /// if getting the property value throws an exception.
        /// </exception>
        public virtual object GetPropertyValue(IExpression propertyExpression)
        {
            return propertyExpression.GetValue(wrappedObject);
        }

        /// <summary>
        /// Sets a property value.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method is provided for convenience only. The
        /// <see cref="Spring.Objects.ObjectWrapper.SetPropertyValue(PropertyValue)"/>
        /// method is more powerful.
        /// </p>
        /// </remarks>
        /// <param name="propertyName">
        /// The name of the property to set value of.
        /// </param>
        /// <param name="val">The new value.</param>
        public virtual void SetPropertyValue(string propertyName, object val)
        {
            try
            {
                IExpression propertyExpression = GetPropertyExpression(propertyName);
                SetPropertyValue(propertyExpression, val);
            }
            catch (RecognitionException e)
            {
                throw new InvalidPropertyException("Failed to parse property name '" + propertyName + "'.", e);
            }
            catch (TokenStreamRecognitionException e)
            {
                throw new InvalidPropertyException("Failed to parse property name '" + propertyName + "'.", e);
            }
        }

        /// <summary>
        /// Sets a property value.
        /// </summary>
        /// <param name="propertyExpression">
        /// The property expression that should be used to set the property value.
        /// </param>
        /// <param name="val">The new value.</param>
        public virtual void SetPropertyValue(IExpression propertyExpression, object val)
        {
            propertyExpression.SetValue(wrappedObject, val);
        }

        /// <summary>
        /// Sets a property value.
        /// </summary>
        /// <remarks>
        /// <p>
        /// <b>This is the preferred way to update an individual property.</b>
        /// </p>
        /// </remarks>
        /// <param name="pv">
        /// The object containing new property value.
        /// </param>
        public virtual void SetPropertyValue(PropertyValue pv)
        {
            SetPropertyValue(pv.Expression, pv.Value);
        }

        /// <summary>Set a number of property values in bulk.</summary>
        /// <remarks>
        /// <p>
        /// Does not allow unknown fields. Equivalent to
        /// <see cref="Spring.Objects.ObjectWrapper.SetPropertyValues(IPropertyValues, bool)"/>
        /// with <see langword="null"/> and <cref lang="false"/> for
        /// arguments.
        /// </p>
        /// </remarks>
        /// <param name="pvs">
        /// The <see cref="Spring.Objects.IPropertyValues"/> to set on the target
        /// object.
        /// </param>
        /// <exception cref="Spring.Core.NotWritablePropertyException">
        /// If an error is encountered while setting a property.
        /// </exception>
        /// <exception cref="Spring.Objects.PropertyAccessExceptionsException">
        /// On a <see cref="System.Type"/> mismatch while setting a property, insufficient permissions, etc.
        /// </exception>
        /// <seealso cref="Spring.Objects.IObjectWrapper.SetPropertyValues(IPropertyValues, bool)"/>
        public virtual void SetPropertyValues(IPropertyValues pvs)
        {
            SetPropertyValues(pvs, false);
        }

        /// <summary>
        /// Perform a bulk update with full control over behavior.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method may throw a reflection-based exception, if there is a critical
        /// failure such as no matching field... less serious exceptions will be accumulated
        /// and thrown as a single <see cref="Spring.Objects.PropertyAccessExceptionsException"/>.
        /// </p>
        /// </remarks>
        /// <param name="propertyValues">
        /// The <see cref="Spring.Objects.PropertyValue"/>s to set on the target object.
        /// </param>
        /// <param name="ignoreUnknown">
        /// Should we ignore unknown values (not found in the object!?).
        /// </param>
        /// <exception cref="NotWritablePropertyException">
        /// If an error is encountered while setting a property (only thrown if the
        /// <paramref name="ignoreUnknown"/> parameter is set to <see langword="false"/>).
        /// </exception>
        /// <exception cref="Spring.Objects.PropertyAccessExceptionsException">
        /// On a <see cref="System.Type"/> mismatch while setting a property, insufficient permissions, etc.
        /// </exception>
        /// <seealso cref="Spring.Objects.IObjectWrapper.SetPropertyValues(IPropertyValues, bool)"/>
        public virtual void SetPropertyValues(IPropertyValues propertyValues, bool ignoreUnknown)
        {
            var propertyAccessExceptions = new List<PropertyAccessException>();
            foreach (PropertyValue pv in propertyValues)
            {
                try
                {
                    SetPropertyValue(pv);
                }
                catch (NotWritablePropertyException ex)
                {
                    if (!ignoreUnknown)
                    {
                        Log.Error($"Failed setting property '{pv.Name}'", ex);
                        throw;
                    }
                }
                catch (InvalidPropertyException ex)
                {
                    if (!ignoreUnknown)
                    {
                        Log.Error($"Failed setting property '{pv.Name}'", ex);
                        throw;
                    }
                }
                catch (TypeMismatchException ex) // otherwise, just ignore it and continue...
                {
                    Log.Error($"Failed setting property '{pv.Name}'", ex);
                    propertyAccessExceptions.Add(ex);
                }
                catch (MethodInvocationException ex)
                {
                    Log.Error($"Failed setting property '{pv.Name}'", ex);
                    propertyAccessExceptions.Add(ex);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed setting property '{pv.Name}' on instance of type '{WrappedType.FullName}'", ex);
                    throw;
                }
            }

            // if we encountered individual exceptions, throw the composite exception...
            if (propertyAccessExceptions.Count > 0)
            {
                throw new PropertyAccessExceptionsException(this, propertyAccessExceptions.ToArray());
            }
        }

        /// <summary>
        /// Returns PropertyInfo for the specified property
        /// </summary>
        /// <param name="propertyName">The name of the property to search for.</param>
        /// <returns>The <see cref="PropertyInfo"/> for the specified property.</returns>
        /// <exception cref="FatalObjectException">If <see cref="PropertyInfo"/> cannot be determined.</exception>
        public PropertyInfo GetPropertyInfo(string propertyName)
        {
            return (PropertyInfo) GetPropertyOrFieldInfo(propertyName);
        }

        /// <summary>
        /// Get the <see cref="System.Type"/> for a particular property.
        /// </summary>
        /// <param name="propertyName">
        /// The property the <see cref="System.Type"/> of which is to be retrieved.
        /// </param>
        /// <returns>
        /// The <see cref="System.Type"/> for a particular property..
        /// </returns>
        public Type GetPropertyType(string propertyName)
        {
            var memberInfo = GetPropertyOrFieldInfo(propertyName);
            switch(memberInfo.MemberType)
            {
                case MemberTypes.Property:
                    return ((PropertyInfo) memberInfo).PropertyType;
                case MemberTypes.Field:
                    return ((FieldInfo) memberInfo).FieldType;
                default:
                    throw new FatalObjectException($"'{propertyName}' is not a valid property expression.");
            }
        }

        /// <summary>
        /// Returns MemberInfo for the specified property or field
        /// </summary>
        /// <param name="propertyOrFieldName">The name of the property or field to search for.</param>
        /// <returns>The <see cref="PropertyInfo"/> or <see cref="FieldInfo"/> for the specified property or field.</returns>
        /// <exception cref="FatalObjectException">If <paramref name="propertyOrFieldName"/> does not resolve to a property or field.</exception>
        private MemberInfo GetPropertyOrFieldInfo(string propertyOrFieldName)
        {
            if(StringUtils.IsNullOrEmpty(propertyOrFieldName))
            {
                throw new FatalObjectException("Can't find property or field info for null or zero length property name.");
            }

            try
            {
                IExpression propertyExpression = GetPropertyExpression(propertyOrFieldName);
                if(propertyExpression is PropertyOrFieldNode propertyOrFieldNode)
                {
                    return propertyOrFieldNode.GetMemberInfo(wrappedObject);
                }

                if(propertyExpression is IndexerNode indexerNode)
                {
                    return indexerNode.GetPropertyInfo(wrappedObject, null);
                }

                if(propertyExpression is Expression expression)
                {
                    return expression.GetPropertyInfo(wrappedObject, null);
                }

                throw new FatalObjectException($"'{propertyOrFieldName}' is not a valid property or field expression.");
            }
            catch(RecognitionException e)
            {
                throw new FatalObjectException($"Failed to parse property or field name '{propertyOrFieldName}'.", e);
            }
            catch(TokenStreamRecognitionException e)
            {
                throw new FatalObjectException($"Failed to parse property or field name '{propertyOrFieldName}'.", e);
            }
        }


        /// <summary>
        /// Get the properties of the wrapped object.
        /// </summary>
        /// <returns>
        /// An array of <see cref="System.Reflection.PropertyInfo"/>s.
        /// </returns>
        public PropertyInfo[] GetPropertyInfos()
        {
            return WrappedType.GetProperties();
        }

        /// <summary>
        /// Return the collection of property descriptors.
        /// </summary>
        public PropertyDescriptorCollection PropertyDescriptors => TypeDescriptor.GetProperties(WrappedInstance);

        /// <summary>
        /// This method is expensive! Only call for diagnostics and debugging reasons,
        /// not in production.
        /// </summary>
        /// <returns>
        /// A string describing the state of this object.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                sb.Append("ObjectWrapper: wrapping class [");
                sb.Append(WrappedType.FullName);
                sb.Append("]; ");
                foreach (PropertyDescriptor p in PropertyDescriptors)
                {
                    object val = GetPropertyValue(p.Name);
                    string valStr = (val != null) ? val.ToString() : "null";
                    sb.Append(p.Name).Append("={").Append(valStr).Append("}");
                }
            }
            catch (Exception ex)
            {
                sb.Append("Exception encountered: ").Append(ex);
            }
            return sb.ToString();
        }

        private static readonly char[] propertyCharCheckArray = { '.', '[', '(', ' ', '{' };

        /// <summary>
        /// Attempts to parse property expression first and falls back to full expression
        /// if that fails. Performance optimization.
        /// </summary>
        /// <param name="propertyName">Property expression to parse.</param>
        /// <returns>Parsed proeprty expression.</returns>
        internal static IExpression GetPropertyExpression(string propertyName)
        {
            IExpression propertyExpression;
            if (propertyName.IndexOfAny(propertyCharCheckArray) < 0)
            {
                try
                {
                    propertyExpression = Expression.ParseProperty(propertyName);
                }
                catch (Exception)
                {
                    propertyExpression = Expression.ParsePrimary(propertyName);
                }
            }
            else
            {
                propertyExpression = Expression.ParsePrimary(propertyName);
            }

            return propertyExpression;
        }
    }
}
