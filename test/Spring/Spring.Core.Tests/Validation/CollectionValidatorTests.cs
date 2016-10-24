using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Spring.Collections;
using Spring.Core.IO;
using Spring.Expressions;
using Spring.Objects;
using Spring.Objects.Factory.Xml;
using Spring.Validation.Actions;

namespace Spring.Validation 
{
    /// <summary>
    /// Unit tests for the CollectionValidator class.
    /// </summary>
    /// <author>Damjan Tomic</author>
    [TestFixture]
    public class CollectionValidatorTests
    {
        [Test]
        public void DefaultsToFastValidateIsFalse()
        {
            CollectionValidator cg = new CollectionValidator();
            Assert.IsFalse(cg.FastValidate);
        }

        [Test]
        public void TestCollection()
        {
            IList persons = new ArrayList();

            persons.Add(new TestObject("Damjan Tomic", 24));
            persons.Add(new TestObject("Goran Milosavljevic", 24));
            persons.Add(new TestObject("Ivan Cikic", 28));
            
            RequiredValidator req = new RequiredValidator("Name", "true");

            RegularExpressionValidator reg = new RegularExpressionValidator("Name", "true", @"[a-z]*\s[a-z]*");
            reg.Options = RegexOptions.IgnoreCase;
            
            CollectionValidator validator = new CollectionValidator();
            validator.Validators.Add(req);
            validator.Validators.Add(reg);                        
            
            Assert.IsTrue(validator.Validate(persons, new ValidationErrors()));
        }

        [Test]
        public void TestDifferentCollectionTypes()
        {
            const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
            <objects xmlns='http://www.springframework.net' xmlns:v='http://www.springframework.net/validation'>
          <v:group id='validatePerson' when='T(Spring.Objects.TestObject) == #this.GetType()'>
            <v:required id ='req' when='true' test='Name'/>
            <v:regex id ='reg' test='Name'>
              <v:property name='Expression' value='[a-z]*\s[a-z]*'/>
              <v:property name='Options' value='IgnoreCase'/>
              <v:message id='reg1' providers='regularni' when='true'>
                 <v:param value='#this.ToString()'/> 
              </v:message>
            </v:regex>
          </v:group>  
           <v:collection id='collectionValidator' validate-all='true'>
                <v:ref name='validatePerson'/>
           </v:collection>
           </objects>";

            MemoryStream stream = new MemoryStream(new UTF8Encoding().GetBytes(xml));
            IResource resource = new InputStreamResource(stream, "collectionValidator");

            XmlObjectFactory objectFactory = new XmlObjectFactory(resource, null);
            CollectionValidator validator = (CollectionValidator) objectFactory.GetObject("collectionValidator");
            
            IList listPersons = new ArrayList();
            IDictionary dictPersons = new Hashtable();
            ISet setPersons = new ListSet();  

            listPersons.Add(new TestObject("DAMJAN Tomic", 24));
            listPersons.Add(new TestObject("Goran Milosavljevic", 24));
            listPersons.Add(new TestObject("Ivan CIKIC", 28));

            dictPersons.Add(1, listPersons[0]);
            dictPersons.Add(2, listPersons[1]);
            dictPersons.Add(3, listPersons[2]);

            setPersons.AddAll(listPersons);
            IValidationErrors ve = new ValidationErrors();

            Assert.IsTrue(validator.Validate(listPersons, ve));                        
            Assert.IsTrue(ve.IsEmpty);                                    
            Assert.IsTrue(validator.Validate(dictPersons, ve));
            Assert.IsTrue(ve.IsEmpty);
            Assert.IsTrue(validator.Validate(setPersons, ve));
            Assert.IsTrue(ve.IsEmpty);
        }


        [Test]
        public void TestWithWrongArgumentType()
        {
            RequiredValidator req = new RequiredValidator("Name", "true");
            CollectionValidator validator = new CollectionValidator();
            validator.Validators.Add(req);
            
            TestObject tObj = new TestObject("Damjan Tomic", 24);

            //This should cause the ArgumentException because tObj is not a Collection
            Assert.Throws<ArgumentException>(() => validator.Validate(tObj, new ValidationErrors()));
        }

