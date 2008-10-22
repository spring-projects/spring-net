#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using NUnit.Framework;
using Spring.Globalization;
using Spring.Objects;

#endregion

namespace Spring.Context.Support
{
    /// <summary>
    /// Unit tests for the ResourceSetMessageSource class.
    /// </summary>
    /// <author></author>
    [TestFixture]
    public sealed class ResourceSetMessageSourceTests
    {
        private ResourceSetMessageSource messageSource;

        private const string ResourceNamespace = "Spring.Resources";
        private const string ResourceFileName = "Spring.Context.Tests";
        // The namespace is added during the build...
        private const string ResourceBaseName = ResourceNamespace + "." + ResourceFileName;
        private IList resourceManagerList;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            CultureTestScope.Set();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            CultureTestScope.Reset();
        }

        /// <summary>
        /// Populate a known ResourceManager in the resourceManagerList.
        /// </summary>
        [SetUp]
        public void Init()
        {
            messageSource = new ResourceSetMessageSource();
            resourceManagerList = new ArrayList();
            Assembly ass = this.GetType().Assembly;
            resourceManagerList.Add(new ResourceManager(ResourceBaseName, ass));
        }

        [TearDown]
        public void Destroy()
        {
            messageSource = null;
        }

        public void DoTestMessageAccess(bool hasParentContext, bool useCodeAsDefaultMessage)
        {
            StaticApplicationContext ac = new StaticApplicationContext();
            if (hasParentContext)
            {
                StaticApplicationContext parent = new StaticApplicationContext();
                parent.Refresh();
                ac.ParentContext = parent;
            }

            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("resourceManagers", resourceManagerList);
            if (useCodeAsDefaultMessage)
            {
                pvs.Add("UseCodeAsDefaultMessage", true);
            }
            ac.RegisterSingleton("messageSource", typeof(ResourceSetMessageSource), pvs);
            ac.Refresh();


            // Localizaiton fallbacks
            GetMessageLocalizaitonFallbacks(ac);

            // MessageSourceAccessor functionality
            MessageSourceAccessor accessor = new MessageSourceAccessor(ac);
            Assert.AreEqual("message3", accessor.GetMessage("code3", CultureInfo.CurrentUICulture, (object[])null));

            // IMessageSourceResolveable
            Assert.AreEqual("message3", ac.GetMessage("code3", CultureInfo.CurrentUICulture, (object[]) null));
            IMessageSourceResolvable resolvable = new DefaultMessageSourceResolvable("code3");
            
            Assert.AreEqual("message3", ac.GetMessage(resolvable, CultureInfo.CurrentUICulture));
            resolvable = new DefaultMessageSourceResolvable(new string[] {"code4", "code3"});
            Assert.AreEqual("message3", ac.GetMessage(resolvable, CultureInfo.CurrentUICulture));

            Assert.AreEqual("message3", ac.GetMessage("code3", CultureInfo.CurrentUICulture, (object[])null));
            resolvable = new DefaultMessageSourceResolvable(new string[] { "code4", "code3" });
            Assert.AreEqual("message3", ac.GetMessage(resolvable, CultureInfo.CurrentUICulture));

            object[] arguments = new object[] { "Hello", new DefaultMessageSourceResolvable(new string[]{"code1"}) };
            Assert.AreEqual("Hello, message1", ac.GetMessage("hello", CultureInfo.CurrentUICulture, arguments));
            

            // test default message without and with args
            Assert.AreEqual("default", ac.GetMessage(null, "default", CultureInfo.CurrentUICulture, null));
            Assert.AreEqual("default", ac.GetMessage(null, "default", CultureInfo.CurrentUICulture, arguments));

            /* not supported 
            Assert.AreEqual("{0}, default", ac.GetMessage(null, "{0}, default", CultureInfo.CurrentUICulture, null));
             */
            
            Assert.AreEqual("Hello, default", ac.GetMessage(null, "{0}, default", CultureInfo.CurrentUICulture, arguments));

            // test resolvable with default message, without and with args
            resolvable = new DefaultMessageSourceResolvable(null, null, "default");
            Assert.AreEqual("default", ac.GetMessage(resolvable, CultureInfo.CurrentUICulture));
            resolvable = new DefaultMessageSourceResolvable(null, arguments, "default");
            Assert.AreEqual("default", ac.GetMessage(resolvable, CultureInfo.CurrentUICulture));

            /* not supported 
                resolvable = new DefaultMessageSourceResolvable(null, null, "{0}, default");
                Assert.AreEqual("{0}, default", ac.GetMessage(resolvable, CultureInfo.CurrentUICulture));
            */

            resolvable = new DefaultMessageSourceResolvable(null, arguments, "{0}, default");
            Assert.AreEqual("Hello, default", ac.GetMessage(resolvable, CultureInfo.CurrentUICulture));


            // test message args
            Assert.AreEqual("Arg1, Arg2", ac.GetMessage("hello", CultureInfo.CurrentUICulture, new object[]{"Arg1", "Arg2"}));

            /* not supported 
                Assert.AreEqual("{0}, {1}", ac.GetMessage("hello", CultureInfo.CurrentUICulture, null));
            */


            /* not supported 
                Assert.AreEqual("Hello\nWorld", ac.GetMessage("escaped"));
            }
            else
            {
                Assert.AreEqual("Hello\\nWorld", ac.GetMessage("escaped"));
            }
            */


            try
            {
                Assert.AreEqual("MyNonExistantMessage", ac.GetMessage("MyNonExistantMessage"));
                if (!useCodeAsDefaultMessage)
                {
                    Assert.Fail("Should have thrown NoSuchMessagException");
                }
            }
            catch (NoSuchMessageException e)
            {
                if (useCodeAsDefaultMessage)
                {
                    Assert.Fail("Should have returned code as default message.");
                    e.ToString();
                }
            }

        }

