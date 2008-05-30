#region License
/*
 * Copyright © 2002-2006 the original author or authors.
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
#endregion

#region Imports

using Spring.Context;
using Spring.Util;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// This strategy replaces the original collection's owner with an intercepting proxy
    /// </summary>
    /// <author>Erich Eichinger</author>
    /// <version>$Id: InterceptControlCollectionOwnerStrategy.cs,v 1.1 2007/08/01 23:11:01 markpollack Exp $</version>
    internal class InterceptControlCollectionOwnerStrategy : IInterceptionStrategy
    {
        public bool Intercept(IApplicationContext defaultApplicationContext, ControlAccessor ctlAccessor,
                              ControlCollectionAccessor ctlColAccessor)
        {
            ctlColAccessor.Owner = new SupportsWebDependencyInjectionOwnerProxy(defaultApplicationContext, ctlAccessor.GetTarget() );
            return true;
        }
    }
}