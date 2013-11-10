using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Spring.Web.Conversation;
using Spring.Web.Conversation;

public partial class TimeOut_NoTimeOut : System.Web.UI.Page
{
    private IConversationState conversation;
    public IConversationState Conversation
    {
        get { return conversation; }
        set { conversation = value; }
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        this.Conversation.StartResumeConversation();

        this.Session["keyTimeOut_Old"] = Conversation["keyTimeOut"];
        Conversation["keyTimeOut"] = "this is the new value";
        this.Session["keyTimeOut_New"] = Conversation["keyTimeOut"];

        //This should not cause the end of the conversation by timeout
        this.Conversation.LastAccess = DateTime.Now.AddMilliseconds(-(this.Conversation.TimeOut / 2));
    }
}
