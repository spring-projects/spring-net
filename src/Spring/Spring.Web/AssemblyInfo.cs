using System.Reflection;
using System.Security;
using System.Web.UI;

[assembly: AssemblyTitle("Spring.Web")]
[assembly: AssemblyDescription("Interfaces and classes that provide web application support in Spring.Net")]
[assembly: TagPrefix("Spring.Web.UI.Controls", "spring")]
//[assembly: AssemblyKeyFile(@"C:\users\aseovic\projects\OpenSource\Spring.Net\Spring.Net.PrivateKey.keys")]
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityCritical]

#if NET_1_0 || NET_1_1
namespace System.Security
{
    ///<summary>
    ///</summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Method)]
    internal class SecurityCriticalAttribute : Attribute
    { }
    [AttributeUsage(AttributeTargets.Method)]
    internal class SecurityTreatAsSafeAttribute : Attribute
    { }
}
#endif
