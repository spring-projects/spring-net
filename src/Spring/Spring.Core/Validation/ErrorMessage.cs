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

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Spring.Context;

namespace Spring.Validation
{
    /// <summary>
    /// Represents a single validation error message.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <author>Goran Milosavljevic</author>
    [Serializable]
    public class ErrorMessage : IXmlSerializable
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ErrorMessage()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage"/> class.
        /// </summary>
        /// <param name="id">Error message resource identifier.</param>
        /// <param name="parameters">Parameters that should be used for message resolution.</param>
        public ErrorMessage(string id, params object[] parameters)
        {
            this.id = id;
            this.parameters = parameters;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage"/> class copying values from another instance.
        /// </summary>
        /// <param name="other">Another Error message instance to copy values from.</param>
        protected ErrorMessage(ErrorMessage other)
        {
            this.id = other.id;
            this.parameters = other.parameters;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the resource identifier for this message.
        /// </summary>
        /// <value>The resource identifier for this message.</value>
        public string Id
        {
            get { return id; }
        }

        /// <summary>
        /// Gets or sets the message parameters.
        /// </summary>
        /// <value>The message parameters.</value>
        public object[] Parameters
        {
            get { return parameters; }
        }

        #endregion

        #region IXmlSerializable implementations

        /// <summary>
        /// This property is reserved, apply the
        /// <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute" />
        /// to the class instead.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Xml.Schema.XmlSchema" />
        /// that describes the XML representation of the object that
        /// is produced by the
        /// <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" />
        /// method and consumed by the
        /// <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" />
        /// method.
        /// </returns>
        ///
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
        public virtual void ReadXml(XmlReader reader)
        {
            id = reader.GetAttribute("Id");
            if (!reader.IsEmptyElement)
            {
                reader.Read();
                reader.Read();
                XmlSerializer xs = new XmlSerializer(typeof(object[]));
                parameters = (object[])xs.Deserialize(reader);
                reader.Read();
            }
            reader.Read();
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="T:System.Xml.XmlWriter"></see> stream
        /// to which the object is serialized.
        /// </param>
        public virtual void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Id", id);

            if (parameters != null)
            {
                writer.WriteStartElement("Parameters");

                XmlSerializer xs = new XmlSerializer(parameters.GetType());
                xs.Serialize(writer, parameters);

                writer.WriteEndElement();
            }
        }

        #endregion

        #region ErrorMessage methods

        /// <summary>
        /// Resolves the message against specified <see cref="IMessageSource"/>.
        /// </summary>
        /// <param name="messageSource">Message source to resolve this error message against.</param>
        /// <returns>Resolved error message.</returns>
        public string GetMessage(IMessageSource messageSource)
        {
            if (messageSource == null)
            {
                return Id;
            }

            if (Parameters == null)
            {
                return messageSource.GetMessage(Id);
            }
            else
            {
                return messageSource.GetMessage(Id, Parameters);
            }
        }

        #endregion

        ///<summary>
        ///Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        ///</summary>
        ///<returns>
        ///true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        ///</returns>
        ///<param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            ErrorMessage other = obj as ErrorMessage;
            return (other != null)
                && (this.id == other.Id);
        }

        ///<summary>
        ///Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        ///</summary>
        ///<returns>
        ///A hash code for the current <see cref="T:System.Object"></see>.
        ///</returns>
        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        #region Data members

        private string id;
        private object[] parameters;

        #endregion
    }
}
