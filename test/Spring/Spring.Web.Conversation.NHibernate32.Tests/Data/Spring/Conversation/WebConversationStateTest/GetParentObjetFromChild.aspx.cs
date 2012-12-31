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

public partial class GetParentObjetFromChild : System.Web.UI.Page
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

        Session["parentKey"] = this.Conversation["parentKey"];
        Session["childKey"] = this.Conversation["childKey"];
        Session["overwrittenKey"] = this.Conversation["overwrittenKey"];
    }
}
