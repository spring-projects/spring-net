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

using DotNetMock.Dynamic;
using NUnit.Framework;

namespace Spring.Services.WindowsService.Common.Deploy
{
    [TestFixture]
    public class AggregatedDeployEventDispatcherTest
	{
        [Test]
		public void DisposeTriggerWhenDisposed ()
		{
		    DynamicMock mock = new DynamicMock(typeof(ITrigger));
            ITrigger trigger = (ITrigger) mock.Object;            

            mock.Expect("Dispose");
            AggregatedDeployEventDispatcher dispatcher = new AggregatedDeployEventDispatcher(trigger);
            dispatcher.Dispose();
            mock.Verify();
		}
	}
}
