/*
 * All the source code provided by AOP Alliance is Public Domain.
 *
 *      http://aopalliance.sourceforge.net/
 *
 */

using System.Runtime.Serialization;

namespace AopAlliance.Aop;

/// <summary>
/// Superclass for all AOP infrastructure exceptions.
/// </summary>
/// <author>Aleksandar Seovic</author>
[Serializable]
public class AspectException : Exception
{
    /// <summary>
    /// Creates a new instance of the
    /// <see cref="AopAlliance.Aop.AspectException"/> class.
    /// </summary>
    public AspectException()
    {
    }

    /// <summary>
    /// Creates a new instance of the
    /// <see cref="AopAlliance.Aop.AspectException"/> class.
    /// </summary>
    /// <param name="message">
    /// A message about the exception.
    /// </param>
    public AspectException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a new instance of the
    /// <see cref="AopAlliance.Aop.AspectException"/> class.
    /// </summary>
    /// <param name="message">
    /// A message about the exception.
    /// </param>
    /// <param name="innerException">
    /// The root exception that is being wrapped.
    /// </param>
    public AspectException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
#if NET
    [Obsolete(Spring.Obsoletions.LegacyFormatterImplMessage, DiagnosticId = Spring.Obsoletions.LegacyFormatterImplDiagId, UrlFormat = Spring.Obsoletions.SharedUrlFormat)]
#endif
    protected AspectException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
