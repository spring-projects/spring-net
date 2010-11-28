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

using System.Net;
using System.Xml;
using System.ServiceModel.Syndication;

namespace Spring.Http.Converters.Feed
{
    /// <summary>
    /// Implementation of <see cref="IHttpMessageConverter"/> that can read and write Atom feeds 
    /// using the <see cref="System.ServiceModel.Syndication.SyndicationFeed"/> class.
    /// </summary>
    /// <remarks>
    /// By default, this converter reads and writes the media type 'application/atom+xml' media type. 
    /// This can be overridden by setting the <see cref="P:SupportedMediaTypes"/> property.
    /// </remarks>
    /// <author>Bruno Baia</author>
    public class Atom10FeedHttpMessageConverter : AbstractFeedHttpMessageConverter
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Atom10FeedHttpMessageConverter"/> 
        /// with 'application/atom+xml', 'application/xml' and 'text/xml' media types. 
        /// </summary>
        public Atom10FeedHttpMessageConverter() : 
            base(new MediaType("application", "atom+xml"), new MediaType("application", "xml"), new MediaType("text", "xml"))
        {
        }

        /// <summary>
        /// Abstract template method that writes the actual body using a <see cref="XmlWriter"/>. Invoked from <see cref="M:WriteInternal"/>.
        /// </summary>
        /// <param name="xmlWriter">The XmlWriter to use.</param>
        /// <param name="content">The object to write to the HTTP request.</param>
        /// <param name="request">The HTTP request to write to.</param>
        protected override void WriteXml(XmlWriter xmlWriter, object content, HttpWebRequest request)
        {
            if (content is SyndicationFeed)
            {
                SyndicationFeed atomFeed = content as SyndicationFeed;
                atomFeed.SaveAsAtom10(xmlWriter);
            }
            else if (content is SyndicationItem)
            {
                SyndicationItem atomItem = content as SyndicationItem;
                atomItem.SaveAsAtom10(xmlWriter);
            }
        }
    }
}
#endif