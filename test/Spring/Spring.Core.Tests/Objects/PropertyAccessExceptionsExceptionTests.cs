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

using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using Spring.Core;

namespace Spring.Objects
{
	/// <summary>
	/// Unit tests for the PropertyAccessExceptionsException class.
    /// </summary>
	[TestFixture]
    public sealed class PropertyAccessExceptionsExceptionTests
    {
        [Test]
        public void GetNonExistantPropertyException()
		{
			ObjectWrapper wrapper = new ObjectWrapper(new TestObject("Rick", 23));
			PropertyAccessExceptionsException ex
				= new PropertyAccessExceptionsException(wrapper, new PropertyAccessException[]
				{
				});
			PropertyAccessException nullException = ex.GetPropertyAccessException("Age");
			Assert.IsNull(nullException);
        }

		[Test]
		public void GetPropertyExceptions()
		{
			ObjectWrapper wrapper = new ObjectWrapper(new TestObject("Rick", 23));
			PropertyAccessExceptionsException ex
				= new PropertyAccessExceptionsException(wrapper, new PropertyAccessException[]
				{
					new TypeMismatchException(new PropertyChangeEventArgs("Doctor", new NestedTestObject("Foo"), "rubbish"), typeof (INestedTestObject)),
					new MethodInvocationException(new TargetInvocationException("Bad format", new FormatException("'12' is not a valid argument for 'foo'..")), new PropertyChangeEventArgs("Friends", new ArrayList(), "trash"))
				});
			PropertyAccessException doctorPropertyException = ex.GetPropertyAccessException("Doctor");
			Assert.IsNotNull(doctorPropertyException);
			Assert.AreEqual("typeMismatch", doctorPropertyException.ErrorCode);
			Assert.AreEqual("Foo", ((NestedTestObject) doctorPropertyException.PropertyChangeArgs.OldValue).Company);
			PropertyAccessException friendsPropertyException = ex.GetPropertyAccessException("Friends");
			Assert.IsNotNull(friendsPropertyException);
			Assert.AreEqual("methodInvocation", friendsPropertyException.ErrorCode);
			Assert.AreEqual(wrapper, ex.ObjectWrapper);
			Assert.AreEqual(wrapper.WrappedInstance, ex.BindObject);
			Assert.AreEqual(ex.Message, ex.ToString());

		}
	}
}
