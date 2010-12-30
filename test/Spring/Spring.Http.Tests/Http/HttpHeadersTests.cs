#region License

/*
 * Copyright 2002-2011 the original author or authors.
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

using NUnit.Framework;

namespace Spring.Http
{
    /// <summary>
    /// Unit tests for the HttpHeaders class.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    [TestFixture]
    public class HttpHeadersTests
    {
        private HttpHeaders headers;

        [SetUp]
        public void SetUp() 
        {
            headers = new HttpHeaders();
        }

        [Test]
        public void AcceptGet()
        {
            Assert.IsEmpty(headers.Accept);

            headers.Add("Accept", "text/plain; q=0.5");
            headers.Add("Accept", "text/html");
            headers.Add("Accept", "text/x-dvi; q=0.8");
            headers.Add("Accept", "text/x-c");

            MediaType[] mediaTypes = headers.Accept;
            Assert.NotNull(mediaTypes, "No media types returned");
            Assert.AreEqual(4, mediaTypes.Length, "Invalid amount of media types");
            Assert.AreEqual("text/plain;q=0.5", mediaTypes[0].ToString());
            Assert.AreEqual("text/html", mediaTypes[1].ToString());
            Assert.AreEqual("text/x-dvi;q=0.8", mediaTypes[2].ToString());
            Assert.AreEqual("text/x-c", mediaTypes[3].ToString());
        }

        [Test]
        public void AcceptSet()
        {
            MediaType mediaType1 = new MediaType("text", "html");
            MediaType mediaType2 = new MediaType("text", "plain");
            MediaType[] mediaTypes = new MediaType[2] { mediaType1, mediaType2 };

            headers.Accept = mediaTypes;
            Assert.AreEqual(mediaTypes, headers.Accept, "Invalid Accept header");
            Assert.AreEqual("text/html,text/plain", headers["Accept"], "Invalid Accept header");
        }


        //[Test]
        //public void acceptCharsets()
        //{
        //    Charset charset1 = Charset.forName("UTF-8");
        //    Charset charset2 = Charset.forName("ISO-8859-1");
        //    List<Charset> charsets = new ArrayList<Charset>(2);
        //    charsets.add(charset1);
        //    charsets.add(charset2);
        //    headers.setAcceptCharset(charsets);
        //    Assert.AreEqual("Invalid Accept header", charsets, headers.getAcceptCharset());
        //    Assert.AreEqual("Invalid Accept header", "utf-8, iso-8859-1", headers.getFirst("Accept-Charset"));
        //}

        [Test]
        public void AllowGet()
        {
            Assert.IsEmpty(headers.Allow);

            headers.Add("Allow", "PUT");
            headers.Add("Allow", "POST");

            HttpMethod[] methods = headers.Allow;
            Assert.NotNull(methods, "No methods returned");
            Assert.AreEqual(2, methods.Length, "Invalid amount of HTTP methods");
            Assert.AreEqual(HttpMethod.PUT, methods[0]);
            Assert.AreEqual(HttpMethod.POST, methods[1]);
        }

        [Test]
        public void AllowSet()
        {
            HttpMethod[] methods = new HttpMethod[2] { HttpMethod.GET, HttpMethod.POST };
            
            headers.Allow = methods;
            Assert.AreEqual(methods, headers.Allow, "Invalid Allow header");
            Assert.AreEqual("GET,POST", headers["Allow"], "Invalid Allow header");
        }

        [Test]
        public void ContentLength()
        {
            long length = 42;

            headers.ContentLength = length;
            Assert.AreEqual(length, headers.ContentLength, "Invalid Content-Length header");
            Assert.AreEqual("42", headers["Content-Length"], "Invalid Content-Length header");
        }

        [Test]
        public void ContentTypeGet()
        {
            Assert.IsNull(headers.ContentType);

            headers.Set("Content-Type", "text/html;charset=UTF-8");

            Assert.AreEqual("text/html;charset=UTF-8", headers.ContentType.ToString(), "Invalid Content-Type header");
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ContentTypeGetMultipleValues()
        {
            headers.Add("Content-Type", "text/html");
            headers.Add("Content-Type", "application/xml");

            MediaType mediaType = headers.ContentType;
        }

        [Test]
        public void ContentTypeSet()
        {
            MediaType contentType = new MediaType("text", "html", "UTF-8");
            
            headers.ContentType = contentType;
            Assert.AreEqual(contentType, headers.ContentType, "Invalid Content-Type header");
            Assert.AreEqual("text/html;charset=UTF-8", headers["Content-Type"], "Invalid Content-Type header");
        }

        [Test]
        public void Location() 
        {
            Uri location = new Uri("http://www.example.com/hotels");
            
            headers.Location = location;
            Assert.AreEqual(location, headers.Location, "Invalid Location header");
            Assert.AreEqual("http://www.example.com/hotels", headers["Location"], "Invalid Location header");
        }

        [Test]
        public void ETag()
        {
            string eTag = "v2.6";

            headers.ETag = eTag;
            Assert.AreEqual(eTag, headers.ETag, "Invalid ETag header");
            Assert.AreEqual("\"v2.6\"", headers["ETag"], "Invalid ETag header");
        }

        [Test]
        public void ETagWithWeaknessIndicator()
        {
            string eTag = "W/\"v2.6\"";

            headers.ETag = eTag;
            Assert.AreEqual(eTag, headers.ETag, "Invalid ETag header");
            Assert.AreEqual(eTag, headers["ETag"], "Invalid ETag header");
        }

        [Test]
        public void IfNoneMatchGet()
        {
            Assert.IsEmpty(headers.IfNoneMatch);

            headers.Add("If-None-Match", "v1.0");
            headers.Add("If-None-Match", "v2.0");

            string[] eTags = headers.IfNoneMatch;
            Assert.NotNull(eTags, "No eTags returned");
            Assert.AreEqual(2, eTags.Length, "Invalid amount of eTags");
            Assert.AreEqual("v1.0", eTags[0]);
            Assert.AreEqual("v2.0", eTags[1]);
        }

        [Test]
        public void IfNoneMatchSet()
        {
            string ifNoneMatch1 = "v2.6";
            string ifNoneMatch2 = "v2.7";
            string[] ifNoneMatchArray = new string[2] { ifNoneMatch1, ifNoneMatch2 };

            headers.IfNoneMatch = ifNoneMatchArray;
            Assert.AreEqual(ifNoneMatchArray, headers.IfNoneMatch, "Invalid If-None-Match header");
            Assert.AreEqual("\"v2.6\",\"v2.7\"", headers.Get("If-None-Match"), "Invalid If-None-Match header");
        }

        [Test]
        public void Date()
        {
            DateTime date = new DateTime(2008, 12, 18, 10, 20, 00, DateTimeKind.Utc);

            headers.Date = date;
            Assert.AreEqual(date, headers.Date, "Invalid Date header");
            Assert.AreEqual("Thu, 18 Dec 2008 10:20:00 GMT", headers["date"], "Invalid Date header");

            // RFC 850
            headers.Set("Date", "Thursday, 18-Dec-08 10:20:00 GMT");
            Assert.AreEqual(date, headers.Date, "Invalid Date header");
        }

        //[Test]//(expected = IllegalArgumentException.class)
        //public void DateInvalid() 
        //{
        //    headers.Set("Date", "Foo Bar Baz");
        //    Assert.IsNotNull(headers.Date);
        //}

        //[Test]
        //public void dateOtherLocale() {
        //    Locale defaultLocale = Locale.getDefault();
        //    try {
        //        Locale.setDefault(new Locale("nl", "nl"));
        //        Calendar calendar = new GregorianCalendar(2008, 11, 18, 11, 20);
        //        calendar.setTimeZone(TimeZone.getTimeZone("CET"));
        //        long date = calendar.getTimeInMillis();
        //        headers.setDate(date);
        //        Assert.AreEqual("Invalid Date header", "Thu, 18 Dec 2008 10:20:00 GMT", headers.getFirst("date"));
        //        Assert.AreEqual("Invalid Date header", date, headers.getDate());
        //    }
        //    finally {
        //        Locale.setDefault(defaultLocale);
        //    }
        //}

        [Test]
        public void LastModified()
        {
            DateTime date = new DateTime(2008, 12, 18, 10, 20, 00, DateTimeKind.Utc);

            headers.LastModified = date;
            Assert.AreEqual(date, headers.LastModified, "Invalid Last-Modified header");
            Assert.AreEqual("Thu, 18 Dec 2008 10:20:00 GMT", headers["Last-Modified"], "Invalid Last-Modified header");
        }

        [Test]
        public void Expires()
        {
            string date = "Thu, 18 Dec 2008 10:20:00 GMT";

            headers.Expires = date;
            Assert.AreEqual(date, headers.Expires, "Invalid Expires header");
            Assert.AreEqual("Thu, 18 Dec 2008 10:20:00 GMT", headers["Expires"], "Invalid Expires header");
        }

        [Test]
        public void IfModifiedSince()
        {
            DateTime date = new DateTime(2008, 12, 18, 10, 20, 00, DateTimeKind.Utc);

            headers.IfModifiedSince = date;
            Assert.AreEqual(date, headers.IfModifiedSince, "Invalid If-Modified-Since header");
            Assert.AreEqual("Thu, 18 Dec 2008 10:20:00 GMT", headers["If-Modified-Since"], "Invalid If-Modified-Since header");
        }

        [Test]
        public void Pragma()
        {
            string pragma = "no-cache";

            headers.Pragma = pragma;
            Assert.AreEqual(pragma, headers.Pragma, "Invalid Pragma header");
            Assert.AreEqual("no-cache", headers["pragma"], "Invalid Pragma header");
        }

        [Test]
        public void CacheControl()
        {
            string cacheControl = "no-cache";

            headers.CacheControl = cacheControl;
            Assert.AreEqual(cacheControl, headers.CacheControl, "Invalid Cache-Control header");
            Assert.AreEqual("no-cache", headers["cache-control"], "Invalid Cache-Control header");
        }

        //[Test]
        //public void contentDisposition() {
        //    headers.setContentDispositionFormData("name", null);
        //    Assert.AreEqual("Invalid Content-Disposition header", "form-data; name=\"name\"", headers.getFirst("Content-Disposition"));

        //    headers.setContentDispositionFormData("name", "filename");
        //    Assert.AreEqual("Invalid Content-Disposition header", "form-data; name=\"name\"; filename=\"filename\"", headers.getFirst("Content-Disposition"));
        //}

        [Test]
        public void GetSingleValue()
        {
            headers.Add("HeaderName", "1,2,3");

            Assert.AreEqual("1,2,3", headers.GetSingleValue("HeaderName"));
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void GetSingleValueWithMultipleValues()
        {
            headers.Add("HeaderName", "1");
            headers.Add("HeaderName", "2");
            headers.Add("HeaderName", "3");

            headers.GetSingleValue("HeaderName");
        }

        [Test]
        public void GetMultiValues()
        {
            headers.Add("HeaderName", "1,2,3");
            Assert.AreEqual(3, headers.GetMultiValues("HeaderName").Length);

            headers.Add("HeaderName", "4");
            Assert.AreEqual(4, headers.GetMultiValues("HeaderName").Length);
        }
    }
}
