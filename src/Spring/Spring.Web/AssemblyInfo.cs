using System.Reflection;
using System.Web.UI;

[assembly: AssemblyTitle("Spring.Web")]
[assembly: AssemblyDescription("Interfaces and classes that provide web application support in Spring.Net")]
[assembly: TagPrefix("Spring.Web.UI.Controls", "spring")]

#if !NET_4_0
[assembly: System.Security.AllowPartiallyTrustedCallers]
[assembly: System.Security.SecurityCritical]
#endif
