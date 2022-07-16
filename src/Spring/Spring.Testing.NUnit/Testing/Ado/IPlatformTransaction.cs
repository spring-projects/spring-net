namespace Spring.Testing.Ado
{
    /// <summary>
    /// Holds status for an active transaction. You *must* dispose this object!
    /// </summary>
    /// <remarks>
    /// <example>
    /// Usage Pattern:
    /// <code>
    /// TBD
    /// </code>
    /// </example>
    /// </remarks>
    public interface IPlatformTransaction : IDisposable
    {
        /// <summary>
        /// Mark transaction for commit on disposal
        /// </summary>
        void Commit();
        /// <summary>
        /// Throw exception and rollback any uncommitted commands
        /// </summary>
        void Rollback();
    }
}
