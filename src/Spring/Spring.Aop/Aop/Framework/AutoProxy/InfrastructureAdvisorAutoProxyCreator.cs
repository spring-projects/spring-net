#region License

/*
 * Copyright 2002-2009 the original author or authors.
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

using System;
using Spring.Objects.Factory.Config;

namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// </summary>
    /// <author>Erich Eichinger</author>
    internal class InfrastructureAdvisorAutoProxyCreator : AbstractAdvisorAutoProxyCreator
    {
        private IConfigurableListableObjectFactory _objectFactory;

        protected override void InitObjectFactory(IConfigurableListableObjectFactory objectFactory)
        {
            base.InitObjectFactory(objectFactory);
            _objectFactory = objectFactory;
        }

        protected override bool IsEligibleAdvisorObject(string advisorName, Type targetType, string targetName)
        {
            return _objectFactory.ContainsObjectDefinition(advisorName)
                   && _objectFactory.GetObjectDefinition(advisorName).Role == ObjectRole.ROLE_INFRASTRUCTURE;
        }
    }
}