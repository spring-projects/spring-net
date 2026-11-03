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

using System.Collections;
using System.Reflection;
using NUnit.Framework;
using Spring.Core;

namespace Spring.Objects;

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
            = new PropertyAccessExceptionsException(wrapper, new PropertyAccessException[] { });
        PropertyAccessException nullException = ex.GetPropertyAccessException("Age");
        Assert.That(nullException, Is.Null);
    }

    [Test]
    public void GetPropertyExceptions()
    {
        ObjectWrapper wrapper = new ObjectWrapper(new TestObject("Rick", 23));
        PropertyAccessExceptionsException ex
            = new PropertyAccessExceptionsException(wrapper, new PropertyAccessException[] { new TypeMismatchException(new PropertyChangeEventArgs("Doctor", new NestedTestObject("Foo"), "rubbish"), typeof(INestedTestObject)), new MethodInvocationException(new TargetInvocationException("Bad format", new FormatException("'12' is not a valid argument for 'foo'..")), new PropertyChangeEventArgs("Friends", new ArrayList(), "trash")) });
        PropertyAccessException doctorPropertyException = ex.GetPropertyAccessException("Doctor");
        Assert.That(doctorPropertyException, Is.Not.Null);
        Assert.That(doctorPropertyException.ErrorCode, Is.EqualTo("typeMismatch"));
        Assert.That(((NestedTestObject) doctorPropertyException.PropertyChangeArgs.OldValue).Company, Is.EqualTo("Foo"));
        PropertyAccessException friendsPropertyException = ex.GetPropertyAccessException("Friends");
        Assert.That(friendsPropertyException, Is.Not.Null);
        Assert.That(friendsPropertyException.ErrorCode, Is.EqualTo("methodInvocation"));
        Assert.That(ex.ObjectWrapper, Is.EqualTo(wrapper));
        Assert.That(ex.BindObject, Is.EqualTo(wrapper.WrappedInstance));
        Assert.That(ex.ToString(), Is.EqualTo(ex.Message));
    }
}
