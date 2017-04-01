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

using FakeItEasy;

using NUnit.Framework;

namespace Spring.Web.Support
{
    /// <summary>
    ///
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class ResultFactoryRegistryTests
    {
        [SetUp]
        public void SetUp()
        {
            ResultFactoryRegistry.Reset();
        }

        [Test]
        public void SetDefaultFactory()
        {
            IResultFactory resultFactory = A.Fake<IResultFactory>();

            IResultFactory prevFactory = ResultFactoryRegistry.DefaultResultFactory;
            Assert.AreSame(prevFactory, ResultFactoryRegistry.SetDefaultFactory(resultFactory));
            Assert.AreSame(resultFactory, ResultFactoryRegistry.DefaultResultFactory);

            // verify default factory is used for unknown result mode
            A.CallTo(() => resultFactory.CreateResult(null, "resultText")).Returns(new Result());
            A.CallTo(() => resultFactory.CreateResult("resultMode", "resultText")).Returns(new Result());

            ResultFactoryRegistry.CreateResult("resultText");
            ResultFactoryRegistry.CreateResult("resultMode:resultText");
        }

        [Test]
        public void ResultModeValuesHavePredefinedFactories()
        {
            IResultFactory defaultFactory = A.Fake<IResultFactory>();
            ResultFactoryRegistry.SetDefaultFactory(defaultFactory);

            foreach (string resultMode in Enum.GetNames(typeof(ResultMode)))
            {
                Assert.IsNotNull(ResultFactoryRegistry.CreateResult(resultMode + ":resultText"));
            }
        }

        [Test]
        public void SelectsFactoryByResultMode()
        {
            IResultFactory resultFactory = A.Fake<IResultFactory>();

            ResultFactoryRegistry.RegisterResultMode("resultMode", resultFactory);

            Result result = new Result();

            // verify factory registry does not allow nulls to be returned
            A.CallTo(() => resultFactory.CreateResult("resultMode", "resultText")).Returns(result);
            Assert.AreSame(result, ResultFactoryRegistry.CreateResult("resultMode:resultText"));
        }

        [Test]
        public void BailsOnNullReturnedFromFactory()
        {
            IResultFactory resultFactory = A.Fake<IResultFactory>();

            ResultFactoryRegistry.RegisterResultMode("resultMode", resultFactory);

            // verify factory registry does not allow nulls to be returned
            A.CallTo(() => resultFactory.CreateResult("resultMode", "resultText")).Returns(null);
            Assert.Throws<ArgumentNullException>(() => ResultFactoryRegistry.CreateResult("resultMode:resultText"));
        }
    }
}