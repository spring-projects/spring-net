#region License

/*
 * Copyright � 2002-2010 the original author or authors.
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
using Spring.Messaging.Ems.Common;
using Spring.Util;

namespace Spring.Messaging.Ems.Support
{
    /// <summary>
    /// Generic utility methods for working with EMS. Mainly for internal use
    /// within the framework, but also useful for custom EMS access code.
    /// </summary>
    public abstract class EmsUtils
    {
        #region Logging

        private static readonly ILog logger = LogManager.GetLogger(typeof(EmsUtils));

        #endregion

        /// <summary> Close the given EMS Connection and ignore any thrown exception.
        /// This is useful for typical <code>finally</code> blocks in manual EMS code.
        /// </summary>
        /// <param name="con">the EMS Connection to close (may be <code>null</code>)
        /// </param>
        public static void CloseConnection(IConnection con)
        {
            CloseConnection(con, false);
        }

        /// <summary> Close the given EMS Connection and ignore any thrown exception.
        /// This is useful for typical <code>finally</code> blocks in manual EMS code.
        /// </summary>
        /// <param name="con">the EMS Connection to close (may be <code>null</code>)
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
                catch (EMSException ex)
                {
                    logger.Debug("Could not close EMS Connection", ex);
                }
                catch (Exception ex)
                {
                    // We don't trust the EMS provider: It might throw another exception.
                    logger.Debug("Unexpected exception on closing EMS Connection", ex);
                }
            }
        }

        /// <summary> Close the given EMS Session and ignore any thrown exception.
		/// This is useful for typical <code>finally</code> blocks in manual EMS code.
		/// </summary>
		/// <param name="session">the EMS Session to close (may be <code>null</code>)
		/// </param>
		public static void CloseSession(ISession session)
        {
            if (session != null)
            {
                try
                {
                    session.Close();
                }
                catch (EMSException ex)
                {
                    logger.Debug("Could not close EMS Session", ex);
                }
                catch (Exception ex)
                {
                    // We don't trust the EMS provider: It might throw RuntimeException or Error.
                    logger.Debug("Unexpected exception on closing EMS Session", ex);
                }
            }
        }

        /// <summary> Close the given EMS MessageProducer and ignore any thrown exception.
		/// This is useful for typical <code>finally</code> blocks in manual EMS code.
		/// </summary>
		/// <param name="producer">the EMS MessageProducer to close (may be <code>null</code>)
		/// </param>
        public static void CloseMessageProducer(IMessageProducer producer)
        {
            if (producer != null)
            {
                try
                {
                    producer.Close();
                }
                catch (EMSException ex)
                {
                    logger.Debug("Could not close EMS MessageProducer", ex);
                }
                catch (Exception ex)
                {
                    // We don't trust the provider: It might throw an exception .
                    logger.Debug("Unexpected exception on closing EMS MessageProducer", ex);
                }
            }
        }

        /// <summary> Close the given EMS MessageConsumer and ignore any thrown exception.
        /// This is useful for typical <code>finally</code> blocks in manual EMS code.
        /// </summary>
        /// <param name="consumer">the EMS MessageConsumer to close (may be <code>null</code>)
        /// </param>
        public static void CloseMessageConsumer(IMessageConsumer consumer)
        {
            if (consumer != null)
            {
                try
                {
                    consumer.Close();
                }
                catch (EMSException ex)
                {
                    logger.Debug("Could not close EMS MessageConsumer", ex);
                }
                catch (Exception ex)
                {
                    // We don't trust the EMS provider: It might throw RuntimeException or Error.
                    logger.Debug("Unexpected exception on closing EMS MessageConsumer", ex);
                }
            }
        }
/*
        /// <summary> Close the given EMS QueueRequestor and ignore any thrown exception.
        /// This is useful for typical <code>finally</code> blocks in manual EMS code.
        /// </summary>
        /// <param name="requestor">the EMS QueueRequestor to close (may be <code>null</code>)
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
//                catch (EMSException ex)
//                {
//                    logger.Debug("Could not close EMS QueueRequestor", ex);
//                }
//                catch (Exception ex)
//                {
//                    // We don't trust the EMS provider: It might throw RuntimeException or Error.
//                    logger.Debug("Unexpected exception on closing EMS QueueRequestor", ex);
//                }
//            }
//        }


        /// <summary> Commit the Session if not within a distributed transaction.</summary>
        /// <remarks>Needs investigation - no distributed tx in .NET messaging providers</remarks>
        /// <param name="session">the EMS Session to commit
        /// </param>
        /// <throws>EMSException if committing failed </throws>
        public static void CommitIfNecessary(ISession session)
        {
		    AssertUtils.ArgumentNotNull(session, "Session must not be null");

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
        /// <param name="session">the EMS Session to rollback
        /// </param>
        /// <throws>  EMSException if committing failed </throws>
	    public static void RollbackIfNecessary(ISession session)
        {
		    AssertUtils.ArgumentNotNull(session, "Session must not be null");
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

        /// <summary>
        /// Closes the given queue browser and ignore any thrown exception.
        /// This is useful for typical <code>finally</code> blocks in manual EMS code.
        /// </summary>
        /// <param name="browser">The queue browser to close (may be <code>null</code>.</param>
        public static void CloseQueueBrowser(QueueBrowser browser)
        {
            if (browser != null)
            {
                try
                {
                    browser.Close();
                } catch (EMSException ex)
                {
                    logger.Debug("Could not close EMS QueueBrowser", ex);
                } catch (Exception ex)
                {
                    logger.Debug("Unexpected exception on closing EMS QueueBrowser", ex);
                }
            }
        }

        /// <summary>
        /// Converts the acknowledgement mode from an integer to an enumeration.  If the integer
        /// does not match a valid enumeration, the returned enumeration is SessionMode.AutoAcknowledge
        /// </summary>
        /// <param name="ackMode">The ack mode.</param>
        /// <returns>The corresponding SessionMode enumeration</returns>
        public static SessionMode ConvertAcknowledgementMode(int ackMode)
        {
            switch (ackMode)
            {
                case Session.AUTO_ACKNOWLEDGE:
                    return SessionMode.AutoAcknowledge;
                case Session.CLIENT_ACKNOWLEDGE:
                    return SessionMode.ClientAcknowledge;
                case Session.DUPS_OK_ACKNOWLEDGE:
                    return SessionMode.DupsOkAcknowledge;
                case Session.EXPLICIT_CLIENT_ACKNOWLEDGE:
                    return SessionMode.ExplicitClientAcknowledge;
                case Session.EXPLICIT_CLIENT_DUPS_OK_ACKNOWLEDGE:
                    return SessionMode.ExplicitClientDupsOkAcknowledge;
                case Session.NO_ACKNOWLEDGE:
                    return SessionMode.NoAcknowledge;
                case Session.SESSION_TRANSACTED:
                    return SessionMode.SessionTransacted;
                default:
                    logger.Warn("Integer acknowledgement mode [" + ackMode + "] not valid. Defaulting to SessionMode.AutoAcknowledge");
                    return SessionMode.AutoAcknowledge;
            }
        }
    }
}
