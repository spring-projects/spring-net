using System;
using System.Collections.Generic;
using System.Text;
using NHibernate;

namespace Spring.ConversationWA
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
        /// Close IDbConnection's for <see cref="IConversationState"/> that 
        /// use 'session-per-conversation'. It calls 
        /// <see cref="IConversationState.PauseConversation"/> in all conversations.
        /// </summary>
        void PauseConversations();

        /// <summary>
        /// Release the ended conversatons And remove it. 
        /// If the conversation support 'session-per-conversation' close the session.
        /// </summary>
        void FreeEnded();

        /// <summary>
        /// Add conversation. If  <see cref="IConversationManager"/> is null 
        /// it is setted with 'this'.
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
        ISessionFactory SessionFactory { get; set; }

        /// <summary>
        /// Ends the "paused conversations" in call to <see cref="ActiveConversation"/>. 
        /// Important: Unexpected behavior may occur if there are nested conversations, 
        /// as in 'StartResumeConversation' only the own conversation and their parents 
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
