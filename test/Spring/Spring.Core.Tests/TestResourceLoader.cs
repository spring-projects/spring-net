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
    /// <para>
    /// The first <c>context</c> argument is always the namespace scope to be used for resolving resource names.
    /// </para>
    /// <para>
    /// Upon first usage, TestResourceLoader registers the "testres://" protocol prefix for loading embedded resources.
    /// A testres:// Url must be of the form "testres://./&lt;context-typename&gt;#&lt;ext" - <see cref="GetStream"/>.
    /// </para>
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

        #pragma warning disable SYSLIB0014 // WebRequest is obsolete
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
        #pragma warning restore SYSLIB0014 // WebRequest is obsolete

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

        /// <summary>
        /// Returns an Uri of the form "testres://./resourcname" that may be passed into <see cref="WebRequest.Create(string)"/> etc.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static Uri GetUri(object context, string ext)
        {
            string resname = context.GetType().AssemblyQualifiedName + "#" + ext;
            Uri uri = new Uri("testres://inline/" + resname);
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

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                XmlSchemaSet schemas = settings.Schemas;

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

                XmlReader validatingReader = XmlReader.Create(stm, settings);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(validatingReader);
                validatingReader.Close();
                return xmlDoc;
            }
        }

        /// <summary>
        /// Returns an embedded assembly resource, who's name is constructed from the given parameters as
        /// <c>context.GetType().FullName + ext</c>
        /// </summary>
        public static Stream GetStream(object context, string ext)
        {
            Type contextType = (context is Type) ? (Type)context : context.GetType();
            string resname = contextType.FullName + ext;
            Stream stm = contextType.Assembly.GetManifestResourceStream(resname);
            Assert.IsNotNull(stm, "Resource '{0}' in assembly '{1}' not found", resname, contextType.Assembly.FullName);
            return stm;
        }

        /// <summary>
        /// Exports a resource obtained via <see cref="GetStream"/> to the specified destination.
        /// </summary>
        public static FileInfo ExportResource(object context, string ext, FileInfo destination)
        {
            Stream istm = GetStream(context, ext);
            using(istm)
            {
                FileStream ostm = destination.OpenWrite();
                using (ostm)
                {
                    byte[] buffer = new byte[2048];
                    int bytesRead = istm.Read(buffer, 0, buffer.Length);
                    while (bytesRead > 0)
                    {
                        ostm.Write(buffer, 0, bytesRead);
                        bytesRead = istm.Read(buffer, 0, buffer.Length);
                    }
                    ostm.Flush();
                    ostm.Close();
                }
                istm.Close();
            }
            return destination;
        }

        /// <summary>
        /// returns  an "assembly://" uri for the specified manifest resource, scoped by the namespace of the specified type.
        /// ("assembly://hint.assemblyname_without_version/hint.Namespace/name")
        /// </summary>
        /// <see cref="Assembly.GetManifestResourceStream(Type,string)"/>
        public static string GetAssemblyResourceUri(Type hint, string name)
        {
            return "assembly://" + hint.Assembly.FullName.Split(',')[0].Trim() + "/" + hint.Namespace + ((name.IndexOf('/')>-1)?".":"/") + name;
        }

        /// <summary>
        /// returns  an "assembly://" uri for the specified manifest resource, scoped by the namespace of the specified instance's type.
        /// ("assembly://hint.assemblyname_without_version/hint.Namespace/name")
        /// </summary>
        /// <see cref="Assembly.GetManifestResourceStream(Type,string)"/>
        public static string GetAssemblyResourceUri(object hint, string name)
        {
            Type hintType = hint.GetType();
            return GetAssemblyResourceUri(hintType, name);
        }
    }
}
