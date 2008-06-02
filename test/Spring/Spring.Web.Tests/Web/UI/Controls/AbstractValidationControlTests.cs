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
using System.Web.UI;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Context;

#endregion

namespace Spring.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class AbstractValidationControlTests
    {
        public class TestValidationControl : AbstractValidationControl
        {
            private IValidationContainer _vc;

            public TestValidationControl()
            {
            }

            public TestValidationControl(IValidationContainer testVc)
            {
                _vc = testVc;
            }

            protected override Spring.Web.UI.Validation.IValidationErrorsRenderer CreateValidationErrorsRenderer()
            {
                throw new Exception("The method or operation is not implemented.");
            }

            protected override IValidationContainer ValidationContainer
            {
                get
                {
                    if (_vc != null) return _vc;
                    return base.ValidationContainer;
                }
            }

            public IMessageSource TheMessageSource
            {
                get { return base.MessageSource; }
            }

            public IValidationContainer TheValidationContainer
            {
                get { return base.ValidationContainer; }
            }
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
            MockRepository mocks = new MockRepository();
            IMessageSource messageSource = (IMessageSource)mocks.DynamicMock(typeof(IMessageSource));
            IValidationContainer container = (IValidationContainer)mocks.DynamicMock(typeof(IValidationContainer));
            TestValidationControl ctl = new TestValidationControl(container);

            Expect.Call(container.MessageSource).Return(messageSource);
            mocks.ReplayAll();

            Assert.AreEqual(messageSource, ctl.TheMessageSource);
        }
    }
}