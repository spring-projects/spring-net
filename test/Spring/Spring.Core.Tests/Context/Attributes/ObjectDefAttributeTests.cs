#region License

/*
 * Copyright © 2010-2011 the original author or authors.
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

using NUnit.Framework;

namespace Spring.Context.Attributes
{
    [TestFixture]
    public class ObjectDefAttributeTests
    {
        [Test]
        public void Can_Accept_Single_Name()
        {
            var def = new ObjectDefAttribute();

            def.Names = "Steve";

            Assert.That(def.NamesToArray[0], Is.EqualTo("Steve"));
        }


        [Test]
        public void Can_Accept_Multiple_Names()
        {
            var def = new ObjectDefAttribute();
            var names = "Name1,Name2,Name3";

            def.Names = names;
            Assert.That(def.NamesToArray[0], Is.EqualTo("Name1"));
            Assert.That(def.NamesToArray[1], Is.EqualTo("Name2"));
            Assert.That(def.NamesToArray[2], Is.EqualTo("Name3"));

        }

        
    }
}
