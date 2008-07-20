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
using System.Collections;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;

#endregion

namespace Spring.Util
{
    /// <summary>
    /// Provides platform independent access to HttpRuntime methods
    /// </summary>
    /// <remarks>
    /// For e.g. testing purposes, the default environment implementation may be replaced using <see cref="SetInstance(IVirtualEnvironment)" />.
    /// </remarks>
    /// <author>Erich Eichinger</author>
    public sealed class VirtualEnvironment
    {
        // default to standard HttpRuntime
        private static IVirtualEnvironment instance = new HttpRuntimeEnvironment();

        /// <summary>
        /// Represents a method that handles Request related events
        /// </summary>
        public delegate void RequestEventHandler(HttpContext context);

        /// <summary>
        /// Represents a method that handles Session related events
        /// </summary>
        public delegate void SessionEventHandler(HttpSessionState session, CacheItemRemovedReason reason);

        private static readonly object syncEndRequestEvent = new object();
        private static RequestEventHandler s_requestEvent;
        private static readonly object syncEndSessionEvent = new object();
        private static SessionEventHandler s_sessionEvent;

        private static volatile bool s_isInitialized = false;

        /// <summary>
        /// Replaces the current enviroment implementation.
        /// </summary>
        /// <param name="newEnvironment">the new environment implementation to be used</param>
        /// <returns>the previously set environment instance</returns>
        public static IVirtualEnvironment SetInstance(IVirtualEnvironment newEnvironment)
        {
            IVirtualEnvironment prevEnvironment = instance;
            instance = newEnvironment;
            return prevEnvironment;
        }

        #region default IVirtualEnvironment Adapter for HttpRuntime

        /// <summary>
        /// Implementation for running within HttpRuntime
        /// </summary>
        private class HttpRuntimeEnvironment : IVirtualEnvironment
        {
            #region HttpSessionState Adapter

            private class SessionDictionaryAdapter : ISessionState
            {
                private readonly HttpSessionState _sessionState;

                public SessionDictionaryAdapter(HttpSessionState sessionState)
                {
                    _sessionState = sessionState;
                }


                public bool Contains(object key)
                {
                    ICollection keys = _sessionState.Keys;
                    foreach (string sessionKey in keys)
                    {
                        if (object.Equals(sessionKey, (string)key)) return true;
                    }
                    return false;
                }

                public void Add(object key, object value)
                {
                    _sessionState.Add((string)key, value);
                }

                public void Clear()
                {
                    _sessionState.Clear();
                }

                public IDictionaryEnumerator GetEnumerator()
                {
                    Hashtable tableCopy = new Hashtable();
                    ICollection keys = _sessionState.Keys;
                    foreach (string sessionKey in keys)
                    {
                        tableCopy.Add(sessionKey, _sessionState[sessionKey]);
                    }
                    return tableCopy.GetEnumerator();
                }

                public void Remove(object key)
                {
                    _sessionState.Remove((string) key);
                }

                public object this[object key]
                {
                    get { return _sessionState[(string)key]; }
                    set { _sessionState[(string)key] = value; }
                }

                public ICollection Keys
                {
                    get { return _sessionState.Keys; }
                }

                public ICollection Values
                {
                    get
                    {
                        object[] values = new object[_sessionState.Count];
                        _sessionState.CopyTo(values, 0);
                        return values;
                    }
                }

                public bool IsReadOnly
                {
                    get { return _sessionState.IsReadOnly; }
                }

                public bool IsFixedSize
                {
                    get { return false; }
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }

                public void CopyTo(Array array, int index)
                {
                    _sessionState.CopyTo(array, index);
                }

                public int Count
                {
                    get { return _sessionState.Count; }
                }

                public object SyncRoot
                {
                    get { return _sessionState.SyncRoot; }
                }

                public bool IsSynchronized
                {
                    get { return _sessionState.IsSynchronized; }
                }

                public void Abandon()
                {
                    _sessionState.Abandon();
                }

                public bool IsCookieless
                {
                    get { return _sessionState.IsCookieless; }
                }

                public bool IsNewSession
                {
                    get { return _sessionState.IsNewSession; }
                }

                public int LCID
                {
                    get { return _sessionState.LCID; }
                    set { _sessionState.LCID = value; }
                }

                public SessionStateMode Mode
                {
                    get { return _sessionState.Mode; }
                }

                public string SessionID
                {
                    get { return _sessionState.SessionID; }
                }

                public int CodePage
                {
                    get { return _sessionState.CodePage; }
                    set { _sessionState.CodePage = value; }
                }
#if NET_2_0
                public HttpCookieMode CookieMode
                {
                    get { return _sessionState.CookieMode; }
                }
#endif
            }

            #endregion //HttpSessionState Adapter

            public string ApplicationVirtualPath
            {
                get
                {
                    string appPath = HttpRuntime.AppDomainAppVirtualPath;
                    if (!appPath.EndsWith("/")) appPath = appPath + "/";
                    return appPath;
                }
            }

            public string CurrentVirtualPath
            {
                get { return HttpContext.Current.Request.Path; }
            }

            public string CurrentVirtualFilePath
            {
                get { return HttpContext.Current.Request.FilePath; }
            }

            public string CurrentExecutionFilePath
            {
                get { return HttpContext.Current.Request.CurrentExecutionFilePath; }
            }