        [Test]
        public void SimpleFormat()
        {
            string msg = String.Format(CultureInfo.CurrentUICulture, "\"Hello World\"");
            Assert.AreEqual("\"Hello World\"", msg);
        }
#if !MONO
        [Test]
        public void MessageAccessFallbackTurnedOff()
        {
            DoTestMessageAccess(false, false);
        }


        [Test]
        public void MessageAccessFallbackTurnedOn()
        {
            DoTestMessageAccess(false, true);
        }


        [Test]
        public void MessageAccessWithParentAndFallbackTurnedOff()
        {
            DoTestMessageAccess(true, false);
        }
#endif

        /// <summary>
        /// Test to string shows expected message with base name listing.
        /// </summary>
        [Test]
        public void ResourceSetMessageSourceToString()
        {
            messageSource.ResourceManagers = resourceManagerList;
            Assert.AreEqual("ResourceSetMessageSource with ResourceManagers of base names = [Spring.Resources.Spring.Context.Tests]",
                            messageSource.ToString(), "ToString not as expected");
        }
#if !MONO
        /// <summary>
        /// Happy day scenario where the requested message key is found and substitutions are made.
        /// </summary>
        [Test]
        public void ResourceSetMessageSourceGetMessage()
        {
            messageSource.ResourceManagers = resourceManagerList;
            GetMessageLocalizaitonFallbacks(messageSource);
        }
#endif
        private void GetMessageLocalizaitonFallbacks(IMessageSource msgSource)
        {
            Assert.AreEqual("This is Spring.NET",
                            msgSource.GetMessage("MyMessage", new object[] { "Spring", ".NET" }), "message not as expected");

            Assert.AreEqual("Isso e Spring.NET",
                            msgSource.GetMessage("MyMessage", new CultureInfo("pt-BR"), new object[] { "Spring", ".NET" }), "message not as expected");

            Assert.AreEqual("Visual Studio loves Spring.NET",
                            msgSource.GetMessage("MyNewMessage", new object[] { "Spring", ".NET" }), "message not as expected");

            // test localization fallbacks
            Assert.AreEqual("Visual Studio loves Spring.NET",
                            msgSource.GetMessage("MyNewMessage", new CultureInfo("pt-BR"), new object[] { "Spring", ".NET" }), "message not as expected");

            Assert.AreEqual("Ovo je Spring.NET",
                            msgSource.GetMessage("MyMessage", new CultureInfo(CultureInfoUtils.SerbianLatinCultureName), new object[] { "Spring", ".NET" }), "message not as expected");

            Assert.AreEqual("Ово је Spring.NET",
                            msgSource.GetMessage("MyMessage", new CultureInfo(CultureInfoUtils.SerbianCyrillicCultureName),
                                                     new object[] { "Spring", ".NET" }), "message not as expected");

            Assert.AreEqual("Visual Studio voli Spring.NET",
                            msgSource.GetMessage("MyNewMessage", new CultureInfo(CultureInfoUtils.SerbianCyrillicCultureName), new object[] { "Spring", ".NET" }), "message not as expected");

            Assert.AreEqual("First name",
                            msgSource.GetMessage("field.firstname", new CultureInfo(CultureInfoUtils.SerbianCyrillicCultureName)), "message not as expected");
        }

