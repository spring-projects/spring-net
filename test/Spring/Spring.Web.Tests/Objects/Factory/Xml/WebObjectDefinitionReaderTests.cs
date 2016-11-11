#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using System.Xml;
using NUnit.Framework;

using Spring.Core.IO;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.TestSupport;

namespace Spring.Objects.Factory
{
    /// <summary>
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class WebObjectDefinitionReaderTests
    {
        public class TestWebObjectDefinitionReader : WebObjectDefinitionReader
        {
            public TestWebObjectDefinitionReader(string contextVirtualPath, IObjectDefinitionRegistry registry, XmlResolver resolver) 
                : base(contextVirtualPath, registry, resolver)
            {}
        }

        [Test]
        public void ControlDefinitionsGetMarkedAbstract()
        {
            const string CONTEXTPATH = "/ContextPath/";
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>
    <object type='MyControl.ascx' />
</objects>";

            WebObjectFactory objectFactory = new WebObjectFactory(CONTEXTPATH, false);
            TestWebObjectDefinitionReader reader = new TestWebObjectDefinitionReader(objectFactory.ContextPath, objectFactory, new XmlUrlResolver());

            using (VirtualEnvironmentMock env = new VirtualEnvironmentMock(CONTEXTPATH + "test.aspx", null, null, CONTEXTPATH, true))
            {
                env.VirtualPath2ArtifactsTable[CONTEXTPATH + "MyControl.ascx"] = typeof(Spring.Web.UI.UserControl);

                reader.LoadObjectDefinitions(new StringResource(xml));
            }

            Assert.IsTrue(objectFactory.ContainsObjectDefinition("Spring.Web.UI.UserControl"));
            Assert.IsTrue(objectFactory.GetObjectDefinition("Spring.Web.UI.UserControl").IsAbstract);
        }

        [Test]
        public void ParsesPagePathIntoObjectNameIfNeitherIdNorNameAttributeSpecified()
        {
            const string CONTEXTPATH = "/ContextPath/";

            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>
    <object type='MyPage.aspx' />
    <object type='~/MyControl.ascx' />
</objects>";

            WebObjectFactory objectFactory = new WebObjectFactory(CONTEXTPATH, false);
            TestWebObjectDefinitionReader reader = new TestWebObjectDefinitionReader(objectFactory.ContextPath, objectFactory, new XmlUrlResolver());

            using (VirtualEnvironmentMock env = new VirtualEnvironmentMock(CONTEXTPATH + "test.aspx", null, null, CONTEXTPATH, true))
            {
                env.VirtualPath2ArtifactsTable[CONTEXTPATH + "MyPage.aspx"] = typeof (Spring.Web.UI.Page);
                env.VirtualPath2ArtifactsTable[CONTEXTPATH + "MyControl.ascx"] = typeof (Spring.Web.UI.UserControl);

                reader.LoadObjectDefinitions(new StringResource(xml));
            }

            Assert.IsTrue(objectFactory.ContainsObjectDefinition("/MyPage.aspx"));
            Assert.AreEqual(typeof(Spring.Web.UI.Page), objectFactory.GetType("/MyPage.aspx"));
            Assert.IsTrue(objectFactory.ContainsObjectDefinition("Spring.Web.UI.UserControl"));
        }

        [Test]
        public void DoesNotGenerateObjectNameIfIdAttributeSpecified()
        {
            const string CONTEXTPATH = "/ContextPath/";
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>
    <object id='mypage' type='MyPage.aspx' />
    <object id='mycontrol' type='MyControl.ascx' />
</objects>";

            WebObjectFactory objectFactory = new WebObjectFactory(CONTEXTPATH, false);
            TestWebObjectDefinitionReader reader = new TestWebObjectDefinitionReader(objectFactory.ContextPath, objectFactory, new XmlUrlResolver());

            using (VirtualEnvironmentMock env = new VirtualEnvironmentMock(CONTEXTPATH + "test.aspx", null, null, CONTEXTPATH, true))
            {
                env.VirtualPath2ArtifactsTable[CONTEXTPATH + "MyPage.aspx"] = typeof (Spring.Web.UI.Page);
                env.VirtualPath2ArtifactsTable[CONTEXTPATH + "MyControl.ascx"] = typeof (Spring.Web.UI.UserControl);

                reader.LoadObjectDefinitions(new StringResource(xml));
            }

            Assert.IsTrue(objectFactory.ContainsObjectDefinition("mypage"));
            Assert.AreEqual(typeof(Spring.Web.UI.Page), objectFactory.GetType("mypage"));
            Assert.IsTrue(objectFactory.ContainsObjectDefinition("mycontrol"));
        }

        [Test]
        public void DoesNotGenerateObjectNameIfNameAttributeSpecified()
        {
            const string CONTEXTPATH = "/ContextPath/";
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>
    <object name='mypage' type='MyPage.aspx' />
    <object name='mycontrol' type='MyControl.ascx' />
</objects>";

            WebObjectFactory objectFactory = new WebObjectFactory(CONTEXTPATH, false);
            TestWebObjectDefinitionReader reader = new TestWebObjectDefinitionReader(objectFactory.ContextPath, objectFactory, new XmlUrlResolver());

            using (VirtualEnvironmentMock env = new VirtualEnvironmentMock(CONTEXTPATH + "test.aspx", null, null, CONTEXTPATH, true))
            {
                env.VirtualPath2ArtifactsTable[CONTEXTPATH + "MyPage.aspx"] = typeof(Spring.Web.UI.Page);
                env.VirtualPath2ArtifactsTable[CONTEXTPATH + "MyControl.ascx"] = typeof(Spring.Web.UI.UserControl);

                reader.LoadObjectDefinitions(new StringResource(xml));
            }

            Assert.IsTrue(objectFactory.ContainsObjectDefinition("mypage"));
            Assert.AreEqual(typeof(Spring.Web.UI.Page), objectFactory.GetType("mypage"));
            Assert.IsTrue(objectFactory.ContainsObjectDefinition("mycontrol"));
        }

        [Test]
        public void DoesNotGenerateObjectNameIfIdAndNameAttributeSpecified()
        {
            const string CONTEXTPATH = "/";
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>
    <object id='mypage' name='mypageAlias' type='~/MyPage.aspx' />
    <object id='mycontrol' name='mycontrolAlias' type='~/MyControl.ascx' />
</objects>";

            WebObjectFactory objectFactory = new WebObjectFactory(CONTEXTPATH, false);
            TestWebObjectDefinitionReader reader = new TestWebObjectDefinitionReader(objectFactory.ContextPath, objectFactory, new XmlUrlResolver());

            using (VirtualEnvironmentMock env = new VirtualEnvironmentMock(CONTEXTPATH + "test.aspx", null, null, CONTEXTPATH, true))
            {
                env.VirtualPath2ArtifactsTable[CONTEXTPATH + "MyPage.aspx"] = typeof(Spring.Web.UI.Page);
                env.VirtualPath2ArtifactsTable[CONTEXTPATH + "MyControl.ascx"] = typeof(Spring.Web.UI.UserControl);

                reader.LoadObjectDefinitions(new StringResource(xml));
            }

            Assert.IsTrue(objectFactory.ContainsObjectDefinition("mypage"));
            Assert.IsTrue(objectFactory.ContainsObject("mypageAlias"));
            Assert.IsTrue(objectFactory.ContainsObjectDefinition("mycontrol"));
            Assert.IsTrue(objectFactory.ContainsObject("mycontrolAlias"));
        }
    }
}