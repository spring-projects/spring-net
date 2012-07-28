using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Common.Logging;
using Iesi.Collections.Generic;

namespace Spring.ConversationWA.Imple
{
    /// <summary>
    /// List that make validation for Circular Dependence for <see cref="IConversationState"/>
    /// </summary>
    /// <author>Hailton de Castro</author>
    [Serializable]
    public class InnerConversationList: IList<IConversationState>, IList
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(InnerConversationList));

        private IConversationState conversationOwner;
        /// <summary>
        /// Contructor. 
        /// </summary>
        /// <param name="conversationOwner">The IConversation that owns this InnerConversationList.</param>
        public InnerConversationList(IConversationState conversationOwner)
        {
            if (conversationOwner == null)
            {
                String exMsgStr = "'conversationOwner' can not be null";

                LOG.Error(exMsgStr);
                throw new InvalidOperationException(exMsgStr);
            }
            if (LOG.IsDebugEnabled) LOG.Error(String.Format("Creating InnerConversationList for '{0}'", conversationOwner.Id));
            this.conversationOwner = conversationOwner;
        }

        private IList<IConversationState> innerList = new List<IConversationState>();

        /// <summary>
        /// Common Helper to be run before insert.
        /// </summary>
        /// <param name="itemAdded"></param>
        private void addPreAddHelper(IConversationState itemAdded)
        {
            if (LOG.IsDebugEnabled) LOG.Debug(String.Format("addPreAddHelper: added={0} into {1}", itemAdded, this.conversationOwner));
            this.validateCircularDependence(itemAdded);
            if (itemAdded.ParenteConversation != null && itemAdded.ParenteConversation != this.conversationOwner)
            {
                throw new InvalidOperationException(
                    String.Format(WebConversationSpringState.MSG_CONVERSATION_ALREADY_HAS_PARENT, itemAdded.ParenteConversation.Id, this.conversationOwner.Id));
            }
        }

        /// <summary>
        /// Common Helper to be run after insert.
        /// </summary>
        private void addPostAddHelper(IConversationState itemAdded)
        {
            if (LOG.IsDebugEnabled) LOG.Debug(String.Format("addPreAddHelper: added={0} into {1}", itemAdded, this.conversationOwner));
            if (itemAdded.ParenteConversation == null)
            {
                itemAdded.ParenteConversation = this.conversationOwner;
            }
        }

        private void validateCircularDependence(IConversationState itemAdded)
        {
            if (LOG.IsDebugEnabled) LOG.Debug(String.Format("Validating Circular Dependence: added={0} into {1}", itemAdded, this.conversationOwner));

            ICollection<IConversationState> visitedColl = new HashedSet<IConversationState>();
            visitedColl.Add(conversationOwner);

            IConversationState parentAncestor = this.conversationOwner;
            String path = this.conversationOwner.Id;
            //string conectorPath = "";

            this.validateCircularDependenceRecursive(itemAdded, visitedColl, path + "->" + itemAdded.Id);
        }

        private void validateCircularDependenceRecursive(IConversationState currentConv, ICollection<IConversationState> visitedColl, String path)
        {
            foreach (IConversationState convItem in currentConv.InnerConversations)
            {
                if (visitedColl.Contains(convItem))
                {
                    String exMsgStr =
                        "ConversationState Circular Dependence detected: " +
                        path + "->" + convItem.Id;

                    LOG.Error(exMsgStr);
                    throw new InvalidOperationException(exMsgStr);
                }

                visitedColl.Add(convItem);

                this.validateCircularDependenceRecursive(convItem, visitedColl, path + "->" + convItem.Id);
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
            this.addPreAddHelper(item);
            this.innerList.Insert(index, item);
            this.addPostAddHelper(item);
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
                this.addPreAddHelper(value);
                this.innerList[index] = value;
                this.addPostAddHelper(value);
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
            this.addPreAddHelper(item);
            this.innerList.Add(item);
            this.addPostAddHelper(item);
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
