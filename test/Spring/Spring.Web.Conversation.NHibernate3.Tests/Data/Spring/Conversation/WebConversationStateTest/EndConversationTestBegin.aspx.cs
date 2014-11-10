using System;

using Spring.Web.Conversation;
using Spring.Bsn;

public partial class EndConversationTestBegin : System.Web.UI.Page
{
    private IConversationEvidenceBsn conversationEvidenceBsn;
    public IConversationEvidenceBsn ConversationEvidenceBsn
    {
        get { return conversationEvidenceBsn; }
        set { conversationEvidenceBsn = value; }
    }


    private IConversationState conversation;
    public IConversationState Conversation
    {
        get { return conversation; }
        set { conversation = value; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        this.Conversation.StartResumeConversation();

        Session["ConversationEvidenceBsn_UniqId"] = this.ConversationEvidenceBsn.UniqueId();
    }
}
