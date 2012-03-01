using System.Data;

using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace Spring.Data.NHibernate.Bytecode
{
    public class InjectableStringUserType : IUserType
    {
        private readonly IDelimiter delimiter;

        public InjectableStringUserType(IDelimiter delimiter)
        {
            this.delimiter = delimiter;
        }

        public object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            string resultString = (string)NHibernateUtil.String.NullSafeGet(rs, names[0]);
            if (resultString != null)
                return delimiter.Delimit(resultString);
            return null;
        }

        public void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            if (value == null)
            {
                NHibernateUtil.String.NullSafeSet(cmd, null, index);
                return;
            }

            value = delimiter.Delimit((string)value);

            NHibernateUtil.String.NullSafeSet(cmd, value, index);
        }

        public object DeepCopy(object value)
        {
            if (value == null) return null;
            return string.Copy((string)value);
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public object Assemble(object cached, object owner)
        {
            return DeepCopy(cached);
        }

        public object Disassemble(object value)
        {
            return DeepCopy(value);
        }

        public SqlType[] SqlTypes
        {
            get
            {
                return new SqlType[] { new SqlType(DbType.String) };
            }
        }

        public System.Type ReturnedType
        {
            get { return typeof(string); }
        }

        public bool IsMutable
        {
            get { return false; }
        }

        public new bool Equals(object x, object y)
        {
            if (x == null || y == null) return false;
            return x.Equals(y);
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }
    }
}
