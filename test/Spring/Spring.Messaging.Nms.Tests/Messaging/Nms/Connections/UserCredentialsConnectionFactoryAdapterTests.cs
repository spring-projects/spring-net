using System.Threading;
using Apache.NMS;
using FakeItEasy;
using NUnit.Framework;

namespace Spring.Messaging.Nms.Connections
{
    [TestFixture]
    public class UserCredentialsConnectionFactoryAdapterTests
    {

        [Test]
        public void CreateConnectionWithoutCredentialsWhenTheyAreNotSet()
        {
            var underlyingConnectionFactory = A.Fake<IConnectionFactory>();
            var connectionFactory = new UserCredentialsConnectionFactoryAdapter(underlyingConnectionFactory);

            connectionFactory.CreateConnection();

            A.CallTo(() => underlyingConnectionFactory.CreateConnection()).MustHaveHappenedOnceExactly();
            A.CallTo(() => underlyingConnectionFactory.CreateConnection(A<string>._, A<string>._)).MustHaveHappened(0, Times.Exactly);
        }


        [Test]
        public void CreateConnectionWithoutCredentialsWhenLoginIsNotSet()
        {
            var underlyingConnectionFactory = A.Fake<IConnectionFactory>();
            var connectionFactory = new UserCredentialsConnectionFactoryAdapter(underlyingConnectionFactory);
            connectionFactory.Password = "Secret";
            connectionFactory.CreateConnection();

            A.CallTo(() => underlyingConnectionFactory.CreateConnection()).MustHaveHappenedOnceExactly();
            A.CallTo(() => underlyingConnectionFactory.CreateConnection(A<string>._, A<string>._)).MustHaveHappened(0, Times.Exactly);
        }

        [Test]
        public void CreateConnectionWithCredentialsWhenTheyAreSet()
        {
            var underlyingConnectionFactory = A.Fake<IConnectionFactory>();
            var connectionFactory = new UserCredentialsConnectionFactoryAdapter(underlyingConnectionFactory);
            connectionFactory.UserName = "SampleUser";
            connectionFactory.Password = "Secret";
            connectionFactory.CreateConnection();

            A.CallTo(() => underlyingConnectionFactory.CreateConnection()).MustHaveHappened(0, Times.Exactly);
            A.CallTo(() => underlyingConnectionFactory.CreateConnection("SampleUser", "Secret")).MustHaveHappenedOnceExactly();
        }


