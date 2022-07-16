using System.Data;
using Spring.Data.Common;
using Spring.Data.Core;
using Spring.Testing.Ado;

namespace Spring.Testing.NUnit
{
    /// <summary>
    /// Subclass of AbstractTransactionalSpringContextTests that adds some convenience
    /// functionality for ADO.NET access. Expects a IDbProvider object
    /// to be defined in the Spring application context.
    /// </summary>
    /// <remarks>
    /// This class exposes a AdoTemplate and provides an easy way to delete from the
    /// database in a new transaction.
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public abstract class AbstractTransactionalDbProviderSpringContextTests : AbstractTransactionalSpringContextTests
    {
        /// <summary>
        /// Holds the <see cref="AdoTemplate"/> that this base class manages
        /// </summary>
        private AdoTemplate adoTemplate;

        /// <summary>
        /// Did this test delete any tables? If so, we forbid transaction completion,
        /// and only allow rollback.
        /// </summary>
        private bool zappedTables;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractTransactionalDbProviderSpringContextTests"/> class.
        /// </summary>
        public AbstractTransactionalDbProviderSpringContextTests()
        {
        }

        /// <summary>
        /// Sets the DbProvider, via Dependency Injection.
        /// </summary>
        /// <value>The IDbProvider.</value>
        public IDbProvider DbProvider
        {
            set { adoTemplate = new AdoTemplate(value); }
        }

        /// <summary>
        /// Gets or sets the AdoTemplate that this base class manages.
        /// </summary>
        /// <value>The ADO template.</value>
        public AdoTemplate AdoTemplate
        {
            get { return adoTemplate; }
            protected set { adoTemplate = value; }
        }

        /// <summary>
        /// Inject dependencies into 'this' instance (that is, this test instance).
        /// </summary>
        /// <remarks>
        /// <p>The default implementation populates protected variables if the
        /// <see cref="AbstractDependencyInjectionSpringContextTests.PopulateProtectedVariables"/> property is set, else
        /// uses autowiring if autowiring is switched on (which it is by default).</p>
        /// <p>You can certainly override this method if you want to totally control
        /// how dependencies are injected into 'this' instance.</p>
        /// <p>AdoTemplate autowiring is Ignored</p>
        /// </remarks>
        protected override void InjectDependencies()
        {
            applicationContext.ObjectFactory.IgnoreDependencyType(typeof(AdoTemplate));
            base.InjectDependencies();
        }

        /// <summary>
        /// Convenient method to delete all rows from these tables.
        /// Calling this method will make avoidance of rollback by calling
        /// SetComplete() impossible.
        /// </summary>
        /// <param name="names"></param>
        protected void DeleteFromTables(String[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                int rowCount = this.adoTemplate.ExecuteNonQuery(CommandType.Text, "DELETE FROM " + names[i]);
                if (logger.IsInfoEnabled)
                {
                    logger.Info("Deleted " + rowCount + " rows from table " + names[i]);
                }
            }
            this.zappedTables = true;
        }

        /// <summary>
        /// Overridden to prevent the transaction committing if a number of tables have been
        /// cleared, as a defensive measure against accidental <i>permanent</i> wiping of a database.
        /// </summary>
        protected override void SetComplete()
        {
            if (this.zappedTables)
            {
                throw new InvalidOperationException("Cannot set complete after deleting tables");
            }
            base.SetComplete();
        }

        /// <summary>
        /// Counts the rows in given table.
        /// </summary>
        /// <param name="tableName">Name of the table to count rows in.</param>
        /// <returns>The number of rows in the table</returns>
        protected int CountRowsInTable(String tableName)
        {
            return (int) adoTemplate.ExecuteScalar(CommandType.Text, "SELECT COUNT(0) FROM " + tableName);

        }

        /// <summary>
        /// Execute the given SQL script using
        /// <see cref="SimpleAdoTestUtils.ExecuteSqlScript(Spring.Data.IAdoOperations,Spring.Core.IO.IResourceLoader,string,bool,System.Text.RegularExpressions.Regex[])"/>
        /// </summary>
        /// <param name="scriptResourcePath"></param>
        /// <param name="continueOnError"></param>
        protected void ExecuteSqlScript(string scriptResourcePath, bool continueOnError)
        {
            SimpleAdoTestUtils.ExecuteSqlScript( this.AdoTemplate, this.applicationContext, scriptResourcePath, continueOnError);
        }
    }
}
