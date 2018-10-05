namespace Spring.Data.NHibernate.Bytecode
{
    public interface IDelimiter
    {
        string Delimit(string source);
    }

    public class ParenDelimiter: IDelimiter
    {
        public string Delimit(string source)
        {
            return !source.StartsWith("[") ? string.Format("[{0}]", source) : source;
        }
    }

    public class NoOpDelimiter: IDelimiter
    {
        public string Delimit(string source)
        {
            return source;
        }
    }
}
