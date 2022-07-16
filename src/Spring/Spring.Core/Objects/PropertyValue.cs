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

using System.Globalization;
using Spring.Core;
using Spring.Expressions;
using Spring.Expressions.Parser.antlr;
using Spring.Util;

namespace Spring.Objects
{
    /// <summary>
    /// Holds information and value for an individual property.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Using an object here, rather than just storing all properties in a
    /// map keyed by property name, allows for more flexibility, and the
    /// ability to handle indexed properties in a special way if necessary.
    /// </p>
    /// <p>
    /// Note that the value doesn't need to be the final required
    /// <see cref="System.Type"/>: an
    /// <see cref="Spring.Objects.IObjectWrapper"/> implementation must
    /// handle any necessary conversion, as this object doesn't know anything
    /// about the objects it will be applied to.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Mark Pollack (.NET)</author>
    [Serializable]
    public class PropertyValue
    {
        private readonly string propertyName;
        private readonly object propertyValue;
        private IExpression propertyExpression;

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.PropertyValue"/> class.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="val">
        /// The value of the property (possibly before type conversion).
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="name"/> is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </exception>
        public PropertyValue(string name, object val)
        {
            AssertUtils.ArgumentHasText(name, "name");

            propertyName = name;
            propertyValue = val;
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.PropertyValue"/> class.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="val">
        /// The value of the property (possibly before type conversion).
        /// </param>
        /// <param name="expression">Pre-parsed property name.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="name"/> or <paramref name="name"/>
        /// is <see langword="null"/>, or if the name contains only whitespace characters.
        /// </exception>
        public PropertyValue(string name, object val, IExpression expression)
        {
            AssertUtils.ArgumentHasText(name, "name");

            propertyName = name;
            propertyExpression = expression;
            propertyValue = val;
        }

        /// <summary>The name of the property.</summary>
        /// <value>The name of the property.</value>
        public string Name
        {
            get { return propertyName; }
        }

        /// <summary>
        /// Parsed property expression.
        /// </summary>
        public IExpression Expression
        {
            get
            {
                if (propertyExpression == null)
                {
                    try
                    {
                        propertyExpression = ObjectWrapper.GetPropertyExpression(propertyName);
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
                return propertyExpression;
            }
        }

        /// <summary>
        /// Return the value of the property.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Note that type conversion will <i>not</i> have occurred here.
        /// It is the responsibility of the
        /// <see cref="Spring.Objects.IObjectWrapper"/> implementation to
        /// perform type conversion.
        /// </p>
        /// </remarks>
        /// <value>The (possibly unresolved) value of the property.</value>
        public object Value
        {
            get { return propertyValue; }
        }

        /// <summary>
        /// Print a string representation of the property.
        /// </summary>
        /// <returns>A string representation of the property.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "PropertyValue: name='{0}'; value=[{1}].", propertyName, propertyValue);
        }

        /// <summary>
        /// Determines whether the supplied <see cref="System.Object"/>
        /// is equal to the current <see cref="Spring.Objects.PropertyValue"/>.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns>
        /// <see langword="true"/> if they are equal in content.
        /// </returns>
        public override bool Equals(object other)
        {
            if (this == other)
            {
                return true;
            }
            if (!(other is PropertyValue))
            {
                return false;
            }
            PropertyValue otherPv = (PropertyValue) other;
            return
                    (propertyName.Equals(otherPv.propertyName)
                     && ((propertyValue == null && otherPv.propertyValue == null) || propertyValue.Equals(otherPv.propertyValue)));
        }

        /// <summary>
        /// Serves as a hash function for a particular type, suitable for use
        /// in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="Spring.Objects.PropertyValue"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return propertyName.GetHashCode() * 29 + (propertyValue != null ? propertyValue.GetHashCode() : 0);
        }
    }
}
