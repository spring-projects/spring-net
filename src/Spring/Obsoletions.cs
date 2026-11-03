// Linked into every library that keeps legacy [Serializable] / ISerializable members for .NET Framework users.
//
// ObsoleteAttribute.DiagnosticId and UrlFormat exist only on modern .NET, so the whole file and every use site
// are guarded by #if NET. This is the one place the multi-targeting rules allow a guard at the declaration.
#if NET
namespace Spring;

/// <summary>
/// The message, diagnostic id and URL that the BCL uses for its own formatter-based serialization members
/// (SYSLIB0051), so that Spring's serialization constructors and GetObjectData overrides report the same way.
/// </summary>
internal static class Obsoletions
{
    internal const string SharedUrlFormat = "https://aka.ms/dotnet-warnings/{0}";

    internal const string LegacyFormatterImplMessage = "This API supports obsolete formatter-based serialization. It should not be called or extended by application code.";

    internal const string LegacyFormatterImplDiagId = "SYSLIB0051";
}
#endif
