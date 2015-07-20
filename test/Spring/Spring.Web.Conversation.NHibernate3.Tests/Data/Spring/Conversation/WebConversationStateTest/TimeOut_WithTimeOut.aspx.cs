using System;

using Spring.Web.Conversation;

public partial class TimeOut_WithTimeOut : System.Web.UI.Page
{
    private IConversationState conversation;
    public IConversationState Conversation
    {
        get { return conversation; }
        set { conversation = value; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        //This should cause the end of the conversation by timeout
        this.Conversation.LastAccess = DateTime.Now.AddMilliseconds(-(this.Conversation.TimeOut * 2));
    }
}
