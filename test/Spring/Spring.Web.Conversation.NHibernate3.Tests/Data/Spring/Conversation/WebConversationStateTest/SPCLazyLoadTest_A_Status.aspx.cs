using System;

using Spring.Web.Conversation;
using Spring.Entities;
using Common.Logging;
using NHibernate;

public partial class SPCLazyLoadTest_A_Status : System.Web.UI.Page
{
    private static readonly ILog LOG = LogManager.GetLogger(typeof(SPCLazyLoadTest_A_Status));

    private IConversationState conversation;
    public IConversationState Conversation
    {
        get { return conversation; }
        set { conversation = value; }
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            this.Conversation.StartResumeConversation();

            SPCMasterEnt sPCMasterEnt = (SPCMasterEnt)this.Session["sPCMasterEnt"];
            foreach (SPCDetailEnt sPCDetailEntItem in sPCMasterEnt.SPCDetailEntList)
            {
                LOG.Debug(String.Format("Page_Load: sPCDetailEntItem.Description={0}", sPCDetailEntItem.Description));
            }
            this.Session["messageTest"] = "no lazy error";
        }
        catch (LazyInitializationException lex)
        {
            this.Session["messageTest"] = lex.GetType().FullName + ": " + lex.Message + "\n" + lex.StackTrace;
        }
        catch (Exception ex)
        {
            this.Session["messageTest"] = ex.GetType().FullName + ": " + ex.Message + "\n" + ex.StackTrace;
        }
    }
}
