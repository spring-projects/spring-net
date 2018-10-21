using System.Reflection;
using AopAlliance.Intercept;

namespace Spring.AopQuickStart
{
    public class ModificationAdvice : IMethodInterceptor
    {
        public virtual object Invoke(IMethodInvocation invocation)
        {
            MethodInfo method = invocation.Method;
            object proxy = invocation.Proxy;

            if (proxy is IIsModified obj)
            {
                if (IsSetter(method))
                {
                    obj.IsModified = HasModificationOccured(method, invocation.This, invocation.Arguments);
                }
            }

            return invocation.Proceed();
        }

        private bool HasModificationOccured(MethodInfo setter, object target, object[] args)
        {
            PropertyInfo property = target.GetType().GetProperty(setter.Name.Substring(4));

            if (property != null)
            {
                // modification check is unimportant
                // for write only methods
                object newVal = args[0];
                object oldVal = property.GetValue(target, null);

                if (newVal == null && (oldVal == null))
                {
                    return false;
                }

                if (newVal == null && (oldVal != null))
                {
                    return true;
                }

                if ((oldVal == null))
                {
                    return true;
                }

                return (!newVal.Equals(oldVal));
            }

            return false;
        }

        private bool IsSetter(MethodInfo method)
        {
            return method.Name.StartsWith("set_") && method.GetParameters().Length == 1;
        }
    }
}