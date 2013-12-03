using System;

using Spring.Bsn;
using Spring.Web.Conversation;

public partial class EndConversationTestEnd : System.Web.UI.Page
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
        this.Conversation.EndConversation();
    }
}
