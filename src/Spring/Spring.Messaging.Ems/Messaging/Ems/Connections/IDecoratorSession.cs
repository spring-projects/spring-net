#region License

/*
 * Copyright 2002-2008 the original author or authors.
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

using Spring.Messaging.Ems.Common;

namespace Spring.Messaging.Ems.Connections
{
    /// <summary>
    /// Subinterface of Session to be implemented by
    /// implementations that wrap an Session to provide added 
    /// functionality. Allows access to the the underlying target Session.
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <see cref="ConnectionFactoryUtils.GetTargetSession(ISession)"/>
    /// <see cref="CachingConnectionFactory"/>
    public interface IDecoratorSession : ISession
    {
        /// <summary>
        /// Gets the target session of the decorator.
        /// This will typically be the native provider Session or a wrapper from a session pool.
        /// </summary>
        /// <value>The underlying session, never null</value>
        ISession TargetSession { get; }
    }
}