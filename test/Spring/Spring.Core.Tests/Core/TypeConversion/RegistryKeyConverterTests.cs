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
using Microsoft.Win32;
using NUnit.Framework;

#pragma warning disable CA1416 // is only supported on windows

namespace Spring.Core.TypeConversion
{
    /// <summary>
    /// Unit tests for the RegistryKeyConverter class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Platform("Win")]
    public sealed class RegistryKeyConverterTests
    {
        [Test]
        public void ConvertFromNullReference()
        {
            RegistryKeyConverter rkc = new RegistryKeyConverter();
            Assert.Throws<ArgumentNullException>(() => rkc.ConvertFrom(null));
        }

        [Test]
        public void ConvertFromNonSupportedOptionBails()
        {
            RegistryKeyConverter rkc = new RegistryKeyConverter();
            Assert.Throws<NotSupportedException>(() => rkc.ConvertFrom(12));
        }

        [Test]
        public void ConvertFrom()
        {
            RegistryKeyConverter rkc = new RegistryKeyConverter();
            Assert.AreEqual(Registry.CurrentUser, rkc.ConvertFrom("HKEY_CURRENT_USER"));
            Assert.AreEqual(Registry.CurrentUser.OpenSubKey("Software").Name,
                            ((RegistryKey) rkc.ConvertFrom(@"HKEY_CURRENT_USER\Software")).Name);
            Assert.AreEqual(Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Microsoft").Name,
                            ((RegistryKey) rkc.ConvertFrom(@"HKEY_CURRENT_USER\Software\Microsoft")).Name);
        }

        [Test]
        public void ConvertFromEmptyString()
        {
            RegistryKeyConverter rkc = new RegistryKeyConverter();
            Assert.Throws<ArgumentNullException>(() => rkc.ConvertFrom(string.Empty));
        }

        [Test]
        public void ConvertFromBadKeyString()
        {
            RegistryKeyConverter rkc = new RegistryKeyConverter();
            Assert.Throws<ArgumentException>(() => rkc.ConvertFrom(@"HKEY_CURRENT_USER\sdgsdfgsdfgxadas\Xyz\Abc"), @"Registry key [HKEY_CURRENT_USER\sdgsdfgsdfgxadas] does not exist.");
        }

        [Test]
        public void ConvertFromBadHiveString()
        {
            RegistryKeyConverter rkc = new RegistryKeyConverter();
            Assert.Throws<ArgumentException>(() => rkc.ConvertFrom(@"HKEY_ERROR\sdgsdfgsdfgxadas"), "Invalid root hive name [HKEY_ERROR].");
        }

    }
}