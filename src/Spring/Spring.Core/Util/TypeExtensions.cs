using System.Reflection;

namespace Spring.Util
{
    internal static class TypeExtensions
    {
        internal static string AssemblyQualifiedNameWithoutVersion(this Type type)
            => type.FullName + ", " + type.GetTypeInfo().Assembly.GetName().Name;
    }
}
