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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;

using FakeItEasy;

using NUnit.Framework;

using Spring.Context;
using Spring.Context.Support;
using Spring.Validation;
using Spring.Web.UI.Validation;

namespace Spring.Web.UI.Controls
{
    /// <summary>
    ///
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class AbstractValidationControlTests
    {
        [Test]
        public void DefaultsToControlIDOrStringEmptyAsProviderName()
        {
            TestValidationControl vc;

            vc = new TestValidationControl(null);
            Assert.AreEqual(string.Empty, vc.Provider);

            vc = new TestValidationControl(null);
            vc.ID = "TestControl";
            Assert.AreEqual(vc.ID, vc.Provider);

            vc = new TestValidationControl(null);
            vc.ID = "TestControl";
            vc.Provider = "TestProvider";

            Assert.AreEqual("TestProvider", vc.Provider);
        }

        [Test]
        public void ResolvesAndRendersValidationErrorsUsingValidationContainer()
        {
            TestValidationControl vc = new TestValidationControl();
            vc.ID = "TestControl";

            Page page = new Page();
            page.Controls.Add(vc);
            page.ValidationErrors.AddError(vc.Provider, new ErrorMessage("msgId"));
            StaticMessageSource msgSrc = new StaticMessageSource();
            msgSrc.AddMessage("msgId", CultureInfo.CurrentUICulture, "Resolved Message Text");
            page.MessageSource = msgSrc;

            vc.TestRender(null);
            Assert.AreEqual("Resolved Message Text", vc.LastErrorsRendered[0]);
        }

        [Test]
        public void ResolvesAndRendersValidationErrorsUsingExplicitlySpecifiedErrorsAndMessageSource()
        {
            TestValidationControl vc = new TestValidationControl();
            vc.ID = "TestControl";

            Page page = new Page();
            page.Controls.Add(vc);

            ValidationErrors errors = new ValidationErrors();
            errors.AddError(vc.Provider, new ErrorMessage("msgId"));
            vc.ValidationErrors = errors;

            StaticMessageSource msgSrc = new StaticMessageSource();
            msgSrc.AddMessage("msgId", CultureInfo.CurrentUICulture, "Resolved Message Text");
            vc.MessageSource = msgSrc;

            vc.TestRender(null);
            Assert.AreEqual("Resolved Message Text", vc.LastErrorsRendered[0]);
        }

        [Test]
        public void ThrowsIfCreateValidationErrorsRendererReturnsNull()
        {
            TestValidationControl vc = new TestValidationControl(null);
            vc.ID = "TestControl";

            Page page = new Page();
            page.Controls.Add(vc);
            page.ValidationErrors.AddError(vc.Provider, new ErrorMessage("msgId"));

            try
            {
                vc.TestRender(null);
                Assert.Fail();
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("Renderer", ane.ParamName);
            }
        }

        [Test]
        public void NoExceptionWhenNoValidationContainer()
        {
            TestValidationControl vc = new TestValidationControl();
            Assert.IsFalse(vc.TheDesignMode);
            Assert.IsNull(vc.TheMessageSource);
            Assert.IsNull(vc.TheValidationContainer);

            vc.TestRender(null);
        }

        [Test]
        public void NoExceptionWhenNoErrors()
        {
            Page page = new Page();
            page.MessageSource = new StaticMessageSource();
            TestValidationControl vc = new TestValidationControl();
            page.Controls.Add(vc);

            // assert assumptions
            Assert.IsFalse(vc.TheDesignMode);
            Assert.AreSame(page, vc.TheValidationContainer);
            Assert.AreSame( page.MessageSource, vc.TheMessageSource );
            Assert.AreEqual(0, page.ValidationErrors.GetResolvedErrors(vc.Provider, vc.TheMessageSource).Count);

            vc.TestRender(null);
        }

        [Test]
        public void ResolveValidationContainerToEnclosingParentRecursively()
        {
            // direct containment
            UserControl uc = new UserControl();
            TestValidationControl vc = new TestValidationControl();
            uc.Controls.Add(vc);
            Assert.AreSame(uc, vc.TheValidationContainer);

            // indirect containment
            uc = new UserControl();
            vc = new TestValidationControl();
            Control inBetweenControl = new Panel();
            inBetweenControl.Controls.Add(vc);
            uc.Controls.Add(inBetweenControl);
            Assert.AreSame(uc, vc.TheValidationContainer);
        }

        [Test]
        public void ResolveValidationContainerToEnclosingPageRecursively()
        {
            // direct containment
            Page page = new Page();
            TestValidationControl vc = new TestValidationControl();
            page.Controls.Add(vc);
            Assert.AreSame(page, vc.TheValidationContainer);

            // indirect containment
            page = new Page();
            vc = new TestValidationControl();
            Control inBetweenControl = new System.Web.UI.WebControls.Panel();
            inBetweenControl.Controls.Add(vc);
            page.Controls.Add(inBetweenControl);
            Assert.AreSame(page, vc.TheValidationContainer);
        }

        [Test]
        public void DefaultsToValidationContainerMessageSource()
        {
            IMessageSource messageSource = A.Fake<IMessageSource>();
            //            IValidationContainer container = (IValidationContainer)mocks.DynamicMock(typeof(IValidationContainer));

            Page page = new Page();
            page.MessageSource = messageSource;
            TestValidationControl ctl = new TestValidationControl();
            page.Controls.Add(ctl);

            Assert.AreEqual(messageSource, ctl.TheMessageSource);
        }

        [Test]
        public void ReadsErrorsFromClosestValidationContainerByDefault()
        {
            // indirect containment
            Spring.Web.UI.Page page = new Spring.Web.UI.Page();
            TestValidationControl vc = new TestValidationControl();
            Spring.Web.UI.UserControl inBetweenControl = new Spring.Web.UI.UserControl();
            inBetweenControl.Controls.Add(vc);
            page.Controls.Add(inBetweenControl);
            Assert.AreSame(inBetweenControl, vc.TheValidationContainer);
        }

        [Test]
        public void DoesNotReadErrorsFromCrossValidationContainerByDefault()
        {
            // containment tree:
            //
            // Page : IValidationContainer
            //   - outerVC
            //   - inBetweenControl1 : IValidationContainer
            //         - innerVC

            Spring.Web.UI.Page page = new Spring.Web.UI.Page();
            Spring.Web.UI.UserControl inBetweenControl1 = new Spring.Web.UI.UserControl();
            inBetweenControl1.ID = "InBetweenControl1";
            TestValidationControl innerVC = new TestValidationControl();
            inBetweenControl1.Controls.Add(innerVC);
            page.Controls.Add(inBetweenControl1);
            TestValidationControl outerVC = new TestValidationControl();
            page.Controls.Add(outerVC);

            inBetweenControl1.ValidationErrors.AddError("provider", new ErrorMessage("msg"));

            Assert.AreSame(inBetweenControl1, innerVC.TheValidationContainer);
            Assert.AreSame(page, outerVC.TheValidationContainer);
            Assert.AreEqual(1, innerVC.TheValidationContainer.ValidationErrors.GetErrors("provider").Count);
            Assert.AreEqual(0, outerVC.TheValidationContainer.ValidationErrors.GetErrors("provider").Count);
        }

        [Test]
        public void CanReadErrorsFromCrossValidationContainerByDefault()
        {
            // containment tree:
            //
            // Page : IValidationContainer
            //   - outerVC
            //   - inBetweenControl1 : IValidationContainer
            //         - innerVC

            Spring.Web.UI.Page page = new Spring.Web.UI.Page();
            Spring.Web.UI.UserControl userControl = new Spring.Web.UI.UserControl();
            userControl.ID = "userControl";
            TestValidationControl innerVC = new TestValidationControl();
            userControl.Controls.Add(innerVC);
            page.Controls.Add(userControl);
            TestValidationControl outerVC = new TestValidationControl();
            page.Controls.Add(outerVC);

            userControl.ValidationErrors.AddError("provider", new ErrorMessage("msg"));

            Assert.AreSame(userControl, innerVC.TheValidationContainer);
            Assert.AreSame(page, outerVC.TheValidationContainer);
            Assert.AreEqual(1, innerVC.TheValidationContainer.ValidationErrors.GetErrors("provider").Count);
            Assert.AreEqual(0, outerVC.TheValidationContainer.ValidationErrors.GetErrors("provider").Count);

            // test local-relative name resolution
            outerVC.ValidationContainerName = "userControl";
            Assert.AreEqual(1, outerVC.TheValidationContainer.ValidationErrors.GetErrors("provider").Count);

            // test global name resolution
            outerVC.ValidationContainerName = "::userControl";
            Assert.AreEqual(1, outerVC.TheValidationContainer.ValidationErrors.GetErrors("provider").Count);
        }

        #region TestValidationControl

        private class TestValidationControl : AbstractValidationControl
        {
            public class CapturingRenderer : IValidationErrorsRenderer
            {
                public IList<string> LastErrorsRendered;
                private readonly IValidationErrorsRenderer _inner;

                public CapturingRenderer(IValidationErrorsRenderer inner)
                {
                    _inner = inner;
                }

                public void RenderErrors(Page page, HtmlTextWriter writer, IList<string> errors)
                {
                    LastErrorsRendered = errors;
                    if (_inner != null)
                    {
                        _inner.RenderErrors(page, writer, errors);
                    }
                }
            }

            private readonly IValidationErrorsRenderer _ver;

            public TestValidationControl()
                : this(new CapturingRenderer(null))
            { }

            public TestValidationControl(IValidationErrorsRenderer renderer)
            {
                _ver =renderer;
            }

            public void TestRender(HtmlTextWriter writer)
            {
                base.Render(writer);
            }

            protected override IValidationErrorsRenderer CreateValidationErrorsRenderer()
            {
                return _ver;
            }

            public IList<string> LastErrorsRendered
            {
                get { return ((CapturingRenderer)_ver).LastErrorsRendered; }
            }

            public IMessageSource TheMessageSource
            {
                get { return base.ResolveMessageSource(); }
            }

            public IValidationContainer TheValidationContainer
            {
                get { return base.FindValidationContainer(); }
            }

            public bool TheDesignMode
            {
                get { return base.DesignMode; }
            }
        }

        #endregion
    }
}