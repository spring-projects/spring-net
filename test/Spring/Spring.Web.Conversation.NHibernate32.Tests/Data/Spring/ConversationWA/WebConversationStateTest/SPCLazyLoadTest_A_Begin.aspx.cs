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
using NHibernate;
using Spring.Entities;

public partial class SPCLazyLoadTest_A_Begin : System.Web.UI.Page
{
    private IConversationState conversation;
    public IConversationState Conversation
    {
        get { return conversation; }
        set { conversation = value; }
    }
    private ISessionFactory sessionFactory;
    public ISessionFactory SessionFactory
    {
        get { return sessionFactory; }
        set { sessionFactory = value; }
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        if (this.Request["endConversation"] != null && bool.Parse(this.Request["endConversation"]))
        {
            this.Conversation.EndConversation();
        }
        else
        {
            this.Conversation.StartResumeConversation();

            ISession session = this.SessionFactory.GetCurrentSession();

            SPCMasterEnt sPCMasterEnt = session.Get<SPCMasterEnt>(1);
            this.Session["sPCMasterEnt"] = sPCMasterEnt;
        }
    }
}
