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
    /// <summary>
    /// Base class for Atom and RSS Feed message converters 
    /// using the <see cref="System.ServiceModel.Syndication.SyndicationFeed"/> class.
    /// </summary>
    /// <author>Bruno Baia</author>
    public abstract class AbstractFeedHttpMessageConverter : AbstractXmlHttpMessageConverter
    {  
        /// <summary>
        /// Creates a new instance of the <see cref="AbstractXmlHttpMessageConverter"/> 
        /// with multiple supported media type.
        /// </summary>
        /// <param name="supportedMediaTypes">The supported media types.</param>
        protected AbstractFeedHttpMessageConverter(params MediaType[] supportedMediaTypes) :
            base(supportedMediaTypes)
        {
        }

        /// <summary>
        /// Indicates whether the given class is supported by this converter.
        /// </summary>
        /// <param name="type">The type to test for support.</param>
        /// <returns><see langword="true"/> if supported; otherwise <see langword="false"/></returns>
        protected override bool Supports(Type type)
        {
            return type.Equals(typeof(SyndicationFeed)) || type.Equals(typeof(SyndicationItem));
        }

        /// <summary>
        /// Abstract template method that reads the actualy object using a <see cref="XmlReader"/>. Invoked from <see cref="M:ReadInternal"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="xmlReader">The XmlReader to use.</param>
        /// <param name="response">The HTTP response to read from.</param>
        /// <returns>The converted object.</returns>
        protected override T ReadXml<T>(XmlReader xmlReader, HttpWebResponse response)
        {
            if (typeof(SyndicationFeed).Equals(typeof(T)))
            {
                return SyndicationFeed.Load(xmlReader) as T;
            }
            if (typeof(SyndicationItem).Equals(typeof(T)))
            {
                return SyndicationItem.Load(xmlReader) as T;
            }
            return null;
        }

        /// <summary>
        /// Returns the default <see cref="XmlReaderSettings">XmlReader settings</see> 
        /// used by this converter to read from the HTTP response.
        /// </summary>
        /// <returns>The XmlReader settings.</returns>
        protected override XmlReaderSettings GetDefaultXmlReaderSettings()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.CloseInput = true;
            settings.IgnoreProcessingInstructions = true;
#if NET_4_0
            settings.DtdProcessing = DtdProcessing.Ignore;
#else
            settings.ProhibitDtd = false;            
#endif
            settings.XmlResolver = null;
            return settings;
        }
    }
}
#endif