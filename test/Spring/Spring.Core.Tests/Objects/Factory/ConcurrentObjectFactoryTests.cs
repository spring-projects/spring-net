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

using NUnit.Framework;
using Spring.Core.IO;
using Spring.Objects.Factory.Xml;
using Spring.Threading;

namespace Spring.Objects.Factory;

[TestFixture]
public class ConcurrentObjectFactoryTests
{
    private IObjectFactory factory;

    private DateTime date1 = DateTime.Parse("2004/08/08");

    private DateTime date2 = DateTime.Parse("2000/02/02");

    [SetUp]
    public void SetUp()
    {
        IResource resource = new ReadOnlyXmlTestResource("concurrent.xml", GetType());
        factory = new XmlObjectFactory(resource);
    }

    [Test]
    public void SingleThread()
    {
        for (int i = 0; i < 100; i++)
        {
            PerformTest();
        }
    }

    [Test]
    public void Concurrent()
    {
        AsyncTestTask t1 = new ObjectFactoryTask(500, this).Start();

        t1.AssertNoException();
    }

    private void PerformTest()
    {
        ConcurrentObject c1 = (ConcurrentObject) factory.GetObject("object1");
        ConcurrentObject c2 = (ConcurrentObject) factory.GetObject("object2");

        Assert.AreEqual(date1, c1.Date);
        Assert.AreEqual(date2, c2.Date);
    }

    public class ConcurrentObject
    {
        private DateTime date;

        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }
    }

    public class ObjectFactoryTask : AsyncTestTask
    {
        private ConcurrentObjectFactoryTests test;

        public ObjectFactoryTask(int iterations, ConcurrentObjectFactoryTests test)
            : base(iterations)
        {
            this.test = test;
        }

        #region Overrides of AsyncTestTask

        public override void DoExecute()
        {
            test.PerformTest();
        }

        #endregion
    }
}
