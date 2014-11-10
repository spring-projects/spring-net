using System;
using System.Reflection;

using Spring.Data.NHibernate.Support;
using Spring.Bsn;
using Spring.Entities;
using Spring.Spring.Data.Common;
using Spring.Context;
using Spring.Web.Conversation;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Impl;

using NUnit.Framework;

/// <summary>
/// Page for <see cref="Spring.Web.Conversation.WebConversationStateTest.ConnectionReleaseModeIssue()"/>.
/// </summary>
public partial class ConnectionReleaseModeIssue : System.Web.UI.Page, IApplicationContextAware
{
    private IConversationState conversation;

    /// <summary>
    /// <see cref="IConversationState"/>
    /// </summary>
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

    private IConnectionReleaseModeIssueBsn connectionReleaseModeIssueBsn;

    public IConnectionReleaseModeIssueBsn ConnectionReleaseModeIssueBsn
    {
        get { return connectionReleaseModeIssueBsn; }
        set { connectionReleaseModeIssueBsn = value; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            // Test with conversation and "connection.release_mode" 
            // "auto"(<see cref="ConnectionReleaseMode.AfterTransaction"/>).
            this.connection_release_mode_auto();

            // Test with NO conversation and "connection.release_mode" 
            // "auto"(<see cref="ConnectionReleaseMode.AfterTransaction"/>) 
            // on block within the scope of "transaction boundary".
            this.connection_release_mode_auto_transaction_boundary();

            // Test with NO conversation and "connection.release_mode" 
            // "auto"(<see cref="ConnectionReleaseMode.AfterTransaction"/>).
            this.connection_release_mode_auto_no_conversation();

            // Test with conversation and "connection.release_mode" 
            // "on_close"(<see cref="ConnectionReleaseMode.OnClose"/>).
            this.connection_release_mode_on_close();

            Session["result"] = "OK";
        }
        catch (Exception ex)
        {
            Session["result"] = ex.Message + " " + ex.StackTrace;
            //throw;
        }
    }

    /// <summary>
    /// Test with conversation and "connection.release_mode" 
    /// "auto"(<see cref="ConnectionReleaseMode.AfterTransaction"/>).
    /// </summary>
    /// <remarks>
    /// Here we can see that every statement causes a closing of the IDbConnection.
    /// </remarks>
    private void connection_release_mode_auto()
    {
        //with conversation and "connection.release_mode" "auto"(AfterTransaction)
        //forcing "auto" by reflection.
        Settings settings = ((SessionFactoryImpl) this.SessionFactory).Settings;
        ConnectionReleaseMode connReleaseModeOriginal = settings.ConnectionReleaseMode;
        this.setConnectionReleaseModeByReflection(settings, ConnectionReleaseMode.AfterTransaction);

        //((SessionFactoryImpl)this.sessionFactory).Settings.ConnectionReleaseMode = ConnectionReleaseMode.AfterTransaction;
        ConnectionCreationTrackingDbProvider.Count = 0;
        this.Conversation.StartResumeConversation();
        ISession sessionA = this.SessionFactory.GetCurrentSession();
        SPCDetailEnt detailEnt = sessionA.Get<SPCDetailEnt>(1);
        Assert.AreEqual(1, ConnectionCreationTrackingDbProvider.Count, "ConnectionCreationTrackingDbProvider.Count");

        SessionScopeSettings sessionScopeSettings = new SessionScopeSettings(this.sessionFactory);
        sessionScopeSettings.SingleSession = true;
        using (new SessionScope(sessionScopeSettings, false))
        {
            ISession sessionB = this.SessionFactory.GetCurrentSession();

            sessionB.Get<SPCMasterEnt>(1);
            Assert.AreEqual(2, ConnectionCreationTrackingDbProvider.Count, "ConnectionCreationTrackingDbProvider.Count");

            Assert.AreSame(sessionA, sessionB, "sessionA, sessionB");
        }

        SPCMasterEnt masterEnt2 = sessionA.Get<SPCMasterEnt>(2);
        Assert.AreEqual(3, ConnectionCreationTrackingDbProvider.Count, "ConnectionCreationTrackingDbProvider.Count");
        Assert.AreEqual(1, masterEnt2.SPCDetailEntList.Count, "masterEnt2.SPCDetailEntList.Count");
        Assert.AreEqual(4, ConnectionCreationTrackingDbProvider.Count, "ConnectionCreationTrackingDbProvider.Count");

        SPCMasterEnt masterEnt3 = sessionA.Get<SPCMasterEnt>(3);
        Assert.AreEqual(5, ConnectionCreationTrackingDbProvider.Count, "ConnectionCreationTrackingDbProvider.Count");
        Assert.AreEqual(1, masterEnt3.SPCDetailEntList.Count, "masterEnt3.SPCDetailEntList.Count");
        Assert.AreEqual(6, ConnectionCreationTrackingDbProvider.Count, "ConnectionCreationTrackingDbProvider.Count");

        //Renew the conversation.
        this.Conversation.EndConversation();
        this.Conversation.ConversationManager.FreeEnded();
        this.Conversation = (IConversationState) this.applicationContext.GetObject("convConnectionReleaseModeIssue");

        this.setConnectionReleaseModeByReflection(settings, connReleaseModeOriginal);
    }

