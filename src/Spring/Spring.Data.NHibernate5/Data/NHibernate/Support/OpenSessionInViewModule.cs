#region Licence

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

using System.Web;

namespace Spring.Data.NHibernate.Support
{
    /// <summary>
    /// Provide support for the open session in view pattern for lazily loaded hibernate objects
    /// used in ASP.NET pages.
    /// </summary>
    /// <author>jjx: http://forum.springframework.net/member.php?u=29</author>
    /// <author>Mark Pollack (.NET)</author>
    /// <author>Erich Eichinger</author>
    /// <author>Harald Radi</author>
    public class OpenSessionInViewModule : SessionScope, IHttpModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSessionInViewModule"/> class.  Creates a SessionScope,
        /// but does not yet associate a session with a thread, that is left to the lifecycle of the request.
        /// </summary>
        public OpenSessionInViewModule() : base("appSettings", typeof(OpenSessionInViewModule), false)
        {

        }

        /// <summary>
        /// Register context handler and look up SessionFactoryObjectName under the application configuration key,
        /// Spring.Data.NHibernate.Support.OpenSessionInViewModule.SessionFactoryObjectName if not using the default value
        /// (i.e. sessionFactory) and look up the SingleSession setting under the application configuration key,
        /// Spring.Data.NHibernate.Support.OpenSessionInViewModule.SingleSession if not using the default value of true.
        /// </summary>
        /// <param name="context">The standard HTTP application context</param>
        public void Init( HttpApplication context )
        {
            context.BeginRequest += context_BeginRequest;
            context.EndRequest += context_EndRequest;
        }
         /// <summary>
         /// A do nothing dispose method.
         /// </summary>
        public override void Dispose()
        {
        }

        private void context_BeginRequest(object sender, EventArgs e)
        {
            Open();
        }

        private void context_EndRequest(object sender, EventArgs e)
        {
            Close();
        }
    }
}
