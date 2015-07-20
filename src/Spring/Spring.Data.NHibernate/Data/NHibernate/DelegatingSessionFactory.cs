using System.Data;
using System.Collections.Generic;

using NHibernate;
using NHibernate.Engine;
using NHibernate.Metadata;
using NHibernate.Stat;

namespace Spring.Data.NHibernate
{
#pragma warning disable 1591
    /// <summary>
    /// DelegatingSessionFactory class
    /// </summary>
    public abstract class DelegatingSessionFactory : ISessionFactory
    {
        public ICollection<string> DefinedFilterNames
        {
            get { return TargetSessionFactory.DefinedFilterNames; }
        }

        public bool IsClosed
        {
            get { return TargetSessionFactory.IsClosed; }
        }

        public IStatistics Statistics
        {
            get { return TargetSessionFactory.Statistics; }
        }

        public abstract ISessionFactory TargetSessionFactory
        {
            get;
        }

        public void Close()
        {
            TargetSessionFactory.Close();
        }

        public void Dispose()
        {
            TargetSessionFactory.Dispose();
        }

        public void Evict(System.Type persistentClass, object id)
        {
            TargetSessionFactory.Evict(persistentClass, id);
        }

        public void Evict(System.Type persistentClass)
        {
            TargetSessionFactory.Evict(persistentClass);
        }

        public void EvictCollection(string roleName, object id)
        {
            TargetSessionFactory.EvictCollection(roleName, id);
        }

        public void EvictCollection(string roleName)
        {
            TargetSessionFactory.EvictCollection(roleName);
        }

        public void EvictEntity(string entityName)
        {
            TargetSessionFactory.EvictEntity(entityName);
        }

        public void EvictEntity(string entityName, object id)
        {
            TargetSessionFactory.EvictEntity(entityName, id);
        }

        public void EvictQueries(string cacheRegion)
        {
            TargetSessionFactory.EvictQueries(cacheRegion);
        }

        public void EvictQueries()
        {
            TargetSessionFactory.EvictQueries();
        }

        public IDictionary<string, IClassMetadata> GetAllClassMetadata()
        {
            return TargetSessionFactory.GetAllClassMetadata();
        }

        public IDictionary<string, ICollectionMetadata> GetAllCollectionMetadata()
        {
            return TargetSessionFactory.GetAllCollectionMetadata();
        }

        public IClassMetadata GetClassMetadata(System.Type persistentType)
        {
            return TargetSessionFactory.GetClassMetadata(persistentType);
        }

        public IClassMetadata GetClassMetadata(string entityName)
        {
            return TargetSessionFactory.GetClassMetadata(entityName);
        }

        public ICollectionMetadata GetCollectionMetadata(string roleName)
        {
            return TargetSessionFactory.GetCollectionMetadata(roleName);
        }

        public ISession GetCurrentSession()
        {
            return TargetSessionFactory.GetCurrentSession();
        }

        public FilterDefinition GetFilterDefinition(string filterName)
        {
            return TargetSessionFactory.GetFilterDefinition(filterName);
        }

        public ISession OpenSession(IInterceptor interceptor)
        {
            return TargetSessionFactory.OpenSession(interceptor);
        }

        public ISession OpenSession()
        {
            return TargetSessionFactory.OpenSession();
        }

        public ISession OpenSession(IDbConnection conn, IInterceptor interceptor)
        {
            return TargetSessionFactory.OpenSession(conn, interceptor);
        }

        public ISession OpenSession(IDbConnection conn)
        {
            return TargetSessionFactory.OpenSession(conn);
        }

        public IStatelessSession OpenStatelessSession()
        {
            return TargetSessionFactory.OpenStatelessSession();
        }

        public IStatelessSession OpenStatelessSession(IDbConnection connection)
        {
            return TargetSessionFactory.OpenStatelessSession(connection);
        }

        IDictionary<string, IClassMetadata> ISessionFactory.GetAllClassMetadata()
        {
            return TargetSessionFactory.GetAllClassMetadata();
        }

        IDictionary<string, ICollectionMetadata> ISessionFactory.GetAllCollectionMetadata()
        {
            return TargetSessionFactory.GetAllCollectionMetadata();
        }

    }
}
