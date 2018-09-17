#region License
/*
* Copyright 2002-2010 the original author or authors.
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
using System.Reflection;
using log4net;

using Spring.Threading;

namespace Spring.Services.WindowsService.Common.Deploy
{
    public class TestingHandler
    {
        ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public ISync sync;
        public bool applicationAdded = false;
        public bool applicationRemoved = false;
        public bool applicationUpdated = false;
        public bool eventGot = false;

        public TestingHandler (ISync sync)
        {
            this.sync = sync;
        }

        public void Handle (object sender, DeployEventArgs args)
        {
            log.Debug(String.Format("handler #{0} application {1}, args.EventType {2}", 
                GetHashCode(), args.Application.FullPath, args.EventType));

            eventGot = true;

            switch (args.EventType)
            {
                case DeployEventType.ApplicationAdded:
                    applicationAdded = true;
                    break;
                case DeployEventType.ApplicationRemoved:
                    applicationRemoved = true;
                    break;
                case DeployEventType.ApplicationUpdated:
                    applicationUpdated = true;
                    break;
            }
            sync.Release ();
            log.Debug("sync released");
        }
    }
}