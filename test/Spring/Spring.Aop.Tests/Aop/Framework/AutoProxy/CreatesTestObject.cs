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

using Spring.Objects;
using Spring.Objects.Factory;

namespace Spring.Aop.Framework.AutoProxy;

/// <summary>
/// This is simple implementation of IFactoryObject that creates a TestObject.
/// </summary>
/// <author>Mark Pollack</author>
public class CreatesTestObject : IFactoryObject, IInitializingObject
{
    private bool initialized = false;
    private ITestObject testObject;

    public CreatesTestObject()
    {
    }

    public object GetObject()
    {
        // return product only, if factory has been fully initialized!
        if (!initialized)
        {
            return null;
        }
        else
        {
            return testObject;
        }
    }

    public Type ObjectType
    {
        get
        {
            // return type only if we are ready to deliver our product!
            if (!initialized)
            {
                return null;
            }
            else
            {
                return typeof(ITestObject);
            }
        }
    }

    public bool IsSingleton
    {
        get { return true; }
    }

    public void AfterPropertiesSet()
    {
        testObject = new TestObject();
        initialized = true;
    }
}
