using System.Collections;
using System.Globalization;
using Spring.Util;

namespace Spring.Collections
{
    /// <summary>
    /// Provides a performance improved hashtable with case-insensitive (string-only! based) key handling.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class CaseInsensitiveHashtable : Hashtable
    {
        private readonly TextInfo _textInfo;
        private readonly CompareInfo _compareInfo;

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
            :this(d, culture.TextInfo, culture.CompareInfo)
        {}

        /// <summary>
        /// Creates a case-insensitive hashtable using the given <see cref="CultureInfo"/>, initially
        /// populated with entries from another dictionary.
        /// </summary>
        protected CaseInsensitiveHashtable(IDictionary d, TextInfo textInfo, CompareInfo compareInfo)
        {
            AssertUtils.ArgumentNotNull(textInfo, "textInfo");
            _textInfo = textInfo;
            _compareInfo = compareInfo;

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
        /// Calculate the hashcode of the given string key, using the configured culture.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected override int GetHash(object key)
        {
            return _textInfo.ToLower((string) key).GetHashCode();
        }

        /// <summary>
        /// Compares two keys
        /// </summary>
        protected override bool KeyEquals(object item, object key)
        {
            return 0==_compareInfo.Compare((string) item, (string) key, CompareOptions.IgnoreCase);
        }

        /// <summary>
        /// Creates a shallow copy of the current instance.
        /// </summary>
        public override object Clone()
        {
            return new CaseInsensitiveHashtable(this, _textInfo, _compareInfo );
        }
    }
}
