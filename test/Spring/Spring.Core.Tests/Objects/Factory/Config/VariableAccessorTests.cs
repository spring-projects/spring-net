/*
 * Copyright � 2002-2011 the original author or authors.
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

using System.Globalization;
using NUnit.Framework;

namespace Spring.Objects.Factory.Config;

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

    private readonly IVariableSource _testVariableSource = GetTestVariableSource();

    private static DictionaryVariableSource GetTestVariableSource()
    {
        return new DictionaryVariableSource(null, true)
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
                .Add("ValidDateTime", TESTDATETIME.ToString())
                .Add("InvalidDateTime", "blabla")
                .Add("ValidDateTimeUtcRoundtripFormatted", TESTDATETIME.ToUniversalTime().ToString("u"));
    }

    [Test]
    public void AcceptsNullVariableSource()
    {
        VariableAccessor va = new VariableAccessor(null);
        Assert.That(va.GetString("somekey", "default"), Is.EqualTo("default"));
    }

    [Test]
    public void GetString()
    {
        VariableAccessor va = new VariableAccessor(_testVariableSource);
        Assert.That(va.GetString("ValidString", "DefaultString"), Is.EqualTo("String"));
        Assert.That(va.GetString("NonExistingString", "DefaultString"), Is.EqualTo("DefaultString"));
    }

    [Test]
    public void GetStringIgnoresEmptyString()
    {
        VariableAccessor va = new VariableAccessor(_testVariableSource);
        Assert.That(va.GetString("EmptyString", "DefaultString"), Is.EqualTo("DefaultString"));
    }

    [Test]
    public void GetChar()
    {
        VariableAccessor va = new VariableAccessor(_testVariableSource);
        Assert.That(va.GetChar("ValidChar", 'a'), Is.EqualTo('c'));
        Assert.That(va.GetChar("InvalidChar", 'a', false), Is.EqualTo('a'));
        Assert.Throws<ArgumentException>(() => va.GetChar("InvalidChar", 'a', true));
    }

    [Test]
    public void GetBoolean()
    {
        VariableAccessor va = new VariableAccessor(_testVariableSource);
        Assert.That(va.GetBoolean("ValidBoolean", false), Is.EqualTo(true));
        Assert.That(va.GetBoolean("InvalidBoolean", true, false), Is.EqualTo(true));
    }

    [Test]
    public void GetByte()
    {
        VariableAccessor va = new VariableAccessor(_testVariableSource);
        Assert.That(va.GetByte("ValidByte", 2), Is.EqualTo((byte) 1));
        Assert.That(va.GetByte("InvalidByte", 2, false), Is.EqualTo((byte) 2));
    }

    [Test]
    public void GetInt16()
    {
        VariableAccessor va = new VariableAccessor(_testVariableSource);
        Assert.That(va.GetInt16("ValidInt16", 2), Is.EqualTo((short) 1));
        Assert.That(va.GetInt16("InvalidInt16", 2, false), Is.EqualTo((short) 2));
        va.GetInt16("InvalidInt16", 2, true);
    }

    [Test]
    public void GetInt32()
    {
        VariableAccessor va = new VariableAccessor(_testVariableSource);
        Assert.That(va.GetInt32("ValidInt32", 2), Is.EqualTo((int) 1));
        Assert.That(va.GetInt32("InvalidInt32", 2, false), Is.EqualTo((int) 2));
    }

    [Test]
    public void GetInt64()
    {
        VariableAccessor va = new VariableAccessor(_testVariableSource);
        Assert.That(va.GetInt64("ValidInt64", 2), Is.EqualTo((long) 1));
        Assert.That(va.GetInt64("InvalidInt64", 2, false), Is.EqualTo((long) 2));
    }

    [Test]
    public void GetFloat()
    {
        VariableAccessor va = new VariableAccessor(_testVariableSource);
        Assert.That(va.GetFloat("ValidFloat", 2.0f), Is.EqualTo((float) 1));
        Assert.That(va.GetFloat("InvalidFloat", 2.0f, false), Is.EqualTo((float) 2));
        va.GetFloat("InvalidFloat", 2, true);
    }

    [Test]
    public void GetDouble()
    {
        VariableAccessor va = new VariableAccessor(_testVariableSource);
        Assert.That(va.GetDouble("ValidDouble", 2.0), Is.EqualTo((double) 1));
        Assert.That(va.GetDouble("InvalidDouble", 2.0, false), Is.EqualTo((double) 2));
        va.GetDouble("InvalidDouble", 2, true);
    }

    [Test]
    public void GetDecimal()
    {
        VariableAccessor va = new VariableAccessor(_testVariableSource);
        Assert.That(va.GetDecimal("ValidDecimal", 2.0m), Is.EqualTo((decimal) 1));
        Assert.That(va.GetDecimal("InvalidDecimal", 2.0m, false), Is.EqualTo((decimal) 2));
        va.GetDecimal("InvalidDecimal", 2, true);
    }

    [Test]
    public void GetGuid()
    {
        VariableAccessor va = new VariableAccessor(_testVariableSource);
        Assert.That(va.GetGuid("ValidGuid", TESTGUID_DEFAULT), Is.EqualTo(TESTGUID));
        Assert.That(va.GetGuid("InvalidGuid", TESTGUID_DEFAULT, false), Is.EqualTo(TESTGUID_DEFAULT));
    }

    [Test]
    [SetCulture("en-US")]
    public void GetDateTime()
    {
        var testVariables = GetTestVariableSource();
        DoGetDateTime(testVariables);
    }

    [Test]
    [SetCulture("it-IT")]
    public void GetDateTimeIt()
    {
        var testVariables = GetTestVariableSource();
        DoGetDateTime(testVariables);
    }

    public static void DoGetDateTime(DictionaryVariableSource testVariables)
    {
        var va = new VariableAccessor(testVariables);
        Assert.That(va.GetDateTime("ValidDateTime", null, TESTDATETIME_DEFAULT), Is.EqualTo(TESTDATETIME));
        Assert.That(va.GetDateTime("ValidDateTimeUtcRoundtripFormatted", "u", TESTDATETIME_DEFAULT), Is.EqualTo(TESTDATETIME.ToUniversalTime()));
        Assert.That(va.GetDateTime("InvalidDateTime", null, TESTDATETIME_DEFAULT, false), Is.EqualTo(TESTDATETIME_DEFAULT));
        Assert.Throws<FormatException>(() => va.GetDateTime("InvalidDateTime", null, TESTDATETIME_DEFAULT, true));
    }
}
