using NHibernate.Bytecode;

using Spring.Data.NHibernate;
using Spring.Data.NHibernate.Bytecode;

namespace Spring.Northwind.Dao.NHibernate
{
    /// <summary>
    /// A custom version of <see cref="LocalSessionFactoryObject" /> that sets 
    /// bytecode provider to be Spring.NET's <see cref="BytecodeProvider" />.
    /// </summary>
    public class CustomLocalSessionFactoryObject : LocalSessionFactoryObject
    {
        /// <summary>
        /// Overwritten to return Spring's bytecode provider for entity injection to work.
        /// </summary>
        public override IBytecodeProvider BytecodeProvider
        {
            get { return new BytecodeProvider(ApplicationContext); }
            set { }
        }
    }
}