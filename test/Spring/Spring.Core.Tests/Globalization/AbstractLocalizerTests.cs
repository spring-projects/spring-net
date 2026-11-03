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

using System.Globalization;
using NUnit.Framework;
using Spring.Context;

namespace Spring.Globalization;

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
        Assert.That(tesla.Name, Is.EqualTo("Nikola Tesla"));
        Assert.That(tesla.Nationality, Is.EqualTo("Serbian"));
        Assert.That(tesla.DOB, Is.EqualTo(new DateTime(1856, 7, 9)));
        Assert.That(tesla.PlaceOfBirth.Country, Is.EqualTo("Croatia"));
        Assert.That(tesla.PlaceOfBirth.City, Is.EqualTo("Smiljan"));
    }

    [Test]
    public void TestSerbianLatin()
    {
        Inventor tesla = CreateInventor(new CultureInfo(CultureInfoUtils.SerbianLatinCultureName));
        Assert.That(tesla.Name, Is.EqualTo("Nikola Tesla"));
        Assert.That(tesla.Nationality, Is.EqualTo("Srbin"));
        Assert.That(tesla.DOB, Is.EqualTo(new DateTime(1856, 7, 9)));
        Assert.That(tesla.PlaceOfBirth.Country, Is.EqualTo("Hrvatska"));
        Assert.That(tesla.PlaceOfBirth.City, Is.EqualTo("Smiljan"));
    }

    [Test]
    public void TestSerbianCyrillic()
    {
        Inventor tesla = CreateInventor(new CultureInfo(CultureInfoUtils.SerbianCyrillicCultureName));
        Assert.That(tesla.Name, Is.EqualTo("Никола Тесла"));
        Assert.That(tesla.Nationality, Is.EqualTo("Србин"));
        Assert.That(tesla.DOB, Is.EqualTo(new DateTime(1856, 7, 9)));
        Assert.That(tesla.PlaceOfBirth.Country, Is.EqualTo("Хрватска"));
        Assert.That(tesla.PlaceOfBirth.City, Is.EqualTo("Смиљан"));
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
        Assert.That(tesla.Name, Is.EqualTo("Nikola Tesla"));
        Assert.That(tesla.Nationality, Is.EqualTo("Srbin"));
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
