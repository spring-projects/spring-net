/*
 * Copyright � 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Reflection;
using Spring.Aop.Support;
using Spring.Objects.Factory;
using Spring.Threading;

namespace Spring.Aop.Framework.AutoProxy;

/// <summary>
/// Before advisor that allow us to manipulate ordering to check
/// that superclass sorting works correctly.
/// </summary>
/// <remarks>
/// It doesn't actually do anything except count
/// method invocations and check for presence of a value in the
/// LogicalThreadContext.
/// </remarks>
/// <author>Mark Pollack (.NET)</author>
public class OrderedLogicalThreadContextCheckAdvisor : StaticMethodMatcherPointcutAdvisor, IInitializingObject
{
    public virtual bool RequireLTCHasValue
    {
        get { return requireLtcHasValue; }

        set { requireLtcHasValue = value; }
    }

    public virtual CountingBeforeAdvice CountingBeforeAdvice
    {
        get { return (CountingBeforeAdvice) Advice; }
    }

    /// <summary> Should we insist on the presence of a transaction attribute or refuse to accept one?</summary>
    private bool requireLtcHasValue = false;

    public virtual void AfterPropertiesSet()
    {
        Advice = new LTCCountingBeforeAdvice(this);
    }

    public override bool Matches(MethodInfo method, Type targetClass)
    {
        return method.Name.StartsWith("set_Age");
    }

    private class LTCCountingBeforeAdvice : CountingBeforeAdvice
    {
        private OrderedLogicalThreadContextCheckAdvisor enclosingInstance;

        public LTCCountingBeforeAdvice(OrderedLogicalThreadContextCheckAdvisor enclosingInstance)
        {
            this.enclosingInstance = enclosingInstance;
        }

        public OrderedLogicalThreadContextCheckAdvisor EnclosingInstance
        {
            get { return enclosingInstance; }
        }

        public override void Before(MethodInfo method, object[] args, object target)
        {
            // do check for presence of LTC value....
            if (EnclosingInstance.requireLtcHasValue)
            {
                if (LogicalThreadContext.GetData(LogicalThreadContextAdvice.ORDERING_SLOT) == null)
                {
                    throw new SystemException("Expected object in LTC ORDERING_SLOT");
                }
            }
            else
            {
                if (LogicalThreadContext.GetData(LogicalThreadContextAdvice.ORDERING_SLOT) != null)
                {
                    throw new SystemException("Expected no object in LTC ORDERING_SLOT");
                }
            }

            base.Before(method, args, target);
        }
    }
}
