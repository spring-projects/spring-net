#region License

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

#endregion

using Common.Logging;
using Spring.Util;
using Apache.NMS;

namespace Spring.Messaging.Nms.Support
{
    /// <summary>
    /// Generic utility methods for working with NMS. Mainly for internal use
    /// within the framework, but also useful for custom NMS access code.
    /// </summary>
    public abstract class NmsUtils
    {
        #region Logging

        private static readonly ILog logger = LogManager.GetLogger(typeof(NmsUtils));

        #endregion

        /// <summary> Close the given NMS Connection and ignore any thrown exception.
        /// This is useful for typical <code>finally</code> blocks in manual NMS code.
        /// </summary>
        /// <param name="con">the NMS Connection to close (may be <code>null</code>)
        /// </param>
        public static void CloseConnection(IConnection con)
        {
            CloseConnection(con, false);
        }

        /// <summary> Close the given NMS Connection and ignore any thrown exception.
        /// This is useful for typical <code>finally</code> blocks in manual NMS code.
        /// </summary>
        /// <param name="con">the NMS Connection to close (may be <code>null</code>)
        /// </param>
        /// <param name="stop">whether to call <code>stop()</code> before closing
        /// </param>
        public static void CloseConnection(IConnection con, bool stop)
        {
            if (con != null)
            {
                try
                {
                    if (stop)
                    {
                        try
                        {
                            con.Stop();
                        }
                        finally
                        {
                            con.Close();
                        }
                    }
                    else
                    {
                        con.Close();
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
		public static void CloseSession(ISession session)
        {
            if (session != null)
            {
                try
                {
                    session.Close();
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
        public static void CloseMessageProducer(IMessageProducer producer)
        {
            if (producer != null)
            {
                try
                {
                    producer.Close();
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
        public static void CloseMessageConsumer(IMessageConsumer consumer)
        {
            if (consumer != null)
            {
                try
                {
                    consumer.Close();
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
/*
        /// <summary> Close the given NMS QueueRequestor and ignore any thrown exception.
        /// This is useful for typical <code>finally</code> blocks in manual NMS code.
        /// </summary>
        /// <param name="requestor">the NMS QueueRequestor to close (may be <code>null</code>)
        /// </param>
 */
//        public static void CloseQueueRequestor(QueueRequestor requestor)
//        {
//            if (requestor != null)
//            {
//                try
//                {
//                    requestor.Close();
//                }
//                catch (NMSException ex)
//                {
//                    logger.Debug("Could not close NMS QueueRequestor", ex);
//                }
//                catch (Exception ex)
//                {
//                    // We don't trust the NMS provider: It might throw RuntimeException or Error.
//                    logger.Debug("Unexpected exception on closing NMS QueueRequestor", ex);
//                }
//            }
//        }


        /// <summary> Commit the Session if not within a distributed transaction.</summary>
        /// <remarks>Needs investigation - no distributed tx in .NET messaging providers</remarks>
        /// <param name="session">the NMS Session to commit
        /// </param>
        /// <throws>NMSException if committing failed </throws>
        public static void CommitIfNecessary(ISession session)
        {
		    AssertUtils.ArgumentNotNull(session, "ISession must not be null");
			
			session.Commit();

			// TODO Investigate
			
//		    try {
//			    session.Commit();
//		    }
//		    catch (TransactionInProgressException ex) {
//		        // TODO Investigate
//			    // Ignore -> can only happen in case of a JTA transaction.
//		    }
//		    catch (IllegalStateException ex) {
//		        // TODO Investigate
//			    // Ignore -> can only happen in case of a JTA transaction.
//		    }
	    }

        /// <summary> Rollback the Session if not within a distributed transaction.</summary>
        /// <remarks>Needs investigation - no distributed tx in EMS</remarks>
        /// <param name="session">the NMS Session to rollback
        /// </param>
        /// <throws>  NMSException if committing failed </throws>
	    public static void RollbackIfNecessary(ISession session)
        {
		    AssertUtils.ArgumentNotNull(session, "ISession must not be null");
			session.Rollback();

			// TODO Investigate

//		    try {
//			    session.Rollback();
//		    }
//		    catch (TransactionInProgressException ex) {
//			    // Ignore -> can only happen in case of a JTA transaction.
//		    }
//		    catch (IllegalStateException ex) {
//			    // Ignore -> can only happen in case of a JTA transaction.
//		    }
	    }
        
    }
}