        [Test]
        public void TestValidationErrorsAreCollected()
        {
            IList persons = new ArrayList();

            persons.Add(new TestObject(null, 24));
            persons.Add(new TestObject("Goran Milosavljevic", 24));
            persons.Add(new TestObject("Ivan Cikic", 28));
            persons.Add(new TestObject(null, 20));

            RequiredValidator req = new RequiredValidator("Name", "true");
            req.Actions.Add(new ErrorMessageAction("1", new string[] { "firstProvider", "secondProvider" }));

            CollectionValidator validator = new CollectionValidator(true,true);
            
            validator.Validators.Add(req);

            IValidationErrors ve = new ValidationErrors();

            Assert.IsFalse(validator.Validate(persons, ve));
            Assert.IsFalse(ve.IsEmpty);
            
        }
        
        [Test]
        public void TestWithNull()
        {
            CollectionValidator validator = new CollectionValidator();
            //This should cause the ArgumentException because we passed null into Validate method
            Assert.Throws<ArgumentException>(() => validator.Validate(null, new ValidationErrors()));
        }
        
        [Test]
        public void TestNestingCollectionValidator()
        {            
            Society soc = new Society();
            soc.Members.Add(new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian"));
            soc.Members.Add(new Inventor("Mihajlo Pupin", new DateTime(1854, 10, 9), "Serbian"));
            
            
            RequiredValidator req = new RequiredValidator("Name", "true");

            RegularExpressionValidator reg = new RegularExpressionValidator("Name", "true", @"[a-z]*\s[a-z]*");
            reg.Options = RegexOptions.IgnoreCase;
            
            CollectionValidator validator = new CollectionValidator();
            validator.Validators.Add(req);
            validator.Validators.Add(reg);                        
                       
            validator.Context = Expression.Parse("Members");
            
            Assert.IsTrue(validator.Validate(soc, new ValidationErrors()));
            
            validator.Context = null;
            Assert.IsTrue(validator.Validate(soc.Members, new ValidationErrors()));
        }
        
        [Test]
        public void TestNestingCollectionValidatorWithXMLDescription()
        {     
              const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
            <objects xmlns='http://www.springframework.net' xmlns:v='http://www.springframework.net/validation'>
              <v:group id='validatePerson' when='T(Spring.Objects.TestObject) == #this.GetType()'>
                <v:required id ='req' when='true' test='Name'/>
                <v:regex id ='reg' test='Name'>
                  <v:property name='Expression' value='[a-z]*\s[a-z]*'/>
                  <v:property name='Options' value='IgnoreCase'/>
                  <v:message id='reg1' providers='regExpr' when='true'>
                     <v:param value='#this.ToString()'/> 
                  </v:message>
                </v:regex>
              </v:group>  
           
              <v:group id='validator'>
                  <v:collection id='collectionValidator' validate-all='true' context='Members' include-element-errors='true'>
                      <v:message id='coll1' providers='membersCollection' when='true'>
                          <v:param value='#this.ToString()'/> 
                      </v:message>
                      <v:ref name='validatePerson'/>
                  </v:collection>     
              </v:group>  
           </objects>";

            MemoryStream stream = new MemoryStream(new UTF8Encoding().GetBytes(xml));
            IResource resource = new InputStreamResource(stream, "collection validator test");

            XmlObjectFactory objectFactory = new XmlObjectFactory(resource, null);                        
                     
            ValidatorGroup validator = (ValidatorGroup) objectFactory.GetObject("validator");
            Society soc = new Society();
                         
            soc.Members.Add(new TestObject("Damjan Tomic", 24));
            soc.Members.Add(new TestObject("Goran Milosavljevic", 24));
            soc.Members.Add(new TestObject("Ivan Cikic", 28));
           
            IValidationErrors err1 = new ValidationErrors();
            
            Assert.IsTrue(validator.Validate(soc, err1));
            
            soc.Members.Add(new TestObject("foo", 30));
            soc.Members.Add(new TestObject("bar", 30));
            Assert.IsFalse(validator.Validate(soc, err1));
             
        }                           
    }            
}
