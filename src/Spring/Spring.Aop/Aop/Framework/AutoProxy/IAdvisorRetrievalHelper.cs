namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// Interface encapsulating the advisor retrieval strategy used by
    /// an <see cref="AbstractAdvisorAutoProxyCreator"/> to retrieve the
    /// applicable list of advisor objects.
    /// </summary>
    public interface IAdvisorRetrievalHelper
    {
        /// <summary>
        /// Get the list of advisor objects to apply on the target.
        /// </summary>
        List<IAdvisor> FindAdvisorObjects(Type targetType, string targetName);
    }
}
