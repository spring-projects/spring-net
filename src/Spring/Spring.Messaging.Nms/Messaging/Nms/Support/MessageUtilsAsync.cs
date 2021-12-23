#region License
// /*
//  * Copyright 2022 the original author or authors.
//  *
//  * Licensed under the Apache License, Version 2.0 (the "License");
//  * you may not use this file except in compliance with the License.
//  * You may obtain a copy of the License at
//  *
//  *      http://www.apache.org/licenses/LICENSE-2.0
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS,
//  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  * See the License for the specific language governing permissions and
//  * limitations under the License.
//  */
#endregion

using Common.Logging;
using Spring.Util;
using Apache.NMS;

namespace Spring.Messaging.Nms.Support
{
    /// <summary>
    /// Async version of NmsUtils
    /// </summary>
    /// <see cref="NmsUtils"/>
    public abstract class NmsUtilsAsync
    {
        #region Logging

        private static readonly ILog logger = LogManager.GetLogger(typeof(NmsUtils));

        #endregion

        /// <summary> Close the given NMS Connection and ignore any thrown exception.
        /// This is useful for typical <code>finally</code> blocks in manual NMS code.
        /// </summary>
        /// <param name="con">the NMS Connection to close (may be <code>null</code>)
        /// </param>
        public static Task CloseConnection(IConnection con)
        {
            return CloseConnection(con, false);
        }

        /// <summary> Close the given NMS Connection and ignore any thrown exception.
        /// This is useful for typical <code>finally</code> blocks in manual NMS code.
        /// </summary>
        /// <param name="con">the NMS Connection to close (may be <code>null</code>)
        /// </param>
        /// <param name="stop">whether to call <code>stop()</code> before closing
        /// </param>
        public static async Task CloseConnection(IConnection con, bool stop)
        {
            if (con != null)
            {
                try
                {
                    if (stop)
                    {
                        try
                        {
                            await con.StopAsync().Awaiter();
                        }
                        finally
                        {
                            await con.CloseAsync().Awaiter();
                        }
                    }
                    else
                    {
                        await con.CloseAsync().Awaiter();
                    }
                }
                catch (NMSException ex)
                {
                    logger.Debug("Could not close NMS Connection", ex);
                }
                catch (Exception ex)
                {
                    // We don't trust the NMS provider: It might throw another exception.
                    logger.Debug("Unexpected exception on closing NMS Connection", ex);
                }
            }
        }
        
        /// <summary> Close the given NMS Session and ignore any thrown exception.
		/// This is useful for typical <code>finally</code> blocks in manual NMS code.
		/// </summary>
		/// <param name="session">the NMS Session to close (may be <code>null</code>)
		/// </param>
		public static async Task CloseSession(ISession session)
        {
            if (session != null)
            {
                try
                {
                    await session.CloseAsync().Awaiter();
                }
                catch (NMSException ex)
                {
                    logger.Debug("Could not close NMS ISession", ex);
                }
                catch (Exception ex)
                {
                    // We don't trust the NMS provider: It might throw RuntimeException or Error.
                    logger.Debug("Unexpected exception on closing NMS ISession", ex);
                }
            }
        }
        
        /// <summary> Close the given NMS MessageProducer and ignore any thrown exception.
		/// This is useful for typical <code>finally</code> blocks in manual NMS code.
		/// </summary>
		/// <param name="producer">the NMS MessageProducer to close (may be <code>null</code>)
		/// </param>
        public static async Task CloseMessageProducer(IMessageProducer producer)
        {
            if (producer != null)
            {
                try
                {
                    await producer.CloseAsync().Awaiter();
                }
                catch (NMSException ex)
                {
                    logger.Debug("Could not close NMS MessageProducer", ex);
                }
                catch (Exception ex)
                {
                    // We don't trust the NMS provider: It might throw RuntimeException or Error.
                    logger.Debug("Unexpected exception on closing NMS MessageProducer", ex);
                }
            }
        }
        
        

        /// <summary> Close the given NMS MessageConsumer and ignore any thrown exception.
        /// This is useful for typical <code>finally</code> blocks in manual NMS code.
        /// </summary>
        /// <param name="consumer">the NMS MessageConsumer to close (may be <code>null</code>)
        /// </param>
        public static async Task CloseMessageConsumer(IMessageConsumer consumer)
        {
            if (consumer != null)
            {
                try
                {
                    await consumer.CloseAsync().Awaiter();
                }
                catch (NMSException ex)
                {
                    logger.Debug("Could not close NMS MessageConsumer", ex);
                }
                catch (Exception ex)
                {
                    // We don't trust the NMS provider: It might throw RuntimeException or Error.
                    logger.Debug("Unexpected exception on closing NMS MessageConsumer", ex);
                }
            }
        }


        /// <summary> Commit the Session if not within a distributed transaction.</summary>
        /// <remarks>Needs investigation - no distributed tx in .NET messaging providers</remarks>
        /// <param name="session">the NMS Session to commit
        /// </param>
        /// <throws>NMSException if committing failed </throws>
        public static async Task CommitIfNecessary(ISession session)
        {
		    AssertUtils.ArgumentNotNull(session, "ISession must not be null");
			
			await session.CommitAsync().Awaiter();
	    }

        /// <summary> Rollback the Session if not within a distributed transaction.</summary>
        /// <remarks>Needs investigation - no distributed tx in EMS</remarks>
        /// <param name="session">the NMS Session to rollback
        /// </param>
        /// <throws>  NMSException if committing failed </throws>
	    public static async Task RollbackIfNecessary(ISession session)
        {
		    AssertUtils.ArgumentNotNull(session, "ISession must not be null");

            await session.RollbackAsync().Awaiter();
	    }
        
    }
}
