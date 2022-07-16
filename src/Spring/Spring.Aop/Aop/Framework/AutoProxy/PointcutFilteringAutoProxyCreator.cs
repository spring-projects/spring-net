#region License

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

#endregion

#region Imports

using Spring.Util;

#endregion

namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// This AutoProxyCreator only proxies objects matching the specified <see cref="IPointcut"/>.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class PointcutFilteringAutoProxyCreator : AbstractFilteringAutoProxyCreator
    {
        private IPointcut _pointcut;

        /// <summary>
        /// Set the pointcut used to filter objects that should automatically get wrapped with proxies.
        /// </summary>
        public IPointcut Pointcut
        {
            set { _pointcut = value; }
            get { return _pointcut; }
        }

        /// <summary>
        /// Determines, whether the given object shall be proxied.
        /// </summary>
        protected override bool IsEligibleForProxying( Type targetType, string targetName )
        {
            AssertUtils.ArgumentNotNull(_pointcut, "Pointcut");

            bool shallProxy = AopUtils.CanApply( _pointcut, targetType, null );
            return shallProxy;
        }
    }
}
