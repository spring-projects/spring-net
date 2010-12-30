using System;
using System.IO;
using System.Collections.Generic;

using Spring.Http;
using Spring.Http.Converters;
using Newtonsoft.Json;

namespace Spring.HttpMessageConverterQuickStart.Converters
{
    /// <summary>
    /// Implementation of <see cref="IHttpMessageConverter"/> that can read and write JSON 
    /// using the Json.NET (Newtonsoft.Json) library.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This implementation supports getting/setting values from JSON directly, 
    /// without the need to deserialize/serialize to a .NET class.
    /// </para>
    /// <para>
    /// By default, this converter supports 'application/json' media type. 
    /// This can be overridden by setting the <see cref="P:SupportedMediaTypes"/> property.
    /// </para>
    /// </remarks>
    /// <author>Bruno Baia</author>
    public class NJsonHttpMessageConverter : IHttpMessageConverter
    {
        private IList<MediaType> supportedMediaTypes;

        public NJsonHttpMessageConverter()
        {
            this.supportedMediaTypes = new List<MediaType>(1);
            this.supportedMediaTypes.Add(MediaType.APPLICATION_JSON);
        }

        #region IHttpMessageConverter Membres

        public bool CanRead(Type type, MediaType mediaType)
        {
            return true;
        }

        public bool CanWrite(Type type, MediaType mediaType)
        {
            return true;
        }

        public IList<MediaType> SupportedMediaTypes
        {
            get { return this.supportedMediaTypes; }
        }

        public T Read<T>(IHttpInputMessage message) where T : class
        {
            // Read from the message stream
            using (StreamReader reader = new StreamReader(message.Body))
            using (JsonTextReader jsonReader = new JsonTextReader(reader))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                return jsonSerializer.Deserialize<T>(jsonReader);
            }
        }

        public void Write(object content, MediaType contentType, IHttpOutputMessage message)
        {
            // Write to the message stream
            message.Body = delegate(Stream stream)
            {
                using (StreamWriter writer = new StreamWriter(stream))
                using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
                {
                    JsonSerializer jsonSerializer = new JsonSerializer();
                    jsonSerializer.Serialize(jsonWriter, content);
                }
            };
        }

        #endregion
    }
}
