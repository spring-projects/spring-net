using System;
using System.Text;

using Spring.Context;
using Spring.Web.Conversation;

using Common.Logging;

public partial class CircularDependenceTest : System.Web.UI.Page, IApplicationContextAware
{
    private static readonly ILog LOG = LogManager.GetLogger(typeof(CircularDependenceTest));

    protected void Page_Load(object sender, EventArgs e)
    {
        IConversationState convCircularDependenceTest_A = (IConversationState)this.applicationContext.GetObject("convCircularDependenceTest_A");
        IConversationState convCircularDependenceTest_A_A_A = (IConversationState)this.applicationContext.GetObject("convCircularDependenceTest_A_A_A");
        StringBuilder sbErrors = new StringBuilder();
        try
        {
            convCircularDependenceTest_A_A_A.InnerConversations.Add(convCircularDependenceTest_A);
            sbErrors.AppendLine("Circular was not detected. ");
        }
        catch (InvalidOperationException ioe)
        {
            LOG.Debug("SERVER SIDE ERROR", ioe);
            if (!ioe.Message.Contains("convCircularDependenceTest_A_A_A->convCircularDependenceTest_A->convCircularDependenceTest_A_A->convCircularDependenceTest_A_A_A"))
            {
                sbErrors.AppendLine(String.Format("Wrong CircularDependence message= '{0}'", ioe.Message));
            }
        }
        catch (Exception ex)
        {
            LOG.Error("SERVER SIDE ERROR", ex);
            sbErrors.AppendLine(String.Format("Unexpected Error: '{0}' \n {1}", ex.Message, ex.StackTrace));
        }

        Session["CircularDependenceTest"] = sbErrors.ToString();
    }

    #region IApplicationContextAware Members
    private IApplicationContext applicationContext;
    public IApplicationContext ApplicationContext
    {
        set { this.applicationContext = value; }
    }
    #endregion
}
