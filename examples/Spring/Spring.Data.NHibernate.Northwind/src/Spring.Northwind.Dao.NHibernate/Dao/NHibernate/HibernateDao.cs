using System.Collections.Generic;
using NHibernate;

namespace Spring.Northwind.Dao.NHibernate
{
    /// <summary>
    /// Base class for data access operations.
    /// </summary>
    public abstract class HibernateDao
    {
        private ISessionFactory sessionFactory;

        /// <summary>
        /// Session factory for sub-classes.
        /// </summary>
        public ISessionFactory SessionFactory
        {
            protected get { return sessionFactory; }
            set { sessionFactory = value; }
        }

        /// <summary>
        /// Get's the current active session. Uses
        /// Open Session In View in the background.
        /// </summary>
        protected ISession Session
        {
            get { return sessionFactory.GetCurrentSession(); }
        }

        protected IList<T> GetAll<T>() where T : class
        {
            ICriteria criteria = Session.CreateCriteria<T>();
            return criteria.List<T>();
        }
    }
}