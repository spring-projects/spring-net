using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;
using Spring.Core.IO;

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
        { }

        public static Uri GetUri(object context, string ext)
        {
            string resname = context.GetType().AssemblyQualifiedName + "#" + ext;
            Uri uri = new Uri("testres://./" + resname, false);
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
            xmlDoc.Load(stm);
            return xmlDoc;
        }

        public static XmlDocument GetXmlValidated(object context, string ext, params IResource[] schemaResources)
        {
            using (Stream stm = GetStream(context, ext))
            {

#if !NET_2_0
                XmlValidatingReader validatingReader = new XmlValidatingReader(stm, XmlNodeType.Document, null);
                validatingReader.ValidationType = ValidationType.Schema;
                XmlSchemaCollection schemas = validatingReader.Schemas;
#else
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                XmlSchemaSet schemas = settings.Schemas;
#endif

                foreach (IResource schemaResource in schemaResources)
                {
                    XmlDocument schemaDoc = new XmlDocument();
                    using (Stream inputStream = schemaResource.InputStream)
                    {
                        schemaDoc.Load(inputStream);
                    }
                    XmlElement root = schemaDoc.DocumentElement;
                    string targetNamespace = root.GetAttribute("targetNamespace", string.Empty);
                    schemas.Add(targetNamespace, new XmlNodeReader(schemaDoc));
                }

#if NET_2_0
                XmlReader validatingReader = XmlReader.Create(stm, settings);
#endif
                XmlDocument xmlDoc = new XmlDocument();
                using (validatingReader)
                {
                    xmlDoc.Load(validatingReader);
                }
                return xmlDoc;
            }
        }

        public static Stream GetStream(object context, string ext)
        {
            Type contextType = (context is Type) ? (Type)context : context.GetType();
            string resname = contextType.FullName + ext;
            Stream stm = contextType.Assembly.GetManifestResourceStream(resname);
            Assert.IsNotNull(stm, "Resource '{0}' in assembly '{1}' not found", resname, contextType.Assembly.FullName);
            return stm;
        }

        /// <summary>
        /// returns  an "assembly://" uri for the specified manifest resource, scoped by the namespace of the specified type.
        /// ("assembly://hint.assemblyname_without_version/hint.Namespace/name")
        /// </summary>
        /// <see cref="Assembly.GetManifestResourceStream(Type,string)"/>
        public static string GetAssemblyResourceUri(Type hint, string name)
        {
            return "assembly://" + hint.Assembly.FullName.Split(',')[0].Trim() + "/" + hint.Namespace + "/" + name;
        }
    }
}
