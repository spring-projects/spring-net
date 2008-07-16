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

namespace Spring.Transaction.Support
{
    /// <summary>
    /// Extension of the <see cref="IPlatformTransactionManager"/> 
    /// interface, indicating a native resource transaction manager, operating on a single
    /// target resource. Such transaction managers differ from DTC based transaction managers in
    /// that they do not use transaction enlistment for an open number of resources but
    /// rather focus on leveraging the native power and simplicity of a single target resource.
    /// </summary>
    /// <para>
    /// This interface is mainly used for abstract introspection of a transaction manager,
    /// giving clients a hint on what kind of transaction manager they have been given
    /// and on what concrete resource the transaction manager is operating on.
    /// </para>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack</author>
    public interface IResourceTransactionManager : IPlatformTransactionManager
    {
        /// <summary>
        /// Gets the resource factory that this transaction manager operates on,
        /// e.g. a IDbProvider or a Hibernate ISessionFactory.
        /// </summary>
        /// <value>The resource factory.</value>
        object ResourceFactory
        { 
            get;
        }
    }
}