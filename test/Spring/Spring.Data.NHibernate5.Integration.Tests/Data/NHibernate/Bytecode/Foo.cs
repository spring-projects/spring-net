namespace Spring.Data.NHibernate.Bytecode
{
    public class Foo
    {
        private string _description;

        public Foo(string description)
        {
            _description = description;
        }

        public virtual string Description
        {
            get { return _description; }
            set { _description = value; }
        }
    }
}
