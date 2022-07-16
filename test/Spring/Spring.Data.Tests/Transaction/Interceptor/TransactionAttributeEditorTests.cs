using System;
using System.Data;
using NUnit.Framework;

namespace Spring.Transaction.Interceptor
{
	[TestFixture]
	public class TransactionAttributeEditorTests
	{
		[Test]
		public void NullTest()
		{
			TransactionAttributeEditor editor = new TransactionAttributeEditor( );
			editor.SetAsText( null );
			ITransactionAttribute ta = editor.Value;
			Assert.IsNull( ta );
		}

		[Test]
		public void EmptyStringTest()
		{
			TransactionAttributeEditor editor = new TransactionAttributeEditor( );
			editor.SetAsText( String.Empty );
			ITransactionAttribute ta = editor.Value;
			Assert.IsNull( ta );
		}

		[Test]
		public void ValidPropagationCodeOnly()
		{
			TransactionAttributeEditor editor = new TransactionAttributeEditor( );
			editor.SetAsText( "PROPAGATION_REQUIRED" );
			ITransactionAttribute ta = editor.Value;
			Assert.IsTrue( ta != null );
			Assert.IsTrue( ta.PropagationBehavior == TransactionPropagation.Required );
			Assert.IsTrue( ta.TransactionIsolationLevel == IsolationLevel.ReadCommitted );
			Assert.IsFalse( ta.ReadOnly );
		}

		[Test]
		public void InvalidPropagationCodeOnly()
		{
			TransactionAttributeEditor editor = new TransactionAttributeEditor( );
			Assert.Throws<ArgumentException>(() => editor.SetAsText( "INVALIDPROPAGATIONCODE" ));
		}

		[Test]
		public void ValidPropagationCodeAndIsolationCode()
		{
			TransactionAttributeEditor editor = new TransactionAttributeEditor( );
			editor.SetAsText( "PROPAGATION_REQUIRED, ISOLATION_READUNCOMMITTED" );
			ITransactionAttribute ta = editor.Value;
			Assert.IsTrue( ta != null );
			Assert.IsTrue( ta.PropagationBehavior == TransactionPropagation.Required );
			Assert.IsTrue( ta.TransactionIsolationLevel == IsolationLevel.ReadUncommitted );
		}

		[Test]
		public void ValidPropagationAndIsolationCodeAndInvalidRollbackRule()
		{
			TransactionAttributeEditor editor = new TransactionAttributeEditor( );
            Assert.Throws<ArgumentException>(() => editor.SetAsText( "PROPAGATION_REQUIRED,ISOLATION_READUNCOMMITTED,XXX" ));
		}

#if !NETCOREAPP
		[Test]
		public void ValidPropagationCodeAndIsolationCodeAndRollbackRules1()
		{
			TransactionAttributeEditor editor = new TransactionAttributeEditor( );
			editor.SetAsText( "PROPAGATION_MANDATORY,ISOLATION_REPEATABLEREAD,timeout_10,-DataException,+RemotingException" );
			ITransactionAttribute ta = editor.Value;
			Assert.IsNotNull( ta );
			Assert.IsTrue( ta.PropagationBehavior == TransactionPropagation.Mandatory );
			Assert.IsTrue( ta.TransactionIsolationLevel == IsolationLevel.RepeatableRead );
			Assert.IsTrue( ta.TransactionTimeout == 10 );
			Assert.IsFalse( ta.ReadOnly );
			Assert.IsTrue( ta.RollbackOn(new SystemException( ) ) );
			// Check for our bizarre customized rollback rules
			Assert.IsTrue( ta.RollbackOn(new DataException( ) ) );
			Assert.IsTrue( !ta.RollbackOn(new System.Runtime.Remoting.RemotingException( ) ) );
		}

