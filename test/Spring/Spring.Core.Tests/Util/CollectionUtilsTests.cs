using System;
using System.Collections;
using NUnit.Framework;

namespace Spring.Util
{
	[TestFixture]
	public class CollectionUtilsTests
	{
		internal class NoContainsNoAddCollection : ICollection
		{
			internal class Iterator : IEnumerator
			{
				#region IEnumerator Members

				public void Reset()
				{
					// TODO:  Add Iterator.Reset implementation
				}

				public object Current
				{
					get
					{
						// TODO:  Add Iterator.Current getter implementation
						return null;
					}
				}

				public bool MoveNext()
				{
					// TODO:  Add Iterator.MoveNext implementation
					return false;
				}

				#endregion

			}

			public void CopyTo(Array array, int index)
			{
				throw new NotImplementedException();
			}

			public int Count
			{
				get { return 0; }
			}

			public object SyncRoot
			{
				get { throw new NotImplementedException(); }
			}

			public bool IsSynchronized
			{
				get { throw new NotImplementedException(); }
			}

			public IEnumerator GetEnumerator()
			{
				return new Iterator();
			}
		}
		[Test]
			[ExpectedException(typeof(ArgumentNullException))]
			public void ContainsNullCollection()
		{
			CollectionUtils.Contains(null, null);	
		}
		[Test]
			public void ContainsNullObject()
		{
			ArrayList list = new ArrayList();
			list.Add(null);
			Assert.IsTrue(CollectionUtils.Contains(list, null));
		}
		[Test]
			[ExpectedException(typeof(InvalidOperationException))]
			public void ContainsCollectionDoesNotImplementContains()
		{
			NoContainsNoAddCollection noAddCollection = new NoContainsNoAddCollection();
			CollectionUtils.Contains(noAddCollection, new object());
		}
		[Test]
			public void ContainsValidElement()
		{
			ArrayList list = new ArrayList();
			list.Add(1);
			list.Add(2);
			list.Add(3);
			list.Add(4);
			
			Assert.IsTrue(CollectionUtils.Contains(list, 3));
		}
		[Test]
			public void ContainsDoesNotContainElement()
		{
			ArrayList list = new ArrayList();
			list.Add(1);
			list.Add(2);
			list.Add(3);
			list.Add(4);

			Assert.IsFalse(CollectionUtils.Contains(list, 5));
		}
		[Test]
			[ExpectedException(typeof(ArgumentNullException))]
			public void AddNullCollection()
		{
			CollectionUtils.Add(null, null);
		}
		[Test]
			public void AddNullObject()
		{
			ArrayList list = new ArrayList();
			CollectionUtils.Add(list, null);
			Assert.IsTrue(list.Count == 1);
		}
		[Test]
			[ExpectedException(typeof(InvalidOperationException))]
			public void AddCollectionDoesNotImplementAdd()
		{
			NoContainsNoAddCollection noAddCollection = new NoContainsNoAddCollection();
			CollectionUtils.Add(noAddCollection, null);
		}
		[Test]
			public void AddValidElement()
		{
			ArrayList list = new ArrayList();
			object obj1 = new object();
			CollectionUtils.Add(list, obj1);
			Assert.IsTrue(list.Count == 1);
		}
		[Test]
			[ExpectedException(typeof(ArgumentNullException))]
			public void ContainsAllNullTargetCollection()
		{
			CollectionUtils.ContainsAll(null, new ArrayList());	
		}

