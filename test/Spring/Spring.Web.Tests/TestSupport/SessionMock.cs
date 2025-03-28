using System.Collections;
using System.Web;
using System.Web.SessionState;
using Spring.Util;

namespace Spring.TestSupport;

/// <summary>
/// Test Session implementation
/// </summary>
public class SessionMock : Hashtable, ISessionState
{
    private bool _isCookieless;
    private bool _isNewSession;
    private int _lcid;
    private SessionStateMode _mode;
    private string _sessionID;
    private int _codePage;
    private HttpCookieMode _cookieMode;

    public SessionMock() : base(CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default)
    {
        Reset();
    }

    protected virtual void Reset()
    {
        _sessionID = Guid.NewGuid().ToString();
        _isNewSession = false;
        _isCookieless = false;
        _mode = SessionStateMode.InProc;
        _codePage = -1;
        _cookieMode = HttpCookieMode.AutoDetect;
    }

    public void Abandon()
    {
        Reset();
    }

    public bool IsCookieless
    {
        get { return _isCookieless; }
        set { _isCookieless = value; }
    }

    public bool IsNewSession
    {
        get { return _isNewSession; }
        set { _isNewSession = value; }
    }

    public int LCID
    {
        get { return _lcid; }
        set { _lcid = value; }
    }

    public SessionStateMode Mode
    {
        get { return _mode; }
        set { _mode = value; }
    }

    public string SessionID
    {
        get { return _sessionID; }
        set { _sessionID = value; }
    }

    public int CodePage
    {
        get { return _codePage; }
        set { _codePage = value; }
    }

    public HttpCookieMode CookieMode
    {
        get { return _cookieMode; }
        set { _cookieMode = value; }
    }
}
