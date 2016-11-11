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

using System;
using System.Collections;

using NUnit.Framework;
using Spring.Core;
using Spring.Globalization.Formatters;
using Spring.Validation;

namespace Spring.DataBinding
{
    /// <summary>
    /// Test cases for the BaseBindingManager.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [TestFixture]
    public class BaseBindingManagerTests
    {
        private BaseBindingManager mgr;

        [OneTimeSetUp]
        public void SetUp()
        {
            mgr = new BaseBindingManager();
            mgr.AddBinding("['name']", "Name");
            mgr.AddBinding("['dob']", "DOB");
            mgr.AddBinding("['dateofgraduation']", "DateOfGraduation");
            mgr.AddBinding("['inventions']", "Inventions");
            mgr.AddBinding("['cityOfBirth']", "PlaceOfBirth.City");
            mgr.AddBinding("['countryOfBirth']", "PlaceOfBirth.Country");
        }

        [Test]
        public void WithoutTypeConversion()
        {
            Hashtable source = new Hashtable();
            Inventor target = new Inventor();

            source["name"] = "Nikola Tesla";
            source["dob"] = new DateTime(1856, 7, 9);

            // I know this is pupin's graduation year but I need a date here
            source["dateofgraduation"] = new DateTime(1883,1,1);
            source["inventions"] = new string[] {"One", "Two"};

            mgr.BindSourceToTarget(source, target, null);

            Assert.IsTrue(mgr.HasBindings);
            Assert.AreEqual("Nikola Tesla", target.Name);
            Assert.AreEqual(new DateTime(1856, 7, 9), target.DOB);
            Assert.AreEqual(new string[] {"One", "Two"}, target.Inventions);
            Assert.IsNull(target.PlaceOfBirth.City);
            Assert.IsNull(target.PlaceOfBirth.Country);

            target.Name = "Tesla, Nikola";
            target.DOB = DateTime.Today;
            target.PlaceOfBirth.City = "Smiljan";
            target.PlaceOfBirth.Country = "Lika";

            mgr.BindTargetToSource(source, target, null);
            Assert.AreEqual("Tesla, Nikola", source["name"]);
            Assert.AreEqual(DateTime.Today, source["dob"]);
            Assert.AreEqual("One", ((string[]) source["inventions"])[0]);
            Assert.AreEqual("Smiljan", source["cityOfBirth"]);
            Assert.AreEqual("Lika", source["countryOfBirth"]);
        }

        [Test]
        public void WithTypeConversion()
        {
            Hashtable source = new Hashtable();
            Inventor target = new Inventor();

            source["name"] = "Nikola Tesla";
            source["dob"] = "1856-7-9";
            source["inventions"] = "One,Two";

            // I know this is pupin's graduation year but I need a date here
            source["dateofgraduation"] = "1883-1-1";

            mgr.BindSourceToTarget(source, target, null);

            Assert.IsTrue(mgr.HasBindings);
            Assert.AreEqual("Nikola Tesla", target.Name);
            Assert.AreEqual(new DateTime(1856, 7, 9), target.DOB);
            Assert.AreEqual(new string[] {"One", "Two"}, target.Inventions);
            Assert.IsNull(target.PlaceOfBirth.City);
            Assert.IsNull(target.PlaceOfBirth.Country);

            target.Name = "Tesla, Nikola";
            target.DOB = DateTime.Today;
            target.PlaceOfBirth.City = "Smiljan";
            target.PlaceOfBirth.Country = "Lika";

            mgr.BindTargetToSource(source, target, null);
            Assert.AreEqual("Tesla, Nikola", source["name"]);
            Assert.AreEqual(DateTime.Today, source["dob"]);
            Assert.AreEqual("One", ((string[]) source["inventions"])[0]);
            Assert.AreEqual("Smiljan", source["cityOfBirth"]);
            Assert.AreEqual("Lika", source["countryOfBirth"]);

        }

        [Test]
        public void BindNullValues()
        {
            Hashtable source;
            Inventor target;

            target = new Inventor();
            source = new Hashtable();

            // this is legal (dog is nullable)
            BaseBindingManager mgr = new BaseBindingManager();
            mgr.AddBinding("['dateofgraduation']", "DateOfGraduation");

            source["dateofgraduation"] = null;
            target.DateOfGraduation = DateTime.Now;
            mgr.BindSourceToTarget(source, target, null);
            Assert.IsNull(target.DateOfGraduation);

            source["dateofgraduation"] = DateTime.Now;
            mgr.BindTargetToSource(source, target, null);
            Assert.IsNull(source["dateofgraduation"]);
        }

        [Test]
        public void BindNullValuesWithFormatter()
        {
            Hashtable source;
            Inventor target;

            target = new Inventor();
            source = new Hashtable();

            // this is legal (dog is nullable)
            BaseBindingManager mgr = new BaseBindingManager();
            mgr.AddBinding("['dateofgraduation']", "DateOfGraduation", new HasTextFilteringFormatter(null, null));

            source["dateofgraduation"] = string.Empty;
            target.DateOfGraduation = DateTime.Now;
            mgr.BindSourceToTarget(source, target, null);
            Assert.IsNull(target.DateOfGraduation);
        }