            public string MapPath(string virtualPath)
            {
                HttpContext ctx = HttpContext.Current;
                if (ctx != null)
                {
                    return ctx.Request.MapPath(virtualPath);
                }
#if NET_2_0

                if (VirtualPathUtility.IsAbsolute(virtualPath) && virtualPath.StartsWith(HttpRuntime.AppDomainAppVirtualPath))
                {
                    virtualPath = VirtualPathUtility.ToAppRelative(virtualPath);
                }
                if (VirtualPathUtility.IsAppRelative(virtualPath))
                {
                    virtualPath = virtualPath.Substring(2); // strip "~/"
                    string physicalPath = Path.Combine(HttpRuntime.AppDomainAppPath, virtualPath);
                    return physicalPath;
                }
#endif
                throw new ArgumentException("can't map context relative path outside a context");
            }

            public ISessionState Session
            {
                get { return new SessionDictionaryAdapter(HttpContext.Current.Session); }
            }

            public IDictionary RequestVariables
            {
                get { return HttpContext.Current.Items; }
            }
        }

        #endregion

        /// <summary>
        /// The virtual (rooted) path of the current Application containing a leading '/' as well as a trailing '/'
        /// </summary>
        public static string ApplicationVirtualPath
        {
            get { return instance.ApplicationVirtualPath; }
        }

        /// <summary>
        /// The virtual (rooted) path of the current Request including <see cref="HttpRequest.PathInfo"/>
        /// </summary>
        public static string CurrentVirtualPath
        {
            get { return instance.CurrentVirtualPath; }
        }

        /// <summary>
        /// The virtual (rooted) path of the current Request without trailing <see cref="HttpRequest.PathInfo"/>
        /// </summary>
        public static string CurrentVirtualFilePath
        {
            get { return instance.CurrentVirtualFilePath; }
        }

        /// <summary>
        /// The virtual (rooted) path of the currently executing script
        /// </summary>
        public static string CurrentExecutionFilePath
        {
            get { return instance.CurrentExecutionFilePath; }
        }

        /// <summary>
        /// Maps a virtual path to it's physical location
        /// </summary>
        public static string MapPath(string virtualPath)
        {
            return instance.MapPath(virtualPath);
        }

        /// <summary>
        /// Receives EndRequest-event from an <see cref="HttpApplication"/> instance
        /// and dispatches it to all handlers registered with this module.
        /// </summary>
        /// <param name="sender">the HttpApplication instance sending this event</param>
        /// <param name="e">always <see cref="EventArgs.Empty"/></param>
        public static void RaiseEndRequest(object sender, EventArgs e)
        {
            // NOTE: don't sync here for performance reasons.
            // It is assumed, that all handlers are registered during application startup
            if (s_requestEvent != null)
            {
                s_requestEvent(((HttpApplication)sender).Context);
            }
        }

        /// <summary>
        /// Receives the EndSession-event and dispatches it to all handlers
        /// registered with this module.
        /// </summary>
        public static void RaiseEndSession(HttpSessionState sessionState, CacheItemRemovedReason reason)
        {
            // NOTE: don't sync here for performance reasons.
            // It is assumed, that all handlers are registered during application startup
            if (s_sessionEvent != null)
            {
                s_sessionEvent(sessionState, reason);
            }
        }

        /// <summary>
        /// Register with this event to receive any EndRequest event occuring in the current AppDomain.
        /// </summary>
        public static event RequestEventHandler EndRequest
        {
            add
            {
                AssertInitialized();
                lock (syncEndRequestEvent)
                {
                    s_requestEvent += value;
                }
            }
            remove
            {
                AssertInitialized();
                lock (syncEndRequestEvent)
                {
                    s_requestEvent -= value;
                }
            }
        }

        /// <summary>
        /// Register with this event to receive any EndSession event occuring in the current AppDomain
        /// </summary>
        /// <remarks>
        /// This event may be raised asynchronously on it's own thread.
        /// Don't rely on e.g. <see cref="HttpContext.Current"/> being available.
        /// </remarks>
        public static event SessionEventHandler EndSession
        {
            add
            {
                AssertInitialized();
                lock (syncEndSessionEvent)
                {
                    s_sessionEvent += value;
                }
            }
            remove
            {
                AssertInitialized();
                lock (syncEndSessionEvent)
                {
                    s_sessionEvent -= value;
                }
            }
        }

        /// <summary>
        /// Signals, that VirtualEnvironment is ready to accept 
        /// handler registrations for EndRequest and EndSession events
        /// </summary>
        public static void SetInitialized()
        {
            s_isInitialized = true;
        }

        /// <summary>
        /// Is this VirtualEnviroment ready to accept handler registrations
        /// for EndRequest and EndSession events ?
        /// </summary>
        public static bool IsInitialized
        {
            get { return s_isInitialized; }
        }

        /// <summary>
        /// Ensures, that WebSupportModule has been initialized. Otherwise an exception is thrown.
        /// </summary>
        private static void AssertInitialized()
        {
            if (!s_isInitialized)
            {
                string msg =
                    @"WebSupportModule not initialized. Did you forget to add " +
                    @"<add name=""Spring"" type=""Spring.Context.Support.WebSupportModule, Spring.Web""/> " +
                    @"to your web.config's <httpModules>-section?";
                throw ConfigurationUtils.CreateConfigurationException(msg);
            }
        }

    }
}