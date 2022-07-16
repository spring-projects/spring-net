#region License

/*
 * Copyright © 2015-2015 the original author or authors.
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

namespace Spring.Collections.Generic
{
    /// <summary>
    /// IDictionary implementation which preserves the order of inserted items.
    /// </summary>
    /// <author>Zbynek Vyskovsky, kvr@centrum.cz</author>
    public class LinkedHashDictionary<TKey, TValue> : AbstractDictionary<TKey, TValue>
    {
        public override void Add(TKey key, TValue value)
        {
            Node node;
            if (items.TryGetValue(key, out node))
            {
                node.value = value;
            }
            else
            {
                node = new Node();
                node.key = key;
                node.value = value;
                if ((node.previousLinked = linkedTail) != null)
                    node.previousLinked.nextLinked = node;
                node.nextLinked = null;
                linkedTail = node;
                if (linkedHead == null)
                    linkedHead = node;
                items.Add(key, node);
            }
        }

        public override bool ContainsKey(TKey key)
        {
            return items.ContainsKey(key);
        }

        public override bool Remove(TKey key)
        {
            Node node;
            if (!items.TryGetValue(key, out node))
                return false;

            if (node.previousLinked != null)
            {
                node.previousLinked.nextLinked = node.nextLinked;
            }
            else
            {
                linkedHead = node.nextLinked;
            }

            if (node.nextLinked != null)
            {
                node.nextLinked.previousLinked = node.previousLinked;
            }
            else
            {
                linkedTail = node.previousLinked;
            }

            items.Remove(key);

            return true;
        }

        public override bool TryGetValue(TKey key, out TValue value)
        {
            Node node;
            if (!items.TryGetValue(key, out node))
            {
                value = default(TValue);
                return false;
            }
            value = node.value;
            return true;
        }

        public override void Clear()
        {
            items.Clear();
            linkedHead = null;
            linkedTail = null;
        }

        public override bool Remove(KeyValuePair<TKey, TValue> item)
        {
            Node node;
            if (!items.TryGetValue(item.Key, out node))
                return false;
            if (!node.value.Equals(item.Value))
                return false;
            return Remove(item.Key);
        }

        public override int Count
        {
            get { return items.Count; }
        }

        protected class Node
        {
            public TKey key;
            public TValue value;

            public Node previousLinked;
            public Node nextLinked;
        }

        protected override IEnumerable<KeyValuePair<TKey, TValue>> EntriesSet()
        {
            List<KeyValuePair<TKey, TValue>> entries = new List<KeyValuePair<TKey, TValue>>();
            for (Node node = linkedHead; node != null; node = node.nextLinked)
                entries.Add(new KeyValuePair<TKey, TValue>(node.key, node.value));
            return entries;
        }

        private Node linkedHead = null;
        private Node linkedTail = null;

        private Dictionary<TKey, Node> items = new Dictionary<TKey, Node>();
    }
}
