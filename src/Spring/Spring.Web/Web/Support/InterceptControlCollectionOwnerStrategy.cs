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

using System.Web.UI;
using Spring.Context;

namespace Spring.Web.Support;

/// <summary>
/// This strategy replaces the original collection's owner with an intercepting proxy
/// </summary>
/// <author>Erich Eichinger</author>
internal class InterceptControlCollectionOwnerStrategy : IInterceptionStrategy
{
    public bool Intercept(IApplicationContext defaultApplicationContext, ControlAccessor ctlAccessor,
        ControlCollectionAccessor ctlColAccessor)
    {
        Control target = ctlAccessor.GetTarget();
        ctlColAccessor.Owner = target is INamingContainer
            ? new NamingContainerSupportsWebDependencyInjectionOwnerProxy(defaultApplicationContext, target)
            : new SupportsWebDependencyInjectionOwnerProxy(defaultApplicationContext, target);
        return true;
    }
}
