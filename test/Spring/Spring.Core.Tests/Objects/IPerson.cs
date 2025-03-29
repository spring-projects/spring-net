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

namespace Spring.Objects;

/// <summary>
/// Extends the normal test object to provide methods that are
/// useful when testing AOP method interception (hence the Get/Set methods).
/// </summary>
/// <author>Rod Johnson</author>
/// <author>Simon White (.NET)</author>
public interface IPerson : ITestObject
{
    int GetAge();
    void SetAge(int age);
    string GetName();
    void SetName(string name);

    /// <summary>
    /// Test for non-property method matching.
    /// If the parameter is an exception, it will be thrown rather than returned.
    /// </summary>
    object Echo(object obj);
}
