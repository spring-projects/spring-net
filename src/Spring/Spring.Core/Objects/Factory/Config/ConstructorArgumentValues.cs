/*
 * Copyright © 2002-2011 the original author or authors.
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

using System.Collections;
using System.Globalization;
using Spring.Collections;
using Spring.Util;

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Holder for constructor argument values for an object.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Supports values for a specific index or parameter name (case
	/// insensitive) in the constructor argument list, and generic matches by
	/// <see cref="System.Type.FullName"/>.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	/// <see cref="Spring.Objects.Factory.Config.IObjectDefinition.ConstructorArgumentValues"/>
	[Serializable]
	public class ConstructorArgumentValues
	{
		private static readonly CultureInfo enUSCultureInfo = new CultureInfo("en-US", false);

		private static readonly IReadOnlyDictionary<int, ValueHolder> _emptyIndexedArgumentValues = new Dictionary<int, ValueHolder>();
		internal Dictionary<int, ValueHolder>  _indexedArgumentValues = null;

		private static readonly IReadOnlyList<ValueHolder> _emptyGenericArgumentValues = new List<ValueHolder>();
		internal List<ValueHolder> _genericArgumentValues = null;

		private static readonly IReadOnlyDictionary<string, ValueHolder> _emptyNamedArgumentValues = new Dictionary<string, ValueHolder>();
		internal Dictionary<string, ValueHolder> _namedArgumentValues = null;

		/// <summary>
		/// Can be used as an argument filler for the
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.GetArgumentValue(int, string,Type,ISet)"/>
		/// overload when one is not looking for an argument by index.
		/// </summary>
		public const int NoIndex = -1289; // yes, the number really is wholly arbitrary...

	    /// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues"/>
		/// class.
		/// </summary>
		public ConstructorArgumentValues()
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues"/>
		/// class.
		/// </summary>
		/// <param name="other">
		/// The <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues"/>
		/// to be used to populate this instance.
		/// </param>
		public ConstructorArgumentValues(ConstructorArgumentValues other)
		{
			AddAll(other);
		}

	    /// <summary>
		/// Return the map of indexed argument values.
		/// </summary>
		/// <returns>
		/// An <see cref="System.Collections.IDictionary"/> with
		/// <see cref="System.Int32"/> indices as keys and
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.ValueHolder"/>s
		/// as values.
		/// </returns>
		public IReadOnlyDictionary<int, ValueHolder> IndexedArgumentValues
		    => _indexedArgumentValues ?? _emptyIndexedArgumentValues;

		/// <summary>
		/// Return the map of named argument values.
		/// </summary>
		/// <returns>
		/// An <see cref="System.Collections.IDictionary"/> with
		/// <see cref="System.String"/> named arguments as keys and
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.ValueHolder"/>s
		/// as values.
		/// </returns>
		public IReadOnlyDictionary<string, ValueHolder> NamedArgumentValues => _namedArgumentValues ?? _emptyNamedArgumentValues;

		/// <summary>
		/// Return the set of generic argument values.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Collections.IList"/> of
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.ValueHolder"/>s.
		/// </returns>
        public IReadOnlyList<ValueHolder> GenericArgumentValues => _genericArgumentValues ?? _emptyGenericArgumentValues;

		/// <summary>
		/// Return the number of arguments held in this instance.
		/// </summary>
		public int ArgumentCount
		{
			get
			{
				int count = 0;
				if (_indexedArgumentValues != null)
				{
					count += _indexedArgumentValues.Count;
				}
				if (_genericArgumentValues != null)
				{
					count += _genericArgumentValues.Count;
				}
				if (_namedArgumentValues != null)
				{
					count += _namedArgumentValues.Count;
				}

				return count;
			}
		}

		/// <summary>
		/// Returns true if this holder does not contain any argument values,
		/// neither indexed ones nor generic ones.
		/// </summary>
		public bool Empty
		{
		    get
		    {
		        if (_indexedArgumentValues != null && _indexedArgumentValues.Count > 0)
		        {
		            return false;
		        }
		        if (_genericArgumentValues != null && _genericArgumentValues.Count > 0)
		        {
		            return false;
		        }
		        if (_namedArgumentValues != null && _namedArgumentValues.Count > 0)
		        {
		            return false;
		        }

		        return true;
		    }
		}

	    /// <summary>
		/// Copy all given argument values into this object.
		/// </summary>
		/// <param name="other">
		/// The <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues"/>
		/// to be used to populate this instance.
		/// </param>
		public void AddAll(ConstructorArgumentValues other)
		{
			if (other != null)
			{
				if (other._genericArgumentValues != null && other._genericArgumentValues.Count > 0)
				{
					GetAndInitializeGenericArgumentValuesIfNeeded().AddRange(other._genericArgumentValues);
				}

				if (other._indexedArgumentValues != null && other._indexedArgumentValues.Count > 0)
				{
					foreach (var entry in other._indexedArgumentValues)
					{
						ValueHolder vh = entry.Value;
						if (vh != null)
						{
							AddOrMergeIndexedArgumentValues(entry.Key, vh.Copy());
						}
					}
				}

				if (other._namedArgumentValues != null && other._namedArgumentValues.Count > 0)
				{
					foreach (var entry in other._namedArgumentValues)
					{
						AddOrMergeNamedArgumentValues(entry.Key, entry.Value);
						//NamedArgumentValues.Add(entry.Key, entry.Value);
					}
				}
			}
		}

	    private void AddOrMergeNamedArgumentValues(string key, ValueHolder newValue)
		{
			var namedArgumentValues = GetAndInitializeNamedArgumentValuesIfNeeded();
			namedArgumentValues[key] = newValue;
		}

		private void AddOrMergeIndexedArgumentValues(int key, ValueHolder newValue)
	    {
		    var dictionary = GetAndInitializeIndexedArgumentValuesIfNeeded();

		    if (newValue.Value is IMergable mergable
		        && dictionary.TryGetValue(key, out var currentValue))
            {
                if (mergable.MergeEnabled)
                {
                    newValue.Value = mergable.Merge(currentValue.Value);
                }
            }
		    dictionary[key] = newValue;
	    }

		/// <summary>
		/// Add argument value for the given index in the constructor argument list.
		/// </summary>
		/// <param name="index">
		/// The index in the constructor argument list.
		/// </param>
		/// <param name="value">
		/// The argument value.
		/// </param>
		public void AddIndexedArgumentValue(int index, object value)
		{
			GetAndInitializeIndexedArgumentValuesIfNeeded()[index] = new ValueHolder(value);
		}

		/// <summary>
		/// Add argument value for the given index in the constructor argument list.
		/// </summary>
		/// <param name="index">The index in the constructor argument list.</param>
		/// <param name="value">The argument value.</param>
		/// <param name="type">
		/// The <see cref="System.Type.FullName"/> of the argument
		/// <see cref="System.Type"/>.
		/// </param>
		public void AddIndexedArgumentValue(int index, object value, string type)
		{
			GetAndInitializeIndexedArgumentValuesIfNeeded()[index] = new ValueHolder(value, type);
		}

		/// <summary>
		/// Add argument value for the given name in the constructor argument list.
		/// </summary>
		/// <param name="name">The name in the constructor argument list.</param>
		/// <param name="value">The argument value.</param>
		/// <exception cref="ArgumentException">
		/// If the supplied <paramref name="name"/> is <see langword="null"/>
		/// or is composed wholly of whitespace.
		/// </exception>
		public void AddNamedArgumentValue(string name, object value)
		{
			AssertUtils.ArgumentHasText(name, "name");
			GetAndInitializeNamedArgumentValuesIfNeeded()[GetCanonicalNamedArgument(name)] = new ValueHolder(value);
		}

		/// <summary>
		/// Get argument value for the given index in the constructor argument list.
		/// </summary>
		/// <param name="index">The index in the constructor argument list.</param>
		/// <param name="requiredType">
		/// The required <see cref="System.Type"/> of the argument.
		/// </param>
		/// <returns>
		/// The
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.ValueHolder"/>
		/// for the argument, or <see langword="null"/> if none set.
		/// </returns>
		public ValueHolder GetIndexedArgumentValue(int index, Type requiredType)
		{
			ValueHolder valueHolder;
            if (IndexedArgumentValues.TryGetValue(index, out valueHolder))
			{
				if (valueHolder.Type == null
					|| requiredType.FullName.Equals(valueHolder.Type)
					|| requiredType.AssemblyQualifiedName.Equals(valueHolder.Type))
				{
					return valueHolder;
				}
			}
			return null;
		}

		/// <summary>
		/// Get argument value for the given name in the constructor argument list.
		/// </summary>
		/// <param name="name">The name in the constructor argument list.</param>
		/// <returns>
		/// The
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.ValueHolder"/>
		/// for the argument, or <see langword="null"/> if none set.
		/// </returns>
		public ValueHolder GetNamedArgumentValue(string name)
		{
			ValueHolder valueHolder = null;
			if (name != null && ContainsNamedArgument(name))
			{
				valueHolder = GetAndInitializeNamedArgumentValuesIfNeeded()[GetCanonicalNamedArgument(name)];
			}

			return valueHolder;
		}

		/// <summary>
		/// Does this set of constructor arguments contain a named argument matching the
		/// supplied <paramref name="argument"/> name?
		/// </summary>
		/// <remarks>
		/// <note>
		/// The comparison is performed in a case-insensitive fashion.
		/// </note>
		/// </remarks>
		/// <param name="argument">The named argument to look up.</param>
		/// <returns>
		/// <see langword="true"/> if this set of constructor arguments
		/// contains a named argument matching the supplied
		/// <paramref name="argument"/> name.
		/// </returns>
		public bool ContainsNamedArgument(string argument)
		{
			return _namedArgumentValues != null && _namedArgumentValues.ContainsKey(GetCanonicalNamedArgument(argument));
		}

		/// <summary>
		/// Add generic argument value to be matched by type.
		/// </summary>
		/// <param name="value">
		/// The argument value.
		/// </param>
		public void AddGenericArgumentValue(object value)
		{
			GetAndInitializeGenericArgumentValuesIfNeeded().Add(new ValueHolder(value));
		}

		/// <summary>
		/// Add generic argument value to be matched by type.
		/// </summary>
		/// <param name="value">The argument value.</param>
		/// <param name="type">
		/// The <see cref="System.Type.FullName"/> of the argument
		/// <see cref="System.Type"/>.
		/// </param>
		public void AddGenericArgumentValue(object value, string type)
		{
			GetAndInitializeGenericArgumentValuesIfNeeded().Add(new ValueHolder(value, type));
		}

		/// <summary>
		/// Look for a generic argument value that matches the given
		/// <see cref="System.Type"/>.
		/// </summary>
		/// <param name="requiredType">
		/// The <see cref="System.Type"/> to match.
		/// </param>
		/// <returns>
		/// The
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.ValueHolder"/>
		/// for the argument, or <see langword="null"/> if none set.
		/// </returns>
		public ValueHolder GetGenericArgumentValue(Type requiredType)
		{
			return GetGenericArgumentValue(requiredType, null);
		}

		/// <summary>
		/// Look for a generic argument value that matches the given
		/// <see cref="System.Type"/>.
		/// </summary>
		/// <param name="requiredType">
		/// The <see cref="System.Type"/> to match.
		/// </param>
		/// <param name="usedValues">
		/// A <see cref="Spring.Collections.ISet"/> of
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.ValueHolder"/>
		/// objects that have already been used in the current resolution
		/// process and should therefore not be returned again; this allows one
		/// to return the next generic argument match in the case of multiple
		/// generic argument values of the same type.
		/// </param>
		/// <returns>
		/// The
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.ValueHolder"/>
		/// for the argument, or <see langword="null"/> if none set.
		/// </returns>
		public ValueHolder GetGenericArgumentValue(
			Type requiredType,
			ISet usedValues)
		{
			if (_genericArgumentValues == null)
			{
				return null;
			}

			for (var i = 0; i < _genericArgumentValues.Count; i++)
			{
				ValueHolder valueHolder = _genericArgumentValues[i];
				if (usedValues == null || !usedValues.Contains(valueHolder))
				{
					if (requiredType != null)
					{
						if (StringUtils.HasText(valueHolder.Type))
						{
							if (valueHolder.Type.Equals(requiredType.FullName)
							    || valueHolder.Type.Equals(requiredType.AssemblyQualifiedName))
							{
								return valueHolder;
							}
						}
						else if (requiredType.IsInstanceOfType(valueHolder.Value)
						         || (requiredType.IsArray
						             && valueHolder.Value is IList))
						{
							return valueHolder;
						}
					}
					// if the value holder is (pretty much) untyped, that's ok to return...
					else if (StringUtils.IsNullOrEmpty(valueHolder.Type))
					{
						return valueHolder;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Look for an argument value that either corresponds to the given index
		/// in the constructor argument list or generically matches by
		/// <see cref="System.Type"/>.
		/// </summary>
		/// <param name="index">
		/// The index in the constructor argument list.
		/// </param>
		/// <param name="requiredType">
		/// The <see cref="System.Type"/> to match.
		/// </param>
		/// <returns>
		/// The
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.ValueHolder"/>
		/// for the argument, or <see langword="null"/> if none is set.
		/// </returns>
		public ValueHolder GetArgumentValue(int index, Type requiredType)
		{
			return GetArgumentValue(index, string.Empty, requiredType, null);
		}

		/// <summary>
		/// Look for an argument value that either corresponds to the given index
		/// in the constructor argument list or generically matches by
		/// <see cref="System.Type"/>.
		/// </summary>
		/// <param name="index">
		/// The index in the constructor argument list.
		/// </param>
		/// <param name="requiredType">
		/// The <see cref="System.Type"/> to match.
		/// </param>
		/// <param name="usedValues">
		/// A <see cref="Spring.Collections.ISet"/> of
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.ValueHolder"/>
		/// objects that have already been used in the current resolution
		/// process and should therefore not be returned again; this allows one
		/// to return the next generic argument match in the case of multiple
		/// generic argument values of the same type.
		/// </param>
		/// <returns>
		/// The
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.ValueHolder"/>
		/// for the argument, or <see langword="null"/> if none is set.
		/// </returns>
		public ValueHolder GetArgumentValue(int index, Type requiredType, ISet usedValues)
		{
			return GetArgumentValue(index, string.Empty, requiredType, usedValues);
		}

		/// <summary>
		/// Look for an argument value that either corresponds to the given index
		/// in the constructor argument list or generically matches by
		/// <see cref="System.Type"/>.
		/// </summary>
		/// <param name="name">
		/// The name of the argument in the constructor argument list. May be
		/// <see langword="null"/>, in which case generic matching by
		/// <see cref="System.Type"/> is assumed.
		/// </param>
		/// <param name="requiredType">
		/// The <see cref="System.Type"/> to match.
		/// </param>
		/// <returns>
		/// The
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.ValueHolder"/>
		/// for the argument, or <see langword="null"/> if none is set.
		/// </returns>
		public ValueHolder GetArgumentValue(string name, Type requiredType)
		{
			return GetArgumentValue(NoIndex, name, requiredType, null);
		}

		/// <summary>
		/// Look for an argument value that either corresponds to the given index
		/// in the constructor argument list or generically matches by
		/// <see cref="System.Type"/>.
		/// </summary>
		/// <param name="name">
		/// The name of the argument in the constructor argument list. May be
		/// <see langword="null"/>, in which case generic matching by
		/// <see cref="System.Type"/> is assumed.
		/// </param>
		/// <param name="requiredType">
		/// The <see cref="System.Type"/> to match.
		/// </param>
		/// <param name="usedValues">
		/// A <see cref="Spring.Collections.ISet"/> of
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.ValueHolder"/>
		/// objects that have already been used in the current resolution
		/// process and should therefore not be returned again; this allows one
		/// to return the next generic argument match in the case of multiple
		/// generic argument values of the same type.
		/// </param>
		/// <returns>
		/// The
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.ValueHolder"/>
		/// for the argument, or <see langword="null"/> if none is set.
		/// </returns>
		public ValueHolder GetArgumentValue(
			string name, Type requiredType, ISet usedValues)
		{
			return GetArgumentValue(NoIndex, name, requiredType, usedValues);
		}

		/// <summary>
		/// Look for an argument value that either corresponds to the given index
		/// in the constructor argument list, or to the named argument, or
		/// generically matches by <see cref="System.Type"/>.
		/// </summary>
		/// <param name="index">
		/// The index of the argument in the constructor argument list. May be
		/// negative, to denote the fact that we are not looking for an
		/// argument by index (see
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.NoIndex"/>.
		/// </param>
		/// <param name="name">
		/// The name of the argument in the constructor argument list. May be
		/// <see langword="null"/>.
		/// </param>
		/// <param name="requiredType">
		/// The <see cref="System.Type"/> to match.
		/// </param>
		/// <param name="usedValues">
		/// A <see cref="Spring.Collections.ISet"/> of
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.ValueHolder"/>
		/// objects that have already been used in the current resolution
		/// process and should therefore not be returned again; this allows one
		/// to return the next generic argument match in the case of multiple
		/// generic argument values of the same type.
		/// </param>
		/// <returns>
		/// The
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.ValueHolder"/>
		/// for the argument, or <see langword="null"/> if none is set.
		/// </returns>
		public ValueHolder GetArgumentValue(
			int index, string name, Type requiredType, ISet usedValues)
		{
			ValueHolder valueHolder = null;
			if(index != NoIndex)
			{
				valueHolder = GetIndexedArgumentValue(index, requiredType);
			}
			if (valueHolder == null)
			{
				valueHolder = GetNamedArgumentValue(name);
				if (valueHolder == null)
				{
					valueHolder = GetGenericArgumentValue(requiredType, usedValues);
				}
			}
			return valueHolder;
		}

		private string GetCanonicalNamedArgument(string argument)
		{
            return argument != null ? argument.ToLower(enUSCultureInfo) : argument;
		}

		private Dictionary<int, ValueHolder> GetAndInitializeIndexedArgumentValuesIfNeeded()
		{
			return _indexedArgumentValues = _indexedArgumentValues ?? new Dictionary<int, ValueHolder>();
		}

		private Dictionary<string, ValueHolder> GetAndInitializeNamedArgumentValuesIfNeeded()
		{
			return _namedArgumentValues = _namedArgumentValues ?? new Dictionary<string, ValueHolder>();
		}

		private List<ValueHolder> GetAndInitializeGenericArgumentValuesIfNeeded()
		{
			return _genericArgumentValues = _genericArgumentValues ?? new List<ValueHolder>();
		}

	    /// <summary>
		/// Holder for a constructor argument value, with an optional
		/// <see cref="System.Type"/> attribute indicating the target
		/// <see cref="System.Type"/> of the actual constructor argument.
		/// </summary>
		[Serializable]
		public class ValueHolder
		{
			private object _ctorValue;
			private readonly string typeName;

			/// <summary>
			/// Creates a new instance of the ValueHolder class.
			/// </summary>
			/// <param name="value">
			/// The value of the constructor argument.
			/// </param>
			internal ValueHolder(object value)
			{
				Value = value;
			}

			/// <summary>
			/// Creates a new instance of the ValueHolder class.
			/// </summary>
			/// <param name="value">
			/// The value of the constructor argument.
			/// </param>
			/// <param name="typeName">
			/// The <see cref="System.Type.FullName"/> of the argument
			/// <see cref="System.Type"/>. Can also be one of the common
			/// <see cref="System.Type"/> aliases (<c>int</c>, <c>bool</c>,
			/// <c>float</c>, etc).
			/// </param>
			internal ValueHolder(object value, string typeName)
			{
				Value = value;
				this.typeName = typeName;
			}

		    public ValueHolder Copy()
            {
                ValueHolder copy = new ValueHolder(_ctorValue, typeName);
                return copy;
            }

			/// <summary>
			/// A <see cref="System.String"/> that represents the current
			/// <see cref="System.Object"/>.
			/// </summary>
			/// <returns>
			/// A <see cref="System.String"/> that represents the current
			/// <see cref="System.Object"/>.
			/// </returns>
			public override string ToString()
			{
				return string.Format(CultureInfo.InvariantCulture,
				                     "'{0}' [{1}]", Value, Type);
			}

		    /// <summary>
			/// Gets and sets the value for the constructor argument.
			/// </summary>
			/// <remarks>
			/// <p>
			/// Only necessary for manipulating a registered value, for example in
			/// <see cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>s.
			/// </p>
			/// </remarks>
			public object Value
			{
				get => _ctorValue;
			    set => _ctorValue = value;
		    }

			/// <summary>
			/// Return the <see cref="System.Type.FullName"/> of the constructor
			/// argument.
			/// </summary>
			public string Type => typeName;
		}
	}
}
