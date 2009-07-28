#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Spring.Context;
using Spring.Util;

#endregion

namespace Spring.Validation
{
    /// <summary>
    /// A container for validation errors.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This class groups validation errors by validator names and allows
    /// access to both the complete errors collection and to the errors for a
    /// certain validator.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    /// <author>Goran Milosavljevic</author>
    [Serializable]
    public class ValidationErrors : IValidationErrors, IXmlSerializable
    {
        #region Constructors
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ValidationErrors()
        {}

        #endregion

        #region IXmlSerializable implementations

        /// <summary>
        /// This property is reserved, apply the 
        /// <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute" />
        /// to the class instead. 
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Xml.Schema.XmlSchema" /> that describes the 
        /// XML representation of the object that is produced by 
        /// the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" /> 
        /// method and consumed by the 
        /// <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" /> 
        /// method.
        /// </returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">
        /// The <see cref="T:System.Xml.XmlReader"></see> stream 
        /// from which the object is deserialized. 
        /// </param>
        public void ReadXml(XmlReader reader)
        {
            if (!reader.IsEmptyElement)
            {
                reader.Read();
                while (reader.Name == "Provider")
                {
                    object key = reader.GetAttribute("Id");
                    reader.Read();
                    while (reader.Name == "ErrorMessage")
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(ErrorMessage));
                        object value = xs.Deserialize(reader);

                        IList mapValue = (IList)errorMap[key];

                        if (mapValue == null)
                        {
                            mapValue = new ArrayList();
                            errorMap[key] = mapValue;
                        }

                        mapValue.Add(value);
                    }
                    reader.Read();
                }
            }
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="T:System.Xml.XmlWriter"></see> stream 
        /// to which the object is serialized. 
        /// </param>
        public void WriteXml(XmlWriter writer)
        {
            foreach (DictionaryEntry entry in errorMap)
            {
                writer.WriteStartElement("Provider");
                writer.WriteAttributeString("Id", entry.Key as String);

                if (entry.Value != null)
                {
                    ArrayList errorsList = (ArrayList)entry.Value;
                    foreach (object o in errorsList)
                    {
                        ErrorMessage error = (ErrorMessage)o;
                        XmlSerializer xs = new XmlSerializer(typeof(ErrorMessage));
                        xs.Serialize(writer, error);
                    }
                }

                writer.WriteEndElement();
            }
        }

        #endregion 

        #region ValidationErrors methods

        /// <summary>
        /// Does this instance contain any validation errors?
        /// </summary>
        /// <remarks>
        /// <p>
        /// If this returns <see lang="true"/>, this means that it (obviously)
        /// contains no validation errors.
        /// </p>
        /// </remarks>
        /// <value><see lang="true"/> if this instance is empty.</value>
        public bool IsEmpty
        {
            get { return this.errorMap.Count == 0; }
        }
        
        /// <summary>
        /// Gets the list of all providers.
        /// </summary>
        public IList Providers
        {
            get { return new ArrayList(this.errorMap.Keys); }
        }

        /// <summary>
        /// Adds the supplied <paramref name="message"/> to this
        /// instance's collection of errors.
        /// </summary>
        /// <param name="provider">
        /// The provider that should be used for message grouping; can't be
        /// <see lang="null"/>.
        /// </param>
        /// <param name="message">The error message to add.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="provider"/> or <paramref name="message"/> is <see langword="null"/>.
        /// </exception>
        public void AddError(string provider, ErrorMessage message)
        {
            AssertUtils.ArgumentNotNull(provider, "provider");
            AssertUtils.ArgumentNotNull(message, "errorMessage");

            IList errors = (IList) errorMap[provider];
            if (errors == null)
            {
                errors = new ArrayList();
                errorMap[provider] = errors;
            }
            errors.Add(message);
        }

        /// <summary>
        /// Merges another instance of <see cref="ValidationErrors"/> into this one.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the supplied <paramref name="errorsToMerge"/> is <see lang="null"/>,
        /// then no errors will be added to this instance, and this method will
        /// (silently) return.
        /// </p>
        /// </remarks>
        /// <param name="errorsToMerge">
        /// The validation errors to merge; can be <see lang="null"/>.
        /// </param>
        public void MergeErrors(IValidationErrors errorsToMerge)
        {
            if (errorsToMerge != null)
            {
                foreach(string provider in errorsToMerge.Providers)
                {
                    ArrayList errList = (ArrayList) this.errorMap[provider];
                    IList other = errorsToMerge.GetErrors(provider);
                    if (errList == null)
                    {
                        this.errorMap[provider] = other;
                    }
                    else
                    {
                        errList.AddRange((IList) other);
                    }
                }
//                foreach (DictionaryEntry errorEntry in errorsToMerge.errorMap)
//                {
//                    ArrayList errList = (ArrayList) this.errorMap[errorEntry.Key];
//                    if (errList == null)
//                    {
//                        this.errorMap[errorEntry.Key] = errorEntry.Value;
//                    }
//                    else
//                    {
//                        errList.AddRange((IList) errorEntry.Value);
//                    }
//                }
            }
        }

        /// <summary>
        /// Gets the list of errors for the supplied lookup <paramref name="provider"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If there are no errors for the supplied lookup <paramref name="provider"/>,
        /// an <b>empty</b> <see cref="System.Collections.IList"/> will be returned.
        /// </p>
        /// </remarks>
        /// <param name="provider">Error key that was used to group messages.</param>
        /// <returns>
        /// A list of all <see cref="ErrorMessage"/>s for the supplied lookup <paramref name="provider"/>.
        /// </returns>
        public IList GetErrors(string provider)
        {
            IList errors = (IList) errorMap[provider];
            return errors == null ? ObjectUtils.EmptyObjects : errors;
        }

        /// <summary>
        /// Gets the list of resolved error messages for the supplied lookup <paramref name="provider"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If there are no errors for the supplied lookup <paramref name="provider"/>,
        /// an <b>empty</b> <see cref="System.Collections.IList"/> will be returned.
        /// </p>
        /// </remarks>
        /// <param name="provider">Error key that was used to group messages.</param>
        /// <param name="messageSource"><see cref="IMessageSource"/> to resolve messages against.</param>
        /// <returns>
        /// A list of resolved error messages for the supplied lookup <paramref name="provider"/>.
        /// </returns>
        public IList GetResolvedErrors(string provider, IMessageSource messageSource)
        {
            AssertUtils.ArgumentNotNull(provider, "provider");

            IList messages = new ArrayList();
            IList errors = (IList) errorMap[provider];
            
            if (errors != null)
            {
                foreach (ErrorMessage error in errors)
                {
                    messages.Add(error.GetMessage(messageSource));
                }
            }

            return messages;
        }

        #endregion
        
        #region Data members

        private readonly IDictionary errorMap = new Hashtable();

        #endregion
    }
}