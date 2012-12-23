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
using Spring.ConversationWA;
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
