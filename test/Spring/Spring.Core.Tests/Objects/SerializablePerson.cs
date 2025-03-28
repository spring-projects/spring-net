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

using System.Runtime.Serialization;

#endregion

namespace Spring.Objects;

/// <summary>
/// Serializable implementation of the IPerson interface.
/// </summary>
/// <author>Rod Johnson</author>
/// <author>Simon White (.NET)</author>
[Serializable]
public class SerializablePerson : TestObject, IPerson, ISerializable
{
    public SerializablePerson()
    {
    }

    protected SerializablePerson(SerializationInfo info, StreamingContext ctxt)
    {
        this.Age = (int) info.GetValue("Age", typeof(int));
        this.Name = (string) info.GetValue("Name", typeof(string));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("Age", this.Age);
        info.AddValue("Name", this.Name);
    }

    public int GetAge()
    {
        return this.Age;
    }

    public void SetAge(int age)
    {
        this.Age = age;
    }

    public string GetName()
    {
        return this.Name;
    }

    public object Echo(object obj)
    {
        if (obj is Exception)
        {
            throw (Exception) obj;
        }

        return obj;
    }
}
