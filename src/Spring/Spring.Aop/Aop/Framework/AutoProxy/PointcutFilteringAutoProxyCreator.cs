#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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

using System;
using System.Collections;
using Spring.Objects.Factory;
using Spring.Util;

#endregion

namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// This AutoProxyCreator only proxies objects matching the specified <see cref="IPointcut"/>. Additionally, proxy creation may
    /// be further restricted by specifying object name patterns like with <see cref="ObjectNameAutoProxyCreator"/>.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class PointcutFilteringAutoProxyCreator : ObjectNameAutoProxyCreator
    {
        private IPointcut _pointcut;

        /// <summary>
        /// Set the pointcut used to filter objects that should automatically get wrapped with proxies.
        /// </summary>
        public IPointcut Pointcut
        {
            set { _pointcut = value; }
        }

        /// <summary>
        /// Determines, whether the given object shall be proxied.
        /// </summary>
        protected override bool ShallProxy( Type objType, string name, ITargetSource customTargetSource )
        {
            if (CollectionUtils.IsEmpty( ObjectNames ) && _pointcut == null)
            {
                throw new ArgumentException("At least one of ObjectNames and Pointcut criteria are required");
            }

            bool isObjectNameMatch = base.ShallProxy( objType, name, customTargetSource );

            // we have a name match, but empty pointcut -> ok
            if (isObjectNameMatch && _pointcut==null)
            {
                return true;
            }

            // positive name match or no names specified -> get the pointcut match
            if ( (isObjectNameMatch || CollectionUtils.IsEmpty(ObjectNames) )
                && _pointcut != null)
            {
                return AopUtils.CanApply( _pointcut, objType, null );
            }

            return false;
        }
    }
}