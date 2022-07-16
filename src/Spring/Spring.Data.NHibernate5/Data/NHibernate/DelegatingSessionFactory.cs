using System.Data.Common;
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
        public abstract ISessionFactory TargetSessionFactory { get; }

        public ICollection<string> DefinedFilterNames => TargetSessionFactory.DefinedFilterNames;

        public bool IsClosed => TargetSessionFactory.IsClosed;

        public IStatistics Statistics => TargetSessionFactory.Statistics;

        public void Close() => TargetSessionFactory.Close();

        public void Dispose() => TargetSessionFactory.Dispose();

        public void Evict(Type persistentClass, object id) => TargetSessionFactory.Evict(persistentClass, id);

        public void Evict(Type persistentClass) => TargetSessionFactory.Evict(persistentClass);

        public void EvictCollection(string roleName, object id) => TargetSessionFactory.EvictCollection(roleName, id);

        public void EvictCollection(string roleName) => TargetSessionFactory.EvictCollection(roleName);

        public void EvictEntity(string entityName) => TargetSessionFactory.EvictEntity(entityName);

        public void EvictEntity(string entityName, object id) => TargetSessionFactory.EvictEntity(entityName, id);

        public void EvictQueries(string cacheRegion) => TargetSessionFactory.EvictQueries(cacheRegion);

        public void EvictQueries() => TargetSessionFactory.EvictQueries();

        public IDictionary<string, IClassMetadata> GetAllClassMetadata() => TargetSessionFactory.GetAllClassMetadata();

        public IDictionary<string, ICollectionMetadata> GetAllCollectionMetadata() 
            => TargetSessionFactory.GetAllCollectionMetadata();

        public IStatelessSession OpenStatelessSession(DbConnection connection)
            => TargetSessionFactory.OpenStatelessSession(connection);

        public IClassMetadata GetClassMetadata(Type persistentType) 
            => TargetSessionFactory.GetClassMetadata(persistentType);

        public IClassMetadata GetClassMetadata(string entityName)
            => TargetSessionFactory.GetClassMetadata(entityName);

        public ICollectionMetadata GetCollectionMetadata(string roleName) 
            => TargetSessionFactory.GetCollectionMetadata(roleName);

        public ISession GetCurrentSession() => TargetSessionFactory.GetCurrentSession();

        public FilterDefinition GetFilterDefinition(string filterName) 
            => TargetSessionFactory.GetFilterDefinition(filterName);

        public Task CloseAsync(CancellationToken cancellationToken = new CancellationToken())
            => TargetSessionFactory.CloseAsync(cancellationToken);

        public Task EvictAsync(Type persistentClass, CancellationToken cancellationToken = new CancellationToken()) 
            => TargetSessionFactory.EvictAsync(persistentClass, cancellationToken);

        public Task EvictAsync(Type persistentClass, object id, CancellationToken cancellationToken = new CancellationToken())
            => TargetSessionFactory.EvictAsync(persistentClass, id, cancellationToken);

        public Task EvictEntityAsync(string entityName, CancellationToken cancellationToken = new CancellationToken()) 
            => TargetSessionFactory.EvictEntityAsync(entityName, cancellationToken);

        public Task EvictEntityAsync(string entityName, object id, CancellationToken cancellationToken = new CancellationToken()) 
            => TargetSessionFactory.EvictEntityAsync(entityName, id, cancellationToken);

        public Task EvictCollectionAsync(string roleName, CancellationToken cancellationToken = new CancellationToken()) 
            => TargetSessionFactory.EvictCollectionAsync(roleName, cancellationToken);

        public Task EvictCollectionAsync(string roleName, object id, CancellationToken cancellationToken = new CancellationToken()) 
            => TargetSessionFactory.EvictCollectionAsync(roleName, id, cancellationToken);

        public Task EvictQueriesAsync(CancellationToken cancellationToken = new CancellationToken())
            => TargetSessionFactory.EvictQueriesAsync(cancellationToken);

        public Task EvictQueriesAsync(string cacheRegion, CancellationToken cancellationToken = new CancellationToken())
            => TargetSessionFactory.EvictQueriesAsync(cacheRegion, cancellationToken);

        public ISessionBuilder WithOptions() => TargetSessionFactory.WithOptions();

        public IStatelessSessionBuilder WithStatelessOptions() => TargetSessionFactory.WithStatelessOptions();

        public ISession OpenSession(IInterceptor interceptor) => TargetSessionFactory.OpenSession(interceptor);

        public ISession OpenSession() => TargetSessionFactory.OpenSession();

        public ISession OpenSession(DbConnection conn, IInterceptor interceptor) 
            => TargetSessionFactory.OpenSession(conn, interceptor);

        public ISession OpenSession(DbConnection conn) => TargetSessionFactory.OpenSession(conn);

        public IStatelessSession OpenStatelessSession() => TargetSessionFactory.OpenStatelessSession();

        IDictionary<string, IClassMetadata> ISessionFactory.GetAllClassMetadata()
            => TargetSessionFactory.GetAllClassMetadata();

        IDictionary<string, ICollectionMetadata> ISessionFactory.GetAllCollectionMetadata() 
            => TargetSessionFactory.GetAllCollectionMetadata();
    }
}
