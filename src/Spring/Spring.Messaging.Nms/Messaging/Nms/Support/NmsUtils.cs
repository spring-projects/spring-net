#region License

/*
 * Copyright © 2002-2006 the original author or authors.
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

using System;
using Common.Logging;
using Spring.Util;
using Apache.NMS;

namespace Spring.Messaging.Nms.Support
{
    public abstract class NmsUtils
    {
        #region Logging

        private static readonly ILog logger = LogManager.GetLogger(typeof(NmsUtils));

        #endregion

        /// <summary> Close the given NMS IConnection and ignore any thrown exception.
        /// This is useful for typical <code>finally</code> blocks in manual NMS code.
        /// </summary>
        /// <param name="con">the NMS IConnection to close (may be <code>null</code>)
        /// </param>
        public static void CloseConnection(IConnection con)
        {
            CloseConnection(con, false);
        }

        /// <summary> Close the given NMS IConnection and ignore any thrown exception.
        /// This is useful for typical <code>finally</code> blocks in manual NMS code.
        /// </summary>
        /// <param name="con">the NMS IConnection to close (may be <code>null</code>)
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
                    logger.Debug("Could not close NMS IConnection", ex);
                }
                catch (Exception ex)
                {
                    // We don't trust the NMS provider: It might throw another exception.
                    logger.Debug("Unexpected exception on closing NMS IConnection", ex);
                }
            }
        }
        
        /// <summary> Close the given NMS ISession and ignore any thrown exception.
		/// This is useful for typical <code>finally</code> blocks in manual NMS code.
		/// </summary>
		/// <param name="session">the NMS ISession to close (may be <code>null</code>)
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
        
        /// <summary> Close the given NMS IMessageProducer and ignore any thrown exception.
		/// This is useful for typical <code>finally</code> blocks in manual NMS code.
		/// </summary>
		/// <param name="producer">the NMS IMessageProducer to close (may be <code>null</code>)
		/// </param>
        public static void CloseMessageProducer(IMessageProducer producer)
        {
            if (producer != null)
            {
                try
                {
                    producer.Dispose();
                }
                catch (NMSException ex)
                {
                    logger.Debug("Could not close NMS IMessageProducer", ex);
                }
                catch (Exception ex)
                {
                    // We don't trust the NMS provider: It might throw RuntimeException or Error.
                    logger.Debug("Unexpected exception on closing NMS IMessageProducer", ex);
                }
            }
        }

        /// <summary> Close the given NMS IMessageConsumer and ignore any thrown exception.
        /// This is useful for typical <code>finally</code> blocks in manual NMS code.
        /// </summary>
        /// <param name="consumer">the NMS IMessageConsumer to close (may be <code>null</code>)
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
                    logger.Debug("Could not close NMS IMessageConsumer", ex);
                }
                catch (Exception ex)
                {
                    // We don't trust the NMS provider: It might throw RuntimeException or Error.
                    logger.Debug("Unexpected exception on closing NMS IMessageConsumer", ex);
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


        /// <summary> Commit the ISession if not within a distributed transaction.</summary>
        /// <remarks>Needs investigation - no distributed tx in EMS</remarks>
        /// <param name="session">the NMS ISession to commit
        /// </param>
        /// <throws>  NMSException if committing failed </throws>
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

        /// <summary> Rollback the ISession if not within a distributed transaction.</summary>
        /// <remarks>Needs investigation - no distributed tx in EMS</remarks>
        /// <param name="session">the NMS ISession to rollback
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
