using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Spring.Web.Conversation;

/// <summary>
/// Base classe for page's that suport conversation.
/// </summary>
public class ConversationPage : Spring.Web.UI.Page
{
  private IConversationState conversation = null;
  /// <summary>
  /// Converastion exposed to pages.
  /// </summary>
  public IConversationState Conversation
  {
    get { return conversation; }
    set { conversation = value; }
  }

  /// <summary>
  /// Here start/resume conversation to reconnect the ISession.
  /// </summary>
  /// <param name="e"></param>
  protected override void OnInit(EventArgs e)
  {
    this.Conversation.StartResumeConversation();
    base.OnInit(e);
  }
}
