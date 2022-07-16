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

using NHibernate;

namespace Spring.Web.Conversation
{
    /// <summary>
    /// manager for Conversations.
    /// </summary>
    /// <author>Hailton de Castro</author>
    public interface IConversationManager
    {
        /// <summary>
        /// Returns the conversation if it is still alive, otherwise it returns null.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IConversationState GetConversationById(String id);

        /// <summary>
        /// Ends all conversations with the timeout exceeded.
        /// </summary>
        void EndOnTimeOut();

        /// <summary>
        /// Close IDbConnections for <see cref="IConversationState"/> that 
        /// use 'session-per-conversation'. It calls 
        /// <see cref="IConversationState.PauseConversation"/> in all conversations.
        /// </summary>
        void PauseConversations();

        /// <summary>
        /// Release the ended conversations And removes them. 
        /// If the conversation supports 'session-per-conversation', also close the session.
        /// </summary>
        void FreeEnded();

        /// <summary>
        /// Add conversation. If  <see cref="IConversationManager"/> is null 
        /// it resolves to 'this'.
        /// </summary>
        /// <param name="conversation"></param>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="conversation"/> already has another manager.
        /// </exception>
        void AddConversation(IConversationState conversation);

        /// <summary>
        /// Makes the 'root conversation' of <paramref name="conversation"/> 
        /// the current active conversation and open/reopen the 
        /// <see cref="IConversationState.RootSessionPerConversation"/> if 
        /// the conversation supports 'session-per-conversation'. Close all 
        /// the connection for all session before.
        /// If <see cref="EndPaused"/> is <c>true</c> will end all 
        /// paused conversations.
        /// </summary>
        void SetActiveConversation(IConversationState conversation);

        /// <summary>
        /// Returns the active conversation if exists, otherwise returns null. 
        /// It depends on <see cref="SetActiveConversation"/>
        /// </summary>
        /// <returns></returns>
        IConversationState ActiveConversation{ get; }

        /// <summary>
        /// <para>If this is non-null run pattern 'session-per-conversation'. 
        /// Must be the same SessionFactory of the managed conversations.
        /// </para>
        /// </summary>
        ISessionFactory SessionFactory { get; }

        /// <summary>
        /// Ends the "paused conversations" in call to <see cref="ActiveConversation"/>. 
        /// Important: Unexpected behavior may occur if there are nested conversations, 
        /// as in <see cref="IConversationState.StartResumeConversation"/> only the current conversation and its parents 
        /// are started, the 'conversations children' remain paused, so these will be ended.
        /// Defaul value: <c>false</c>.
        /// </summary>
        /// <remarks>
        /// <para>When it is true, "start/resume a conversation" will cause the other to be 
        /// ended and cleaned up.
        /// </para>
        /// <para>This is useful to avoid memory leak where there are many conversations. 
        /// This leak can be very considerable, as the conversation may keep a "NHibernate session" 
        /// that can contain many objects in its cache from the database queries.
        /// </para>
        /// </remarks>
        bool EndPaused { get; set; }
    }
}
