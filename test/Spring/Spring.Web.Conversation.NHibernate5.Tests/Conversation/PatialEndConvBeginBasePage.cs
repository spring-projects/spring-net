#region License

/*
 * Copyright � 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using Spring.Web.UI;

namespace Spring.Web.Conversation
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