    /// <summary>
    /// Test with NO conversation and "connection.release_mode" 
    /// "auto"(<see cref="ConnectionReleaseMode.AfterTransaction"/>) 
    /// on block within the scope of "transaction boundary".
    /// </summary>
    /// <remarks>
    /// Here we can see that only a IDbConnection is open, even 
    /// with the execution of various statements. This is because 
    /// the statements are made within a "transaction boundary".
    /// </remarks>
    private void connection_release_mode_auto_transaction_boundary()
    {
        //with conversation and "connection.release_mode" "auto"(AfterTransaction)
        //forcing "auto" by reflection.
        Settings settings = ((SessionFactoryImpl) this.SessionFactory).Settings;
        ConnectionReleaseMode connReleaseModeOriginal = settings.ConnectionReleaseMode;
        this.setConnectionReleaseModeByReflection(settings, ConnectionReleaseMode.AfterTransaction);

        ConnectionCreationTrackingDbProvider.Count = 0;
        this.ConnectionReleaseModeIssueBsn.Test();

        this.setConnectionReleaseModeByReflection(settings, connReleaseModeOriginal);
    }

    /// <summary>
    /// Test with NO conversation and "connection.release_mode" 
    /// "auto"(<see cref="ConnectionReleaseMode.AfterTransaction"/>).
    /// </summary>
    private void connection_release_mode_auto_no_conversation()
    {
        //with NO conversation and "connection.release_mode" "auto"(AfterTransaction)
        //forcing "auto" by reflection.
        Settings settings = ((SessionFactoryImpl) this.SessionFactory).Settings;
        ConnectionReleaseMode connReleaseModeOriginal = settings.ConnectionReleaseMode;
        this.setConnectionReleaseModeByReflection(settings, ConnectionReleaseMode.AfterTransaction);

        //with no conversation
        SessionScopeSettings sessionScopeSettings = new SessionScopeSettings(this.sessionFactory);
        ConnectionCreationTrackingDbProvider.Count = 0;
        using (new SessionScope(sessionScopeSettings, true))
        {
            ISession sessionNoConv = this.SessionFactory.GetCurrentSession();
            SPCMasterEnt masterEnt2 = sessionNoConv.Get<SPCMasterEnt>(2);
            Assert.AreEqual(1, ConnectionCreationTrackingDbProvider.Count, "ConnectionCreationTrackingDbProvider.Count");
            Assert.AreEqual(1, masterEnt2.SPCDetailEntList.Count, "masterEnt2.SPCDetailEntList.Count");
            Assert.AreEqual(2, ConnectionCreationTrackingDbProvider.Count, "ConnectionCreationTrackingDbProvider.Count");

            SPCMasterEnt masterEnt3 = sessionNoConv.Get<SPCMasterEnt>(3);
            Assert.AreEqual(3, ConnectionCreationTrackingDbProvider.Count, "ConnectionCreationTrackingDbProvider.Count");
            Assert.AreEqual(1, masterEnt3.SPCDetailEntList.Count, "masterEnt3.SPCDetailEntList.Count");
            Assert.AreEqual(4, ConnectionCreationTrackingDbProvider.Count, "ConnectionCreationTrackingDbProvider.Count");
        }

        this.setConnectionReleaseModeByReflection(settings, connReleaseModeOriginal);
    }

