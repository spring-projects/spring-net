

using System;
using System.Messaging;
using System.Xml;

namespace Spring.Messaging.Support.Converters
{
    public class XmlDocumentConverter : IMessageConverter
    {
        #region IMessageConverter Members

        public Message ToMessage(object obj)
        {
            XmlDocument doc = obj as XmlDocument;
            if (doc != null)
            {
                Message m = new Message();
                doc.Save(m.BodyStream);
                return m;
            }
            else
            {
                throw new MessagingException("Expected object to be of type System.Xml.XmlDocument");
            }
        }

        public object FromMessage(Message message)
        {
            XmlDocument doc = new XmlDocument(); 
            doc.Load(message.BodyStream);
            return doc;
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}