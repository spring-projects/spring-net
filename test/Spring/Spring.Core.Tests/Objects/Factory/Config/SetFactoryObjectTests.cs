#region License

/*
 * Copyright 2004 the original author or authors.
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
using Spring.Collections;
using NUnit.Framework;

#endregion

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Unit tests for the SetFactoryObject class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class SetFactoryObjectTests
    {

		[Test]
		[ExpectedException(typeof (ArgumentException), ExpectedMessage = "The Type passed to the TargetSetType property must implement the 'Spring.Collections.ISet' interface.")]
		public void SetTargetSetTypeToNonSetType()
		{
			SetFactoryObject lfo = new SetFactoryObject();
			lfo.TargetSetType = typeof (ICollection);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException), ExpectedMessage="The Type passed to the TargetSetType property cannot be an interface; it must be a concrete class that implements the 'Spring.Collections.ISet' interface.")]
		public void SetTargetSetTypeToDerivedISetInterfaceType()
		{
			SetFactoryObject lfo = new SetFactoryObject();
			lfo.TargetSetType = typeof (IExtendedSet);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException), ExpectedMessage="The Type passed to the TargetSetType property cannot be abstract (MustInherit in VisualBasic.NET); it must be a concrete class that implements the 'Spring.Collections.ISet' interface.")]
		public void SetTargetSetTypeToAbstractISetInterfaceType()
		{
			SetFactoryObject lfo = new SetFactoryObject();
			lfo.TargetSetType = typeof (AbstractSet);
		}

		private interface IExtendedSet : ISet
		{
		}

		private abstract class AbstractSet : HybridSet
		{
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void SetTargetSetTypeToNull()
		{
			SetFactoryObject lfo = new SetFactoryObject();
			lfo.TargetSetType = null;
		}

		[Test]
		[ExpectedException(typeof (ArgumentException), ExpectedMessage="The 'SourceSet' property cannot be null (Nothing in Visual Basic.NET).")]
		public void GetObjectWithoutSupplyingASourceSet()
		{
			SetFactoryObject lfo = new SetFactoryObject();
			lfo.IsSingleton = false;
			lfo.GetObject();
		}

		[Test]
		public void ObjectTypeReallyIsISet()
		{
			Assert.AreEqual(typeof (ISet), new SetFactoryObject().ObjectType);
		}
	}
}
