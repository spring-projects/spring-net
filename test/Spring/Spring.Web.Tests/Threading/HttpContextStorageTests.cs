#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

using System.IO;
using System.Web;
using NUnit.Framework;

#endregion

namespace Spring.Threading
{
    /// <summary>
    /// Apply common thread-storage tests for <see cref="HttpContextStorage"/>
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class HttpContextStorageTests : CommonThreadStorageTests
    {
        protected override IThreadStorage CreateStorage()
        {
            return new HttpContextStorage();
        }

        protected override void ThreadSetup()
        {
            HttpContext.Current = new HttpContext(new HttpRequest("/page.aspx", "http://localhost/page.aspx", null), new HttpResponse(new StringWriter()));
        }

        protected override void ThreadCleanup()
        {
            HttpContext.Current = null;
        }
    }
}