        [Test]
        public void UnhandledTypeConversionExceptionSourceToTarget()
        {
            BaseBindingManager dbm = new BaseBindingManager();
            Hashtable source = new Hashtable();
            source["boolValue"] = false;
            Inventor target = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            
            dbm.AddBinding("['boolValue']", "DOB");

            try
            {
                dbm.BindSourceToTarget(source, target, null);
                Assert.Fail("Binding boolean to date should throw an exception.");
            }
            catch (TypeMismatchException)
            {}

            // binding state is not remembered with ValidationErrors=null!
            dbm.BindTargetToSource(source, target, null);
            Assert.AreEqual(target.DOB, source["boolValue"]);
        }

        [Test]
        public void UnhandledTypeConversionExceptionTargetToSource()
        {
            BaseBindingManager dbm = new BaseBindingManager();
            Inventor st = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            st.Inventions = new string[] {"Invention One", "Invention Two"};

            dbm.AddBinding("pob", "DOB");

            try
            {
                dbm.BindTargetToSource(st, st, null);
                Assert.Fail("Binding date to custom Place type should throw an exception.");
            }
            catch (TypeMismatchException)
            {}

            // binding state is not remembered with ValidationErrors=null!
            try
            {
                dbm.BindSourceToTarget(st, st, null);
                Assert.Fail("Binding custom Place to date type should throw an exception.");
            }
            catch (TypeMismatchException)
            {}
        }

        [Test]
        public void HandledTypeConversionExceptionSourceToTarget()
        {
            BaseBindingManager dbm = new BaseBindingManager();
            IValidationErrors errors = new ValidationErrors();
            Hashtable source = new Hashtable();
            source["boolValue"] = false;
            Inventor target = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");

            SimpleExpressionBinding binding = new SimpleExpressionBinding("['boolValue']", "DOB");
            binding.SetErrorMessage("error", "errors");
            dbm.AddBinding(binding);

            dbm.BindSourceToTarget(source, target, errors);
            Assert.IsFalse(binding.IsValid(errors));
            Assert.IsFalse(errors.IsEmpty);
            Assert.AreEqual(1, errors.GetErrors("errors").Count);

            // make sure that the old value doesn't override current invalid value
            dbm.BindTargetToSource(source, target, errors);
            Assert.AreEqual(false, source["boolValue"]);
        }

        [Test]
        public void HandledTypeConversionExceptionTargetToSource()
        {
            BaseBindingManager dbm = new BaseBindingManager();
            IValidationErrors errors = new ValidationErrors();
            Inventor st = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            st.Inventions = new string[] {"Invention One", "Invention Two"};

            SimpleExpressionBinding binding = new SimpleExpressionBinding("pob", "DOB");
            binding.SetErrorMessage("error", "errors");
            dbm.AddBinding(binding);

            dbm.BindTargetToSource(st, st, errors);
            Assert.IsFalse(binding.IsValid(errors));
            Assert.IsFalse(errors.IsEmpty);
            Assert.AreEqual(1, errors.GetErrors("errors").Count);

            // make sure that the old value doesn't override current invalid value
            dbm.BindSourceToTarget(st, st, errors);
            Assert.AreEqual(new DateTime(1856, 7, 9), st.DOB);
        }

        [Test]
        public void DirectionSourceToTarget()
        {
            BaseBindingManager dbm = new BaseBindingManager();
            Inventor source = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            Hashtable target = new Hashtable();
            
            dbm.AddBinding("Name", "['name']", BindingDirection.SourceToTarget);
            dbm.BindSourceToTarget(source, target, null);
            Assert.AreEqual("Nikola Tesla", target["name"]);

            target["name"] = "Mihajlo Pupin";
            dbm.BindTargetToSource(source, target, null);
            Assert.AreEqual("Nikola Tesla", source.Name);
        }

        [Test]
        public void DirectionTargetToSource()
        {
            BaseBindingManager dbm = new BaseBindingManager();
            Inventor source = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            Hashtable target = new Hashtable();
            
            dbm.AddBinding("Name", "['name']", BindingDirection.TargetToSource);
            dbm.BindSourceToTarget(source, target, null);
            Assert.IsNull(target["name"]);

            target["name"] = "Mihajlo Pupin";
            dbm.BindTargetToSource(source, target, null);
            Assert.AreEqual("Mihajlo Pupin", source.Name);
        }

//    	[Test]
//		public void ResetBindingStates()
//    	{
//    		BaseBindingManager dbm = new BaseBindingManager();
//			Inventor source = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
//			Hashtable target = new Hashtable();
//            
//			dbm.AddBinding("Name", "['name']");
//    		dbm.AddBinding("WrongProperty", "['wrongproperty']");
//    		
//			try
//			{
//				dbm.BindSourceToTarget(source, target, null);
//				throw new AssertionException("should throw InvalidPropertyException");
//			} catch(Spring.Objects.InvalidPropertyException) {} // ok
//    		
//    		// will ignore previously failed bindings - bindingState is false
//    		dbm.BindSourceToTarget(source, target, null);
//    		
//    		// now reset states
//    		dbm.ResetBindingStates();
//    		
//    		// now throws again
//			try
//			{
//				dbm.BindSourceToTarget(source, target, null);
//				throw new AssertionException("should throw InvalidPropertyException");
//			} 
//			catch(Spring.Objects.InvalidPropertyException) {} // ok    		
//    	}
    }
}