    /// <summary>
    /// Test with conversation and "connection.release_mode" 
    /// "on_close"(<see cref="ConnectionReleaseMode.OnClose"/>).
    /// </summary>
    /// <remarks>
    /// Here we can see that only a IDbConnection is open, even 
    /// with the execution of various statements.
    /// </remarks>
    private void connection_release_mode_on_close()
    {
        //forcing "on_close" by reflection.
        Settings settings = ((SessionFactoryImpl) this.SessionFactory).Settings;
        ConnectionReleaseMode connReleaseModeOriginal = settings.ConnectionReleaseMode;
        this.setConnectionReleaseModeByReflection(settings, ConnectionReleaseMode.OnClose);

        //with conversation and "connection.release_mode" "on_close"(AfterTransaction)
        ConnectionCreationTrackingDbProvider.Count = 0;
        this.Conversation.StartResumeConversation();
        ISession sessionA = this.SessionFactory.GetCurrentSession();
        SPCDetailEnt detailEnt = sessionA.Get<SPCDetailEnt>(1);
        Assert.AreEqual(1, ConnectionCreationTrackingDbProvider.Count, "ConnectionCreationTrackingDbProvider.Count");

        SessionScopeSettings sessionScopeSettings = new SessionScopeSettings(this.sessionFactory);
        sessionScopeSettings.SingleSession = true;
        SPCMasterEnt masterEnt;
        using (new SessionScope(sessionScopeSettings, false))
        {
            ISession sessionB = this.SessionFactory.GetCurrentSession();

            masterEnt = sessionB.Get<SPCMasterEnt>(1);

            Assert.AreSame(sessionA, sessionB, "sessionA, sessionB");
        }

        Assert.AreEqual(3, masterEnt.SPCDetailEntList.Count, "masterEnt.SPCDetailEntList.Count");

        Assert.AreEqual(1, ConnectionCreationTrackingDbProvider.Count, "ConnectionCreationTrackingDbProvider.Count");

        //Renew the conversation.
        this.Conversation.EndConversation();
        this.Conversation.ConversationManager.FreeEnded();
        this.Conversation = (IConversationState) this.applicationContext.GetObject("convConnectionReleaseModeIssue");

        this.setConnectionReleaseModeByReflection(settings, connReleaseModeOriginal);
    }

    /// <summary>
    /// Sets the <see cref="Settings.ConnectionReleaseMode"/> by reflection.
    /// </summary>
    /// <param name="settings">The settings.</param>
    /// <param name="mode">The mode.</param>
    private void setConnectionReleaseModeByReflection(Settings settings, ConnectionReleaseMode mode)
    {
        PropertyInfo pInfoConnectionReleaseMode =
            settings.GetType().GetProperty(
                "ConnectionReleaseMode",
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.SetProperty |
                BindingFlags.Instance);

        pInfoConnectionReleaseMode.SetValue(settings, mode, null);
    }

    #region IApplicationContextAware Members

    private IApplicationContext applicationContext;

    public IApplicationContext ApplicationContext
    {
        set { this.applicationContext = value; }
    }

    #endregion
}