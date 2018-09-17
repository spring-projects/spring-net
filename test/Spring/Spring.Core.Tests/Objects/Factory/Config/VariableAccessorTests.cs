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
using System.Globalization;

using NUnit.Framework;

#endregion

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Tests <see cref="VariableAccessor"/> functionality.
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class VariableAccessorTests
    {
        private static readonly Guid TESTGUID = Guid.NewGuid();
        private static readonly Guid TESTGUID_DEFAULT = Guid.NewGuid();

        private static readonly DateTime TESTDATETIME = new DateTime(2007, 07, 06, 11, 12, 13);
        private static readonly DateTime TESTDATETIME_DEFAULT = TESTDATETIME.AddDays(-1);
        
        private readonly IVariableSource _testVariableSource = new DictionaryVariableSource(null, true)
            .Add("ValidString", "String")
            .Add("EmptyString", "")
            .Add("ValidChar", "c")
            .Add("InvalidChar", "12")
            .Add("ValidBoolean", "true")
            .Add("InvalidBoolean", "")
            .Add("ValidByte", "1")
            .Add("InvalidByte", "")
            .Add("ValidInt16", "1")
            .Add("InvalidInt16", "")
            .Add("ValidInt32", "1")
            .Add("InvalidInt32", "")
            .Add("ValidInt64", "1")
            .Add("InvalidInt64", "")
            .Add("ValidFloat", "1")
            .Add("InvalidFloat", "")
            .Add("ValidDouble", "1")
            .Add("InvalidDouble", "")
            .Add("ValidDecimal", "1")
            .Add("InvalidDecimal", "")
            .Add("ValidGuid", TESTGUID.ToString())
            .Add("InvalidGuid", "")
            .Add("ValidDateTime", TESTDATETIME.ToString(CultureInfo.InvariantCulture))
            .Add("InvalidDateTime", "blabla")
            .Add("ValidDateTimeUtcRoundtripFormatted", TESTDATETIME.ToUniversalTime().ToString("u"))
            ;

        [Test]
        public void AcceptsNullVariableSource()
        {
            VariableAccessor va = new VariableAccessor(null);
            Assert.AreEqual("default", va.GetString("somekey", "default"));
        }

        [Test]
        public void GetString()
        {
            VariableAccessor va = new VariableAccessor(_testVariableSource);
            Assert.AreEqual("String", va.GetString("ValidString", "DefaultString"));
            Assert.AreEqual("DefaultString", va.GetString("NonExistingString", "DefaultString"));
        }

        [Test]
        public void GetStringIgnoresEmptyString()
        {
            VariableAccessor va = new VariableAccessor(_testVariableSource);
            Assert.AreEqual("DefaultString", va.GetString("EmptyString", "DefaultString"));
        }

        [Test]
        public void GetChar()
        {
            VariableAccessor va = new VariableAccessor(_testVariableSource);
            Assert.AreEqual('c', va.GetChar("ValidChar", 'a'));
            Assert.AreEqual('a', va.GetChar("InvalidChar", 'a', false));
            Assert.Throws<ArgumentException>(() => va.GetChar("InvalidChar", 'a', true));
        }

        [Test]
        public void GetBoolean()
        {
            VariableAccessor va = new VariableAccessor(_testVariableSource);
            Assert.AreEqual(true, va.GetBoolean("ValidBoolean", false));
            Assert.AreEqual(true, va.GetBoolean("InvalidBoolean", true, false));
        }

        [Test]
        public void GetByte()
        {
            VariableAccessor va = new VariableAccessor(_testVariableSource);
            Assert.AreEqual((byte) 1, va.GetByte("ValidByte", 2));
            Assert.AreEqual((byte) 2, va.GetByte("InvalidByte", 2, false));
        }

        [Test]
        public void GetInt16()
        {
            VariableAccessor va = new VariableAccessor(_testVariableSource);
            Assert.AreEqual((short) 1, va.GetInt16("ValidInt16", 2));
            Assert.AreEqual((short) 2, va.GetInt16("InvalidInt16", 2, false));
            va.GetInt16("InvalidInt16", 2, true);
        }

        [Test]
        public void GetInt32()
        {
            VariableAccessor va = new VariableAccessor(_testVariableSource);
            Assert.AreEqual((int) 1, va.GetInt32("ValidInt32", 2));
            Assert.AreEqual((int) 2, va.GetInt32("InvalidInt32", 2, false));
        }

        [Test]
        public void GetInt64()
        {
            VariableAccessor va = new VariableAccessor(_testVariableSource);
            Assert.AreEqual((long) 1, va.GetInt64("ValidInt64", 2));
            Assert.AreEqual((long) 2, va.GetInt64("InvalidInt64", 2, false));
        }

        [Test]
        public void GetFloat()
        {
            VariableAccessor va = new VariableAccessor(_testVariableSource);
            Assert.AreEqual((float) 1, va.GetFloat("ValidFloat", 2.0f));
            Assert.AreEqual((float) 2, va.GetFloat("InvalidFloat", 2.0f, false));
            va.GetFloat("InvalidFloat", 2, true);
        }

        [Test]
        public void GetDouble()
        {
            VariableAccessor va = new VariableAccessor(_testVariableSource);
            Assert.AreEqual((double)1, va.GetDouble("ValidDouble", 2.0));
            Assert.AreEqual((double)2, va.GetDouble("InvalidDouble", 2.0, false));
            va.GetDouble("InvalidDouble", 2, true);
        }

        [Test]
        public void GetDecimal()
        {
            VariableAccessor va = new VariableAccessor(_testVariableSource);
            Assert.AreEqual((decimal)1, va.GetDecimal("ValidDecimal", 2.0m));
            Assert.AreEqual((decimal)2, va.GetDecimal("InvalidDecimal", 2.0m, false));
            va.GetDecimal("InvalidDecimal", 2, true);
        }

        [Test]
        public void GetGuid()
        {
            VariableAccessor va = new VariableAccessor(_testVariableSource);
            Assert.AreEqual(TESTGUID, va.GetGuid("ValidGuid", TESTGUID_DEFAULT));
            Assert.AreEqual(TESTGUID_DEFAULT, va.GetGuid("InvalidGuid", TESTGUID_DEFAULT, false));
        }

        [Test]
        public void GetDateTime()
        {
            VariableAccessor va = new VariableAccessor(_testVariableSource);
            Assert.AreEqual(TESTDATETIME, va.GetDateTime("ValidDateTime", null, TESTDATETIME_DEFAULT));
            Assert.AreEqual(TESTDATETIME.ToUniversalTime(), va.GetDateTime("ValidDateTimeUtcRoundtripFormatted", "u", TESTDATETIME_DEFAULT));
            Assert.AreEqual(TESTDATETIME_DEFAULT, va.GetDateTime("InvalidDateTime", null, TESTDATETIME_DEFAULT, false));
            Assert.Throws<FormatException>(() => va.GetDateTime("InvalidDateTime", null, TESTDATETIME_DEFAULT, true));
        }
    }
}