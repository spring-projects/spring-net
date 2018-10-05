using System;
using NHibernate;
using Spring.Transaction.Interceptor;
using System.Threading;

namespace Spring.Data.NHibernate
{
    [Transaction]
    public class SimpleTestDao : ITestObjectDao
    {

        private int _secondsToSleepBeforeException;
        public int SecondsToSleepBeforeException
        {
            get { return _secondsToSleepBeforeException; }
            set
            {
                _secondsToSleepBeforeException = value * 1000;
            }
        }
        

        public ISessionFactory SessionFactory
        {
            get { return sessionFactory; }
            set { sessionFactory = value; }
        }

        private ISessionFactory sessionFactory;

        [Transaction]
        public void Create(TestObject to)
        {
            sessionFactory.GetCurrentSession().FlushMode = FlushMode.Always;
            sessionFactory.GetCurrentSession().Save(to);
            Thread.Sleep(_secondsToSleepBeforeException);

            throw new Exception();
        }

        public void Update(TestObject to)
        {
            throw new NotImplementedException();
        }

        public void Delete(TestObject to)
        {
            throw new NotImplementedException();
        }

        public TestObject FindByName(string name)
        {
            throw new NotImplementedException();
        }

        public void CreateUpdateRollback(TestObject to)
        {
            throw new NotImplementedException();
        }
    }
}