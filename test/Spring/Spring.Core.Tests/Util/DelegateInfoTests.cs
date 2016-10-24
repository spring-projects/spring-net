#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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
using System.Reflection;
using NUnit.Framework;

#endregion

namespace Spring.Util
{
	/// <summary>
	/// Unit tests for the DelegateInfo class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class DelegateInfoTests
	{
		[Test]
		public void Instantiation()
		{
			new DelegateInfo(typeof (EventHandler));
		}

		[Test]
		public void InstantiationWithBadType()
		{
            Assert.Throws<ArgumentException>(() => new DelegateInfo(typeof (string)));
		}

		[Test]
		public void IsDelegateWithBadType()
		{
			Assert.IsFalse(DelegateInfo.IsDelegate(typeof (string)));
		}

		[Test]
		public void IsDelegateWithNullType()
		{
			Assert.IsFalse(DelegateInfo.IsDelegate(null));
		}

		[Test]
		public void IsDelegate()
		{
			Assert.IsTrue(DelegateInfo.IsDelegate(typeof (EventHandler)));
		}

		[Test]
		public void GetReturnType()
		{
			DelegateInfo del = new DelegateInfo(typeof (EventHandler));
			Assert.AreEqual(typeof (void), del.GetReturnType());
		}

		[Test]
		public void GetReturnTypeWithNonVoidReturningDelegate()
		{
			DelegateInfo del = new DelegateInfo(typeof (FooHandler));
			Assert.AreEqual(typeof (string), del.GetReturnType());
		}

		[Test]
		public void GetParameterTypesWithNoArgHandler()
		{
			DelegateInfo del = new DelegateInfo(typeof (NoParametersHandler));
			Type[] types = del.GetParameterTypes();
			Assert.IsNotNull(types);
			Assert.AreEqual(0, types.Length);
		}

		[Test]
		public void GetParameterTypes()
		{
			DelegateInfo del = new DelegateInfo(typeof (FooHandler));
			Type[] types = del.GetParameterTypes();
			Assert.IsNotNull(types);
			Assert.AreEqual(3, types.Length);
		}

		[Test]
		public void IsSignatureCompatibleWithBadMethods()
		{
			DelegateInfo del = new DelegateInfo(typeof (FooHandler));

			// null method...
			MethodInfo method = GetType().GetMethod("SeaBass said that? Well, if that guy over there is SeaBass...");
			Assert.IsFalse(del.IsSignatureCompatible(method));

			// method that doesn;t have the required number of parameters...
			method = GetType().GetMethod("OddNumberOfParametersForFooHandler");
			Assert.IsFalse(del.IsSignatureCompatible(method));

			// method that doesn't have the required number of parameters...
			method = GetType().GetMethod("IncompatibleParametersForFooHandler");
			Assert.IsFalse(del.IsSignatureCompatible(method));
		}

		public string OddNumberOfParametersForFooHandler(string bar, int x)
		{
			return string.Empty;
		}

		public string IncompatibleParametersForFooHandler(string bar, int x, string nope)
		{
			return string.Empty;
		}

		[Test]
		public void IsSignatureCompatibleWithInnerClassStaticMethod()
		{
			DelegateInfo del = new DelegateInfo(typeof (FooHandler));
			MethodInfo method =
				typeof (Yossarian).GetMethod(
					"DoFoo",
					BindingFlags.Public | BindingFlags.Static);
			Assert.IsTrue(del.IsSignatureCompatible(method));
		}

		[Test]
		public void IsSignatureCompatible()
		{
			DelegateInfo del = new DelegateInfo(typeof (FooHandler));
			MethodInfo method = GetType().GetMethod("DoFoo");
			Assert.IsFalse(del.IsSignatureCompatible(method));
		}

		[Test]
		public void IsSignatureCompatibleStaticVersion()
		{
			EventInfo evt = typeof (Assembly).GetEvent("ModuleResolve");

			MethodInfo rubbishMethod = GetType().GetMethod("MethodSignatureIsCompatibleStatic");
			Assert.IsFalse(DelegateInfo.IsSignatureCompatible(evt, rubbishMethod));

			MethodInfo compatibleMethod = typeof (Yossarian).GetMethod("Resolve");
			Assert.IsTrue(DelegateInfo.IsSignatureCompatible(evt, compatibleMethod));

			Assert.IsFalse(DelegateInfo.IsSignatureCompatible(null, compatibleMethod));
			Assert.IsFalse(DelegateInfo.IsSignatureCompatible(evt, null));
		}

		private string DoFoo(string bar, int x, EventArgs e)
		{
			return bar;
		}

		internal sealed class Yossarian
		{
			public static string DoFoo(string bar, int x, EventArgs e)
			{
				return bar;
			}

			public Module Resolve(object sender, ResolveEventArgs e)
			{
				return null;
			}
		}
	}

	internal delegate string FooHandler(string bar, int x, EventArgs e);

	internal delegate void NoParametersHandler();
}