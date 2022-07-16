namespace Spring.Objects.Factory.Attributes
{
    /// <summary>
    /// This annotation may be used on a field or parameter as a qualifier for
    /// candidate beans when autowiring. It may also be used to annotate other
    /// custom annotations that can then in turn be used as qualifiers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class QualifierAttribute : Attribute
    {
        private readonly string _value;

        /// <summary>
        /// Instantiate a new qualifier with an empty name
        /// </summary>
        public QualifierAttribute()
        {
            _value = "";
        }

        /// <summary>
        /// Instantiate a new qualifier with a givin name
        /// </summary>
        /// <param name="value">name to use as qualifier</param>
        public QualifierAttribute(string value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the name associated with this qualifier
        /// </summary>
        public string Value { get { return _value; } }


        /// <summary>
        /// Checks weather the attribute is the same
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var o1 = obj as QualifierAttribute;
            if (_value != o1._value) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return _value != null ? _value.GetHashCode() : 0;
        }
    }
}