		[Test]
		public void ValidPropagationCodeAndIsolationCodeAndRollbackRules2()
		{
			TransactionAttributeEditor editor = new TransactionAttributeEditor( );
			editor.SetAsText( "+DataException,readOnly,ISOLATION_READCOMMITTED,-RemotingException,PROPAGATION_SUPPORTS" );
			ITransactionAttribute ta = editor.Value;
			Assert.IsNotNull( ta );
			Assert.IsTrue( ta.PropagationBehavior == TransactionPropagation.Supports );
			Assert.IsTrue( ta.TransactionIsolationLevel == IsolationLevel.ReadCommitted );
			Assert.IsTrue( ta.TransactionTimeout == -1 );
			Assert.IsTrue( ta.ReadOnly );
			Assert.IsTrue( ta.RollbackOn( new SystemException( ) ) );
			// Check for our bizarre customized rollback rules
			Assert.IsFalse( ta.RollbackOn(new DataException( ) ) );
			Assert.IsTrue( ta.RollbackOn(new System.Runtime.Remoting.RemotingException( ) ) );
		}
#endif

		[Test]
		public void DefaultTransactionAttributeToString()
		{
			DefaultTransactionAttribute source = new DefaultTransactionAttribute( );
			source.PropagationBehavior = TransactionPropagation.Supports;
			source.TransactionIsolationLevel = IsolationLevel.RepeatableRead;
			source.TransactionTimeout = 10;
			source.ReadOnly = true;

			TransactionAttributeEditor editor = new TransactionAttributeEditor( );
			editor.SetAsText( source.ToString( ) );
			ITransactionAttribute ta = editor.Value;
			Assert.AreEqual( source, ta );
			Assert.AreEqual( ta.PropagationBehavior, TransactionPropagation.Supports );
			Assert.AreEqual( ta.TransactionIsolationLevel, IsolationLevel.RepeatableRead );
			Assert.AreEqual( ta.TransactionTimeout, 10 );
			Assert.IsTrue( ta.ReadOnly );
			Assert.IsTrue( ta.RollbackOn( new SystemException( ) ) );
            //mlp 3/17 changed rollback to rollback on all exceptions.
			Assert.IsTrue( ta.RollbackOn( new ApplicationException( ) ) );

			source.TransactionTimeout = 9;
			Assert.IsFalse( ta == source );
			source.TransactionTimeout = 10;
			Assert.AreEqual( ta, source );
		}

		[Test]
			public void RuleBasedTransactionAttributeToString()
		{
			RuleBasedTransactionAttribute source = new RuleBasedTransactionAttribute();
			source.PropagationBehavior = TransactionPropagation.Supports;
			source.TransactionIsolationLevel = IsolationLevel.RepeatableRead;
			source.TransactionTimeout = 10;
			source.ReadOnly = true;
			source.AddRollbackRule( new RollbackRuleAttribute("ArgumentException"));
			source.AddRollbackRule( new NoRollbackRuleAttribute("IllegalTransactionStateException"));

			TransactionAttributeEditor editor = new TransactionAttributeEditor();
			editor.SetAsText( source.ToString() );
			ITransactionAttribute ta = editor.Value;
			Assert.AreEqual( source, ta );
			Assert.AreEqual( ta.PropagationBehavior, TransactionPropagation.Supports );
			Assert.AreEqual( ta.TransactionIsolationLevel, IsolationLevel.RepeatableRead );
			Assert.AreEqual( ta.TransactionTimeout, 10 );
			Assert.IsTrue( ta.ReadOnly );
			Assert.IsTrue(ta.RollbackOn(new ArgumentException()));
			Assert.IsFalse( ta.RollbackOn(new IllegalTransactionStateException()));

			source.ClearRollbackRules();
			Assert.IsFalse( ta == source );
			source.AddRollbackRule( new RollbackRuleAttribute("ArgumentException"));
			source.AddRollbackRule( new NoRollbackRuleAttribute("IllegalTransactionStateException"));
			Assert.AreEqual( ta, source );
		}
	}
}
