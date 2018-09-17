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
using NUnit.Framework;

namespace Spring.Services.WindowsService.Common.Deploy
{
    [TestFixture]
	public class DeployEventAggregatorTest
	{
        DeployEventAggregator aggregator;

        [SetUp]
        public void SetUp ()
        {
            aggregator = new DeployEventAggregator();
        }

        [Test]
        public void AdditionsAbsorbUpdates ()
        {
            aggregator.Aggregate(new DeployEventArgs(null, DeployEventType.ApplicationAdded));
            aggregator.Aggregate(new DeployEventArgs(null, DeployEventType.ApplicationUpdated));
            Assert.AreEqual(DeployEventType.ApplicationAdded, aggregator.Result.EventType);
        }

        [Test]
        public void RemovalsEliminateAdditions ()
        {
            aggregator.Aggregate(new DeployEventArgs(null, DeployEventType.ApplicationAdded));
            aggregator.Aggregate(new DeployEventArgs(null, DeployEventType.ApplicationRemoved));
            Assert.AreEqual(DeployEventType.ApplicationNeverAdded, aggregator.Result.EventType);
        }

        [Test]
        public void RemovalsEliminateAdditionsButThenCanAdd ()
        {
            RemovalsEliminateAdditions();
            aggregator.Aggregate(new DeployEventArgs(null, DeployEventType.ApplicationAdded));
            Assert.AreEqual(DeployEventType.ApplicationAdded, aggregator.Result.EventType);
        }

        [Test]
        public void MultipleEventsOfTheSameTypeCollapse ()
        {
            aggregator.Aggregate(new DeployEventArgs(null, DeployEventType.ApplicationAdded));
            aggregator.Aggregate(new DeployEventArgs(null, DeployEventType.ApplicationAdded));
            Assert.AreEqual(DeployEventType.ApplicationAdded, aggregator.Result.EventType);
        }

        [Test]
        public void RemovalsSupercedesUpdatesButAdditionSuperceedesThem ()
        {
            aggregator.Aggregate(new DeployEventArgs(null, DeployEventType.ApplicationUpdated));
            aggregator.Aggregate(new DeployEventArgs(null, DeployEventType.ApplicationRemoved));
            Assert.AreEqual(DeployEventType.ApplicationRemoved, aggregator.Result.EventType);
            // remove
            aggregator.Aggregate(new DeployEventArgs(null, DeployEventType.ApplicationAdded));
            Assert.AreEqual(DeployEventType.ApplicationUpdated, aggregator.Result.EventType);
        }

        [Test]
        public void ARemovalFollowedByAnAdditionResultInAnUpdate ()
        {
            aggregator.Aggregate(new DeployEventArgs(null, DeployEventType.ApplicationRemoved));
            aggregator.Aggregate(new DeployEventArgs(null, DeployEventType.ApplicationAdded));
            Assert.AreEqual(DeployEventType.ApplicationUpdated, aggregator.Result.EventType);
        }

        [Test]
        public void ARemovalAfterAnAdditionFrozeStateUntilASecondAddition()
        {
            aggregator.Aggregate(new DeployEventArgs(null, DeployEventType.ApplicationAdded));
            aggregator.Aggregate(new DeployEventArgs(null, DeployEventType.ApplicationUpdated));
            aggregator.Aggregate(new DeployEventArgs(null, DeployEventType.ApplicationRemoved));
            aggregator.Aggregate(new DeployEventArgs(null, DeployEventType.ApplicationUpdated));
            Assert.AreEqual(DeployEventType.ApplicationNone, aggregator.Result.EventType);
            aggregator.Aggregate(new DeployEventArgs(null, DeployEventType.ApplicationAdded));
            Assert.AreEqual(DeployEventType.ApplicationAdded, aggregator.Result.EventType);
            aggregator.Aggregate(new DeployEventArgs(null, DeployEventType.ApplicationUpdated));
            Assert.AreEqual(DeployEventType.ApplicationAdded, aggregator.Result.EventType);
        }
	}
}
