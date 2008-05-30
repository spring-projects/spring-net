using System;
using Spring.Aop;
using Spring.Aop.Support;

namespace Spring.AopQuickStart
{
    public class ModificationAdvisor : DefaultPointcutAdvisor
    {
        public ModificationAdvisor(Type type) 
            : base(new SetterPointcut(type), new ModificationAdvice())
        {}

        private class SetterPointcut : IPointcut
        {
            private IMethodMatcher methodMatcher = TrueMethodMatcher.True;
            private ITypeFilter typeFilter;

            public SetterPointcut(Type type)
            {
                typeFilter = new RootTypeFilter(type);
            }

            public ITypeFilter TypeFilter
            {
                get { return typeFilter; }
            }

            public IMethodMatcher MethodMatcher
            {
                get { return methodMatcher; }
            }
        }
    }
}
