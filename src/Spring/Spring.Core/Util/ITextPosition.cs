using System;

namespace Spring.Util
{
#if NET_2_0
    /// <summary>
    /// Holds text position information for e.g. error reporting purposes.
    /// </summary>
    /// <seealso cref="ConfigXmlElement" />
    /// <seealso cref="ConfigXmlAttribute" />
    public interface ITextPosition : System.Configuration.Internal.IConfigErrorInfo
    {
        ///<summary>
        /// Gets a string specifying the file/resource name related to the configuration details.
        ///</summary>
        new string Filename { get; }
        ///<summary>
        /// Gets an integer specifying the line number related to the configuration details.
        ///</summary>
        new int LineNumber { get; }
        /// <summary>
        /// Gets an integer specifying the line position related to the configuration details.
        /// </summary>
        int LinePosition { get; }
    }
#else
    /// <summary>
    /// Holds text position information for e.g. error reporting purposes.
    /// </summary>
    /// <seealso cref="ConfigXmlElement" />
    /// <seealso cref="ConfigXmlAttribute" />
    public interface ITextPosition
    {
        /// <summary>
        /// Gets a string specifying the file/resource name related to the configuration details.
        /// </summary>
        string Filename { get; }
        /// <summary>
        /// Gets an integer specifying the line number related to the configuration details.
        /// </summary>
        int LineNumber { get; }
        /// <summary>
        /// Gets an integer specifying the line position related to the configuration details.
        /// </summary>
        int LinePosition { get; }
    }
#endif
}
