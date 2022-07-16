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

#region Imports

using System.Collections;
using System.ComponentModel;
using System.Text;
using Spring.Core.TypeConversion;

#endregion

namespace Spring.Util
{
	/// <summary>
	/// UniqueKey allows for generating keys unique to a type or particular instance and a partial name, 
	/// that can e.g. be used as keys in <see cref="Hashtable"/>.
	/// </summary>
	/// <example>
	/// // shows usage type-scoped keys
	/// UniqueKey classAKey = UniqueKey.GetTypeScoped(typeof(ClassA), "myKey");
	/// UniqueKey classBKey = UniqueKey.GetTypeScoped(typeof(ClassB), "myKey");
	/// 
	/// HttpContext.Current.Items.Add( classAKey, "some value unqiue for class A having key 'myKey'");
	/// object value = HttpContext.Current.Items[ UniqueKey.GetTypeScoped(typeof(ClassA), "myKey") ];
	/// Assert.AreEqual( "some value unique for class A having key 'myKey'", value);
	/// 
	/// HttpContext.Current.Items.Add( classBKey, "some value unqiue for class B having key 'myKey'");
	/// object value = HttpContext.Current.Items[ UniqueKey.GetTypeScoped(typeof(ClassB), "myKey") ];
	/// Assert.AreEqual( "some value unique for class B having key 'myKey'", value);
	/// </example>
    [Serializable]
    [TypeConverter(typeof(UniqueKeyConverter))]
    public sealed class UniqueKey : IEquatable<UniqueKey>
    {
        private readonly string _generatedKey;

        /// <summary>
        /// Initialize a new instance of <see cref="UniqueKey"/> from its string representation.  
        /// See <see cref="GetInstanceScoped"/> and See <see cref="GetTypeScoped"/> for details.
        /// </summary>
        /// <param name="key">The string representation of the new <see cref="UniqueKey"/> instance.</param>
        internal UniqueKey(string key)
        {
            AssertUtils.ArgumentNotNull(key, "key");
            _generatedKey = key;
        }

        /// <summary>
        /// Compares this instance to another.
        /// </summary>
        public bool Equals(UniqueKey uniqueKey)
        {
            if (uniqueKey == null) return false;
            return Equals(_generatedKey, uniqueKey._generatedKey);
        }

        /// <summary>
        /// Compares this instance to another.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as UniqueKey);
        }

        /// <summary>
        /// Returns the hash code for this key.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _generatedKey.GetHashCode();
        }

        /// <summary>
        /// Returns a string representation of this key.
        /// </summary>
        public override string ToString()
        {
            return _generatedKey;
        }

        /// <summary>
        /// Creates a new key instance unique to the given instance.
        /// </summary>
        /// <param name="instance">The instance the key shall be unique to</param>
        /// <param name="partialKey">The partial key to be made unique</param>
        /// <remarks>
        /// </remarks>
        /// <exception cref="ArgumentException">If <paramref name="instance"/> is of type <see cref="Type"/></exception>
        public static UniqueKey GetInstanceScoped(object instance, string partialKey)
        {
            if (instance is Type)
            {
                throw new ArgumentException(
                    "please use GetTypeScoped(Type,string) for creating type specific keys", "instance");
            }
            return new UniqueKey(GetInstanceScopedString(instance, partialKey));
        }

        /// <summary>
        /// Creates a new key instance unique to the given type.
        /// </summary>
        /// <param name="type">The type the key shall be unique to</param>
        /// <param name="partialKey">The partial key to be made unique</param>
        public static UniqueKey GetTypeScoped(Type type, string partialKey)
        {
            return new UniqueKey(GetTypeScopedString(type, partialKey));
        }

        /// <summary>
        /// Returns a key unique for the given instance.
        /// </summary>
        /// <param name="instance">The instance the key shall be unique to</param>
        /// <param name="partialKey">The partial key to be made unique</param>
        /// <returns>A key formatted as <i>typename[instance-id].partialkey</i></returns>
        public static string GetInstanceScopedString(object instance, string partialKey)
        {
            AssertUtils.ArgumentNotNull(instance, "instance");
            AssertUtils.ArgumentHasText(partialKey, "partialKey");

            if (instance is Type)
            {
                throw new ArgumentException(
                    "please use GetUniqueKey(Type,string) for creating type specific keys", "instance");
            }
            return GetUniqueKey(instance.GetType(), instance, partialKey);
        }

        /// <summary>
        /// Returns a key unique for the given type.
        /// </summary>
        /// <param name="type">The type the key shall be unique to</param>
        /// <param name="partialKey">The partial key to be made unique</param>
        /// <returns>A key formatted as <i>typename.partialkey</i></returns>
        public static string GetTypeScopedString(Type type, string partialKey)
        {
            AssertUtils.ArgumentNotNull(type, "type");
            AssertUtils.ArgumentHasText(partialKey, "partialKey");

            return GetUniqueKey(type, null, partialKey);
        }

        private static string GetUniqueKey(Type type, object instance, string partialKey)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(type.FullName);
            if (instance != null) sb.Append('[').Append(instance.GetHashCode()).Append(']');
            sb.Append('.').Append(partialKey);
            return sb.ToString();
        }
    }
}
