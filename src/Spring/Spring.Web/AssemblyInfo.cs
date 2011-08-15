using System.Reflection;
using System.Security;
using System.Web.UI;

[assembly: AssemblyTitle("Spring.Web")]
[assembly: AssemblyDescription("Interfaces and classes that provide web application support in Spring.Net")]
[assembly: TagPrefix("Spring.Web.UI.Controls", "spring")]
//[assembly: AssemblyKeyFile(@"C:\users\aseovic\projects\OpenSource\Spring.Net\Spring.Net.PrivateKey.keys")]

#if !NET_4_0
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityCritical]
#endif
