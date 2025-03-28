using System.Collections;
using System.Web;
using System.Web.SessionState;

namespace Spring.Util;

/// <summary>
/// Abstracts HttpSession
/// </summary>
public interface ISessionState : IDictionary
{
    /// <summary>
    /// <see cref="System.Web.SessionState.HttpSessionState.Abandon"/>
    /// </summary>
    void Abandon();

    /// <summary>
    /// <see cref="System.Web.SessionState.HttpSessionState.IsCookieless"/>
    /// </summary>
    bool IsCookieless { get; }

    /// <summary>
    /// <see cref="System.Web.SessionState.HttpSessionState.IsNewSession"/>
    /// </summary>
    bool IsNewSession { get; }

    /// <summary>
    /// <see cref="System.Web.SessionState.HttpSessionState.LCID"/>
    /// </summary>
    int LCID { get; set; }

    /// <summary>
    /// <see cref="System.Web.SessionState.HttpSessionState.Mode"/>
    /// </summary>
    SessionStateMode Mode { get; }

    /// <summary>
    /// <see cref="System.Web.SessionState.HttpSessionState.SessionID"/>
    /// </summary>
    string SessionID { get; }

    /// <summary>
    /// <see cref="System.Web.SessionState.HttpSessionState.CodePage"/>
    /// </summary>
    int CodePage { get; set; }

#if !MONO
    /// <summary>
    /// <see cref="System.Web.SessionState.HttpSessionState.CookieMode"/>
    /// </summary>
    HttpCookieMode CookieMode { get; }
#endif
}
