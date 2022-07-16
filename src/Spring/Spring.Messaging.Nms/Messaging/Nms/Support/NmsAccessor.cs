#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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
using Spring.Messaging.Nms.Core;
using Spring.Objects.Factory;
using Apache.NMS;

namespace Spring.Messaging.Nms.Support
{
    /// <summary> Base class for NmsTemplate and other NMS-accessing gateway helpers</summary>
    /// <remarks>It defines common properties like the ConnectionFactory}. The subclass
    /// NmsIDestinationAccessor adds further, destination-related properties.
    /// <para>
    /// Not intended to be used directly. See NmsTemplate.
    /// </para>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class NmsAccessor : IInitializingObject
    {
        #region Logging

        private readonly ILog logger = LogManager.GetLogger(typeof(NmsAccessor));

        #endregion

        #region Fields

        private IConnectionFactory connectionFactory;

        private AcknowledgementMode sessionAcknowledgeMode = AcknowledgementMode.AutoAcknowledge;

        #endregion

        #region Properties


        /// <summary>
        /// Gets or sets the connection factory to use for obtaining NMS Connections.
        /// </summary>
        /// <value>The connection factory.</value>
        virtual public IConnectionFactory ConnectionFactory
        {
            get
            {
                return connectionFactory;
            }

            set
            {
                this.connectionFactory = value;
            }
        }


        /// <summary>
        /// Gets or sets the session acknowledge mode for NMS Sessions including whether or not the session is transacted
        /// </summary>
        /// <remarks>
        /// Set the NMS acknowledgement mode that is used when creating a NMS
        /// Session to send a message. The default is AUTO_ACKNOWLEDGE.
        /// </remarks>
        /// <value>The session acknowledge mode.</value>
        virtual public AcknowledgementMode SessionAcknowledgeMode
        {
            get
            {
                return sessionAcknowledgeMode;
            }

            set
            {
                this.sessionAcknowledgeMode = value;
            }

        }

        /// <summary>
        /// Set the transaction mode that is used when creating a NMS Session.
        /// Default is "false".
        /// </summary>
        /// <remarks>
        /// <para>Setting this flag to "true" will use a short local NMS transaction
        /// when running outside of a managed transaction, and a synchronized local
        /// NMS transaction in case of a managed transaction being present.
        /// The latter has the effect of a local NMS
        /// transaction being managed alongside the main transaction (which might
        /// be a native ADO.NET transaction), with the NMS transaction committing
        /// right after the main transaction.
        /// </para>
        /// </remarks>
        public bool SessionTransacted
        {
            get
            {
                return SessionAcknowledgeMode == AcknowledgementMode.Transactional;
            }
            set
            {
                if (value)
                {
                    sessionAcknowledgeMode = AcknowledgementMode.Transactional;
                }
            }
        }

        #endregion


        /// <summary>
        /// Verify that ConnectionFactory property has been set.
        /// </summary>
        public virtual void AfterPropertiesSet()
        {
            if (ConnectionFactory == null)
            {
                throw new ArgumentException("ConnectionFactory is required");
            }
            if (Tracer.Trace == null)
            {
                if (logger.IsTraceEnabled)
                {
                    logger.Trace("Setting Apache.NMS.Tracer.Trace to default implementation that directs output to Common.Logging");
                }
                Tracer.Trace = new NmsTrace();
            }
        }

        /// <summary>
        /// Creates the connection via the ConnectionFactory.
        /// </summary>
        /// <returns></returns>
        protected virtual IConnection CreateConnection()
        {
            return ConnectionFactory.CreateConnection();
        }

        /// <summary>
        /// Creates the session for the given Connection
        /// </summary>
        /// <param name="con">The connection to create a session for.</param>
        /// <returns>The new session</returns>
        protected virtual ISession CreateSession(IConnection con)
        {
            return con.CreateSession(SessionAcknowledgeMode);
        }

        /// <summary>
        /// Returns whether the ISession is in client acknowledgement mode.
        /// </summary>
        /// <param name="session">The session to check.</param>
        /// <returns>true if in client ack mode, false otherwise</returns>
        protected virtual bool IsClientAcknowledge(ISession session)
        {
            return (session.AcknowledgementMode == AcknowledgementMode.ClientAcknowledge);
        }
    }
}
