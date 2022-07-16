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
using Spring.Data.Common;

namespace Spring.Web.Conversation
{
    /// <summary>
    /// Port to conversation. If the object is not found in the current
    /// conversation, will be tried on the parent if the parent is
    /// not null.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// If <see cref="Id"/> is different from spring name for this instance.
    /// </exception>
    /// <author>Hailton de Castro</author>
    public interface IConversationState: IDictionary<string, object>
    {
        /// <summary>
        /// Conversation id.
        /// </summary>
        String Id { get; set; }

        /// <summary>
        /// Starts or resumes the conversation and the <see cref="ParentConversation"/>.
        /// <para>If <see cref="RootSessionPerConversation"/> is not null, so
        /// <see cref="ISessionFactory.GetCurrentSession"/> is called to
        /// Raise SessionHolder for make the reconnection.
        /// </para>
        /// <para>Make <see cref="IsNew"/> return false.
        /// </para>
        /// <para>Update the <see cref="LastAccess"/>.
        /// </para>
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>If this conversation is ended.
        /// </item>
        /// <item>If <see cref="RootSessionPerConversation"/> is not null and
        /// <see cref="RootSessionPerConversation"/> different from
        /// <see cref="ISessionFactory.GetCurrentSession"/>
        /// </item>
        /// </list>
        /// </exception>
        void StartResumeConversation();

        /// <summary>
        /// Return true until <see cref="StartResumeConversation"/> is called.
        /// </summary>
        bool IsNew { get; }

        /// <summary>
        /// Ends the conversation. End each of the 'inner conversations' in
        /// <see cref="InnerConversations"/>. Returns false if the
        /// conversation and all <see cref="IConversationState"/> of
        /// <see cref="InnerConversations"/> has already been ended.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>If <see cref="System.Web.HttpContext.Current"/>.
        /// <see cref="System.Web.SessionState.HttpSessionState">Session</see>["spring.objects"]
        /// is null.
        /// </item>
        /// <item>The 'spring session scopes' are not located in the key
        /// 'spring.objects' of HttpSessionState.
        /// </item>
        /// </list>
        /// </exception>
        void EndConversation();

        /// <summary>
        /// Return true if this conversation is ended.
        /// </summary>
        bool Ended { get; }

        /// <summary>
        /// Inner conversation. After added if the <see cref="ParentConversation"/>
        /// is null it will resolve to 'this'.
        /// </summary>
        /// <exception cref="InvalidOperationException">at
        /// <see cref="T:System.Collections.Generic.ICollection`1.Add(T)"/>,
        /// <see cref="T:System.Collections.Generic.IList`1.this[int]"/>,
        /// <see cref="T:System.Collections.Generic.IList`1.Insert(int, T)"/>
        /// if Circular Dependency is detected.</exception>
        IList<IConversationState> InnerConversations { get; }

        /// <summary>
        /// Conversation parent.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>If this conversation already has a different parent.
        /// </item>
        /// <item>If this Conversation is not new.
        /// </item>
        /// <item>If Circular Dependency is detected.
        /// </item>
        /// <item>The Parent conversation is not new.
        /// </item>
        /// </list>
        /// </exception>
        IConversationState ParentConversation { get; set;}

        /// <summary>
        /// TimeOut for the conversation in milliseconds.
        /// If <c>0</c> TimeOut will be ignored.
        /// </summary>
        Int32 TimeOut { get; set; }

        /// <summary>
        /// Last acces for a value into this Conversation or Inner Conversation.
        /// Reset to DateTime.Now each time <see cref="StartResumeConversation()"/>
        /// is called.
        /// </summary>
        DateTime LastAccess { get; set; }

        /// <summary>
        /// Conversation Manager. When this is setted if
        /// <see cref="IConversationManager.GetConversationById(String)"/>
        /// returns null so AddConversation is called.
        /// </summary>
        IConversationManager ConversationManager { get; set; }

        /// <summary>
        /// <para><see cref="ISession"/> that is stored in the root conversation.
        /// </para>
        /// <para>
        /// <see cref="ConversationManager"/> must support 'session-per-conversation'.
        /// </para>
        /// </summary>
        ISession RootSessionPerConversation { get; set; }

        /// <summary>
        /// <para>If this is non-null run pattern 'session-per-conversation'.
        /// It also depends on <see cref="DbProvider"/> and <see cref="ConversationManager"/>.
        /// <see cref="ConversationManager"/> must support ConversationManager.
        /// </para>
        /// </summary>
        ISessionFactory SessionFactory { get; }

        /// <summary>
        /// <para>If this is non-null run pattern 'session-per-conversation'.
        /// It also depends on <see cref="SessionFactory"/> and <see cref="ConversationManager"/>.
        /// <see cref="ConversationManager"/> must support ConversationManager.
        /// </para>
        /// </summary>
        IDbProvider DbProvider { get; }

        /// <summary>
        /// Indicates that the conversation is paused.
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// Starts or resumes the conversation and each 'inner conversation' in
        /// <see cref="InnerConversations"/>.
        /// It is not about 'Session-per-conversation' because it is done by
        /// <see cref="IConversationManager"/>.
        /// </summary>
        void PauseConversation();
    }
}
