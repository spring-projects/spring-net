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

using System;
using System.ComponentModel;
using TIBCO.EMS;

namespace Spring.Messaging.Ems.Common
{
    public interface IConnection
    {
        Connection NativeConnection { get; }
        event EMSExceptionHandler EMSExceptionHandler;
        string ActiveURL { get; }
        string ClientID { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        long ConnID { get; }

        IExceptionListener ExceptionListener { get; set; }
        bool IsClosed { get; }

        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("EMS clients should no longer use this method; it has been deprecated.")]
        bool IsSecure { get; }

        ConnectionMetaData MetaData { get; }

        void Close();
        ISession CreateSession(bool transacted, int acknowledgeMode);
        ISession CreateSession(bool transacted, SessionMode acknowledgeMode);
        void Start();
        void Stop();
        string ToString();
    }
}