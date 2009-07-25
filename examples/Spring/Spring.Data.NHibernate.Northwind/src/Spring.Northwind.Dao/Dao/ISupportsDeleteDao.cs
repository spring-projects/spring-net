namespace Spring.Northwind.Dao
{
    /// <summary>
    /// Role interface for DAOs that support deletion of entities.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    public interface ISupportsDeleteDao<TEntity>
    {
        /// <summary>
        /// Deletes the entity.
        /// </summary>
        /// <param name="entity">Entity to delete.</param>
        void Delete(TEntity entity);
    }
}