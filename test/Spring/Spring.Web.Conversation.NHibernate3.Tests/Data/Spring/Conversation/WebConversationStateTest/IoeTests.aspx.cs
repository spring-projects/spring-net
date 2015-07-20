using System;

using Spring.Web.Conversation;
using System.Text.RegularExpressions;
using NHibernate;
using Spring.Data.NHibernate.Support;
using Common.Logging;
using Spring.Objects.Factory;

public partial class IoeTests : System.Web.UI.Page
{
    private static readonly ILog LOG = LogManager.GetLogger(typeof(IoeTests));

    private IConversationState conversationA;
    public IConversationState ConversationA
    {
        get { return conversationA; }
        set { conversationA = value; }
    }
    private IConversationState conversationAA;
    public IConversationState ConversationAA
    {
        get { return conversationAA; }
        set { conversationAA = value; }
    }

    private IConversationManager conversationManager;
    public IConversationManager ConversationManager
    {
        get { return conversationManager; }
        set { conversationManager = value; }
    }

    private ISessionFactory sessionFactory;
    public ISessionFactory SessionFactory
    {
        get { return sessionFactory; }
        set { sessionFactory = value; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if ("reset".Equals(this.Request.Params["test"]))
            {
                this.ConversationA.EndConversation();
                this.Session["testResult"] = "OK";
            }
            else if ("alreadyHasAnotherManagerNotRaise".Equals(this.Request.Params["test"]))
            {
                this.alreadyHasAnotherManagerNotRaise();
            }
            else if ("alreadyHasAnotherManagerRaise".Equals(this.Request.Params["test"]))
            {
                this.alreadyHasAnotherManagerRaise();
            }
            else if ("conversationAlreadyDifferentParent".Equals(this.Request.Params["test"]))
            {
                this.conversationAlreadyDifferentParent();
            }
            else if ("setParentConversationIsNotNew".Equals(this.Request.Params["test"]))
            {
                this.setParentConversationIsNotNew();
            }
            else if ("startResumeConversationIsEnded".Equals(this.Request.Params["test"]))
            {
                this.startResumeConversationIsEnded();
            }
            else if ("participatingHibernateNotAlowed".Equals(this.Request.Params["test"]))
            {
                this.participatingHibernateNotAlowed();
            }
            else if ("idIsDifferentFromSpringName".Equals(this.Request.Params["test"]))
            {
                this.idIsDifferentFromSpringName();
            }
            else
            {
                throw new Exception("'test' request parameter was not recognized");
            }
        }
        catch (Exception ex)
        {
            this.Session["testResult"] = "FAIL: " + ex.ToString();
        }
    }

    private void alreadyHasAnotherManagerNotRaise()
    {
        IConversationManager otherConversationManager = new WebConversationManager();

        //Not raise error
        IConversationState otherConversationState = null;
        try
        {
            otherConversationState = new WebConversationSpringState();
            otherConversationState.Id = "otherConversationState";
            otherConversationManager.AddConversation(otherConversationState);
            this.Session["testResult"] = "OK";
        }
        catch (InvalidOperationException ioe)
        {
            this.Session["testResult"] = "NOT OK " + ioe.ToString();
        }
        finally
        {
            otherConversationState.EndConversation();
        }
    }

    private void alreadyHasAnotherManagerRaise()
    {
        Regex msgErrorRx = new Regex(".*already.*has.*another.*manager.*");
        IConversationManager otherConversationManager = null;

        //Raise error
        try
        {
            otherConversationManager = new WebConversationManager();
            otherConversationManager.AddConversation(this.ConversationA);
            this.Session["testResult"] = "NOT OK";
        }
        catch (InvalidOperationException ioe)
        {
            if (msgErrorRx.IsMatch(ioe.Message))
            {
                this.Session["testResult"] = "OK";
            }
            else
            {
                this.Session["testResult"] = "NOT OK " + ioe.Message;
            }
        }
    }

