#region License

/*
 * Copyright © 2002-2009 the original author or authors.
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
using TIBCO.EMS;

namespace Spring.Messaging.Ems.Common
{
    public class EmsConnection : IConnection
    {
        #region Logging

        private readonly ILog logger = LogManager.GetLogger(typeof(EmsConnection));

        #endregion

        private Connection nativeConnection;

        public EmsConnection(Connection connection)
        {
            this.nativeConnection = connection;
            this.nativeConnection.ExceptionHandler += HandleEmsException;
        }



        #region Implementation of IConnection

        public Connection NativeConnection
        {
            get { return this.nativeConnection; }
        }

        public event EMSExceptionHandler EMSExceptionHandler;

        public string ActiveURL
        {
            get { return nativeConnection.ActiveURL; }
        }

        public string ClientID
        {
            get { return nativeConnection.ClientID; }
            set { nativeConnection.ClientID = value; }
        }

        public long ConnID
        {
            get { return nativeConnection.ConnID; }
        }

        public IExceptionListener ExceptionListener
        {
            get { return nativeConnection.ExceptionListener;  }
            set { nativeConnection.ExceptionListener = value; }
        }

        public bool IsClosed
        {
            get { return nativeConnection.IsClosed; }
        }

        public bool IsSecure
        {
            get { return nativeConnection.IsSecure; }
        }

        public ConnectionMetaData MetaData
        {
            get { return nativeConnection.MetaData; }
        }

        public void Close()
        {
            nativeConnection.Close();            
        }

        public ISession CreateSession(bool transacted, int acknowledgeMode)
        {
            Session nativeSession = nativeConnection.CreateSession(transacted, acknowledgeMode);
            return new EmsSession(nativeSession);
        }

        public ISession CreateSession(bool transacted, SessionMode acknowledgeMode)
        {
            Session nativeSession = nativeConnection.CreateSession(transacted, acknowledgeMode);
            return new EmsSession(nativeSession);
        }

        public void Start()
        {
            nativeConnection.Start();
        }

        public void Stop()
        {
            nativeConnection.Stop();
        }

        #endregion

        private void HandleEmsException(object sender, EMSExceptionEventArgs arg)
        {
            if (EMSExceptionHandler != null)
            {
                EMSExceptionHandler(sender, arg);
            }
            else
            {
                logger.Error("No exception handler registered with EmsConnection wrapper class.", arg.Exception);
            }
        }
    }
}