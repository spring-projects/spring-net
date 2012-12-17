using System;
using System.Collections.Generic;
using System.Text;
using Spring.Web.UI;
using Spring.ConversationWA;

namespace Spring.ConversationWA
{
    /// <summary>
    /// Base class for test pages for test 
    /// <see cref="WebConversationStateTest.PatialEndConvTest()"/>.
    /// </summary>
    public class PatialEndConvBeginBasePage: Page
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

        /// <summary>
        /// Common Begin. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Page_Load(object sender, EventArgs e)
        {
            this.Conversation.StartResumeConversation();

            Session["ConversationStr"] = this.Conversation.ToString();
        }
    }
}
