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
using System.Web.UI;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.TestSupport;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// Unit tests for the ControlInterceptor class.
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class ControlInterceptionTests
    {
        private const string RES_OBJECTS =
            "assembly://Spring.Web.Tests/Spring.Web.Support/ControlInterceptionTests.objects.xml";

				static ControlInterceptionTests()
				{
					Common.Logging.LogManager.Adapter = new Common.Logging.Simple.TraceLoggerFactoryAdapter();
				}

        [SetUp]
        public void SetUp()
        {
            string physDir = AppDomain.CurrentDomain.BaseDirectory + "\\";
            IApplicationContext appContext =
                new XmlApplicationContext(AbstractApplicationContext.DefaultRootContextName, false, RES_OBJECTS);
            ContextRegistry.RegisterContext(appContext);
            appContext = new GenericApplicationContext("/controls/", false, appContext);
            ContextRegistry.RegisterContext(appContext);
        }

        [TearDown]
        public void TearDown()
        {
            ContextRegistry.Clear();
        }

        [Test]
        public void ADDandADDATInjectDependencies()
        {
            Control ctl = GetRootControl();

            MockControl mock = new MockControl();
            ctl.Controls.Add(mock);
            Assert.IsTrue(mock.GotIt);

            mock = new MockControl();
            ctl.Controls.AddAt(0, mock);
            Assert.IsTrue(mock.GotIt);
        }

        [Test]
        public void HierarchyGetsInjectedAfterAttachToTree()
        {
            MockControl subControl = new MockControl();
            MockControl subSubControl = new MockControl();
            subControl.Controls.Add(subSubControl);

            // DI occcurs *after* adding control(s) to rooted hierarchy
            Assert.IsFalse(subControl.GotIt);
            Assert.IsFalse(subSubControl.GotIt);

            Control rootControl = GetRootControl();
            rootControl.Controls.Add(subControl);
            Assert.IsTrue(subControl.GotIt);
            Assert.IsTrue(subSubControl.GotIt);

            MockControl subSubSubControl = new MockControl();
            subSubControl.Controls.Add(subSubSubControl);
            Assert.IsTrue(subSubSubControl.GotIt);
        }

        [Test]
        public void DealsWithNoninheritableControlCollections()
        {
            Control rootControl = GetRootControl();

            Control ctl = new ControlWithNoninheritableCollectionType();
            rootControl.Controls.Add(ctl);

            MockControl subControl = new MockControl();
            ctl.Controls.Add(subControl);
            Assert.IsTrue(subControl.GotIt);

            MockControl subControl2 = new MockControl();
            ctl.Controls.AddAt(0, subControl2);
            Assert.IsTrue(subControl2.GotIt);
        }

        [Test]
        public void DealsWithControlCollectionsHavingNonStandardConstructor()
        {
            Control rootControl = GetRootControl();

            Control ctl = new ControlWithCollectionTypeHavingNonStandardConstructor();
            rootControl.Controls.Add(ctl);

            MockControl subControl = new MockControl();
            ctl.Controls.Add(subControl);
            Assert.IsTrue(subControl.GotIt);

            MockControl subControl2 = new MockControl();
            ctl.Controls.AddAt(0, subControl2);
            Assert.IsTrue(subControl2.GotIt);
        }

        [Test]
        public void DealsWithControlsImplementingISupportsWebDependencyInjection()
        {
            Control rootControl = GetRootControl();

            Control ctl = new ControlSupportingWebDI();
            rootControl.Controls.Add(ctl);

            MockControl subControl = new MockControl();
            ctl.Controls.Add(subControl);
            Assert.IsTrue(subControl.GotIt);

            MockControl subControl2 = new MockControl();
            ctl.Controls.AddAt(0, subControl2);
            Assert.IsTrue(subControl2.GotIt);
        }

        [Test]
        public void SetsApplicationContextOnIApplicationContextAware()
        {
            using (new TestWebContext("/", "context.aspx"))
            {
                Control rootControl = GetRootControl();

                Control ctl = new ControlSupportingWebDI();
                rootControl.Controls.Add(ctl);

                MockControl subControl = new MockControl();
                ctl.Controls.Add(subControl);
                Assert.IsTrue(subControl.GotIt);
                Assert.IsNotNull(subControl.ApplicationContext);
                Assert.IsTrue(ContextRegistry.GetContext() == subControl.ApplicationContext);

                // controls get AppContext from to their location (if any)
                MockControl subControl2 = new MockControl("/controls");
                ctl.Controls.AddAt(0, subControl2);
                Assert.IsTrue(subControl2.GotIt);
                Assert.IsNotNull(subControl2.ApplicationContext);
                Assert.IsTrue(ContextRegistry.GetContext("/controls/") == subControl2.ApplicationContext);
            }
        }

        private Control GetRootControl()
        {
            Page ctl = new Page();
            WebDependencyInjectionUtils.InjectDependenciesRecursive(ContextRegistry.GetContext(), ctl);
            return ctl;
        }
    }

    #region Test Support Classes

    public class MockControl : UserControl, IApplicationContextAware
    {
        private bool _gotIt = false;
        private IApplicationContext _applicationContext;
        private string _templateSourceDirectory = string.Empty;

        public MockControl()
        {
        }

        public MockControl(string _templateSourceDirectory)
        {
            this._templateSourceDirectory = _templateSourceDirectory;
        }

        public bool GotIt
        {
            get { return this._gotIt; }
            set
            {
                // ensure DI occurs only once
                Assert.IsFalse(GotIt);
                this._gotIt = value;
            }
        }

        public IApplicationContext ApplicationContext
        {
            get { return _applicationContext; }
            set { _applicationContext = value; }
        }

        public override string TemplateSourceDirectory
        {
            get { return _templateSourceDirectory; }
        }
    }

    public class SimpleControl : Control
    {
    }

    internal class NoninheritableControlCollection : ControlCollection
    {
        public NoninheritableControlCollection(Control owner)
            : base(owner)
        {
        }
    }

    public class ControlCollectionWithNonStandardConstructor : ControlCollection
    {
        private int _someValue;
        private string _someAdditionalParameter;

        public ControlCollectionWithNonStandardConstructor(Control owner, string someAdditionalParamater, int someValue)
            : base(owner)
        {
            _someAdditionalParameter = someAdditionalParamater;
            _someValue = someValue;
        }

        public int SomeValue
        {
            get { return this._someValue; }
        }

        public string SomeAdditionalParameter
        {
            get { return this._someAdditionalParameter; }
        }
    }

    public class ControlWithNoninheritableCollectionType : Control
    {
        protected override ControlCollection CreateControlCollection()
        {
            return new NoninheritableControlCollection(this);
        }
    }

    public class ControlWithCollectionTypeHavingNonStandardConstructor : Control
    {
        protected override ControlCollection CreateControlCollection()
        {
            return new ControlCollectionWithNonStandardConstructor(this, "someAdditionalParameter", 10);
        }
    }

    public class ControlSupportingWebDI : Control, ISupportsWebDependencyInjection
    {
        private IApplicationContext _defaultApplicationContext;

        public IApplicationContext DefaultApplicationContext
        {
            get { return this._defaultApplicationContext; }
            set { this._defaultApplicationContext = value; }
        }

        protected override void AddedControl(Control control, int index)
        {
            WebDependencyInjectionUtils.InjectDependenciesRecursive(_defaultApplicationContext, control);
            base.AddedControl(control, index);
        }
    }

    #endregion Test Support Classes

    #region DI Templates // look at them using reflector

    internal class DIControlCollection : ControlCollection, ISupportsWebDependencyInjection
    {
        private IApplicationContext _defaultApplicationContext;

        public DIControlCollection(Control owner) : base(owner)
        {
        }

        IApplicationContext ISupportsWebDependencyInjection.DefaultApplicationContext
        {
            get { return _defaultApplicationContext; }
            set { _defaultApplicationContext = value; }
        }

        public override void Add(Control child)
        {
            child = WebDependencyInjectionUtils.InjectDependenciesRecursive(_defaultApplicationContext, child);
            base.Add(child);
        }

        public override void AddAt(int index, Control child)
        {
            child = WebDependencyInjectionUtils.InjectDependenciesRecursive(_defaultApplicationContext, child);
            base.AddAt(index, child);
        }
    }

    #endregion
}
