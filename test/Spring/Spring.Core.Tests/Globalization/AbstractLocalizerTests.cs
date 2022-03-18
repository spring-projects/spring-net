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

using Spring.Context;

#endregion

namespace Spring.Globalization
{
    /// <summary>
    /// Unit tests for the localizers.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public abstract class AbstractLocalizerTests
    {
        private ILocalizer localizer;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            CultureTestScope.Set("de-AT", "sr");
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            CultureTestScope.Reset();
        }

        [SetUp]
        public void Init()
        {
            localizer = CreateLocalizer();
        }

        [Test]
        public void TestInvariantCulture()
        {
            Inventor tesla = CreateInventor(CultureInfo.InvariantCulture);
            Assert.AreEqual("Nikola Tesla", tesla.Name);
            Assert.AreEqual("Serbian", tesla.Nationality);
            Assert.AreEqual(new DateTime(1856, 7, 9), tesla.DOB);
            Assert.AreEqual("Croatia", tesla.PlaceOfBirth.Country);
            Assert.AreEqual("Smiljan", tesla.PlaceOfBirth.City);
        }

        [Test]
        public void TestSerbianLatin()
        {
            Inventor tesla = CreateInventor(new CultureInfo(CultureInfoUtils.SerbianLatinCultureName));
            Assert.AreEqual("Nikola Tesla", tesla.Name);
            Assert.AreEqual("Srbin", tesla.Nationality);
            Assert.AreEqual(new DateTime(1856, 7, 9), tesla.DOB);
            Assert.AreEqual("Hrvatska", tesla.PlaceOfBirth.Country);
            Assert.AreEqual("Smiljan", tesla.PlaceOfBirth.City);
        }

        [Test]
        public void TestSerbianCyrillic()
        {
            Inventor tesla = CreateInventor(new CultureInfo(CultureInfoUtils.SerbianCyrillicCultureName));
            Assert.AreEqual("Никола Тесла", tesla.Name);
            Assert.AreEqual("Србин", tesla.Nationality);
            Assert.AreEqual(new DateTime(1856, 7, 9), tesla.DOB);
            Assert.AreEqual("Хрватска", tesla.PlaceOfBirth.Country);
            Assert.AreEqual("Смиљан", tesla.PlaceOfBirth.City);
        }

        [Test]
        public void NullReferenceHandling()
        {
//            ResourceSetMessageSource messageSource = new ResourceSetMessageSource();
//            messageSource.ResourceManagers.Add(new ResourceManager("Spring.Resources.Tesla", GetType().Assembly));
//            ResourceSetLocalizer localizer = new ResourceSetLocalizer();
            IMessageSource messageSource = CreateMessageSource();

            // target must not be null
            try
            {
                localizer.ApplyResources(null, messageSource, CultureInfo.InvariantCulture);
                Assert.Fail();
            }
            catch (ArgumentNullException) { }
            try
            {
                localizer.ApplyResources(null, messageSource);
                Assert.Fail();
            }
            catch (ArgumentNullException) { }

            // messageSource may be null
            localizer.ApplyResources(new object(), null);
            localizer.ApplyResources(new object(), null, CultureInfo.InvariantCulture);

            try
            {
                localizer.ApplyResources(new object(), messageSource, null);
                Assert.Fail();
            }
            catch (ArgumentNullException) { }
        }
#if !MONO
        [Test]
        public void DefaultResolvesUsingCurrentUICulture()
        {
            Inventor tesla = CreateInventor(null);
            Assert.AreEqual("Nikola Tesla", tesla.Name);
            Assert.AreEqual("Srbin", tesla.Nationality);
        }
#endif
        private Inventor CreateInventor(CultureInfo culture)
        {
            Inventor inventor = new Inventor();
            IMessageSource messageSource = CreateMessageSource();
            if (culture == null)
            {
                localizer.ApplyResources(inventor, messageSource);
            }
            else
            {
                localizer.ApplyResources(inventor, messageSource, culture);
            }
            return inventor;
        }

        protected abstract ILocalizer CreateLocalizer();
        protected abstract IMessageSource CreateMessageSource();
    }
}