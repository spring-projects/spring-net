using System;

using Spring.Web.Conversation;

public partial class EndPausedTest : System.Web.UI.Page
{
    private IConversationState convA;

    public IConversationState ConvA
    {
        get { return convA; }
        set { convA = value; }
    }
    private IConversationState convB;

    public IConversationState ConvB
    {
        get { return convB; }
        set { convB = value; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (this.Request["testPhase"] == "begin")
        {
            if (!this.convA.Ended && !this.convB.Ended)
                this.Session["result"] = "OK";
            else
                this.Session["result"] = "(!this.convA.Ended && !this.convB.Ended) is false";
        }
        else if (this.Request["testPhase"] == "startConvA")
        {
            this.convA.StartResumeConversation();
            if (!this.convA.Ended && this.convB.Ended)
                this.Session["result"] = "OK";
            else
                this.Session["result"] = "(!this.convA.Ended && this.convB.Ended) is false";
        }
    }
}
