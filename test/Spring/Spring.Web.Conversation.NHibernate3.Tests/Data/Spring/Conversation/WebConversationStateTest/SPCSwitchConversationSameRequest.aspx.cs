using System;

using Spring.Web.Conversation;
using Spring.Entities;
using NHibernate;
using Common.Logging;

public partial class SPCSwitchConversationSameRequest : System.Web.UI.Page
{
    private static readonly ILog LOG = LogManager.GetLogger(typeof(SPCSwitchConversationSameRequest));

    private IConversationState conversationA;
    public IConversationState ConversationA
    {
        get { return conversationA; }
        set { conversationA = value; }
    }
    private IConversationState conversationB;
    public IConversationState ConversationB
    {
        get { return conversationB; }
        set { conversationB = value; }
    }

    private ISessionFactory sessionFactory;
    public ISessionFactory SessionFactory
    {
        get { return sessionFactory; }
        set { sessionFactory = value; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        this.ConversationA.StartResumeConversation();
        SPCMasterEnt sPCMasterEntA = this.SessionFactory.GetCurrentSession().Get<SPCMasterEnt>(1);

        this.ConversationB.StartResumeConversation();
        SPCMasterEnt sPCMasterEntB = this.SessionFactory.GetCurrentSession().Get<SPCMasterEnt>(1);

        //testeRaizeLazy_A
        try
        {
            this.LoopSPCMasterEnt(sPCMasterEntA, "sPCMasterEntA");
        }
        catch (LazyInitializationException)
        {
            this.Session["testeRaizeLazy_A"] = "OK";
        }
        catch (Exception ex)
        {
            this.Session["testeRaizeLazy_A"] = ex.ToString();
        }

        //testeRaizeLazy_B
        this.ConversationA.StartResumeConversation();
        try
        {
            this.LoopSPCMasterEnt(sPCMasterEntB, "sPCMasterEntB");
        }
        catch (LazyInitializationException)
        {
            this.Session["testeRaizeLazy_B"] = "OK";
        }
        catch (Exception ex)
        {
            this.Session["testeRaizeLazy_B"] = ex.ToString();
        }

        //testeNoRaizeLazy_A
        this.ConversationA.StartResumeConversation();
        try
        {
            this.LoopSPCMasterEnt(sPCMasterEntA, "sPCMasterEntA");
            this.Session["testeNoRaizeLazy_A"] = "OK";
        }
        catch (Exception ex)
        {
            this.Session["testeNoRaizeLazy_A"] = ex.ToString();
        }

        //testeNoRaizeLazy_B
        this.ConversationB.StartResumeConversation();
        try
        {
            this.LoopSPCMasterEnt(sPCMasterEntB, "sPCMasterEntB");
            this.Session["testeNoRaizeLazy_B"] = "OK";
        }
        catch (Exception ex)
        {
            this.Session["testeNoRaizeLazy_B"] = ex.ToString();
        }

        this.ConversationA.EndConversation();
        this.ConversationB.EndConversation();
    }

    private void LoopSPCMasterEnt(SPCMasterEnt sPCMasterEnt, String desc)
    {
        foreach (SPCDetailEnt sPCDetailEntItem in sPCMasterEnt.SPCDetailEntList)
        {
            LOG.Debug(String.Format("Page_Load({1}): sPCDetailEntItem.Description={0}", sPCDetailEntItem.Description, desc));
        }
    }
}
