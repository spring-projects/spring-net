using System;
using System.Web.Management;
using System.Configuration.Provider;
using System.Collections.Specialized;
using System.Web.Hosting;
using System.IO;
using System.Security.Permissions;
using System.Web;


public class TextFileWebEventProvider : WebEventProvider
{
    private string _LogFileName;

    public override void Initialize(string name,
                                    NameValueCollection config)
    {
// Verify that config isn't null
        if (config == null)
            throw new ArgumentNullException("config");
// Assign the provider a default name if it doesn't have one
        if (String.IsNullOrEmpty(name))
            name = "TextFileWebEventProvider";
// Add a default "description" attribute to config if the
// attribute doesn't exist or is empty
        if (string.IsNullOrEmpty(config["description"]))
        {
            config.Remove("description");
            config.Add("description", "Text file Web event provider");
        }
// Call the base class's Initialize method
        base.Initialize(name, config);
// Initialize _LogFileName and make sure the path
// is app-relative
        string path = config["logFileName"];
        if (String.IsNullOrEmpty(path))
            throw new ProviderException
                ("Missing logFileName attribute");
        if (!VirtualPathUtility.IsAppRelative(path))
            throw new ArgumentException
                ("logFileName must be app-relative");
        string fullyQualifiedPath = VirtualPathUtility.Combine
            (VirtualPathUtility.AppendTrailingSlash
                 (HttpRuntime.AppDomainAppVirtualPath), path);
        _LogFileName = HostingEnvironment.MapPath(fullyQualifiedPath);
        config.Remove("logFileName");
// Make sure we have permission to write to the log file
// throw an exception if we don't
        FileIOPermission permission =
            new FileIOPermission(FileIOPermissionAccess.Write |
                                 FileIOPermissionAccess.Append, _LogFileName);
        permission.Demand();
// Throw an exception if unrecognized attributes remain
        if (config.Count > 0)
        {
            string attr = config.GetKey(0);
            if (!String.IsNullOrEmpty(attr))
                throw new ProviderException
                    ("Unrecognized attribute: " + attr);
        }
    }

    public override void ProcessEvent(WebBaseEvent raisedEvent)
    {
// Write an entry to the log file
        LogEntry(FormatEntry(raisedEvent));
    }

    public override void Flush()
    {
    }

    public override void Shutdown()
    {
    }

// Helper methods
    private string FormatEntry(WebBaseEvent e)
    {
        return String.Format("{0}\t{1}\t{2} (Event Code: {3})",
                             e.EventTime, e.GetType().ToString(), e.Message,
                             e.EventCode);
    }

    private void LogEntry(string entry)
    {
        StreamWriter writer = null;
        try
        {
            writer = new StreamWriter(_LogFileName, true);
            writer.WriteLine(entry);
        }
        finally
        {
            if (writer != null)
                writer.Close();
        }
    }
}