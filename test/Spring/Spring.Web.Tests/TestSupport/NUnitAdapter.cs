#region Copyright (c) 2002, 2005 by James Shore
/********************************************************************************************************************
'
' Copyright (c) 2002, 2005 by James Shore
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
' THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
'
'******************************************************************************************************************/
#endregion

#region Instructions
/********************************************************************************************************************
 * 
 * This file allows NUnitAsp to be used with NUnit.  To use, copy this file 
 * into your test project.  For additional information, see the NUnitAsp
 * documentation in your download package or visit
 * http://nunitasp.sourceforge.net.
 * 
 *******************************************************************************************************************/
#endregion

using NUnit.Extensions.Asp;
using NUnit.Framework;

namespace Spring.TestSupport
{ 
	/// <summary>
	/// Base class for NUnitAsp test fixtures.  Extend this class to use NUnitAsp.
	/// </summary>
	[TestFixture]
	public abstract class WebFormTestCase : WebApplicationTests
	{
		private bool setupCalled = false;

	    protected WebFormTestCase(string virtualPath, string relativePhysicalPath) : base(virtualPath, relativePhysicalPath)
	    {}

	    /// <summary>
		/// Do not call.  For use by NUnit only.
		/// </summary>
		[SetUp]
		public void MasterSetUp() 
		{
			setupCalled = true;
			HttpClient.Default = new HttpClient();
			SetUp();
		}

		/// <summary>
		/// Executed before each test method is run.  Override in subclasses to do subclass
		/// set up.  NOTE: The [SetUp] attribute cannot be used in subclasses because it is already
		/// in use.
		/// </summary>
		protected virtual void SetUp()
		{
		}

		/// <summary>
		/// Do not call.  For use by NUnit only.
		/// </summary>
		[TearDown]
		public void MasterTearDown()
		{
			TearDown();
		}

		/// <summary>
		/// Executed after each test method is run.  Override in subclasses to do subclass
		/// clean up.  NOTE: [TearDown] attribute cannot be used in subclasses because it is
		/// already in use.
		/// </summary>
		protected virtual void TearDown()
		{
		}

		/// <summary>
		/// The web form currently loaded by the browser.
		/// </summary>
		protected WebFormTester CurrentWebForm
		{
			get 
			{
				AssertSetUp();
				return new WebFormTester(HttpClient.Default);
			}
		}

		/// <summary>
		/// The web browser.
		/// </summary>
		protected HttpClient Browser 
		{
			get 
			{
				AssertSetUp();
				return HttpClient.Default;
			}
		}

		private void AssertSetUp()
		{
			if (!setupCalled) 
			{
				Assert.Fail("A required setup method in WebFormTestCase was not called.  This is probably because you used the [SetUp] attribute in a subclass of WebFormTestCase.  That is not supported.  Override the SetUp() method instead.");
			}
		}
	}









// Everything below this line is for backwards compatibility and may be deleted.

	/// <summary>
	/// For backwards compatibility; will be deprecated in the future.
	/// This class provides convenience methods for common assertions.  You
	/// should use Assert and WebAssert methods instead.
	/// </summary>
	public class CompatibilityAdapter
	{
		/// <summary>
		/// For backwards compatibility; will be deprecated in the future.
		/// </summary>
		public static void AssertTrue(bool condition)
		{
			Assert.IsTrue(condition);
		}

		/// <summary>
		/// For backwards compatibility; will be deprecated in the future.
		/// </summary>
		public static void AssertTrue(string message, bool condition)
		{
			Assert.IsTrue(condition, message);
		}

		/// <summary>
		/// For backwards compatibility; will be deprecated in the future.
		/// </summary>
		public static void AssertEquals(object expected, object actual)
		{
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		/// For backwards compatibility; will be deprecated in the future.
		/// </summary>
		public static void AssertEquals(string message, object expected, object actual)
		{
			Assert.AreEqual(expected, actual, message);
		}

		/// <summary>
		/// For backwards compatibility; will be deprecated in the future.
		/// </summary>
		public static void AssertNotNull(object o)
		{
			Assert.IsNotNull(o);
		}

		/// <summary>
		/// For backwards compatibility; will be deprecated in the future.
		/// </summary>
		public static void AssertNotNull(string message, object o)
		{
			Assert.IsNotNull(o, message);
		}

		/// <summary>
		/// For backwards compatibility; will be deprecated in the future.
		/// </summary>
		public static void AssertNull(object o)
		{
			Assert.IsNull(o);
		}

		/// <summary>
		/// For backwards compatibility; will be deprecated in the future.
		/// </summary>
		public static void AssertNull(string message, object o)
		{
			Assert.IsNull(o, message);
		}

		/// <summary>
		/// For backwards compatibility; will be deprecated in the future.
		/// </summary>
		public static void AssertSame(object expected, object actual)
		{
			Assert.AreSame(expected, actual);
		}

		/// <summary>
		/// For backwards compatibility; will be deprecated in the future.
		/// </summary>
		public static void AssertSame(string message, object expected, object actual)
		{
			Assert.AreSame(expected, actual, message);
		}

		/// <summary>
		/// For backwards compatibility; will be deprecated in the future.
		/// </summary>
		public static void Fail(string message)
		{
			Assert.Fail(message);
		}

		/// <summary>
		/// For backwards compatibility; will be deprecated in the future.
		/// </summary>
		public static void AssertVisibility(ControlTester tester, bool expectedVisibility)
		{
			if (expectedVisibility) WebAssert.Visible(tester);
			else WebAssert.NotVisible(tester);
		}

		/// <summary>
		/// For backwards compatibility; will be deprecated in the future.
		/// </summary>
		public static void AssertEquals(string[] expected, string[] actual)
		{
			WebAssert.AreEqual(expected, actual);
		}

		/// <summary>
		/// For backwards compatibility; will be deprecated in the future.
		/// </summary>
		public static void AssertEquals(string message, string[] expected, string[] actual)
		{
			WebAssert.AreEqual(expected, actual, message);
		}

		/// <summary>
		/// For backwards compatibility; will be deprecated in the future.
		/// </summary>
		//[CLSCompliant(false)]
		public static void AssertEquals(string[][] expected, string[][] actual)
		{
			WebAssert.AreEqual(expected, actual);
		}

		/// <summary>
		/// For backwards compatibility; will be deprecated in the future.
		/// </summary>
		//[CLSCompliant(false)]
		public static void AssertEquals(string message, string[][] expected, string[][] actual)
		{
			WebAssert.AreEqual(expected, actual, message);
		}

		/// <summary>
		/// For backwards compatibility; will be deprecated in the future.
		/// </summary>
		//[CLSCompliant(false)]
		public static void AssertEqualsIgnoreOrder(string message, string[][] expected, string[][] actual)
		{
			WebAssert.AreEqualIgnoringOrder(expected, actual, message);
		}

		/// <summary>
		/// For backwards compatibility; will be deprecated in the future.
		/// </summary>
		//[CLSCompliant(false)]
		public static void AssertSortOrder(string message, string[][] data, int column, bool isAscending, DataType type)
		{
			WebAssert.Sorted(data, column, isAscending, type, message);
		}
	}
}