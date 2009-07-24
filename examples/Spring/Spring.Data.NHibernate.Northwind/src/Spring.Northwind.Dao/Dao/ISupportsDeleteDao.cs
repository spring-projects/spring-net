namespace Spring.Northwind.Dao
{
    public interface ISupportsDeleteDao<TEntity>
    {
        void Delete(TEntity entity);
    }
}