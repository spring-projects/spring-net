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
using System.Reflection;
using NUnit.Framework;

#endregion

namespace Spring.Util
{
	/// <summary>
	/// Unit tests for the EventRaiser class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class EventRaiserTests
    {
        [Test]
        public void Raise () 
        {
            OneThirstyDude dude = new OneThirstyDude ();
            Soda bru = new Soda ();
            bru.Pop += new PopHandler (dude.HandlePop);
            bru.OnPop ("Iron Brew", new EventRaiser ());
            Assert.AreEqual ("Iron Brew", dude.Soda);
        }

        [Test]
        public void RaiseWithBadNumberOfArguments () 
        {
            OneThirstyDude dude = new OneThirstyDude ();
            Soda bru = new Soda ();
            bru.Pop += new PopHandler (dude.HandlePop);
            Assert.Throws<TargetParameterCountException>(() => bru.OnPopWithBadNumberOfArguments ("Iron Brew", new EventRaiser ()));
        }

        [Test]
        public void RaiseWithNullEvent () 
        {
            OneThirstyDude dude = new OneThirstyDude ();
            Soda bru = new Soda ();
            bru.Pop += new PopHandler (dude.HandlePop);
            bru.OnPopWithNullEvent ("Iron Brew", new EventRaiser ());
            Assert.AreEqual (string.Empty, dude.Soda);
        }

        public void RaiseWithAnEventHandlerThatThrowsAnException () 
        {
            OneThirstyDude dude = new OneThirstyDude ();
            Soda bru = new Soda ();
            bru.Pop += new PopHandler (dude.HandlePopWithException);
            Assert.Throws<FormatException>(() => bru.OnPop ("Iron Brew", new EventRaiser ()), "Iron Brew");
        }
	}

    internal delegate void PopHandler (object sender, string soda);

    internal sealed class Soda 
    {
        public event PopHandler Pop;

        public IEventExceptionsCollector OnPop (string soda, EventRaiser raiser) 
        {
            return raiser.Raise (Pop, this, soda);
        }

        public void OnPopWithBadNumberOfArguments (string soda, EventRaiser raiser) 
        {
            raiser.Raise (Pop, /*this,*/ soda); // wrong number of args to the event
        }

        public void OnPopWithNullEvent (string soda, EventRaiser raiser) 
        {
            raiser.Raise (null, this, soda); // no event...
        }
    }

    internal sealed class OneThirstyDude 
    {
        public void HandlePop (object sender, string soda) 
        {
            _soda = soda;
        }

        public void HandlePopWithException (object sender, string soda) 
        {
            _soda = soda;
            throw new FormatException (soda);
        }

		public static void StaticHandlePop(object sender, string soda) 
		{
		}

        public string Soda 
        {
            get 
            {
                return _soda;
            }
        }

        private string _soda = string.Empty;
    }
}
