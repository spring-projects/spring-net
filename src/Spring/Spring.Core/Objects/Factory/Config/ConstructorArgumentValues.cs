#region License

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

#endregion

#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Spring.Collections;
using Spring.Util;

#endregion

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
		/// <summary>
		/// Can be used as an argument filler for the
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.GetArgumentValue(int, string,Type,ISet)"/>
		/// overload when one is not looking for an argument by index.
		/// </summary>
		public const int NoIndex = -1289; // yes, the number really is wholly arbitrary...

		#region Constructor (s) / Destructor

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

		#endregion

		#region Fields

        private CultureInfo enUSCultureInfo = new CultureInfo("en-US", false);
		private IDictionary<int, ValueHolder>  _indexedArgumentValues = new Dictionary<int, ValueHolder>();
        private List<ValueHolder> _genericArgumentValues = new List<ValueHolder>();
		private IDictionary<string, object> _namedArgumentValues = new Dictionary<string, object>();

		#endregion

		#region Properties

		/// <summary>
		/// Return the map of indexed argument values.
		/// </summary>
		/// <returns>
		/// An <see cref="System.Collections.IDictionary"/> with
		/// <see cref="System.Int32"/> indices as keys and
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.ValueHolder"/>s
		/// as values.
		/// </returns>
		public virtual IDictionary<int, ValueHolder> IndexedArgumentValues
		{
			get { return _indexedArgumentValues; }
		}

		/// <summary>
		/// Return the map of named argument values.
		/// </summary>
		/// <returns>
		/// An <see cref="System.Collections.IDictionary"/> with
		/// <see cref="System.String"/> named arguments as keys and
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.ValueHolder"/>s
		/// as values.
		/// </returns>
		public virtual IDictionary<string, object> NamedArgumentValues
		{
			get { return _namedArgumentValues; }
		}

		/// <summary>
		/// Return the set of generic argument values.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Collections.IList"/> of
		/// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues.ValueHolder"/>s.
		/// </returns>
        public virtual IList<ValueHolder> GenericArgumentValues
		{
			get { return _genericArgumentValues; }

		}

		/// <summary>
		/// Return the number of arguments held in this instance.
		/// </summary>
		public virtual int ArgumentCount
		{
			get
			{
				return IndexedArgumentValues.Count
					+ GenericArgumentValues.Count
					+ NamedArgumentValues.Count;
			}

		}

		/// <summary>
		/// Returns true if this holder does not contain any argument values,
		/// neither indexed ones nor generic ones.
		/// </summary>
		public virtual bool Empty
		{
			get
			{
				return IndexedArgumentValues.Count == 0
					&& GenericArgumentValues.Count == 0
					&& NamedArgumentValues.Count == 0;
			}
		}

		#endregion

		#region Methods

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
				foreach (ValueHolder o in other.GenericArgumentValues)
				{
					GenericArgumentValues.Add(o);
				}
				foreach (KeyValuePair<int, ValueHolder> entry in other.IndexedArgumentValues)
				{
				    ValueHolder vh = entry.Value;
                    if (vh != null)
                    {
                        AddOrMergeIndexedArgumentValues( entry.Key, vh.Copy());
                    }
				}
				foreach (KeyValuePair<string, object> entry in other.NamedArgumentValues)
				{
				    AddOrMergeNamedArgumentValues(entry.Key, entry.Value);
					//NamedArgumentValues.Add(entry.Key, entry.Value);
				}
			}
		}

	    private void AddOrMergeNamedArgumentValues(string key, object newValue)
	    {
	        if (_namedArgumentValues.ContainsKey(key) )
	        {
	            _namedArgumentValues[key] = newValue;
	        } else
	        {	            
                _namedArgumentValues.Add(key, newValue);
	        }
	    }

	    private void AddOrMergeIndexedArgumentValues(int key, ValueHolder newValue)
	    {
	        ValueHolder currentValue;
	        IMergable mergable = newValue.Value as IMergable;
            if (_indexedArgumentValues.TryGetValue(key, out currentValue) && mergable != null )
            {
                if (mergable.MergeEnabled)
                {
                    newValue.Value = mergable.Merge(currentValue.Value);
                }
            }
	        _indexedArgumentValues[key] = newValue;
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
		public virtual void AddIndexedArgumentValue(int index, object value)
		{
			IndexedArgumentValues[index] = new ValueHolder(value);
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
		public virtual void AddIndexedArgumentValue(int index, object value, string type)
		{
			IndexedArgumentValues[index] = new ValueHolder(value, type);
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
		public virtual void AddNamedArgumentValue(string name, object value)
		{
			AssertUtils.ArgumentHasText(name, "name");
			NamedArgumentValues[GetCanonicalNamedArgument(name)] = new ValueHolder(value);
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
		public virtual ValueHolder GetIndexedArgumentValue(int index, Type requiredType)
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
        public virtual ValueHolder GetNamedArgumentValue(string name)
        {
            ValueHolder valueHolder = null;
            if (name != null && ContainsNamedArgument(name))
            {
                valueHolder = (ValueHolder)NamedArgumentValues[GetCanonicalNamedArgument(name)];
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
			return NamedArgumentValues.ContainsKey(GetCanonicalNamedArgument(argument));
		}

		/// <summary>
		/// Add generic argument value to be matched by type.
		/// </summary>
		/// <param name="value">
		/// The argument value.
		/// </param>
		public virtual void AddGenericArgumentValue(object value)
		{
			GenericArgumentValues.Add(new ValueHolder(value));
		}

		/// <summary>
		/// Add generic argument value to be matched by type.
		/// </summary>
		/// <param name="value">The argument value.</param>
		/// <param name="type">
		/// The <see cref="System.Type.FullName"/> of the argument
		/// <see cref="System.Type"/>.
		/// </param>
		public virtual void AddGenericArgumentValue(object value, string type)
		{
			GenericArgumentValues.Add(new ValueHolder(value, type));
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
		public virtual ValueHolder GetGenericArgumentValue(Type requiredType)
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
		public virtual ValueHolder GetGenericArgumentValue(
			Type requiredType, ISet usedValues)
		{
			foreach (ValueHolder valueHolder in GenericArgumentValues)
			{
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
								&& typeof (IList).IsInstanceOfType(valueHolder.Value)))
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
		public virtual ValueHolder GetArgumentValue(int index, Type requiredType)
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
		public virtual ValueHolder GetArgumentValue(int index, Type requiredType, ISet usedValues)
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
		public virtual ValueHolder GetArgumentValue(string name, Type requiredType)
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
		public virtual ValueHolder GetArgumentValue(
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
		public virtual ValueHolder GetArgumentValue(
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

	    #endregion

		#region Inner Class : ValueHolder

		/// <summary>
		/// Holder for a constructor argument value, with an optional
		/// <see cref="System.Type"/> attribute indicating the target
		/// <see cref="System.Type"/> of the actual constructor argument.
		/// </summary>
		[Serializable]
		public class ValueHolder
		{
			#region Constructor (s) / Destructor

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

			#endregion

			#region Methods

            public ValueHolder Copy()
            {
                ValueHolder copy = new ValueHolder(this._ctorValue, this.typeName);
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

			#endregion

			#region Properties

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
				get { return _ctorValue; }
				set { _ctorValue = value; }
			}

			/// <summary>
			/// Return the <see cref="System.Type.FullName"/> of the constructor
			/// argument.
			/// </summary>
			public string Type
			{
				get { return typeName; }
			}

			#endregion

			#region Fields

			private object _ctorValue;
			private string typeName;

			#endregion
		}

		#endregion
	}
}