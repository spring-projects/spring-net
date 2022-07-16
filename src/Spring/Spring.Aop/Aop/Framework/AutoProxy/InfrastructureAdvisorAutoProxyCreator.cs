#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using Spring.Objects.Factory.Config;

namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// A special version of an APC that explicitely cares for infrastructure (=internal)
    /// advisors only
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class InfrastructureAdvisorAutoProxyCreator : AbstractAdvisorAutoProxyCreator
    {
        /// <summary>
        /// Overridden to create a special version of an <see cref="IAdvisorRetrievalHelper"/>
        /// that accepts only infrastructure advisor definitions
        /// </summary>
        /// <param name="objectFactory"></param>
        /// <returns></returns>
        protected override IAdvisorRetrievalHelper CreateAdvisorRetrievalHelper(IConfigurableListableObjectFactory objectFactory)
        {
            return new InfrastructurAdvisorRetrievalHelper(this, objectFactory);
        }

        private class InfrastructurAdvisorRetrievalHelper : ObjectFactoryAdvisorRetrievalHelper
        {
            private readonly InfrastructureAdvisorAutoProxyCreator _owner;

            public InfrastructurAdvisorRetrievalHelper(InfrastructureAdvisorAutoProxyCreator owner, IConfigurableListableObjectFactory objectFactory)
                : base(objectFactory)
            {
                _owner = owner;
            }

            protected override bool IsEligibleObject(string advisorName, Type objectType, string objectName)
            {
                return this.ObjectFactory.ContainsObjectDefinition(advisorName)
                       && this.ObjectFactory.GetObjectDefinition(advisorName).Role == ObjectRole.ROLE_INFRASTRUCTURE;
            }
        }
    }
}
