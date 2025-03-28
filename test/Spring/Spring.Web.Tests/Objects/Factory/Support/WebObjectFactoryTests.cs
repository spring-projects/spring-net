#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

using NUnit.Framework;
using Spring.Objects.Factory.Config;
using Spring.TestSupport;

#endregion

namespace Spring.Objects.Factory.Support;

/// <summary>
/// Unit tests for the WebObjectFactory class.
/// </summary>
/// <author>Rick Evans</author>
[TestFixture]
public sealed class WebObjectFactoryTests
{
    [Test]
    public void CanBeUsedOnNonWebThread()
    {
        WebObjectFactory wof;
        RootWebObjectDefinition rwod;

        // we need to create WOF within a valid HttpContext environment 'cause we will
        // make use of 'request' and 'session' scope.
        using (new VirtualEnvironmentMock("/somedir/some.file", null, null, "/", true))
        {
            wof = new WebObjectFactory("/somedir/", false);
        }

        rwod = new RootWebObjectDefinition(typeof(object), new ConstructorArgumentValues(), new MutablePropertyValues());
        rwod.Scope = ObjectScope.Application.ToString();
        wof.RegisterObjectDefinition("applicationScopedObject", rwod);

        rwod = new RootWebObjectDefinition(typeof(object), new ConstructorArgumentValues(), new MutablePropertyValues());
        rwod.Scope = ObjectScope.Request.ToString();
        wof.RegisterObjectDefinition("requestScopedObject", rwod);

        rwod = new RootWebObjectDefinition(typeof(object), new ConstructorArgumentValues(), new MutablePropertyValues());
        rwod.Scope = ObjectScope.Session.ToString();
        wof.RegisterObjectDefinition("sessionScopedObject", rwod);

        object o;
        o = wof.GetObject("applicationScopedObject");
        Assert.IsNotNull(o);

        AssertGetObjectThrows(typeof(ObjectCreationException), wof, "requestScopedObject");
        AssertGetObjectThrows(typeof(ObjectCreationException), wof, "sessionScopedObject");
    }

    private void AssertGetObjectThrows(Type exceptionType, WebObjectFactory wof, string objectName)
    {
        try
        {
            wof.GetObject(objectName);
            Assert.Fail("must not reach this line");
        }
        catch (Exception ex)
        {
            Assert.AreEqual(exceptionType, ex.GetType());
        }
    }
}
