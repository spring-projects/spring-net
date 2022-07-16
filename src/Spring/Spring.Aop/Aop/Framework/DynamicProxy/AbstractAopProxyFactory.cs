using Spring.Aop.Support;
using Spring.Aop.Target;

namespace Spring.Aop.Framework.DynamicProxy
{
    /// <summary>
    /// A reusable base implementation of <see cref="IAopProxyFactory"/>, providing
    /// some useful default implementations
    /// </summary>
    /// <author>Erich Eichinger</author>
    [Serializable]
    public abstract class AbstractAopProxyFactory : IAopProxyFactory
    {
        /// <summary>
        /// Creates an <see cref="Spring.Aop.Framework.IAopProxy"/> for the
        /// supplied <paramref name="advisedSupport"/> configuration.
        /// </summary>
        /// <param name="advisedSupport">The AOP configuration.</param>
        /// <returns>An <see cref="Spring.Aop.Framework.IAopProxy"/>.</returns>
        /// <exception cref="AopConfigException">
        /// If the supplied <paramref name="advisedSupport"/> configuration is
        /// invalid.
        /// </exception>
        /// <seealso cref="Spring.Aop.Framework.IAopProxyFactory.CreateAopProxy"/>
        public IAopProxy CreateAopProxy(AdvisedSupport advisedSupport)
        {
            if (advisedSupport == null)
            {
                throw new AopConfigException("Cannot create IAopProxy with null ProxyConfig");
            }
            if (advisedSupport.Advisors.Count == 0 && advisedSupport.TargetSource == EmptyTargetSource.Empty)
            {
                throw new AopConfigException("Cannot create IAopProxy with no advisors and no target source");
            }

            EliminateDuplicateAdvisors(advisedSupport);

            return DoCreateAopProxyInstance(advisedSupport);
        }

        /// <summary>
        /// Actually creates the proxy instance based on the supplied <see cref="AdvisedSupport"/>.
        /// </summary>
        /// <returns>the proxy instance described by <paramref name="advisedSupport"/>. Must not be <c>null</c></returns>
        protected abstract IAopProxy DoCreateAopProxyInstance(AdvisedSupport advisedSupport);

        /// <summary>
        /// If possible, checks for advisor duplicates on the supplied <paramref name="advisedSupport"/> and 
        /// eliminates them.
        /// </summary>
        protected virtual void EliminateDuplicateAdvisors(AdvisedSupport advisedSupport)
        {
            if (advisedSupport.TargetSource is SingletonTargetSource
                && IsAopProxyType(advisedSupport.TargetSource))
            {
                IAdvised innerProxy = (IAdvised)advisedSupport.TargetSource.GetTarget();
                // eliminate duplicate advisors
                List<IAdvisor> thisAdvisors = new List<IAdvisor>(advisedSupport.Advisors);
                foreach (IAdvisor innerAdvisor in innerProxy.Advisors)
                {
                    foreach (IAdvisor thisAdvisor in thisAdvisors)
                    {
                        if (ReferenceEquals(thisAdvisor, innerAdvisor)
                            || (thisAdvisor.GetType() == typeof(DefaultPointcutAdvisor)
                                && thisAdvisor.Equals(innerAdvisor)
                               )
                            )
                        {
                            advisedSupport.RemoveAdvisor(thisAdvisor);
                        }
                    }
                }

                // elimination of duplicate introductions is not necessary 
                // since they do not propagate to nested proxy anyway
            }
        }

        /// <summary>
        /// Checks, if the given <paramref name="targetSource"/> holds a proxy generated by this factory.
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsAopProxyType(ITargetSource targetSource)
        {
            return AopUtils.IsAopProxyType(targetSource.TargetType);
        }
    }
}
