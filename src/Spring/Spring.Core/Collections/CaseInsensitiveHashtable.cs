using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;
using Spring.Util;

namespace Spring.Collections
{
    /// <summary>
    /// Provides a performance improved hashtable with case-insensitive (string-only! based) key handling.
    /// </summary>
    /// <author>Erich Eichinger</author>
    [Serializable]
    public class CaseInsensitiveHashtable : Hashtable
    {
        private readonly CultureInfo _culture;

        /// <summary>
        /// Creates a case-insensitive hashtable using <see cref="CultureInfo.CurrentCulture"/>.
        /// </summary>
        public CaseInsensitiveHashtable()
            : this(null, CultureInfo.CurrentCulture)
        {}

        /// <summary>
        /// Creates a case-insensitive hashtable using the given <see cref="CultureInfo"/>.
        /// </summary>
        /// <param name="culture">the <see cref="CultureInfo"/> to calculate the hashcode</param>
        public CaseInsensitiveHashtable(CultureInfo culture)
            : this(null, culture)
        {}

        /// <summary>
        /// Creates a case-insensitive hashtable using the given <see cref="CultureInfo"/>, initially
        /// populated with entries from another dictionary.
        /// </summary>
        /// <param name="d">the dictionary to copy entries from</param>
        /// <param name="culture">the <see cref="CultureInfo"/> to calculate the hashcode</param>
        public CaseInsensitiveHashtable(IDictionary d, CultureInfo culture)
        {
            AssertUtils.ArgumentNotNull(culture, "culture");
            _culture = culture;

            if (d != null)
            {
                IDictionaryEnumerator enumerator = d.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    this.Add(enumerator.Key, enumerator.Value);
                }
            }            
        }

        /// <summary>
        /// Initializes a new, empty instance of the <see cref="T:System.Collections.Hashtable"></see> class that is serializable using the specified <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> and <see cref="T:System.Runtime.Serialization.StreamingContext"></see> objects.
        /// </summary>
        /// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext"></see> object containing the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Hashtable"></see>. </param>
        /// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> object containing the information required to serialize the <see cref="T:System.Collections.Hashtable"></see> object.</param>
        /// <exception cref="T:System.ArgumentNullException">info is null. </exception>
        protected CaseInsensitiveHashtable(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            string cultureName = info.GetString("_cultureName");
            _culture = new CultureInfo(cultureName);
            AssertUtils.ArgumentNotNull(_culture, "Culture");
        }

        ///<summary>
        ///Implements the <see cref="ISerializable"></see> interface and returns the data needed to serialize the <see cref="CaseInsensitiveHashtable"></see>.
        ///</summary>
        ///
        ///<param name="context">A <see cref="StreamingContext"></see> object containing the source and destination of the serialized stream associated with the <see cref="CaseInsensitiveHashtable"></see>. </param>
        ///<param name="info">A <see cref="SerializationInfo"></see> object containing the information required to serialize the <see cref="CaseInsensitiveHashtable"></see>. </param>
        ///<exception cref="System.ArgumentNullException">info is null. </exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_cultureName", _culture.Name);
        }

        /// <summary>
        /// Calculate the hashcode of the given string key, using the configured culture.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected override int GetHash(object key)
        {
            if (!(key is string)) return key.GetHashCode();
            return _culture.TextInfo.ToLower((string) key).GetHashCode();
        }

        /// <summary>
        /// Compares two keys
        /// </summary>
        protected override bool KeyEquals(object item, object key)
        {
            if (!(key is string))
            {
                return Equals(item,key);
            }
            return 0==_culture.CompareInfo.Compare((string) item, (string) key, CompareOptions.IgnoreCase);
        }

        /// <summary>
        /// Creates a shallow copy of the current instance.
        /// </summary>
        public override object Clone()
        {
            return new CaseInsensitiveHashtable(this, _culture );
        }
    }
}
