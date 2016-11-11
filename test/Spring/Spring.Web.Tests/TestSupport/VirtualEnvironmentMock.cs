using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using Spring.Collections;
using Spring.Util;

namespace Spring.TestSupport
{
    /// <summary>
    /// Test environment implementation.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class VirtualEnvironmentMock : IVirtualEnvironment, IDisposable
    {
        private readonly IVirtualEnvironment _prevEnvironment;

        private string _currentVirtualFilePath;
        private string _pathInfo;
        private HttpValueCollection _query;
        private string _currentExecutionFilePath;
        private readonly string _applicationVirtualPath;
        private ISessionState _session = new SessionMock();
        private IDictionary _requestVariables = new CaseInsensitiveHashtable(); //CollectionsUtil.CreateCaseInsensitiveHashtable();
        private NameValueCollection requestParams = new NameValueCollection();
        private IDictionary virtualPath2ArtifactsTable = new CaseInsensitiveHashtable();

        public VirtualEnvironmentMock(string currentVirtualFilePath, string pathInfo, string queryText, string applicationVirtualPath, bool autoInitialize)
        {
            _currentVirtualFilePath = currentVirtualFilePath;
            _currentExecutionFilePath = currentVirtualFilePath;
            _pathInfo = (pathInfo == null || pathInfo.Length == 0) ? "" : "/" + pathInfo.TrimStart('/'); // prevent null string and ensure '/' prefixed
            _query =  new HttpValueCollection(queryText);
            _applicationVirtualPath = "/" + ("" + applicationVirtualPath).Trim('/');
            if (!_applicationVirtualPath.EndsWith("/")) _applicationVirtualPath = _applicationVirtualPath + "/";

//            if (!_currentVirtualFilePath.StartsWith(_applicationVirtualPath))
//            {
//                throw new ArgumentException("currentVirtualFilePath must begin with applicationVirtualPath");
//            }

            _prevEnvironment = VirtualEnvironment.SetInstance(this);
            if (autoInitialize)
            {
                VirtualEnvironment.SetInitialized();
            }
        }

        public IDictionary VirtualPath2ArtifactsTable
        {
            get { return virtualPath2ArtifactsTable; }
        }

        public string ApplicationVirtualPath
        {
            get { return _applicationVirtualPath; }
        }

        public string CurrentVirtualPath
        {
            get { return _currentVirtualFilePath + _pathInfo; }
        }

        public string CurrentVirtualPathAndQuery
        {
            get 
            { 
                string result = _currentVirtualFilePath + _pathInfo;
                if (_query.Count > 0)
                {
                    result = result + "?" + _query.ToString();
                }
                return result;
            }
        }

        public string CurrentVirtualFilePath
        {
            get { return _currentVirtualFilePath; }
        }

        public string CurrentExecutionFilePath
        {
            get { return this._currentExecutionFilePath; }
            set { this._currentExecutionFilePath = value; }
        }

        public NameValueCollection QueryString
        {
            get { return _query; }
        }

        public string MapPath(string virtualPath)
        {
            string basePath = Path.GetDirectoryName(new Uri(GetType().Assembly.CodeBase).LocalPath);
            string resultPath = WebUtils.CreateAbsolutePath(this.CurrentVirtualFilePath, virtualPath);
            resultPath = basePath.TrimEnd('\\') + "\\" + resultPath.Replace('/', '\\').TrimStart('\\');
            return resultPath;
        }

        public IDisposable RewritePath(string newVirtualPath, bool rebaseClientPath)
        {
            IDisposable ctx = new RewriteContext(CurrentVirtualPathAndQuery, false, this);

            int index = newVirtualPath.IndexOf('?');
            if (index >= 0)
            {
                 string newQueryString = (index < (newVirtualPath.Length - 1)) ? newVirtualPath.Substring(index + 1) : string.Empty;
                _query = new HttpValueCollection(newQueryString);
                newVirtualPath = newVirtualPath.Substring(0, index);
            }

            _currentVirtualFilePath = newVirtualPath;

            return ctx;
        }

        public Type GetCompiledType(string virtualPath)
        {
            object o = virtualPath2ArtifactsTable[virtualPath];
            if (o == null)
                throw new FileNotFoundException(virtualPath);
            else if (o is Type)
                return (Type) o;
            else
                return o.GetType();
        }

        public object CreateInstanceFromVirtualPath(string virtualPath, Type requiredBaseType)
        {
            object o = virtualPath2ArtifactsTable[virtualPath];
            if (o == null)
                throw new FileNotFoundException(virtualPath);
            else if (o is Type)
                return Activator.CreateInstance((Type)o);
            else
                return o;
        }

        public ISessionState Session
        {
            get { return _session; }
            set { _session = value; }
        }

        public IDictionary RequestVariables
        {
            get { return _requestVariables; }
            set { _requestVariables = value; }
        }

        public NameValueCollection RequestParams
        {
            get { return requestParams; }
        }

        public void Dispose()
        {
            VirtualEnvironment.SetInstance(_prevEnvironment);
        }

        private class RewriteContext : IDisposable
        {
            private string originalPath;
            private bool rebaseClientPath;
            private VirtualEnvironmentMock runtime;

            public RewriteContext(string originalPath, bool rebaseClientPath, VirtualEnvironmentMock runtime)
            {
                this.originalPath = originalPath;
                this.rebaseClientPath = rebaseClientPath;
                this.runtime = runtime;
            }

            public void Dispose()
            {
                if (originalPath != null)
                {
                    this.runtime.RewritePath(originalPath, rebaseClientPath);
                }
            }
        }

        private class HttpValueCollection : NameValueCollection
        {
            public HttpValueCollection(string queryText)
            {
                FillFromString(queryText, false, Encoding.UTF8);
            }

            public override string ToString()
            {
                return ToString(true);
            }

            private void FillFromString(string s, bool urlencoded, Encoding encoding)
            {
                int num = (s != null) ? s.Length : 0;
                for (int i = 0; i < num; i++)
                {
                    int startIndex = i;
                    int num4 = -1;
                    while (i < num)
                    {
                        char ch = s[i];
                        if (ch == '=')
                        {
                            if (num4 < 0)
                            {
                                num4 = i;
                            }
                        }
                        else if (ch == '&')
                        {
                            break;
                        }
                        i++;
                    }
                    string str = null;
                    string str2 = null;
                    if (num4 >= 0)
                    {
                        str = s.Substring(startIndex, num4 - startIndex);
                        str2 = s.Substring(num4 + 1, (i - num4) - 1);
                    }
                    else
                    {
                        str2 = s.Substring(startIndex, i - startIndex);
                    }
                    if (urlencoded)
                    {
                        base.Add(HttpUtility.UrlDecode(str, encoding), HttpUtility.UrlDecode(str2, encoding));
                    }
                    else
                    {
                        base.Add(str, str2);
                    }
                    if ((i == (num - 1)) && (s[i] == '&'))
                    {
                        base.Add(null, string.Empty);
                    }
                }
            }

            internal virtual string ToString(bool urlencoded)
            {
                StringBuilder builder = new StringBuilder();
                int count = this.Count;
                for (int i = 0; i < count; i++)
                {
                    string str3;
                    string key = this.GetKey(i);
                    if (urlencoded)
                    {
                        key = HttpUtility.UrlEncodeUnicode(key);
                    }
                    string str2 = ((key != null) && (key.Length > 0)) ? (key + "=") : "";
                    ArrayList list = (ArrayList)base.BaseGet(i);
                    int num3 = (list != null) ? list.Count : 0;
                    if (i > 0)
                    {
                        builder.Append('&');
                    }
                    if (num3 == 1)
                    {
                        builder.Append(str2);
                        str3 = (string)list[0];
                        if (urlencoded)
                        {
                            str3 = HttpUtility.UrlEncodeUnicode(str3);
                        }
                        builder.Append(str3);
                    }
                    else if (num3 == 0)
                    {
                        builder.Append(str2);
                    }
                    else
                    {
                        for (int j = 0; j < num3; j++)
                        {
                            if (j > 0)
                            {
                                builder.Append('&');
                            }
                            builder.Append(str2);
                            str3 = (string)list[j];
                            if (urlencoded)
                            {
                                str3 = HttpUtility.UrlEncodeUnicode(str3);
                            }
                            builder.Append(str3);
                        }
                    }
                }
                return builder.ToString();
            }
        }
    }
}