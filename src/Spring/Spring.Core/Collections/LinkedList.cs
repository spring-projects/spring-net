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

namespace Spring.Collections
{
	/// <summary>
	/// Simple linked list implementation.
	/// </summary>
	/// <author>Simon White</author>
    [Serializable]
    public class LinkedList : IList
	{
		private int _nodeIndex;
		private Node _rootNode;
		private int _modId;

		#region Constructors

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Collections.LinkedList"/> class.
		/// </summary>
		public LinkedList()
		{
			_rootNode = new Node(null, null, null);
			_rootNode.PreviousNode = _rootNode;
			_rootNode.NextNode = _rootNode;
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Collections.LinkedList"/> class that contains all
		///  elements of the specified list.
		/// </summary>
		/// <param name="list">
		/// A list of elements that defines the initial contents.
		/// </param>
		public LinkedList(IList list) : this()
		{
			AddAll(list);
		}

		#endregion

		#region IList Members

		/// <summary>
		/// Is list read only?
		/// </summary>
		/// <value>
		/// <see langword="true"/> if the list is read only.
		/// </value>
		public bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Returns the node at the specified index.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is the indexer for the
		/// <see cref="Spring.Collections.LinkedList"/> class.
		/// </p>
		/// </remarks>
		/// <seealso cref="GetNode(int)"/>
		public object this[int index]
		{
			get { return GetNode(index).Value; }
			set { GetNode(index).Value = value; }
		}

		/// <summary>
		/// Removes the object at the specified index.
		/// </summary>
		/// <param name="index">The lookup index.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// If the specified <paramref name="index"/> is greater than the
		/// number of objects within the list.
		/// </exception>
		public void RemoveAt(int index)
		{
			CheckUpdateState();
			RemoveNode(GetNode(index));
		}

		/// <summary>
		/// Inserts an object at the specified index.
		/// </summary>
		/// <param name="index">The lookup index.</param>
		/// <param name="value">The object to be inserted.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// If the specified <paramref name="index"/> is greater than the
		/// number of objects within the list.
		/// </exception>
		public void Insert(int index, object value)
		{
			CheckUpdateState();

			Node node = null;
			if (index == _nodeIndex)
			{
				node = new Node(value, _rootNode.PreviousNode, _rootNode);
			}
			else
			{
				Node insert = GetNode(index);
				node = new Node(value, insert.PreviousNode, insert);
			}
			node.PreviousNode.NextNode = node;
			node.NextNode.PreviousNode = node;
			_nodeIndex++;
			_modId++;
		}

		/// <summary>
		/// Removes the first instance of the specified object found.
		/// </summary>
		/// <param name="value">The object to remove</param>
		public void Remove(object value)
		{
			CheckUpdateState();
			NodeHolder nh = GetNode(value);
			RemoveNode(nh.Node);
		}

		/// <summary>
		/// Returns <see langword="true"/> if this list contains the specified
		/// element.
		/// </summary>
		/// <param name="value">The element to look for.</param>
		/// <returns>
		/// <see langword="true"/> if this list contains the specified element.
		/// </returns>
		public bool Contains(object value)
		{
			return GetNode(value) != null;
		}

		/// <summary>
		/// Removes all objects from the list.
		/// </summary>
		public void Clear()
		{
			_rootNode = new Node(null, null, null);
			_nodeIndex = 0;
			_modId++;
		}

		/// <summary>
		/// Returns the index of the first instance of the specified
		/// <paramref name="value"/> found.
		/// </summary>
		/// <param name="value">The object to search for</param>
		/// <returns>
		/// The index of the first instance found, or -1 if the element was not
		/// found.
		/// </returns>
		public int IndexOf(object value)
		{
			NodeHolder nh = GetNode(value);
			if (nh == null)
			{
				return -1;
			}
			return nh.Index;
		}

		/// <summary>
		/// Adds the specified object to the end of the list.
		/// </summary>
		/// <param name="value">The object to add</param>
		/// <returns>The index that the object was added at.</returns>
		public int Add(object value)
		{
			Insert(_nodeIndex, value);
			return _nodeIndex - 1;
		}

		/// <summary>
		/// Adds all of the elements of the supplied
		/// <paramref name="elements"/>list to the end of this list.
		/// </summary>
		/// <param name="elements">The list of objects to add.</param>
		public void AddAll(IList elements)
		{
			foreach (object obj in elements)
			{
				Add(obj);
			}
		}

		/// <summary>
		/// Is the list a fixed size?
		/// </summary>
		/// <value>
		/// <see langword="true"/> if the list is a fixed size list.
		/// </value>
		public bool IsFixedSize
		{
			get { return false; }
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Checks whether the list can be modified.
		/// </summary>
		/// <exception cref="System.NotSupportedException">
		/// If the list cannot be modified.
		/// </exception>
		private void CheckUpdateState()
		{
			if (IsReadOnly || IsFixedSize)
			{
				throw new NotSupportedException("LinkedList cannot be modified.");
			}
		}

		/// <summary>
		/// Validates the specified index.
		/// </summary>
		/// <param name="index">The lookup index.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// If the index is invalid.
		/// </exception>
		private void ValidateIndex(int index)
		{
			if (index < 0 || index >= _nodeIndex)
			{
				throw new ArgumentOutOfRangeException("index");
			}
		}

		/// <summary>
		/// Returns the node at the specified index.
		/// </summary>
		/// <param name="index">The lookup index.</param>
		/// <returns>The node at the specified index.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// If the specified <paramref name="index"/> is greater than the
		/// number of objects within the list.
		/// </exception>
		private Node GetNode(int index)
		{
			ValidateIndex(index);
			Node node = _rootNode;
			for (int i = 0; i <= index; i++)
			{
				node = node.NextNode;
			}
			return node;
		}

		/// <summary>
		/// Returns the node (and index) of the first node that contains
		/// the specified value.
		/// </summary>
		/// <param name="value">The value to search for.</param>
		/// <returns>
		/// The node, or <see langword="null"/> if not found.
		/// </returns>
		private NodeHolder GetNode(object value)
		{
            int i = 0;
            if (value == null)
            {

                for (Node n = _rootNode.NextNode; n != _rootNode; n = n.NextNode)
                {
                    if (n.Value == null)
                    {
                        return new NodeHolder(n, i);
                    }
                    i++;
                }
            }
            else
            {

                for (Node n = _rootNode.NextNode; n != _rootNode; n = n.NextNode)
                {
                    if (value.Equals(n.Value))
                    {
                        return new NodeHolder(n, i);
                    }
                    i++;
                }
            }
			return null;
		}

		/// <summary>
		/// Removes the specified node.
		/// </summary>
		/// <param name="node">The node to be removed.</param>
		private void RemoveNode(Node node)
		{
			Node previousNode = node.PreviousNode;
			previousNode.NextNode = node.NextNode;
			node.NextNode.PreviousNode = previousNode;
			node.PreviousNode = null;
			node.NextNode = null;
			_nodeIndex--;
			_modId++;
		}

		#endregion

		#region ICollection Members

		/// <summary>
		/// Returns <see langword="true"/> if the list is synchronized across
		/// threads.
		/// </summary>
		/// <remarks>
		/// <note>
		/// This implementation <b>always</b> returns <see langword="false"/>.
		/// </note>
		/// <p>
		/// Note that enumeration is inherently not thread-safe. Use the
		/// <see cref="SyncRoot"/> to lock the object during enumeration.
		/// </p>
		/// </remarks>
		public bool IsSynchronized
		{
			get { return false; }
		}

		/// <summary>
		/// The number of objects within the list.
		/// </summary>
		public int Count
		{
			get { return _nodeIndex; }
		}

		/// <summary>
		/// Copies the elements in this list to an array.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The type of array needs to be compatible with the objects in this
		/// list, obviously.
		/// </p>
		/// </remarks>
		/// <param name="array">
		/// An array that will be the target of the copy operation.
		/// </param>
		/// <param name="index">
		/// The zero-based index where copying will start.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="array"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// If the supplied <paramref name="index"/> is less than zero
		/// or is greater than the length of <paramref name="array"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// If the supplied <paramref name="array"/> is of insufficient size.
		/// </exception>
		public void CopyTo(Array array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if ((index < 0) || (index > array.Length))
			{
				throw new ArgumentOutOfRangeException("index", String.Format("Index {0} is out of range.", index));
			}
			if ((array.Length - index) < this._nodeIndex)
			{
				throw new ArgumentException("Array is of insufficient size.");
			}

			Node node = this._rootNode;
			for (int i = 0, pos = index; i < this._nodeIndex; i++, pos++)
			{
				node = node.NextNode;
				array.SetValue(node.Value, pos);
			}
		}

		/// <summary>
		/// An object that can be used to synchronize this
		/// <see cref="Spring.Collections.LinkedList"/> to make it thread-safe.
		/// </summary>
		/// <value>
		/// An object that can be used to synchronize this
		/// <see cref="Spring.Collections.LinkedList"/> to make it thread-safe.
		/// </value>
		public object SyncRoot
		{
			get { return this; }
		}

		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Gets an enumerator for the elements in the
		/// <see cref="Spring.Collections.LinkedList"/>.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Enumerators are fail fast.
		/// </p>
		/// </remarks>
		/// <returns>
		/// An <see cref="System.Collections.IEnumerator"/> over the elements
		/// in the <see cref="Spring.Collections.LinkedList"/>.
		/// </returns>
		public IEnumerator GetEnumerator()
		{
			return new LinkedListEnumerator(this);
		}

		#endregion

		#region Inner Classes

        [Serializable]
        private class Node
		{
			private object _value;
			private Node _next;
			private Node _previous;

			public object Value
			{
				get { return _value; }
				set { _value = value; }
			}

			public Node NextNode
			{
				get { return _next; }
				set { this._next = value; }
			}

			public Node PreviousNode
			{
				get { return _previous; }
				set { this._previous = value; }
			}

			public Node(object val, Node previous, Node next)
			{
				this._value = val;
				this._next = next;
				this._previous = previous;
			}
		}

		private class NodeHolder
		{
			private int _index;
			private Node _node;

			public int Index
			{
				get { return _index; }
			}

			public Node Node
			{
				get { return _node; }
			}

			public NodeHolder(Node node, int index)
			{
				this._node = node;
				this._index = index;
			}
		}

		private class LinkedListEnumerator : IEnumerator
		{
			private LinkedList _ll;
			private Node _current;
			private int _modId;

			public object Current
			{
				get { return _current.Value; }
			}

			public LinkedListEnumerator(LinkedList ll)
			{
				this._ll = ll;
				this._modId = ll._modId;
				this._current = _ll._rootNode;
			}

			public bool MoveNext()
			{
				if (this._modId != this._ll._modId)
				{
					throw new InvalidOperationException("LinkedList has been modified.");
				}

				_current = _current.NextNode;
				return (_current == _ll._rootNode ? false : true);
			}

			public void Reset()
			{
				_current = _ll._rootNode;
			}
		}

		#endregion
	}
}
