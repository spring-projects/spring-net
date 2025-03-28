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

using System.Reflection;

namespace Spring.Util;

/// <summary>
/// Collects information on the constructor to use to create the instance and the argument instances to pass into the
/// constructor.
/// </summary>
public class ConstructorInstantiationInfo
{
    private ConstructorInfo constructorInfo;
    private object[] argInstances;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstructorInstantiationInfo"/> class.
    /// </summary>
    /// <param name="constructorInfo">The constructor info.</param>
    /// <param name="argInstances">The arg instances.</param>
    public ConstructorInstantiationInfo(ConstructorInfo constructorInfo, object[] argInstances)
    {
        this.constructorInfo = constructorInfo;
        this.argInstances = argInstances;
    }

    /// <summary>
    /// Gets the constructor info.
    /// </summary>
    /// <value>The constructor info.</value>
    public ConstructorInfo ConstructorInfo
    {
        get { return constructorInfo; }
    }

    /// <summary>
    /// Gets the arg instances.
    /// </summary>
    /// <value>The arg instances.</value>
    public object[] ArgInstances
    {
        get { return argInstances; }
    }
}
