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

using System.ServiceModel;
using NUnit.Framework;
using Spring.Objects.Factory.Support;

namespace Spring.ServiceModel;

/// <summary>
/// </summary>
/// <author>Erich Eichinger</author>
[TestFixture]
public class SpringServiceHostTests
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        int Add(int a, int b);
    }

    internal class Service : IService
    {
        public int Add(int a, int b)
        {
            return (a + b);
        }
    }

    [Test]
    public void CanCreateHostTwice()
    {
        DefaultListableObjectFactory of = new DefaultListableObjectFactory();

        string svcRegisteredName = System.Guid.NewGuid().ToString();

        of.RegisterObjectDefinition(svcRegisteredName, new RootObjectDefinition(new RootObjectDefinition(typeof(Service))));
        SpringServiceHost ssh = new SpringServiceHost(svcRegisteredName, of, true);
        SpringServiceHost ssh1 = new SpringServiceHost(svcRegisteredName, of, true);
    }
}
