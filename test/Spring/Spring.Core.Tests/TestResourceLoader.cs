using System;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;
using NUnit.Framework;

namespace Spring
{
	/// <summary>
	/// Supports obtaining embedded resources from assembly.
	/// </summary>
	/// <remarks>
	/// The first <c>context</c> argument is always the namespace scope to be used for resolving resource names.
	/// </remarks>
	public class TestResourceLoader
	{
        #region WebRequest

        public class TestResourceWebResponse : WebResponse
        {
            private Type resourceType;
            private string resourceName;

            public TestResourceWebResponse(Uri requestUri)
            {
                string typeName = HttpUtility.UrlDecode(requestUri.AbsolutePath.Substring(1)); // strip leading '/'
                resourceType = Type.GetType(typeName, true);
                resourceName = HttpUtility.UrlDecode(requestUri.Fragment.Substring(1)); // strip leading '#'
            }

            public override System.IO.Stream GetResponseStream()
            {
                Stream stm = TestResourceLoader.GetStream(resourceType, resourceName);
                return stm;
            }
        }

        public class TestResourceWebRequest : WebRequest
        {
            private Uri requestUri;

            public TestResourceWebRequest(Uri requestUri)
            {
                this.requestUri = requestUri;
            }

            public override WebResponse GetResponse()
            {
                return new TestResourceWebResponse(requestUri);
            }

        }

        public class TestResourceWebRequestFactory : IWebRequestCreate
        {
            public WebRequest Create(Uri uri)
            {
                return new TestResourceWebRequest(uri);
            }
        }

        #endregion

        static TestResourceLoader()
        {
            WebRequest.RegisterPrefix("testres", new TestResourceWebRequestFactory());            
        }

		private TestResourceLoader()
		{}
        
        public static Uri GetUri(object context, string ext)
        {
            string resname = context.GetType().AssemblyQualifiedName + "#" + ext;
            Uri uri = new Uri("testres://(local)/" + resname, false);
            return uri;
        }

        public static string GetText(object context, string ext)
        {
            Stream stm = GetStream(context, ext);
            return new StreamReader(stm).ReadToEnd();
        }

        public static XmlDocument GetXml(object context, string ext)
        {
            Stream stm = GetStream(context, ext);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load( stm );
            return xmlDoc;
        }

	    public static Stream GetStream(object context, string ext)
	    {
	        Type contextType = (context is Type) ? (Type)context : context.GetType();
	        string resname = contextType.FullName + ext;
	        Stream stm = contextType.Assembly.GetManifestResourceStream(resname);
	        Assert.IsNotNull(stm, "Resource '{0}' in assembly '{1}' not found", resname, contextType.Assembly.FullName);
	        return stm;
	    }
	}
}
