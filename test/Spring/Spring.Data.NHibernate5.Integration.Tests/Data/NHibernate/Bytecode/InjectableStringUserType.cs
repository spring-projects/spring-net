using System.Data;
using System.Data.Common;
using NHibernate;
using NHibernate.Engine;
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

        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            string resultString = (string) NHibernateUtil.String.NullSafeGet(rs, names[0], session);
            if (resultString != null)
                return delimiter.Delimit(resultString);
            return null;
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            if (value == null)
            {
                NHibernateUtil.String.NullSafeSet(cmd, null, index, session);
                return;
            }

            value = delimiter.Delimit((string) value);

            NHibernateUtil.String.NullSafeSet(cmd, value, index, session);
        }

        public object DeepCopy(object value)
        {
            if (value == null) return null;
            return string.Copy((string) value);
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

        public SqlType[] SqlTypes => new[] {new SqlType(DbType.String)};

        public System.Type ReturnedType => typeof(string);

        public bool IsMutable => false;

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