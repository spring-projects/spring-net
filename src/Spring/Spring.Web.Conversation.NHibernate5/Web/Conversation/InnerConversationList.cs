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

using System.Collections;
using Common.Logging;
using Spring.Collections.Generic;

namespace Spring.Web.Conversation
{
    /// <summary>
    /// List that make validation for Circular Dependency for <see cref="IConversationState"/>
    /// </summary>
    /// <author>Hailton de Castro</author>
    [Serializable]
    public class InnerConversationList: IList<IConversationState>, IList
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(InnerConversationList));

        private IConversationState conversationOwner;

        private IList<IConversationState> innerList = new List<IConversationState>();
        
        /// <summary>
        /// Contructor. 
        /// </summary>
        /// <param name="conversationOwner">The <see cref="IConversationState"/> that owns this <see cref="InnerConversationList"/>.</param>
        public InnerConversationList(IConversationState conversationOwner)
        {
            if (conversationOwner == null)
            {
                String message = "'conversationOwner' can not be null";

                LOG.Error(message);
                throw new InvalidOperationException(message);
            }
            if (LOG.IsDebugEnabled) LOG.Error(String.Format("Creating InnerConversationList for '{0}'", conversationOwner.Id));
            this.conversationOwner = conversationOwner;
        }

        /// <summary>
        /// Common Helper to be run before insert.
        /// </summary>
        /// <param name="itemAdded"></param>
        private void PreAddProcessor(IConversationState itemAdded)
        {
            if (LOG.IsDebugEnabled) LOG.Debug(String.Format("PreAddProcessor: added={0} into {1}", itemAdded, this.conversationOwner));
            this.ValidateCircularDependency(itemAdded);
            if (itemAdded.ParentConversation != null && itemAdded.ParentConversation != this.conversationOwner)
            {
                throw new InvalidOperationException(
                    String.Format(WebConversationSpringState.MSG_CONVERSATION_ALREADY_HAS_PARENT, itemAdded.ParentConversation.Id, this.conversationOwner.Id));
            }
        }

        /// <summary>
        /// Common Helper to be run after insert.
        /// </summary>
        private void PostAddProcessor(IConversationState itemAdded)
        {
            if (LOG.IsDebugEnabled) LOG.Debug(String.Format("PostAddProcessor: added={0} into {1}", itemAdded, this.conversationOwner));
            if (itemAdded.ParentConversation == null)
            {
                itemAdded.ParentConversation = this.conversationOwner;
            }
        }

        private void ValidateCircularDependency(IConversationState itemAdded)
        {
            if (LOG.IsDebugEnabled) LOG.Debug(String.Format("Validating Circular Dependency: added={0} into {1}", itemAdded, this.conversationOwner));

            ICollection<IConversationState> visitedColl = new HashedSet<IConversationState>();
            visitedColl.Add(conversationOwner);

            IConversationState parentAncestor = this.conversationOwner;
            String path = this.conversationOwner.Id;
            //string conectorPath = "";

            this.ValidateCircularDependencyRecursive(itemAdded, visitedColl, path + "->" + itemAdded.Id);
        }

        private void ValidateCircularDependencyRecursive(IConversationState currentConv, ICollection<IConversationState> visitedColl, String path)
        {
            foreach (IConversationState convItem in currentConv.InnerConversations)
            {
                if (visitedColl.Contains(convItem))
                {
                    String exMsgStr =
                        "ConversationState Circular Dependency detected: " +
                        path + "->" + convItem.Id;

                    LOG.Error(exMsgStr);
                    throw new InvalidOperationException(exMsgStr);
                }

                visitedColl.Add(convItem);

                this.ValidateCircularDependencyRecursive(convItem, visitedColl, path + "->" + convItem.Id);
            }
        }

        #region IList<IConversationState> Members
        
        /// <summary>
        /// <see cref="T:IList`1"/> 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(IConversationState item)
        {
            return this.innerList.IndexOf(item);
        }

        /// <summary>
        /// <see cref="T:IList`1"/> 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, IConversationState item)
        {
            this.PreAddProcessor(item);
            this.innerList.Insert(index, item);
            this.PostAddProcessor(item);
        }

        /// <summary>
        /// <see cref="T:IList`1"/> 
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            this.innerList.RemoveAt(index);
        }

        /// <summary>
        /// <see cref="T:IList`1"/> 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IConversationState this[int index]
        {
            get
            {
                return this.innerList[index];
            }
            set
            {
                this.PreAddProcessor(value);
                this.innerList[index] = value;
                this.PostAddProcessor(value);
            }
        }

        #endregion

        #region ICollection<IConversationState> Members
        /// <summary>
        /// <see cref="T:ICollection`1"/> 
        /// </summary>
        /// <param name="item"></param>
        public void Add(IConversationState item)
        {
            this.PreAddProcessor(item);
            this.innerList.Add(item);
            this.PostAddProcessor(item);
        }

        /// <summary>
        /// <see cref="T:ICollection`1"/> 
        /// </summary>
        public void Clear()
        {
            this.innerList.Clear();
        }

        /// <summary>
        /// <see cref="T:ICollection`1"/> 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(IConversationState item)
        {
            return this.innerList.Contains(item);
        }

        /// <summary>
        /// <see cref="T:ICollection`1"/> 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(IConversationState[] array, int arrayIndex)
        {
            this.innerList.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// <see cref="T:ICollection`1"/> 
        /// </summary>
        public int Count
        {
            get { return this.innerList.Count; }
        }

        /// <summary>
        /// <see cref="T:ICollection`1"/> 
        /// </summary>
        public bool IsReadOnly
        {
            get { return this.innerList.IsReadOnly; }
        }

        /// <summary>
        /// <see cref="T:ICollection`1"/> 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(IConversationState item)
        {
            return this.innerList.Remove(item);
        }

        #endregion

        #region IEnumerable<IConversationState> Members
        /// <summary>
        /// <see cref="T:IEnumerable`1"/> 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IConversationState> GetEnumerator()
        {
            return this.innerList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// <see cref="IEnumerable"/>
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.innerList.GetEnumerator();
        }

        #endregion

        #region IList Members
        /// <summary>
        /// <see cref="T:IList`1"/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Add(object value)
        {
            ((IList<IConversationState>)this).Add((IConversationState)value);
            return this.innerList.Count - 1;
        }

        /// <summary>
        /// <see cref="T:IList`1"/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(object value)
        {
            return ((IList<IConversationState>)this).Contains((IConversationState)value);
        }

        /// <summary>
        /// <see cref="T:IList`1"/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int IndexOf(object value)
        {
            return ((IList<IConversationState>)this).IndexOf((IConversationState)value);
        }

        /// <summary>
        /// <see cref="T:IList`1"/>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void Insert(int index, object value)
        {
            ((IList<IConversationState>)this).Insert(index, (IConversationState)value);
        }

        /// <summary>
        /// <see cref="T:IList`1"/>
        /// </summary>
        public bool IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        /// <see cref="T:IList`1"/>
        /// </summary>
        /// <param name="value"></param>
        public void Remove(object value)
        {
            ((IList<IConversationState>)this).Add((IConversationState)value);
        }

        /// <summary>
        /// <see cref="T:IList`1"/>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        object IList.this[int index]
        {
            get
            {
                return ((IList<IConversationState>)this)[index];
            }
            set
            {
                ((IList<IConversationState>)this)[index] = (IConversationState)value;
            }
        }

        #endregion

        #region ICollection Members
        /// <summary>
        /// <see cref="ICollection"/>
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        public void CopyTo(Array array, int index)
        {
            IConversationState[] convArr = new IConversationState[array.Length];
            ((IList<IConversationState>)this).CopyTo(convArr, index);
            convArr.CopyTo(array, index);
        }

        /// <summary>
        /// <see cref="ICollection"/>
        /// </summary>
        public bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// <see cref="ICollection"/>
        /// </summary>
        public object SyncRoot
        {
            get { return this; }
        }

        #endregion
    }
}
