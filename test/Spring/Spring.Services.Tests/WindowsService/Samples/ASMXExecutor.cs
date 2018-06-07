using System;
using System.Globalization;
using System.IO;
using System.Runtime.Remoting;

namespace Spring.Service.Sample
{
    public class SoapWorkerRequest : SimpleWorkerRequest
    {
        private string soapstr;
        private string soapAction;

        public SoapWorkerRequest (string page, string soapstr,
                                  string soapAction, TextWriter output) : base (page, null, output)
        {
            this.soapstr = soapstr;
            this.soapAction = soapAction;
        }

        public override string GetHttpVerbName ()
        {
            return "POST";
        }

        public override int ReadEntityBody (byte[] buffer, int size)
        {
            char[] chars = soapstr.ToCharArray ();
            for (int i = 0; i < chars.Length; i++)
                buffer [i] = (byte) chars [i];
            return chars.Length;
        }

        public override string GetKnownRequestHeader (int index)
        {
            string retval;
            switch (index)
            {
                case 11:
                    retval = String.Format ("{0}", soapstr.Length);
                    break;
                case 12:
                    retval = "text/xml; charset=utf-8";
                    break;
                default:
                    retval = null;
                    break;
            }
            return retval;
        }

        public override string[][] GetUnknownRequestHeaders ()
        {
            string[] vals = {"SOAPAction", soapAction};
            string[][] namesvals = {vals};
            return namesvals;
        }

        public override string MapPath (string path)
        {
            if (path == GetFilePath ())
                return GetFilePathTranslated ();
            return base.MapPath (path);
        }
    }

    public class MyExeHost : MarshalByRefObject
    {
        public void ProcessRequest (String page, string soapmsg, string soapAction)
        {
            HttpWorkerRequest hwr =
                new SoapWorkerRequest (page, soapmsg, soapAction, Console.Out);
            HttpRuntime.ProcessRequest (hwr);
        }
    }

    internal class MyAspHost
    {
        public static object CreateApplicationHost (Type hostType,
                                                    string virtualDir, string physicalDir)
        {
            if (!(physicalDir.EndsWith ("\\")))
                physicalDir = physicalDir + "\\";
            string aspDir = HttpRuntime.AspInstallDirectory;
            string domainId = DateTime.Now.ToString (
                DateTimeFormatInfo.InvariantInfo).GetHashCode ().ToString ("x");
            string appName = (virtualDir + physicalDir).GetHashCode ().ToString ("x");
            AppDomainSetup setup = new AppDomainSetup ();
            setup.ApplicationName = appName;
            setup.ConfigurationFile = "web.config";
            AppDomain ad = AppDomain.CreateDomain (domainId, null, setup);
            ad.SetData (".appDomain", "*");
            ad.SetData (".appPath", physicalDir);
            ad.SetData (".appVPath", virtualDir);
            ad.SetData (".domainId", domainId);
            ad.SetData (".hostingVirtualPath", virtualDir);
            ad.SetData (".hostingInstallDir", aspDir);
            ObjectHandle oh = ad.CreateInstance (hostType.Module.Assembly.FullName,
                                                 hostType.FullName);
            return oh.Unwrap ();
        }

        private static void Main (string[] args)
        {
            MyExeHost myHost = (MyExeHost) CreateApplicationHost (typeof (MyExeHost),
                                                                  "/", Directory.GetCurrentDirectory ());
            myHost.ProcessRequest (args [0], Console.In.ReadToEnd (), args [1]);
        }
    }
}

//To compile the program, open a Visual Studio .NET command prompt (or any other command prompt session with the C# compiler in the path) and run this command:
//csc /t:exe /r:System.Web.dll WsHostTest.cs
//Put the exe file in a directory with an ASMX file. Here’s the simple file I’ve used in my tests:
//<%@ WebService Language="C#" Debug="true" Class="foo" %>
//using System.Web.Services;
//
//
//public class foo
//{
//    [WebMethod()]
//    public string echo(string input)
//    {
//        return input;
//    }
//}
//
//I also created a file with the SOAP message I wanted to use as input for the program. Here is the contents of this file:
//
//<?xml version="1.0" encoding="utf-8"?>
//<?xml version="1.0" encoding="utf-8"?>
//<soap:Envelope
//xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/"
//xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
//xmlns:xsd="http://www.w3.org/2001/XMLSchema">
//<soap:Body>
//<echo xmlns="http://tempuri.org/">
//<input>Howdy</input>
//</echo>
//</soap:Body>
//</soap:Envelope>
//</pre>
//
//To call the program, I entered this command line:
//
//WsHostTest echo.asmx http://tempuri.org/echo <soaptext.txt