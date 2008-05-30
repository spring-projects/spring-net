
public partial class DI_NestedContexts_ContextB_Default : System.Web.UI.Page
{
    private string _message;
    private string _globalMessage;

    public string Message
    {
        get { return this._message; }
        set { this._message = value; }
    }

    public string GlobalMessage
    {
        get { return this._globalMessage; }
        set { this._globalMessage = value; }
    }
}
