namespace Spring.Northwind.Dao
{
    /// <summary>
    /// Role interface for DAOs that support saves and updates to entities.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <typeparam name="TId">Entity id type.</typeparam>
    public interface ISupportsSave<TEntity, TId>
    {
        /// <summary>
        /// Saves the given entity.
        /// </summary>
        /// <param name="entity">Entity to save.</param>
        /// <returns>The id for saved entity.</returns>
        TId Save(TEntity entity);

        /// <summary>
        /// Saves or updates the entity. Behavior depends on the current state of entity's ID.
        /// </summary>
        /// <param name="entity">Entity to save or update.</param>
        void Update(TEntity entity);     
    }
}