    private void conversationAlreadyDifferentParent()
    {
        Regex msgErrorRx = new Regex(".*conversation.*already.*different.*parent.*");
        //BEGIN: Raise error
        IConversationState otherConversationState = null;
        try
        {
            otherConversationState = new WebConversationSpringState();
            otherConversationState.Id = "otherConversationState";
            //try first by 'InnerConversations.Add'
            otherConversationState.InnerConversations.Add(this.ConversationAA);
            throw new Exception("NOT OK: No raise for 'InnerConversations.Add(this.ConversationAA)'");
        }
        catch (InvalidOperationException ioe)
        {
            if (msgErrorRx.IsMatch(ioe.Message))
            {
                this.Session["testResult"] = "OK";
            }
            else
            {
                throw new Exception("NOT OK " + ioe.Message);
            }
        }
        finally
        {
            otherConversationState.EndConversation();
        }

        try
        {
            otherConversationState = new WebConversationSpringState();
            otherConversationState.Id = "otherConversationState";
            //try second by 'ParentConversation = '
            this.ConversationAA.ParentConversation = otherConversationState;
            throw new Exception("NOT OK: No raise for 'this.ConversationAA.ParentConversation = otherConversationState'");
        }
        catch (InvalidOperationException ioe)
        {
            if (msgErrorRx.IsMatch(ioe.Message))
            {
                this.Session["testResult"] = "OK";
            }
            else
            {
                throw new Exception("NOT OK, 'ex.Message' not match :" + ioe.Message);
            }
        }
        finally
        {
            otherConversationState.EndConversation();
        }
        //END: Raise error

        //BEGIN: NO Raise error
        otherConversationState = null;
        try
        {
            otherConversationState = new WebConversationSpringState();
            otherConversationState.Id = "otherConversationState";
            //try first by 'InnerConversations.Add'
            this.ConversationAA.InnerConversations.Add(otherConversationState);
            this.Session["testResult"] = "OK";
        }
        finally
        {
            otherConversationState.EndConversation();
        }

        try
        {
            otherConversationState = new WebConversationSpringState();
            otherConversationState.Id = "otherConversationState";
            //try second by 'ParentConversation = '
            otherConversationState.ParentConversation = this.ConversationA;
            this.Session["testResult"] = "OK";
        }
        finally
        {
            otherConversationState.EndConversation();
        }
        //END: NO Raise error
    }

    private void setParentConversationIsNotNew()
    {
        Regex msgErrorRx = new Regex(".*Conversation.*not.*new.*Conversation.Id.*Parent.*Tried.*");
        IConversationState otherConversationState = null;
        //BEGIN: Raise error
        try
        {
            otherConversationState = new WebConversationSpringState();
            otherConversationState.Id = "otherConversationState";
            // make not new
            otherConversationState.StartResumeConversation();
            //try first by 'InnerConversations.Add'
            this.ConversationA.InnerConversations.Add(otherConversationState);
            throw new Exception("NOT OK: No raise for 'this.ConversationA.InnerConversations.Add(otherConversationState)'");
        }
        catch (InvalidOperationException ioe)
        {
            if (msgErrorRx.IsMatch(ioe.Message))
            {
                this.Session["testResult"] = "OK";
            }
            else
            {
                throw new Exception("NOT OK " + ioe.Message);
            }
        }
        finally
        {
            otherConversationState.EndConversation();
        }

        try
        {
            otherConversationState = new WebConversationSpringState();
            otherConversationState.Id = "otherConversationState";
            // make not new
            otherConversationState.StartResumeConversation();
            //try second by 'ParentConversation = '
            otherConversationState.ParentConversation = this.ConversationA;
            throw new Exception("NOT OK: No raise for 'this.ConversationAA.ParentConversation = otherConversationState'");
        }
        catch (InvalidOperationException ioe)
        {
            if (msgErrorRx.IsMatch(ioe.Message))
            {
                this.Session["testResult"] = "OK";
            }
            else
            {
                throw new Exception("NOT OK, 'ex.Message' not match :" + ioe.Message);
            }
        }
        finally
        {
            otherConversationState.EndConversation();
        }
        //END: Raise error

        //BEGIN: NO Raise error
        try
        {
            otherConversationState = new WebConversationSpringState();
            otherConversationState.Id = "otherConversationState";
            // leave new
            //try first by 'InnerConversations.Add'
            this.ConversationA.InnerConversations.Add(otherConversationState);
            this.Session["testResult"] = "OK";
        }
        finally
        {
            otherConversationState.EndConversation();
        }

        try
        {
            otherConversationState = new WebConversationSpringState();
            otherConversationState.Id = "otherConversationState";
            // leave new
            //try second by 'ParentConversation = '
            otherConversationState.ParentConversation = this.ConversationA;
            this.Session["testResult"] = "OK";
        }
        finally
        {
            otherConversationState.EndConversation();
        }
        //END: NO Raise error
    }

