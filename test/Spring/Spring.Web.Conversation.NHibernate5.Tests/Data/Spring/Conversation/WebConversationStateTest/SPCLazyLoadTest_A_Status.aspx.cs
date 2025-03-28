using Microsoft.Extensions.Logging;
using Spring.Web.Conversation;
using Spring.Entities;
using NHibernate;
using Spring;

public partial class SPCLazyLoadTest_A_Status : System.Web.UI.Page
{
    private static readonly ILogger<SPCLazyLoadTest_A_Status> LOG = LogManager.GetLogger<SPCLazyLoadTest_A_Status>();

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

            SPCMasterEnt sPCMasterEnt = (SPCMasterEnt) this.Session["sPCMasterEnt"];
            foreach (SPCDetailEnt sPCDetailEntItem in sPCMasterEnt.SPCDetailEntList)
            {
                LOG.LogDebug(String.Format("Page_Load: sPCDetailEntItem.Description={0}", sPCDetailEntItem.Description));
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
