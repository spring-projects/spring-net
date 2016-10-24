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

using System.Globalization;
using System.Resources;

using NUnit.Framework;

using Spring.Context;
using Spring.Context.Support;

#endregion

namespace Spring.Globalization.Localizers
{
    /// <summary>
    /// Unit tests for the ResourceSetLocalizer class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [TestFixture]
    public sealed class ResourceSetLocalizerTests : AbstractLocalizerTests
    {
        protected override ILocalizer CreateLocalizer()
        {
            return new ResourceSetLocalizer();
        }

        protected override IMessageSource CreateMessageSource()
        {
            ResourceSetMessageSource messageSource = new ResourceSetMessageSource();
            messageSource.ResourceManagers.Add(new ResourceManager("Spring.Resources.Tesla", GetType().Assembly));
            return messageSource;
        }

        [Test]
        public void DoesNotThrowOnMissingResource()
        {
            ResourceSetMessageSource messageSource = new ResourceSetMessageSource();
            messageSource.ResourceManagers.Add(new ResourceManager("do not exist", GetType().Assembly));
            ResourceSetLocalizer localizer = new ResourceSetLocalizer();
            localizer.ApplyResources(new Inventor(), messageSource, CultureInfo.InvariantCulture);
        }
    }
}