        /// <summary>
        /// Test the happy day scenario of returning an object
        /// </summary>
        [Test]
        public void GetResource()
        {
            //Add another resource manager to the list
            Assembly ass = Assembly.LoadWithPartialName("Spring.Core.Tests");
            string baseName = "Spring" + "." + "Resources.Images";

            resourceManagerList.Add(new ResourceManager(baseName, ass));
            messageSource.ResourceManagers = resourceManagerList;
            object obj = messageSource.GetResourceObject("bubblechamber", CultureInfo.CurrentCulture);
            Assert.IsNotNull(obj, "expected to retrieve object form resource set");
            Bitmap bitMap = null;

            //.NET 1.0 returns this as a base64 string while .NET 1.1 returns it as a Bitmap
            //There are some isues with resx compatability between framework versions.
            if (obj is string)
            {
                //thanks dotnetdave for the string to bitmap cookbook.
                //http://www.vsdntips.com/Tips/VS.NET/Csharp/76.aspx
                string ImageText = obj as string;
                Byte[] bitmapData = new Byte[ImageText.Length];
                bitmapData = Convert.FromBase64String(FixBase64ForImage(ImageText));
                MemoryStream streamBitmap = new MemoryStream(bitmapData);
                bitMap = new Bitmap((Bitmap)Image.FromStream(streamBitmap));

            }
            else
            {
                bitMap = obj as Bitmap;
            }
            Assert.IsNotNull(bitMap, "expected to retrieve BitMap. Instead Type = " + obj.GetType());
            Assert.AreEqual(146, bitMap.Size.Width, "Width of image wrong");
            Assert.AreEqual(119, bitMap.Size.Height, "Height of image wrong");

            object obj2 = messageSource.GetResourceObject("notExist", CultureInfo.CurrentCulture);
            Assert.IsNull(obj2, "non existent resourse found");
        }

