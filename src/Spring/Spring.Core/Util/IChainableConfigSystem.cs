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

using System.Configuration.Internal;

namespace Spring.Util;

/// <summary>
/// Implement this interface to create your own, delegating <see cref="IInternalConfigSystem"/>
/// and set them using <see cref="ConfigurationUtils.SetConfigurationSystem"/>
/// </summary>
public interface IChainableConfigSystem : IInternalConfigSystem
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="innerConfigSystem"></param>
    void SetInnerConfigurationSystem(IInternalConfigSystem innerConfigSystem);
}
