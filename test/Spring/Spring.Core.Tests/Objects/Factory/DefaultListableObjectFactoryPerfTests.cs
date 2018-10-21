#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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
using System.Diagnostics;
using NUnit.Framework;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Threading;

#endregion

namespace Spring.Objects.Factory
{
    /// <summary>
    /// This class contains tests related to performance of the object factory implementation and some concurrency test.
    /// The performance test are ignored by default.
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class DefaultListableObjectFactoryPerfTests
    {

        private DateTime start, stop;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Ignore("just a test")]
        public void Test()
        {
            int numIterations = 10000;
            start = DateTime.Now;
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            InitFactory(factory);
            for (int i = 0; i < numIterations; i++)
            {
              FFoo foo = (FFoo)factory.GetObject("foo");
            }
            stop = DateTime.Now;
            double timeElapsed = Elapsed;
            PrintTest("Creations", numIterations, timeElapsed);


        }

        private void InitFactory(DefaultListableObjectFactory factory)
        {
            Console.WriteLine("init factory");
            RootObjectDefinition tee = new RootObjectDefinition(typeof(Tee), true);
            tee.IsLazyInit = true;
            ConstructorArgumentValues teeValues = new ConstructorArgumentValues();
            teeValues.AddGenericArgumentValue("test");
            tee.ConstructorArgumentValues = teeValues;

            RootObjectDefinition bar = new RootObjectDefinition(typeof(BBar), false);
            ConstructorArgumentValues barValues = new ConstructorArgumentValues();
            barValues.AddGenericArgumentValue(new RuntimeObjectReference("tee"));
            barValues.AddGenericArgumentValue(5);
            bar.ConstructorArgumentValues = barValues;

            RootObjectDefinition foo = new RootObjectDefinition(typeof(FFoo), false);
            MutablePropertyValues fooValues = new MutablePropertyValues();
            fooValues.Add("i", 5);
            fooValues.Add("bar", new RuntimeObjectReference("bar"));
            fooValues.Add("copy", new RuntimeObjectReference("bar"));
            fooValues.Add("s", "test");
            foo.PropertyValues = fooValues;

            factory.RegisterObjectDefinition("foo", foo);
            factory.RegisterObjectDefinition("bar", bar);
            factory.RegisterObjectDefinition("tee", tee);
        }


        private double Elapsed
        {
            get { return (stop.Ticks - start.Ticks) / 10000000f; }
        }

        private static void PrintTest(string name, int iterations, double duration)
        {
            Debug.WriteLine(
                String.Format("{0,-60} {1,12:#,###} {2,12:##0.000} {3,12:#,###}", name, iterations, duration,
                              iterations / duration));
        }

        [Test]
        public void ConcurrencyTest()
        {
            AsyncTestTask t1 = new BeanFactoryTask(100).Start();
            t1.AssertNoException();
        }

    }

    public class BeanFactoryTask : AsyncTestTask
    {
        DefaultListableObjectFactory factory = new DefaultListableObjectFactory();

        public BeanFactoryTask(int iterations)
            : base(iterations)
        {
            InitializeFactory();
            FFoo foo = (FFoo)factory.GetObject("foo");
            Assert.AreEqual(5, foo.i);
            Assert.AreEqual("test", foo.s);
            Assert.AreSame(foo.bar.tee, foo.copy.tee);
            Assert.AreEqual(5, foo.bar.i);
            Assert.AreEqual("test", foo.bar.Tee.S);
        }

        private void InitializeFactory()
        {
            Console.WriteLine("init factory");
            RootObjectDefinition tee = new RootObjectDefinition(typeof(Tee), true);
            tee.IsLazyInit = true;
            ConstructorArgumentValues teeValues = new ConstructorArgumentValues();
            teeValues.AddGenericArgumentValue("test");
            tee.ConstructorArgumentValues = teeValues;

            RootObjectDefinition bar = new RootObjectDefinition(typeof(BBar), false);
            ConstructorArgumentValues barValues = new ConstructorArgumentValues();
            barValues.AddGenericArgumentValue(new RuntimeObjectReference("tee"));
            barValues.AddGenericArgumentValue(5);
            bar.ConstructorArgumentValues = barValues;

            RootObjectDefinition foo = new RootObjectDefinition(typeof(FFoo), false);
            MutablePropertyValues fooValues = new MutablePropertyValues();
            fooValues.Add("i", 5);
            fooValues.Add("bar", new RuntimeObjectReference("bar"));
            fooValues.Add("copy", new RuntimeObjectReference("bar"));
            fooValues.Add("s", "test");
            foo.PropertyValues = fooValues;

            factory.RegisterObjectDefinition("foo", foo);
            factory.RegisterObjectDefinition("bar", bar);
            factory.RegisterObjectDefinition("tee", tee);

        }

        public override void DoExecute()
        {
            FFoo foo = (FFoo)factory.GetObject("foo");
        }
    }

    public interface ITee
    {
        string S
        { get; }
    }

    public class Tee : ITee
    {
        private string s;

        public Tee(string s)
        {
            this.s = s;
        }

        public string S
        {
            get { return s; }
        }
    }

    public interface IBBar
    {
        ITee Tee
        {
            get;
        }

        int I
        {
            get;
        }
    }

    public class BBar : IBBar
    {
        public Tee tee;
        public int i;

        public BBar(Tee tee, int i)
        {
            this.tee = tee;
            this.i = i;
        }

        public ITee Tee
        {
            get { return tee; }
        }

        public int I
        {
            get { return i; }
        }

    }

    public class FFoo
    {
        public BBar bar;
        public BBar copy;
        public string s;
        public int i;

        public int I
        {
            set { i = value; }
        }


        public BBar Bar
        {
            set { bar = value; }
        }

        public BBar Copy
        {
            set { copy = value; }
        }

        public string S
        {
            set { s = value; }
        }
    }
}