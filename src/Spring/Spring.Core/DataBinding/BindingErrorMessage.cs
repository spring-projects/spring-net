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

using Spring.Util;
using Spring.Validation;

namespace Spring.DataBinding
{
    /// <summary>
    /// Represents an ErrorMessage specific to a binding instance.
    /// </summary>
    /// <author>Erich Eichinger</author>
    [Serializable]
    public class BindingErrorMessage : ErrorMessage
    {
        private string _bindingId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage"/> class.
        /// </summary>
        /// <param name="bindingId">the id of the binding this error message is associated with</param>
        /// <param name="id">the message id</param>
        /// <param name="parameters">optional parameters to this message</param>
        public BindingErrorMessage(string bindingId, string id, params object[] parameters) : base(id, parameters)
        {
            AssertUtils.ArgumentNotNull(bindingId, "bindingId");
            _bindingId = bindingId;
        }

        /// <summary>
        /// Get the ID of the binding this message instance relates to.
        /// </summary>
        public string BindingId
        {
            get { return _bindingId; }
        }

        /// <summary>
        /// Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">
        /// The <see cref="T:System.Xml.XmlReader"></see> stream
        /// from which the object is deserialized.
        /// </param>
        public override void ReadXml(System.Xml.XmlReader reader)
        {
            base.ReadXml(reader);
            _bindingId = reader.GetAttribute("bindingId");
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="T:System.Xml.XmlWriter"></see> stream
        /// to which the object is serialized.
        /// </param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteAttributeString("bindingId", _bindingId);
        }

        ///<summary>
        ///Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        ///</summary>
        ///<returns>
        ///true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        ///</returns>
        ///<param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            BindingErrorMessage other = obj as BindingErrorMessage;
            return (other != null)
                && (this.BindingId == other.BindingId)
                && (base.Equals(obj));
        }

        ///<summary>
        ///Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        ///</summary>
        ///<returns>
        ///A hash code for the current <see cref="T:System.Object"></see>.
        ///</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() + 31*this.BindingId.GetHashCode();
        }
    }
}
