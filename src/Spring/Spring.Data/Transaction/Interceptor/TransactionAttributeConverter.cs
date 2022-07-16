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

using System.ComponentModel;
using System.Globalization;

namespace Spring.Transaction.Interceptor
{
    /// <summary>
    /// Type converter for <see cref="Spring.Transaction.Interceptor.ITransactionAttribute"/>
    /// objects.
    /// </summary>
    /// <remarks>
    /// Takes <see cref="System.String"/>s of the form
    /// <p><code>PROPAGATION_NAME,ISOLATION_NAME,readOnly,timeout_NNNN,+Exception1,-Exception2</code></p>
    /// <p>where only propagation code is required. For example:</p>
    /// <p><code>PROPAGATION_MANDATORY,ISOLATION_DEFAULT</code></p>
    /// <p>
    /// The tokens can be in <strong>any</strong> order. Propagation and isolation codes
    /// must use the names of the values in the <see cref="Spring.Transaction.TransactionPropagation"/>
    /// enumeration. Timeout values are in seconds. If no timeout is specified, the transaction
    /// manager will apply a default timeout specific to the particular transaction manager.
    /// </p>
    /// <p>
    /// A "+" before an exception name substring indicates that transactions should commit even
    /// if this exception is thrown; a "-" that they should roll back.
    /// </p>
    /// </remarks>
    /// <author>Mark Pollack</author>
    public class TransactionAttributeConverter : TypeConverter
    {
        /// <summary>
        /// Returns whether this converter can convert an object of the given type to an ITransactionAttribute, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that provides a format context.</param>
        /// <param name="sourceType">A <see cref="T:System.Type"></see> that represents the type you want to convert from.</param>
        /// <returns>
        /// true if this converter can perform the conversion; otherwise, false.
        /// </returns>
        public override bool CanConvertFrom(
            ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof (string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Converts from string to ITransactionAttribute
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="val">The string value to convert</param>
        /// <returns>An ITransactionAttribute instance</returns>
        public override object ConvertFrom(
            ITypeDescriptorContext context, CultureInfo culture, object val)
        {
            if (val is string)
            {
                string value = val as string;
                TransactionAttributeEditor editor = new TransactionAttributeEditor();
                editor.SetAsText(value);
                return editor.Value;
            }
            return base.ConvertFrom(context, culture, val);
        }
    }
}
