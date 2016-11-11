#region License

/*
 * Copyright 2004 the original author or authors.
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

#endregion

namespace Spring.Util
{
    /// <summary>
    /// Unit tests for the DefensiveEventRaiser class.
    /// </summary>
    /// <author>Rick Evans</author>
    [TestFixture]
    public sealed class DefensiveEventRaiserTests
    {
        [Test]
        public void RaiseSwallowsExceptionRaisedByHandlers()
        {
            OneThirstyDude dude = new OneThirstyDude();
            Soda bru = new Soda();
            bru.Pop += new PopHandler(dude.HandlePopWithException);
            bru.OnPop("Iron Brew", new DefensiveEventRaiser());
            Assert.AreEqual("Iron Brew", dude.Soda); // should have got through before exception was thrown
        }

        [Test]
        public void RaiseSwallowsExceptionRaisedByHandlerButCallsAllOtherHandlers()
        {
            bool firstCall = false;
            bool secondCall = false;
            bool thirdCall = false;

            OneThirstyDude dude = new OneThirstyDude();
            Soda bru = new Soda();
            bru.Pop += (sender, soda) => firstCall = true;
			bru.Pop += (sender, soda) => { secondCall = true; throw new Exception(); };
			bru.Pop += (sender, soda) => { thirdCall = true; };

            DefensiveEventRaiser eventRaiser = new DefensiveEventRaiser();

            IEventExceptionsCollector exceptions = bru.OnPop( "Iron Brew", eventRaiser );

            Assert.AreEqual(1, exceptions.Exceptions.Count);
            Assert.IsTrue(firstCall);
            Assert.IsTrue(secondCall);
            Assert.IsTrue(thirdCall);
        }
    }
}
