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
using Spring.Data.NHibernate.Support;
using NHibernate;
using Spring.Entities;
using Spring.Spring.Data.Common;
using NUnit.Framework;
using Spring.Bsn;
using NHibernate.Impl;
using System.Reflection;
using NHibernate.Cfg;
using Spring.Context;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

/// <summary>
/// Page for <see cref="Spring.ConversationWA.WebConversationStateTest.SerializeConversationTest()"/>.
/// </summary>
public partial class SerializeConversationTest : System.Web.UI.Page
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

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            this.Conversation.StartResumeConversation();
            ISession ss = this.Conversation.SessionFactory.GetCurrentSession();
            IList<SPCDetailEnt> deatilList = ss.CreateCriteria<SPCDetailEnt>().List<SPCDetailEnt>();

            this.Conversation.ConversationManager.PauseConversations();

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();

            System.Collections.ArrayList sessionContent = new System.Collections.ArrayList();
            foreach (String keyItem in this.Session.Keys)
            {
                sessionContent.Add(this.Session[keyItem]);
            }

            bf.Serialize(ms, sessionContent);


            if (this.Session["SPCDetailEnt#1"] == null)
            {
                //at the first time
                this.Session["SPCDetailEnt#1"] = ss.Get<SPCDetailEnt>(1);
            }
            else
            {
                //at the second time
                if (!Object.ReferenceEquals(this.Session["SPCDetailEnt#1"], ss.Get<SPCDetailEnt>(1)))
                    throw new InvalidOperationException("!Object.ReferenceEquals(this.Session['SPCDetailEnt#1'], ss.Get<SPCDetailEnt>(1))");
            }

            Response.Clear();
            Response.Write("OK");
        }
        catch (Exception ex)
        {
            Response.Clear();
            Response.Write(ex.Message + " " + ex.StackTrace);
        }
    }
}