    private void startResumeConversationIsEnded()
    {
        Regex msgErrorRx = new Regex(".*StartResumeConversation.*conversation.*is.*ended.*");
        IConversationState otherConversationState = null;
        //BEGIN: Raise error
        try
        {
            otherConversationState = new WebConversationSpringState();
            otherConversationState.Id = "otherConversationState";
            // make not new
            otherConversationState.EndConversation();
            otherConversationState.StartResumeConversation();
            throw new Exception("NOT OK: No raise for 'otherConversationState.StartResumeConversation()'");
        }
        catch (InvalidOperationException ioe)
        {
            if (msgErrorRx.IsMatch(ioe.Message))
            {
                this.Session["testResult"] = "OK";
            }
            else
            {
                throw new Exception("NOT OK " + ioe.Message);
            }
        }
        finally
        {
            otherConversationState.EndConversation();
        }
        //END: Raise error

        //BEGIN: NO Raise error
        try
        {
            otherConversationState = new WebConversationSpringState();
            otherConversationState.Id = "otherConversationState";
            // make not new
            otherConversationState.StartResumeConversation();
            otherConversationState.EndConversation();
            this.Session["testResult"] = "OK";
        }
        finally
        {
            otherConversationState.EndConversation();
        }
        //END: NO Raise error
    }
    
    private void participatingHibernateNotAlowed()
    {
        Regex msgErrorRx = new Regex(".*Participating.*Hibernate.*NOT.*ALLOWED.*");
        try
        {
            //No raise
            this.ConversationA.StartResumeConversation();
            ISession session = this.SessionFactory.GetCurrentSession();
            this.ConversationManager.FreeEnded();
            this.ConversationManager.PauseConversations();
            this.Session["testResult"] = "OK";
        }
        finally
        {
            this.ConversationManager.FreeEnded();
            this.ConversationManager.PauseConversations();
        }

        try
        {
            //Raise
            this.ConversationA.StartResumeConversation();
            this.ConversationManager.FreeEnded();
            this.ConversationManager.PauseConversations();

            SessionScopeSettings sessionScopeSettings = new SessionScopeSettings(this.sessionFactory);
            sessionScopeSettings.SingleSession = true;
            using (new SessionScope(sessionScopeSettings, true))
            {
                ISession session = this.SessionFactory.GetCurrentSession();
                this.ConversationA.StartResumeConversation();
            }
            throw new Exception("NOT OK: No raise for 'this.ConversationA.StartResumeConversation()'");
        }
        catch (InvalidOperationException ioe)
        {
            if (msgErrorRx.IsMatch(ioe.Message))
            {
                this.Session["testResult"] = "OK";
            }
            else
            {
                throw new Exception("NOT OK " + ioe.Message);
            }
        }
        finally
        {
            this.ConversationManager.FreeEnded();
            this.ConversationManager.PauseConversations();
        }
        
    }

    private void idIsDifferentFromSpringName()
    {
        Regex msgErrorRx = new Regex(".*Id.*is.*different.*from.*spring.*name.*");
        IConversationState otherConversationState = null;
        //BEGIN: Raise error
        try
        {
            otherConversationState = new WebConversationSpringState();
            otherConversationState.Id = "otherConversationState";
            // different name
            ((IObjectNameAware)otherConversationState).ObjectName = "different_name";
            throw new Exception("NOT OK: No raise for '((IObjectNameAware)otherConversationState).ObjectName ='");
        }
        catch (InvalidOperationException ioe)
        {
            if (msgErrorRx.IsMatch(ioe.Message))
            {
                this.Session["testResult"] = "OK";
            }
            else
            {
                throw new Exception("NOT OK " + ioe.Message);
            }
        }
        finally
        {
            otherConversationState.EndConversation();
        }
        //END: Raise error

        //BEGIN: NO Raise error
        try
        {
            otherConversationState = new WebConversationSpringState();
            otherConversationState.Id = "otherConversationState";
            // make not new
            // different name
            ((IObjectNameAware)otherConversationState).ObjectName = "otherConversationState";
            this.Session["testResult"] = "OK";
        }
        finally
        {
            otherConversationState.EndConversation();
        }
        //END: NO Raise error
    }
}