		[Test]
			[ExpectedException(typeof(ArgumentNullException))]
			public void ContainsAllSourceNullCollection()
		{
			CollectionUtils.ContainsAll(new ArrayList(), null);	
		}
		[Test]
			[ExpectedException(typeof(InvalidOperationException))]
			public void ContainsAllDoesNotImplementContains()
		{
			CollectionUtils.ContainsAll(new NoContainsNoAddCollection(), new ArrayList());
		}
		[Test]
			public void DoesNotContainAllElements()
		{
			ArrayList target = new ArrayList();
			target.Add(1);
			target.Add(2);
			target.Add(3);

			ArrayList source = new ArrayList();
			source.Add(1);

			Assert.IsTrue(CollectionUtils.ContainsAll(target, source));
		}
		[Test]
			public void ContainsAllElements()
		{
			ArrayList target = new ArrayList();
			target.Add(1);
			target.Add(2);
			target.Add(3);

			ArrayList source = new ArrayList();
			source.Add(1);
			source.Add(2);
			source.Add(3);

			Assert.IsTrue(CollectionUtils.ContainsAll(target, source));
		}
		[Test]
			public void ContainsAllElementsWithNoElementsInSourceCollection()
		{
			ArrayList target = new ArrayList();
			target.Add(1);
			target.Add(2);
			target.Add(3);

			ArrayList source = new ArrayList();
			Assert.IsTrue(CollectionUtils.ContainsAll(target, source));
		}
		[Test]
		public void ContainsAllElementsWithNoElementsEitherCollection()
		{
			ArrayList target = new ArrayList();
			ArrayList source = new ArrayList();
			Assert.IsFalse(CollectionUtils.ContainsAll(target, source));
		}
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ToArrayNullTargetCollection()
		{
			CollectionUtils.ToArrayList(null);	
		}
		[Test]
		public void ToArrayAllElements()
		{
			ArrayList target = new ArrayList();
			target.Add(1);
			target.Add(2);
			target.Add(3);

			ArrayList source = CollectionUtils.ToArrayList(target);

			Assert.AreEqual(target.Count, source.Count);
		}
		[Test]
			public void EmptyArrayElements()
		{
			ArrayList source = CollectionUtils.ToArrayList(new NoContainsNoAddCollection());
			Assert.AreEqual(0, source.Count);
		}

		[Test] 
			[ExpectedException(typeof(ArgumentNullException))]
		public void RemoveAllTargetNullCollection()
		{
			CollectionUtils.RemoveAll(null, new ArrayList());	
		}
		[Test] 
			[ExpectedException(typeof(ArgumentNullException))]
		public void RemoveAllSourceNullCollection()
		{
			CollectionUtils.RemoveAll(new ArrayList(), null);	
		}
		[Test]
			[ExpectedException(typeof(InvalidOperationException))]
			public void RemoveAllTargetCollectionDoesNotImplementContains()
		{
			CollectionUtils.RemoveAll(new NoContainsNoAddCollection(), new ArrayList());
		}
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void RemoveAllTargetCollectionDoesNotImplementRemove()
		{
			CollectionUtils.RemoveAll(new NoContainsNoAddCollection(), new ArrayList());
		}
		[Test]
			public void RemoveAllNoElements()
		{
			ArrayList target = new ArrayList();
			target.Add(1);
			target.Add(2);
			target.Add(3);

			ArrayList source = new ArrayList();
			source.Add(4);
			source.Add(5);
			source.Add(6);

			CollectionUtils.RemoveAll(target, source);
			Assert.IsTrue(3 == target.Count);
		}
		[Test]
		public void RemoveAllSomeElements()
		{
			ArrayList target = new ArrayList();
			target.Add(1);
			target.Add(2);
			target.Add(3);
			target.Add(4);
			target.Add(5);

			ArrayList source = new ArrayList();
			source.Add(4);
			source.Add(5);
			source.Add(6);

			CollectionUtils.RemoveAll(target, source);
			Assert.IsTrue(3 == target.Count);
		}
		[Test]
		public void RemoveAllAllElements()
		{
			ArrayList target = new ArrayList();
			target.Add(1);
			target.Add(2);
			target.Add(3);
			target.Add(4);
			target.Add(5);

			ArrayList source = new ArrayList();
			source.Add(1);
			source.Add(2);
			source.Add(3);
			source.Add(4);
			source.Add(5);
			source.Add(6);

			CollectionUtils.RemoveAll(target, source);
			Assert.IsTrue(0 == target.Count);
		}
	}
}
