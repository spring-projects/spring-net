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
using Spring.Objects.Factory;

namespace Spring.Messaging.Ems.Support
{
    /// <summary> Base class for EmsTemplate and other EMS-accessing gateway helpers</summary>
    /// <remarks>It defines common properties like the ConnectionFactory}. The subclass
    /// EmsDestinationAccessor adds further, destination-related properties.
    /// <para>
    /// Not intended to be used directly. See EmsTemplate.
    /// </para>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class EmsAccessor : IInitializingObject
    {
        #region Logging

        private readonly ILog logger = LogManager.GetLogger(typeof(EmsAccessor));

        #endregion

        #region Fields

        private IConnectionFactory connectionFactory;

        private bool sessionTransacted = false;

        private int sessionAcknowledgeMode = Session.AUTO_ACKNOWLEDGE;

        #endregion

        #region Properties


        /// <summary>
        /// Gets or sets the connection factory to use for obtaining EMS Connections.
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
        /// Gets or sets the session acknowledge mode for EMS Sessions including whether or not the session is transacted
        /// </summary>
        /// <remarks>
        /// Set the EMS acknowledgement mode that is used when creating a EMS
        /// Session to send a message. The default is AUTO_ACKNOWLEDGE.
        /// </remarks>
        /// <value>The session acknowledge mode.</value>
        virtual public int SessionAcknowledgeMode
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
        /// Set the transaction mode that is used when creating a EMS Session.
        /// Default is "false".
        /// </summary>
        /// <remarks>
        /// <para>Setting this flag to "true" will use a short local EMS transaction
        /// when running outside of a managed transaction, and a synchronized local
        /// EMS transaction in case of a managed transaction being present.
        /// The latter has the effect of a local EMS
        /// transaction being managed alongside the main transaction (which might
        /// be a native ADO.NET transaction), with the EMS transaction committing
        /// right after the main transaction.
        /// </para>
        /// </remarks>
        public bool SessionTransacted
        {
            get
            {
                return sessionTransacted;
            }
            set
            {
                if (value)
                {
                    sessionTransacted = value;
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
            return con.CreateSession(sessionTransacted, SessionAcknowledgeMode);
        }

        /// <summary>
        /// Returns whether the Session is in client acknowledgement mode.
        /// </summary>
        /// <param name="session">The session to check.</param>
        /// <returns>true if in client ack mode, false otherwise</returns>
        protected virtual bool IsClientAcknowledge(ISession session)
        {
            return (session.AcknowledgeMode == Session.CLIENT_ACKNOWLEDGE);
        }
    }
}
