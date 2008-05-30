using AopAlliance.Aop;
using Spring.Aop.Support;

namespace Spring.AopQuickStart
{
    public class IsModifiedMixin : IIsModified, IAdvice
    {
        private bool isModified = false;

        public virtual bool IsModified
        {
            get { return isModified; }
            set { isModified = value; }
        }
    }

    public class IsModifiedAdvisor : DefaultIntroductionAdvisor
    {
        public IsModifiedAdvisor()
            : base(new IsModifiedMixin())
        {}
    }
}
