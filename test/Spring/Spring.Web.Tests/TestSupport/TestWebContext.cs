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
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Hosting;

#endregion

namespace Spring.TestSupport
{
    public class TestWebContext : IDisposable
    {
        private readonly TextWriter _out;
        private readonly HttpWorkerRequest _wr;
        [ThreadStatic] private static TestWebContext _wc;

        public static void Create(string virtualPath, string page)
        {
            _wc = new TestWebContext(virtualPath, page);
        }

        public static void Release()
        {
            if (_wc != null)
            {
                _wc.Dispose();
            }
        }

        public TestWebContext(string virtualPath, string page)
        {
            _out = new StringWriter();
            HttpWorkerRequest wr;
            object appPath = Thread.GetDomain().GetData(".appPath");
            if (appPath != null)
            {
                wr = new SimpleWorkerRequest(page, string.Empty, _out);
            }
            else
            {
                string physDir = AppDomain.CurrentDomain.BaseDirectory + "\\";
                wr = new SimpleWorkerRequest(virtualPath, physDir, page, string.Empty, _out);
            }
            HttpContext ctx = new HttpContext(wr);
            HttpContext.Current = ctx;
            HttpBrowserCapabilities browser = new HttpBrowserCapabilities();
#if NET_2_0
            browser.Capabilities = CollectionsUtil.CreateCaseInsensitiveHashtable();
            browser.Capabilities[string.Empty] = "Test User Agent"; // string.Empty is the key for "user agent"
#endif
            ctx.Request.Browser = browser;
            _wr = wr;
        }

        public HttpWorkerRequest HttpWorkerRequest
        {
            get { return _wr; }
        }

        public TextWriter Out
        {
            get { return _out; }
        }

        public void Dispose()
        {
            HttpContext.Current = null;
        }
    }
}