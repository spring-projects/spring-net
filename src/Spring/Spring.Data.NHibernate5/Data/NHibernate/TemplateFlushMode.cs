/*
 * Copyright © 2002-2011 the original author or authors.
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

namespace Spring.Data.NHibernate
{
	/// <summary>
	/// Enumeration for the various Hibernate flush modes. 
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	public enum TemplateFlushMode 
	{
        /// <summary>Never flush is a good strategy for read-only units of work.
        /// </summary>
        /// <remarks>
        /// Hibernate will not track and look for changes in this case,
        /// avoiding any overhead of modification detection.
        /// <p>In case of an existing ISession, TemplateFlushMode.Never will turn 
        /// the hibenrate flush mode
        /// to FlushMode.Never for the scope of the current operation, resetting the previous
        /// flush mode afterwards.
        /// </p>
        /// </remarks>
        Never,

        /// <summary>Automatic flushing is the default mode for a Hibernate Session.
        /// </summary>
        /// <remarks>
        /// A session will get flushed on transaction commit, and on certain find
        /// operations that might involve already modified instances, but not
        /// after each unit of work like with eager flushing.
        /// <p>In case of an existing Session, TemplateFlushMode.Auto
        /// will participate in the existing flush mode, not modifying 
        /// it for the current operation.
        /// This in particular means that this setting will not modify an existing
        /// hibernate flush mode FlushMode.Never, in contrast to TemplateFlushMode.Eager.
        /// </p>
        /// </remarks>
        Auto,

        /// <summary>
        /// Eager flushing leads to immediate synchronization with the database,
        /// even if in a transaction. 
        /// </summary>
        /// <remarks>
        /// This causes inconsistencies to show up and throw
        /// a respective exception immediately, and ADO access code that participates
        /// in the same transaction will see the changes as the database is already
        /// aware of them then. But the drawbacks are:
        /// <ul>
        /// <li>additional communication roundtrips with the database, instead of a
        /// single batch at transaction commit;</li>
        /// <li>the fact that an actual database rollback is needed if the Hibernate
        /// transaction rolls back (due to already submitted SQL statements).</li>
        /// </ul>
        /// <p>In case of an existing Session, TemplateFlushMode.Eager
        /// will turn the NHibernate flush mode 
        /// to FlushMode.Auto for the scope of the current operation and issue a flush at the
        /// end, resetting the previous flush mode afterwards.
        /// </p>
        /// </remarks>
        Eager,

        /// <summary>
        /// Flushing at commit only is intended for units of work where no
        /// intermediate flushing is desired, not even for find operations
        /// that might involve already modified instances.
        /// </summary>
        /// <remarks>
        /// <p>In case of an existing Session, TemplateFlushMode.Commit
        /// will turn the NHibernate flush mode
        /// to FlushMode.Commit for the scope of the current operation, resetting the previous
        /// flush mode afterwards. The only exception is an existing flush mode
        /// FlushMode.Never, which will not be modified through this setting.
        /// </p>
        /// </remarks>
        Commit

	}
}