        [Test]
        public void SetConnectionCredentialsOnlyForCurrentThread()
        {
            var underlyingConnectionFactory = A.Fake<IConnectionFactory>();
            var connectionFactory = new UserCredentialsConnectionFactoryAdapter(underlyingConnectionFactory);

            // Call CreateConnection on thread that also called SetCredentialsForCurrentThread
            MultithreadingTestHelper.RunOnSeparateThread(() =>
            {
                connectionFactory.SetCredentialsForCurrentThread("SampleUser", "Password");
                connectionFactory.CreateConnection();

                A.CallTo(() => underlyingConnectionFactory.CreateConnection()).MustHaveHappened(0, Times.Exactly);
                A.CallTo(() => underlyingConnectionFactory.CreateConnection("SampleUser", "Password")).MustHaveHappenedOnceExactly();
            }).Wait();

            // Call CreateConnection on thread that didn't callSetCredentialsForCurrentThread
            MultithreadingTestHelper.RunOnSeparateThread(() =>
            {
                connectionFactory.CreateConnection();

                A.CallTo(() => underlyingConnectionFactory.CreateConnection()).MustHaveHappenedOnceExactly();
                A.CallTo(() => underlyingConnectionFactory.CreateConnection(A<string>._, A<string>._)).MustHaveHappenedOnceExactly();
            }).Wait();


            // Call CreateConnection on the main thread
            connectionFactory.CreateConnection();

            A.CallTo(() => underlyingConnectionFactory.CreateConnection()).MustHaveHappened(2, Times.Exactly);
            A.CallTo(() => underlyingConnectionFactory.CreateConnection(A<string>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void InheritCredentialsIfTheyAreNotSetForSpecificThread()
        {
            var underlyingConnectionFactory = A.Fake<IConnectionFactory>();
            var connectionFactory = new UserCredentialsConnectionFactoryAdapter(underlyingConnectionFactory);


            connectionFactory.UserName = "SampleUser";
            connectionFactory.Password = "Secret";

            // Call CreateConnection on thread that also called SetCredentialsForCurrentThread
            MultithreadingTestHelper.RunOnSeparateThread(() =>
            {
                connectionFactory.CreateConnection();
                A.CallTo(() => underlyingConnectionFactory.CreateConnection()).MustHaveHappened(0, Times.Exactly);
                A.CallTo(() => underlyingConnectionFactory.CreateConnection("SampleUser", "Secret")).MustHaveHappenedOnceExactly();


                connectionFactory.SetCredentialsForCurrentThread("ThreadSampleUser", "ThreadPassword");
                connectionFactory.CreateConnection();
                A.CallTo(() => underlyingConnectionFactory.CreateConnection()).MustHaveHappened(0, Times.Exactly);
                A.CallTo(() => underlyingConnectionFactory.CreateConnection("ThreadSampleUser", "ThreadPassword")).MustHaveHappenedOnceExactly();

                connectionFactory.RemoveCredentialsFromCurrentThread();
                connectionFactory.CreateConnection();
                A.CallTo(() => underlyingConnectionFactory.CreateConnection()).MustHaveHappened(0, Times.Exactly);
                A.CallTo(() => underlyingConnectionFactory.CreateConnection("SampleUser", "Secret")).MustHaveHappened(2, Times.Exactly);
            }).Wait();

            connectionFactory.CreateConnection();
            A.CallTo(() => underlyingConnectionFactory.CreateConnection()).MustHaveHappened(0, Times.Exactly);
            A.CallTo(() => underlyingConnectionFactory.CreateConnection("SampleUser", "Secret")).MustHaveHappened(3, Times.Exactly);
        }

        [Test]
        public void SetDifferentCredentialsOnDifferentThreads()
        {
            var underlyingConnectionFactory = A.Fake<IConnectionFactory>();
            var connectionFactory = new UserCredentialsConnectionFactoryAdapter(underlyingConnectionFactory);

            connectionFactory.UserName = "SampleUser";
            connectionFactory.Password = "Secret";

            var barrier = new Barrier(2);

            // Call CreateConnection on thread that also called SetCredentialsForCurrentThread
            var thread1 = MultithreadingTestHelper.RunOnSeparateThread(() =>
            {
                connectionFactory.SetCredentialsForCurrentThread("Thread1SampleUser", "Password");
                barrier.SignalAndWait();

                connectionFactory.CreateConnection();
                A.CallTo(() => underlyingConnectionFactory.CreateConnection()).MustHaveHappened(0, Times.Exactly);
                A.CallTo(() => underlyingConnectionFactory.CreateConnection("Thread1SampleUser", "Password")).MustHaveHappenedOnceExactly();
            });

            // Call CreateConnection on thread that didn't callSetCredentialsForCurrentThread
            var thread2 = MultithreadingTestHelper.RunOnSeparateThread(() =>
            {
                connectionFactory.SetCredentialsForCurrentThread("Thread2SampleUser", "Password");
                barrier.SignalAndWait();

                connectionFactory.CreateConnection();
                A.CallTo(() => underlyingConnectionFactory.CreateConnection()).MustHaveHappened(0, Times.Exactly);
                A.CallTo(() => underlyingConnectionFactory.CreateConnection("Thread2SampleUser", "Password")).MustHaveHappenedOnceExactly();
            });


            connectionFactory.CreateConnection();
            A.CallTo(() => underlyingConnectionFactory.CreateConnection()).MustHaveHappened(0, Times.Exactly);
            A.CallTo(() => underlyingConnectionFactory.CreateConnection("SampleUser", "Secret")).MustHaveHappenedOnceExactly();

            thread1.Wait();
            thread2.Wait();
        }
    }
}
