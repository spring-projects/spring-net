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
	/// Partial IDictionary implementation which serves as a common base for final implementations.
	/// </summary>
	/// <author>Zbynek Vyskovsky, kvr@centrum.cz</author>
	public abstract class AbstractDictionary<TKey, TValue>: System.Collections.Generic.IDictionary<TKey, TValue>
	{
		public abstract void		Add(TKey key, TValue value);

		public abstract bool		ContainsKey(TKey key);

		public abstract bool		Remove(TKey key);

		public abstract bool		TryGetValue(TKey key, out TValue value);

		public abstract void		Clear();

		public bool			Contains(KeyValuePair<TKey, TValue> item)
		{
			TValue value;
			return TryGetValue(item.Key, out value) && (item.Value == null ? value == null : item.Value.Equals(value));
		}

		public void			CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
		}

		public abstract int		Count { get; }

		public bool			IsReadOnly
		{
			get {
				return false;
			}
		}

		public abstract bool		Remove(KeyValuePair<TKey, TValue> item);

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return EntriesSet().GetEnumerator();
		}

		System.Collections.IEnumerator	System.Collections.IEnumerable.GetEnumerator()
		{
			return EntriesSet().GetEnumerator();
		}

		public ICollection<TKey>	Keys
		{
			get {
				return EntriesSet().Select((KeyValuePair<TKey, TValue> entry) => entry.Key).ToList();
			}
		}

		public ICollection<TValue>	Values
		{
			get {
				return EntriesSet().Select((KeyValuePair<TKey, TValue> entry) => entry.Value).ToList();
			}
		}

		public TValue			this[TKey key]
		{
			get {
				TValue value;
				if (!TryGetValue(key, out value))
					throw new KeyNotFoundException("Key not found in map: "+key);
				return value;
			}
			set {
				Add(key, value);
			}
		}

		public void			Add(KeyValuePair<TKey, TValue> item)
		{
			Add(item.Key, item.Value);
		}

		protected abstract IEnumerable<KeyValuePair<TKey, TValue>> EntriesSet();
	}
}
