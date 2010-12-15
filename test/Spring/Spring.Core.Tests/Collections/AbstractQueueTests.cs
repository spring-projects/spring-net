#region License

/*
 * Copyright © 2002-2010 the original author or authors.
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

#region Imports

using System;
using System.Collections;
using NUnit.Framework;

#endregion

namespace Spring.Collections
{
	/// <summary>
	/// Unit tests for the AbstractQueue class.
	/// </summary>
	/// <author>Griffin Caprio</author>
	[TestFixture]
	public sealed class AbstractQueueTests
	{
		private sealed class SucceedQueue : AbstractQueue
		{
			
			public override int Count
			{
				get { return 0; }
			}
			public override int Capacity
			{
				get
				{
					return 0;
				}
			}
			public override bool Offer(object x)
			{
				if (x == null)
				{
					throw new NullReferenceException();
				}
				return true;
			}

			public override object Peek()
			{
				return Int32.Parse("1");
			}

			public override object Poll()
			{
				return Int32.Parse("1");
			}

			public override IEnumerator GetEnumerator()
			{
				return null;
			}

			public override void CopyTo(Array array, Int32 index)
			{
			}

			public override object SyncRoot
			{
				get { return null; }

			}

			public override Boolean IsSynchronized
			{
				get { return false; }

			}

			public override bool IsEmpty
			{
				get { throw new NotImplementedException(); }
			}
		}

		private sealed class FailQueue : AbstractQueue
		{
			public override int Count
			{
				get { return 0; }
			}
			public override int Capacity
			{
				get
				{
					return 0;
				}
			}
			public override bool Offer(object x)
			{
				if (x == null)
				{
					throw new NullReferenceException();
				}
				return false;
			}

			public override object Peek()
			{
				return null;
			}

			public override object Poll()
			{
				return null;
			}

			public override IEnumerator GetEnumerator()
			{
				return null;
			}

			public override void CopyTo(Array array, Int32 index)
			{
			}

			public override object SyncRoot
			{
				get { return null; }
			}

			public override Boolean IsSynchronized
			{
				get { return false; }
			}

			public override bool IsEmpty
			{
				get { throw new NotImplementedException(); }
			}
		}

		[Test]
		public void AddSucceed()
		{
			SucceedQueue q = new SucceedQueue();
			Assert.IsTrue(q.Add(Int32.Parse("2")));
		}

		[Test]
		[ExpectedException(typeof (InvalidOperationException))]
		public void AddFail()
		{
			FailQueue q = new FailQueue();
			q.Add(Int32.Parse("1"));
		}

		[Test]
		[ExpectedException(typeof (NullReferenceException))]
		public void AddNPE()
		{
			SucceedQueue q = new SucceedQueue();
			q.Add(null);
		}

		[Test]
		public void RemoveSucceed()
		{
			SucceedQueue q = new SucceedQueue();
			q.Remove();
		}

		[Test]
		[ExpectedException(typeof (NoElementsException))]
		public void RemoveFail()
		{
			FailQueue q = new FailQueue();
			q.Remove();
		}

		[Test]
		public void ElementSucceed()
		{
			SucceedQueue q = new SucceedQueue();
			q.Element();
		}

		[Test]
		[ExpectedException(typeof (NoElementsException))]
		public void ElementF()
		{
			FailQueue q = new FailQueue();
			q.Element();
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void AddAll1()
		{
			SucceedQueue q = new SucceedQueue();
			q.AddAll(null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void AddAllSelf()
		{
			SucceedQueue q = new SucceedQueue();
			q.AddAll(q);
		}
	}
}