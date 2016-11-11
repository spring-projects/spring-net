using System;
using System.Data;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Core.TypeResolution;

namespace Spring.Transaction.Interceptor
{
    /// <summary>
    /// Test MatchAlwaysTransactionAttributeSourceTests
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class MatchAlwaysTransactionAttributeSourceTests
    {
        [Test]
        public void GetTransactionAttribute()
        {
            MatchAlwaysTransactionAttributeSource tas = new MatchAlwaysTransactionAttributeSource();
            ITransactionAttribute ta = tas.ReturnTransactionAttribute(typeof (Object).GetMethod("GetHashCode"), null);
            Assert.IsNotNull(ta);
            Assert.IsTrue(TransactionPropagation.Required == ta.PropagationBehavior);
            tas.TransactionAttribute = new DefaultTransactionAttribute(TransactionPropagation.Supports);
            ta =
                tas.ReturnTransactionAttribute(typeof (DataException).GetType().GetMethod("GetHashCode"),
                                               typeof (DataException));
            Assert.IsNotNull(ta);
            Assert.IsTrue(TransactionPropagation.Supports == ta.PropagationBehavior);
        }


        [Test]
        public void CanConfigureInAppContext()
        {
            TypeRegistry.RegisterType("TransactionPropagation", typeof(TransactionPropagation));
            IApplicationContext ctx =
                new XmlApplicationContext(
                    "assembly://Spring.Data.Tests/Spring.Transaction.Interceptor/MatchAlwaysTransactionAttributeSourceTests.xml");
            MatchAlwaysTransactionAttributeSource tas =
                (MatchAlwaysTransactionAttributeSource) ctx.GetObject("MatchAlwaysTransactionAttributeSource");
            Assert.AreEqual(TransactionPropagation.RequiresNew,
                            tas.ReturnTransactionAttribute(null, null).PropagationBehavior);
        }
    }
}