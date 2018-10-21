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

using Spring.Context;
using Spring.Objects.Factory.Config;

namespace Spring.Core
{
    /// <summary>
    /// Extension of the <see cref="IOrdered"/> interface, expressing a 'priority'
    /// ordering: Order values expressed by IPriorityOrdered objects always
    /// apply before order values of 'plain' Ordered values.
    /// </summary>
    /// <remarks>
    /// <para>This is primarily a special-purpose interface, used for objects
    /// where it is particularly important to determine 'prioritized'
    /// objects first, without even obtaining the remaining objects.
    /// A typical example: Prioritized post-processors in a Spring
    /// <see cref="IApplicationContext"/>
    /// </para>
    /// <para>IPriorityOrdered post-processor objects are initialized in
    /// a special phase, ahead of other post-processor objects.</para>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    /// <see cref="VariablePlaceholderConfigurer"/>
    /// <see cref="TypeAliasConfigurer"/>
    /// <see cref="ResourceHandlerConfigurer"/>
    public interface IPriorityOrdered : IOrdered
    {
    }
}