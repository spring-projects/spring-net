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

using AopAlliance.Aop;
using NUnit.Framework;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

namespace Spring.Aop.Framework.AutoProxy;

/// <summary>
/// </summary>
/// <author>Erich Eichinger</author>
[TestFixture]
public class InfrastructureAdvisorAutoProxyCreatorTests
{
    public class TestAdvisorAutoProxyCreator : InfrastructureAdvisorAutoProxyCreator
    {
        public IList<object> GetAdvicesAndAdvisorsForObject(Type targetType, string targetName)
        {
            return base.GetAdvicesAndAdvisorsForObject(targetType, targetName, null);
        }
    }

    public class TestAdvisor : IAdvisor
    {
        public string Name;

        public bool IsPerInstance
        {
            get { throw new NotImplementedException(); }
        }

        public IAdvice Advice
        {
            get { throw new NotImplementedException(); }
        }
    }

    [Test]
    public void DoesAcceptInfrastructureAdvisorsOnlyDuringScanning()
    {
        DefaultListableObjectFactory of = new DefaultListableObjectFactory();

        GenericObjectDefinition infrastructureAdvisorDefinition = new GenericObjectDefinition();
        infrastructureAdvisorDefinition.ObjectType = typeof(TestAdvisor);
        infrastructureAdvisorDefinition.PropertyValues.Add("Name", "InfrastructureAdvisor");
        infrastructureAdvisorDefinition.Role = ObjectRole.ROLE_INFRASTRUCTURE;
        of.RegisterObjectDefinition("infrastructure", infrastructureAdvisorDefinition);

        GenericObjectDefinition regularAdvisorDefinition = new GenericObjectDefinition();
        regularAdvisorDefinition.ObjectType = typeof(TestAdvisor);
        regularAdvisorDefinition.PropertyValues.Add("Name", "RegularAdvisor");
        //            regularAdvisorDefinition.Role = ObjectRole.ROLE_APPLICATION;
        of.RegisterObjectDefinition("regular", regularAdvisorDefinition);

        TestAdvisorAutoProxyCreator apc = new TestAdvisorAutoProxyCreator();
        apc.ObjectFactory = of;
        IList<object> advisors = apc.GetAdvicesAndAdvisorsForObject(typeof(object), "dummyTarget");
        Assert.AreEqual(1, advisors.Count);
        Assert.AreEqual("InfrastructureAdvisor", ((TestAdvisor) advisors[0]).Name);
    }
}
