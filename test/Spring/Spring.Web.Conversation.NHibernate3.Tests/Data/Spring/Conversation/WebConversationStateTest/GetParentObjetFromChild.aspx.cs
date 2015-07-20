using System;

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