        private string FixBase64ForImage(string Image)
        {
            StringBuilder sbText = new StringBuilder(Image, Image.Length);
            sbText.Replace("\r\n", String.Empty);
            sbText.Replace(" ", String.Empty);
            return sbText.ToString();
        }

#if ! NET_1_0
        /// <summary>
        /// Test applying a family of resources to an object.
        /// </summary>
        [Test]
        public void ApplyResources()
        {
            //Add another resource manager to the list
            TestObject to = new TestObject();
            ComponentResourceManager mgr = new ComponentResourceManager(to.GetType());
            resourceManagerList.Add(mgr);
            messageSource.ResourceManagers = resourceManagerList;

            messageSource.ApplyResources(to, "testObject", CultureInfo.CurrentCulture);
            Assert.AreEqual("Mark", to.Name);
            Assert.AreEqual(35, to.Age);
        }
#endif
#if !MONO
        /// <summary>
        /// Test when the code being resolves itself implements IMessageResolvable.
        /// </summary>
        [Test]
        public void MessageResolveableGetMessage()
        {
            string[] codes = { "field.firstname" };
            DefaultMessageSourceResolvable dmr = new DefaultMessageSourceResolvable(codes, null);

            messageSource.ResourceManagers = resourceManagerList;
            Assert.AreEqual(messageSource.GetMessage("error.required", CultureInfo.CurrentCulture, dmr, "dude!"), "First name is required dude!", "message not as expected");
        }
#endif
        /// <summary>
        /// Get exception when resource doesn't exist.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NoSuchMessageException))]
        public void ResourceSetMessageSourceGetNonExistantResource()
        {
            messageSource.ResourceManagers = resourceManagerList;
            messageSource.GetMessage("MyNonExistantMessage", CultureInfo.CurrentCulture, new object[] { "Spring", ".NET" });
        }

#if !NET_2_0
		/// <summary>
		/// Test resource whose value is null
		/// </summary>
		[Test]
		public void ResourceSetMessageSourceGetNullResource()
		{
			messageSource.ResourceManagers = resourceManagerList;
			object resource = messageSource.GetResourceObject("MyNullMessage", CultureInfo.InvariantCulture);
			Assert.AreEqual(String.Empty, resource, "should've returned empty string");
		}
#endif

#if NET_2_0
        /// <summary>
        /// Test resource whose value is null.
        /// </summary>
        [Test]
        [Ignore("See SPRNET-246")]
        public void GetNullResourceIn2PointOh()
        {
            messageSource.ResourceManagers = resourceManagerList;
            object resource = messageSource.GetResourceObject("MyNullMessage", CultureInfo.InvariantCulture);
            Assert.AreEqual(String.Empty, resource, "should've returned empty string");
        }
#endif

        /// <summary>
        /// Exercise repeated requests to hit the cache of MessageSets.  Note order of test is not assured
        /// in NUnit.
        /// </summary>
        [Test]
        public void ResourceSetMessageSourceCachedMessage()
        {
            messageSource.ResourceManagers = resourceManagerList;
            Assert.AreEqual("This is Spring.NET", messageSource.GetMessage("MyMessage", CultureInfo.InvariantCulture, new object[] { "Spring", ".NET" }), "message");
            Assert.AreEqual("Visual Studio loves Spring.NET", messageSource.GetMessage("MyNewMessage", CultureInfo.InvariantCulture, "Spring", ".NET"), "message");
            Assert.AreEqual("This is Spring.NET", messageSource.GetMessage("MyMessage", CultureInfo.InvariantCulture, new object[] { "Spring", ".NET" }), "message");
        }

        /// <summary>
        /// Test when there are two resource managers listed.
        /// </summary>
        [Test]
        public void TwoResourceManagers()
        {
            //Add another resource manager to the list
            Assembly ass = Assembly.LoadWithPartialName("Spring.Core.Tests");
            string baseName = "Spring" + "." + "Resources.SampleResources";
            resourceManagerList.Add(new ResourceManager(baseName, ass));

            messageSource.ResourceManagers = resourceManagerList;

            //Repeat the test for the first resource manager

            Assert.AreEqual("This is Spring.NET",
                            messageSource.GetMessage("MyMessage", new object[] { "Spring", ".NET" }), "message not as expected");

            //Now with the newly added one
            Assert.AreEqual("Hello Mr. Anderson",
                            messageSource.GetMessage("message",
                                                     new object[] { "Mr.", "Anderson" }), "message not as expected");
        }
    }
}