#if NET_3_5
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

using System;
using System.Net;
using System.Xml;
using System.ServiceModel.Syndication;

using Spring.Http.Converters.Xml;

namespace Spring.Http.Converters.Feed
{
    public abstract class AbstractFeedHttpMessageConverter : AbstractXmlHttpMessageConverter
    {
        /**
         * Construct an {@code AbstractHttpMessageConverter} with multiple supported media type.
         * @param supportedMediaTypes the supported media types
         */
        protected AbstractFeedHttpMessageConverter(params MediaType[] supportedMediaTypes) :
            base(supportedMediaTypes)
        {
        }

        protected override bool Supports(Type type)
        {
            return type.Equals(typeof(SyndicationFeed));
        }

        protected override T ReadXml<T>(XmlReader xmlReader, HttpWebResponse response)
        {
            return SyndicationFeed.Load(xmlReader) as T;
        }

        protected override XmlReaderSettings GetDefaultXmlReaderSettings()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.CloseInput = true;
            settings.IgnoreProcessingInstructions = true;
            settings.ProhibitDtd = false;            
            settings.XmlResolver = null;
            return settings;
        }
    }
}
#endif