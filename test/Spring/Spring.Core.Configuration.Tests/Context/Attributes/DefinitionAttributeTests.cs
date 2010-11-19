using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Spring.Context.Attributes;

namespace Spring.Context.Attributes
{
    [TestFixture]
    public class DefinitionAttributeTests
    {
        [Test]
        public void Can_Accept_Single_Name()
        {
            var def = new DefinitionAttribute();

            def.Names = "Steve";

            Assert.That(def.NamesToArray[0], Is.EqualTo("Steve"));
        }


        [Test]
        public void Can_Accept_Multiple_Names()
        {
            var def = new DefinitionAttribute();
            var names = "Name1,Name2,Name3";

            def.Names = names;
            Assert.That(def.NamesToArray[0], Is.EqualTo("Name1"));
            Assert.That(def.NamesToArray[1], Is.EqualTo("Name2"));
            Assert.That(def.NamesToArray[2], Is.EqualTo("Name3"));

        }

        
    }
}
