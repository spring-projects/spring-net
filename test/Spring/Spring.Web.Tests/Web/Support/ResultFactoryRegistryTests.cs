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
using NUnit.Framework;
using Rhino.Mocks;

#endregion

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
            MockRepository mocks = new MockRepository();
            IResultFactory resultFactory = (IResultFactory)mocks.CreateMock( typeof( IResultFactory ) );

            IResultFactory prevFactory = ResultFactoryRegistry.DefaultResultFactory;
            Assert.AreSame( prevFactory, ResultFactoryRegistry.SetDefaultFactory( resultFactory ) );
            Assert.AreSame( resultFactory, ResultFactoryRegistry.DefaultResultFactory );

            // verify default factory is used for unknown result mode
            using (Record( mocks ))
            {
                Expect.Call( resultFactory.CreateResult( null, "resultText" ) ).Return( new Result() );
                Expect.Call( resultFactory.CreateResult( "resultMode", "resultText" ) ).Return( new Result() );
            }

            using (Playback( mocks ))
            {
                ResultFactoryRegistry.CreateResult( "resultText" );
                ResultFactoryRegistry.CreateResult( "resultMode:resultText" );
            }
        }

        [Test]
        public void ResultModeValuesHavePredefinedFactories()
        {
            MockRepository mocks = new MockRepository();
            IResultFactory defaultFactory = (IResultFactory)mocks.CreateMock( typeof( IResultFactory ) );

            ResultFactoryRegistry.SetDefaultFactory( defaultFactory );

            // verify factory registry knows all ResultModes
            using (Record( mocks ))
            {
                // defaultFactory must never be called!
            }

            using (Playback( mocks ))
            {
                foreach (string resultMode in Enum.GetNames( typeof( ResultMode ) ))
                {
                    Assert.IsNotNull( ResultFactoryRegistry.CreateResult( resultMode+":resultText" ) );
                }
            }
        }

        [Test]
        public void SelectsFactoryByResultMode()
        {
            MockRepository mocks = new MockRepository();
            IResultFactory resultFactory = (IResultFactory)mocks.CreateMock( typeof( IResultFactory ) );

            ResultFactoryRegistry.RegisterResultMode( "resultMode", resultFactory );

            Result result = new Result();

            // verify factory registry does not allow nulls to be returned
            using (Record( mocks ))
            {
                Expect.Call( resultFactory.CreateResult( "resultMode", "resultText" ) ).Return( result );
            }

            using (Playback( mocks ))
            {
                Assert.AreSame( result, ResultFactoryRegistry.CreateResult( "resultMode:resultText" ) );
            }
        }

        [Test]
        public void BailsOnNullReturnedFromFactory()
        {
            MockRepository mocks = new MockRepository();
            IResultFactory resultFactory = (IResultFactory) mocks.CreateMock(typeof(IResultFactory));

            ResultFactoryRegistry.RegisterResultMode("resultMode", resultFactory);

            // verify factory registry does not allow nulls to be returned
            using (Record(mocks))
            {
                Expect.Call(resultFactory.CreateResult("resultMode", "resultText")).Return(null);
            }

            using (Playback(mocks))
            {
                Assert.Throws<ArgumentNullException>(() => ResultFactoryRegistry.CreateResult("resultMode:resultText"));
            }
        }

        #region Rhino.Mocks Compatibility Adapter

        private static IDisposable Record( MockRepository mocks )
        {
            return mocks.Record();
        }

        private static IDisposable Playback( MockRepository mocks )
        {
            return mocks.Playback();
        }

        #endregion Rhino.Mocks Compatibility Adapter
    }
}