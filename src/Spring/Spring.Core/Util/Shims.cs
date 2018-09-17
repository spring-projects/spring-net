#if NETSTANDARD
// ReSharper disable once CheckNamespace
namespace System.Runtime.Remoting
{
    internal class RemotingServices
    {
        public static bool IsTransparentProxy(object o) => false;
    }
}
#endif