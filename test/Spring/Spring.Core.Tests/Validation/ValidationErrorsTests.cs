#region License

/*
 * Copyright 2004 the original author or authors.
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
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using NUnit.Framework;

using Spring.Context.Support;

#endregion

namespace Spring.Validation
{
	/// <summary>
	/// Unit tests for the ValidationErrors class.
	/// </summary>
	/// <author>Rick Evans</author>
    /// <author>Goran Milosavljevic</author>
	[TestFixture]
	public sealed class ValidationErrorsTests
	{
		private const string GoodErrorKey = "key";
		private ErrorMessage ErrorMessageTwo = new ErrorMessage("This Is Eva Green", null);
		private ErrorMessage ErrorMessageOne = new ErrorMessage("Kissing Leads To Brain Disease", null);

        [Test]
        public void ResolveErrorsWithoutMessageSource()
        {
            ValidationErrors errors = new ValidationErrors();
            errors.AddError(string.Empty, ErrorMessageOne);
            IList<string> resolvedErrors = errors.GetResolvedErrors(string.Empty, null);
            Assert.AreEqual(ErrorMessageOne.Id, (string)resolvedErrors[0]);
        }
		[Test]
		public void ContainsNoErrorsDirectlyAfterInstantiation()
		{
			IValidationErrors errors = new ValidationErrors();
			Assert.IsTrue(errors.IsEmpty);
			Assert.IsNotNull(errors.GetErrors(GoodErrorKey));
		}

		[Test]
		public void AddErrorWithNullMessage()
		{
            Assert.Throws<ArgumentNullException>(() => new ValidationErrors().AddError(GoodErrorKey, null));
		}

		[Test]
		public void AddErrorWithNullKey()
		{
			IValidationErrors errors = new ValidationErrors();
            Assert.Throws<ArgumentNullException>(() => errors.AddError(null, ErrorMessageOne));
		}

		[Test]
		public void AddErrorSunnyDay()
		{
			IValidationErrors errors = new ValidationErrors();
			errors.AddError(GoodErrorKey, ErrorMessageOne);
			Assert.IsFalse(errors.IsEmpty);
			Assert.IsNotNull(errors.GetErrors(GoodErrorKey));
			Assert.AreEqual(1, errors.GetErrors(GoodErrorKey).Count);
		}

		[Test]
		public void AddTwoErrorsSameKey()
		{
			IValidationErrors errors = new ValidationErrors();
			errors.AddError(GoodErrorKey, ErrorMessageOne);
			errors.AddError(GoodErrorKey, ErrorMessageTwo);
			Assert.IsFalse(errors.IsEmpty);
			Assert.IsNotNull(errors.GetErrors(GoodErrorKey));
			Assert.AreEqual(2, errors.GetErrors(GoodErrorKey).Count);
		}

        [Test]
        public void EmptyErrorsReturnEmptyCollections()
        {
            IValidationErrors errors = new ValidationErrors();

            IList<ErrorMessage> typedErrors = errors.GetErrors("xyz");
            Assert.IsNotNull(typedErrors);
            Assert.AreEqual(0, typedErrors.Count);

            IList<string> resolvedErrors = errors.GetResolvedErrors("xyz", new NullMessageSource());
            Assert.IsNotNull(resolvedErrors);
            Assert.AreEqual(0, resolvedErrors.Count);
        }

		[Test]
		public void MergeErrorsWithNull()
		{
			IValidationErrors errors = new ValidationErrors();
			errors.AddError(GoodErrorKey, ErrorMessageOne);
			errors.AddError(GoodErrorKey, ErrorMessageTwo);
			errors.MergeErrors(null);

			// must be unchanged with no Exception thrown...
			Assert.IsFalse(errors.IsEmpty);
			Assert.IsNotNull(errors.GetErrors(GoodErrorKey));
			Assert.AreEqual(2, errors.GetErrors(GoodErrorKey).Count);
		}

		[Test]
		public void MergeErrors()
		{
			ValidationErrors otherErrors = new ValidationErrors();
			const string anotherKey = "anotherKey";
			otherErrors.AddError(anotherKey, ErrorMessageTwo);
			otherErrors.AddError(GoodErrorKey, ErrorMessageTwo);


			IValidationErrors errors = new ValidationErrors();
			errors.AddError(GoodErrorKey, ErrorMessageOne);
			errors.MergeErrors(otherErrors);

			Assert.IsFalse(errors.IsEmpty);
		    IList<ErrorMessage> mergedErrors = errors.GetErrors(GoodErrorKey);
		    Assert.IsNotNull(mergedErrors);
			Assert.AreEqual(2, mergedErrors.Count);
            Assert.AreEqual(ErrorMessageOne, mergedErrors[0]);
            Assert.AreEqual(ErrorMessageTwo, mergedErrors[1]);

			IList<ErrorMessage> otherErrorsForKey = errors.GetErrors(anotherKey);
			Assert.IsNotNull(otherErrorsForKey);
            Assert.AreEqual(1, otherErrorsForKey.Count);
            Assert.AreEqual(ErrorMessageTwo, otherErrorsForKey[0]);
        }

        [Test]
        public void SerializeErrors()
        {
            ValidationErrors errors = new ValidationErrors();
            ErrorMessageOne = new ErrorMessage("Kissing Leads To Brain Disease", new object[] {"Param11", 5, "Param13"});
            ErrorMessageTwo = new ErrorMessage("This Is Eva Green", new object[] { "Param21", 'g' , new object[] {"Goran", "Milosavljevic"} });
            ErrorMessage ErrorMessageThree = new ErrorMessage("Third error message", null);
            ErrorMessage ErrorMessageFour = new ErrorMessage("Fourth error message", new object[]{});
            errors.AddError("key1", ErrorMessageOne);
            errors.AddError("key1", ErrorMessageTwo);
            errors.AddError("key2", ErrorMessageThree);
            errors.AddError("key3", ErrorMessageFour);

            Stream streamBefore = new MemoryStream();
            Stream streamAfter = new MemoryStream();

            // serialize ValidationErrors
            XmlSerializer serializer = new XmlSerializer(typeof(ValidationErrors));
            serializer.Serialize(streamBefore, errors);
            streamBefore.Position = 0;

            // deserialize ValidationErrors
            ValidationErrors result = (ValidationErrors) serializer.Deserialize(streamBefore);
            
            // serialize ValidationErrors
            serializer.Serialize(streamAfter, result);
            
            // compare ValidationErrors instances
            byte[] byteBefore = new byte[streamBefore.Length];
            byte[] byteAfter = new byte[streamAfter.Length];
            
            Assert.AreEqual(byteAfter.Length, byteBefore.Length);
            
            streamBefore.Position = 0;
            streamAfter.Position = 0;
            streamBefore.Read(byteBefore, 0, (int) streamBefore.Length);
            streamAfter.Read(byteAfter, 0, (int) streamAfter.Length);
            
            Assert.AreEqual(byteBefore, byteAfter);
        }
	}
}
