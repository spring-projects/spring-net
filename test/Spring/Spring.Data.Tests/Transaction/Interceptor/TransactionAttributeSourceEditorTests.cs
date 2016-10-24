using System;
using System.Data;
using System.Reflection;

using NUnit.Framework;

namespace Spring.Transaction.Interceptor
{
    [TestFixture]
    public class TransactionAttributeSourceEditorTests
    {
        [Test]
        public void Null()
        {
            TransactionAttributeSourceEditor pe = new TransactionAttributeSourceEditor();
            pe.SetAsText(null);
            ITransactionAttributeSource tas = pe.Value;
            MethodInfo m = typeof(object).GetMethod("GetHashCode");
            Assert.IsTrue(tas.ReturnTransactionAttribute(m, null) == null);
        }

        [Test]
        public void Invalid()
        {
            TransactionAttributeSourceEditor pe = new TransactionAttributeSourceEditor();
            Assert.Throws<ArgumentException>(() => pe.SetAsText("foo=bar"));
        }

        [Test]
        public void MatchesSpecific()
        {
            TransactionAttributeSourceEditor pe = new TransactionAttributeSourceEditor();
            pe.SetAsText("System.Object.GetHashCode, Mscorlib =PROPAGATION_REQUIRED\n" +
                         "System.Object.Equals, Mscorlib =PROPAGATION_MANDATORY\n" +
                         "System.Object.*pe, Mscorlib=PROPAGATION_SUPPORTS");
            ITransactionAttributeSource tas = pe.Value;

            checkTransactionProperties(tas, typeof(object).GetMethod("GetHashCode"), TransactionPropagation.Required);
            checkTransactionProperties(tas, typeof(object).GetMethod("Equals", new Type[] {typeof(object)}), TransactionPropagation.Mandatory);
            checkTransactionProperties(tas, typeof(object).GetMethod("GetType"), TransactionPropagation.Supports);
            checkTransactionProperties(tas, typeof(object).GetMethod("ToString"));
        }

        [Test]
        public void MatchesAll()
        {
            TransactionAttributeSourceEditor pe = new TransactionAttributeSourceEditor();
            pe.SetAsText("System.Object.*, Mscorlib=PROPAGATION_REQUIRED");
            ITransactionAttributeSource tas = pe.Value;

            checkTransactionProperties(tas, typeof(object).GetMethod("GetHashCode"), TransactionPropagation.Required);
            checkTransactionProperties(tas, typeof(object).GetMethod("Equals", new Type[] {typeof(object)}), TransactionPropagation.Required);
            checkTransactionProperties(tas, typeof(object).GetMethod("GetType"), TransactionPropagation.Required);
            checkTransactionProperties(tas, typeof(object).GetMethod("ToString"), TransactionPropagation.Required);
        }

        private void checkTransactionProperties(ITransactionAttributeSource tas, MethodInfo method)
        {
            ITransactionAttribute ta = tas.ReturnTransactionAttribute(method, null);
            Assert.IsNull(ta);
        }

        private void checkTransactionProperties(ITransactionAttributeSource tas, MethodInfo method, TransactionPropagation transactionPropagation)
        {
            ITransactionAttribute ta = tas.ReturnTransactionAttribute(method, null);
            Assert.IsTrue(ta != null);
            Assert.IsTrue(ta.TransactionIsolationLevel == IsolationLevel.ReadCommitted);
            Assert.IsTrue(ta.PropagationBehavior == transactionPropagation);
        }
    }
}