using System;
using System.Reflection;
using AopAlliance.Intercept;
using Spring.Aop.Framework;

namespace Spring.AopQuickStart
{
    public class ModificationAdvice : IMethodInterceptor
    {
        public virtual Object Invoke(IMethodInvocation invocation)
        {
            MethodInfo method = invocation.Method;
            object proxy = invocation.Proxy;

            if (proxy is IIsModified)
            {
                IIsModified obj = (IIsModified) proxy;
                if (IsSetter(method))
                {
                    obj.IsModified = HasModificationOccured(method, invocation.This, invocation.Arguments);
                }
            }
            return invocation.Proceed();
        }

        private bool HasModificationOccured(MethodInfo setter, Object target, Object[] args)
        {
            PropertyInfo property = target.GetType().GetProperty(setter.Name.Substring(4));

            if (property != null)
            {
                // modification check is unimportant
                // for write only methods
                Object newVal = args[0];
                Object oldVal = property.GetValue(target, null);

                if ((newVal == null) && (oldVal == null))
                {
                    return false;
                }
                else if ((newVal == null) && (oldVal != null))
                {
                    return true;
                }
                else if ((newVal != null) && (oldVal == null))
                {
                    return true;
                }
                else
                {
                    return (!newVal.Equals(oldVal));
                }
            }

            return false;
        }

        private bool IsSetter(MethodInfo method)
        {
            return (method.Name.StartsWith("set_")) && (method.GetParameters().Length == 1);
        }
        
    }
}
