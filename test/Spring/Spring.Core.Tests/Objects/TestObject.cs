/*
 * Copyright 2002-2010 the original author or authors.
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using Spring.Collections;
using Spring.Context;
using Spring.Objects.Factory;

namespace Spring.Objects
{
	/// <summary>
	/// Simple test object used for testing object factories, AOP framework etc.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Mark Pollack (.NET)</author>
    public class TestObject : MarshalByRefObject, ITestObject, IObjectFactoryAware, IComparable, IOther, IApplicationContextAware, IObjectNameAware, IInitializingObject, ISharedStateAware
	{
	    public event EventHandler Click;

		public static event EventHandler StaticClick;

		/// <summary>
		/// Public method to programmatically raise the <event>Click</event> event
		/// while testing.
		/// </summary>
		public void OnClick()
		{
			if (Click != null)
			{
				Click(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Public method to programmatically raise the <b>static</b>
		/// <event>Click</event> event while testing.
		/// </summary>
		public static void OnStaticClick()
		{
			if (TestObject.StaticClick != null)
			{
				TestObject.StaticClick(typeof (TestObject), EventArgs.Empty);
			}
		}

	    public int ObjectNumber
	    {
	        get { return objectNumber; }
            set { objectNumber = value; }
	    }

        // for use in ObjectWrapperTests.SetPropertyValuesFailsWhenSettingReadOnlyProperty
        public int ReadOnlyObjectNumber
        {
            get { return objectNumber; }
        }

	    public IndexedTestObject NestedIndexedObject
		{
			get { return nestedIndexedObject; }
			set { nestedIndexedObject = value; }
		}

		/// <summary>
		/// Public Enumeration Property for FileMode.
		/// </summary>
		public FileMode FileMode
		{
			get { return FileModeEnum; }
			set { FileModeEnum = value; }
		}

		public IObjectFactory ObjectFactory
		{
			get { return objectFactory; }
			set { objectFactory = value; }
		}

		public CultureInfo MyCulture
		{
			get { return myCulture; }
			set { myCulture = value; }
		}

		public string Touchy
		{
			get { return touchy; }
			set
			{
				if (value.IndexOf('.') != - 1)
				{
					throw new Exception("Can't contain a .");
				}
				if (value.IndexOf(',') != - 1)
				{
					throw new FormatException("Number format exception: contains a ,");
				}
				this.touchy = value;
			}
		}

		public bool PostProcessed
		{
			get { return postProcessed; }

			set { this.postProcessed = value; }
		}

		public string Name
		{
			get { return name; }
			set { this.name = value; }
		}

		public string Nickname
		{
			get { return nickname; }
			set { this.nickname = value; }
		}

		public virtual int Age
		{
			get { return age; }

			set { this.age = value; }
		}

		public virtual DateTime Date
		{
			get { return date; }

			set { this.date = value; }
		}

		public virtual Single MyFloat
		{
			get { return myFloat; }
			set { this.myFloat = value; }
		}

		public virtual ITestObject Spouse
		{
			get { return spouse; }
			set { this.spouse = value; }
		}

		public virtual ITestObject Sibling
		{
			get { return sibling; }
			set { this.sibling = value; }
		}

		public virtual INestedTestObject Doctor
		{
			get { return doctor; }
			set { this.doctor = value; }
		}

		public virtual INestedTestObject Lawyer
		{
			get { return lawyer; }
			set { this.lawyer = value; }
		}

		public virtual NestedTestObject RealLawyer
		{
			get { return myRealLawyer; }
			set { this.myRealLawyer = value; }
		}

		/// <summary>
		/// A collection of friends.
		/// </summary>
		public virtual ICollection Friends
		{
			get { return friends; }
			set { this.friends = value; }
		}

		/// <summary>
		/// A read-only collection of pets.
		/// </summary>
		public virtual ICollection Pets
		{
			get { return pets; }
		}

		public virtual TestObjectList TypedFriends
		{
			get { return typedFriends; }
			set { typedFriends = value; }
		}

		/// <summary>
		/// A read-only map of periodic table values.
		/// </summary>
		public virtual IDictionary PeriodicTable
		{
			get { return periodicTable; }
		}

		/// <summary>
		/// A read-only set of computer names
		/// </summary>
		public virtual ISet Computers
		{
			get { return computers; }
		}

		public virtual ISet SomeSet
		{
			get { return someSet; }
			set { this.someSet = value; }
		}

		public virtual string[] Hats
		{
			get { return hats; }
			set { hats = value; }
		}

		public virtual IDictionary SomeMap
		{
			get { return someMap; }
			set { this.someMap = value; }
		}

	    public virtual IList SomeList
	    {
            get { return someList; }
            set { this.someList = value;}
	    }

	    public virtual List<string>  SomeGenericStringList
	    {
            get { return someGenericStringList;  }
            set { this.someGenericStringList = value; }
	    }

	    private IList<int> someGenericIListInt32;
	    public virtual IList<int> SomeGenericIListInt32
	    {
	        get { return someGenericIListInt32; }
	        set { someGenericIListInt32 = value; }
	    }

	    private IDictionary<string, int> someGenericIDictionaryStringInt32;
	    public virtual IDictionary<string, int> SomeGenericIDictionaryStringInt32
	    {
	        get { return someGenericIDictionaryStringInt32; }
	        set { someGenericIDictionaryStringInt32 = value; }
	    }

	    private IEnumerable<int> someGenericIEnumerableInt32;
	    public virtual IEnumerable<int> SomeGenericIEnumerableInt32
	    {
	        get { return someGenericIEnumerableInt32; }
	        set { someGenericIEnumerableInt32 = value; }
	    }

	    public virtual NameValueCollection SomeNameValueCollection
	    {
            get { return someNameValueCollection; }
            set { this.someNameValueCollection = value;}
	    }

		protected virtual string HappyPlace
		{
			get { return _happyPlace; }
			set { _happyPlace = value; }
		}

		// used in reflective tests, so don't remove this property...
		private string[] SamsoniteSuitcase
		{
			get { return _samsoniteSuitcase; }
			set { _samsoniteSuitcase = value; }
		}

		public Type ClassProperty
		{
			get { return classProperty; }
			set { classProperty = value; }
		}

	    public IApplicationContext ApplicationContext
	    {
	        get { return applicationContext; }
	        set { applicationContext = value; }
	    }

	    public string ObjectName
	    {
            get { return objectName; }
            set { objectName = value; }
	    }

	    public int ExceptionMethodCallCount
	    {
	        get { return exceptionMethodCallCount; }
	    }

	    public bool InitCompleted
	    {
	        get { return initCompleted; }
	        set { initCompleted = value; }
	    }

	    public Size Size
	    {
            get { return size; }
            set { size = value;}
	    }

	    public string this[int index]
		{
			get
			{
				if (index < 0 || index >= favoriteQuotes.Length)
				{
					throw new ArgumentException("Index out of range");
				}
				return favoriteQuotes[index];
			}
			set
			{
				if (index < 0 || index >= favoriteQuotes.Length)
				{
					throw new ArgumentException("index is out of range.");
				}
				favoriteQuotes[index] = value;
			}
		}

	    private int exceptionMethodCallCount;
        private int objectNumber = 0;
		public FileMode FileModeEnum;
		private IObjectFactory objectFactory;
		private bool postProcessed;
		private int age;
		private string name;
		private string nickname;
		private ITestObject spouse;
		private ITestObject sibling;
		private string touchy;
		private ICollection friends = new LinkedList();
		private TestObjectList typedFriends = new TestObjectList();
		private ICollection pets = new LinkedList();
		private IDictionary periodicTable = new Hashtable();

		private string[] favoriteQuotes = new string[]
			{
				"He who ha-ha ho-ho",
				"The quick brown fox jumped over the lazy dogs."
			};

		private Type classProperty;
		private ISet computers = new HybridSet();
		private ISet someSet = new HybridSet();
		private IDictionary someMap = new Hashtable();
	    private IList someList = new ArrayList();
		private DateTime date = DateTime.Now;
		private Single myFloat = (float) 0.0;
		private CultureInfo myCulture = CultureInfo.InvariantCulture;
		private string[] hats = null;
		private INestedTestObject doctor = new NestedTestObject();
		private INestedTestObject lawyer = new NestedTestObject();
		private NestedTestObject myRealLawyer = new NestedTestObject();
		private IndexedTestObject nestedIndexedObject;
		private string _happyPlace = DefaultHappyPlace;
		private string[] _samsoniteSuitcase = DefaultContentsOfTheSuitcase;

		public const string DefaultHappyPlace = "The_SeaBass_Diner";

		public static readonly string[] DefaultContentsOfTheSuitcase
			= new string[] {"John Pryor's 'Leaving On A Jet Plane' LP"};

	    private IApplicationContext applicationContext;
	    private string objectName;
	    private Size size;
	    private bool initCompleted;

        private IDictionary sharedState;
	    private NameValueCollection someNameValueCollection;
	    private List<string> someGenericStringList;

	    public TestObject()
		{
		}

		public TestObject(string name, int age)
		{
			this.name = name;
			this.age = age;
		}

		public TestObject(string name, int age, INestedTestObject doctor)
		{
			this.name = name;
			this.age = age;
			this.doctor = doctor;
		}

		public TestObject(string name, int age, INestedTestObject doctor, INestedTestObject lawyer)
		{
			this.name = name;
			this.age = age;
			this.doctor = doctor;
			this.lawyer = lawyer;
		}

		public TestObject(ITestObject spouse)
		{
			this.spouse = spouse;
		}

        public TestObject(IList someList)
        {
            this.someList = someList;
        }

        public TestObject(ISet someSet)
        {
            this.someSet = someSet;
        }

        public TestObject(IDictionary someMap)
        {
            this.someMap = someMap;
        }

        public TestObject(NameValueCollection someProps)
        {
            this.someNameValueCollection = someProps;
        }

	    public static TestObject Create(string name)
		{
			return new TestObject(name, 30);
        }

	    public void AfterPropertiesSet()
        {
            initCompleted = true;
        }

        public void AddComputerName(string name)
		{
			if (computers == null)
			{
				computers = new HybridSet();
			}
			computers.Add(name);
		}

		public void AddPeriodicElement(string name, string element)
		{
			if (periodicTable == null)
			{
				periodicTable = new Hashtable();
			}
			periodicTable.Add(name, element);
		}

		public string GetNameWithHonorific(bool isFemale, params string[] lettersAfterName)
		{
			StringBuilder buffer = new StringBuilder();
			buffer.Append(isFemale ? "Ms " : "Mr ");
			buffer.Append(Name);
			buffer.Append(" ");
			foreach (string letters in lettersAfterName)
			{
				buffer.Append(letters);
			}
			return buffer.ToString();
		}

		public override bool Equals(object other)
		{
			return Equals(this, other);
		}

		new public static bool Equals(object @this, object other)
		{
			if (other == null || !(other is ITestObject))
			{
				return false;
			}
			ITestObject tb2 = (ITestObject) other;
			ITestObject tb1 = (ITestObject) @this;
			if (tb2.Age != tb1.Age)
			{
				return false;
			}

			if ((object) tb1.Name == null)
			{
				return (object) tb2.Name == null;
			}

			if (!tb2.Name.Equals(tb1.Name))
			{
				return false;
			}

			return true;
		}

		public override int GetHashCode()
		{
			return (name != null ? name.GetHashCode() : base.GetHashCode());
		}

		public virtual int CompareTo(object other)
		{
			if ((object) this.name != null && other is TestObject)
			{
				return String.CompareOrdinal(this.name, ((TestObject) other).name);
			}
			else
			{
				return 1;
			}
		}

		public virtual string GetDescription()
		{
			string s = "name=" + name + "; age=" + age + "; touchy=" + touchy;
			s += ("; spouse={" + (spouse != null ? spouse.Name : null) + "}");
			return s;
		}

		public override string ToString()
		{
			string s = "name=" + name + "; age=" + age + "; touchy=" + touchy;
			s += ("; spouse={" + (spouse != null ? spouse.Name : null) + "}");
			return s;
		}

        //Used in testing messaging
        public void SetName(string name)
        {
            this.name = name;
        }

		/// <summary>
		/// Throw the given exception
		/// </summary>
		/// <param name="t">An exception to throw.</param>
		public virtual void Exceptional(Exception t)
		{
		    exceptionMethodCallCount++;
			if (t != null)
			{
				throw t;
			}
		}


	    /// <summary>
        /// Throw the given exception
        /// </summary>
        /// <param name="t">An exception to throw.</param>
        /// <returns>314 if exception is null</returns>
        public virtual int ExceptionalWithReturnValue(Exception t)
        {
            exceptionMethodCallCount++;
            if (t != null)
            {
                throw t;
            }
            return 314;
        }

		/// <summary>
		/// Return a reference to the object itself. 'Return this'
		/// </summary>
		/// <returns>a reference to the object itse.f</returns>
		public virtual object ReturnsThis()
		{
			return this;
		}

	    /// <summary>
		/// Funny Named method.
		/// </summary>
		public virtual void Absquatulate()
		{
		}

	    public IDictionary SharedState
	    {
	        get { return sharedState; }
	        set { sharedState = value; }
	    }
	}

    public class TestObjectConverter : TypeConverter
	{
		public override bool CanConvertTo(
			ITypeDescriptorContext context, Type destinationType)
		{
			if ((destinationType == typeof (TestObjectConverter))
				|| (destinationType == typeof (InstanceDescriptor)))
			{
				return true;
			}
			return base.CanConvertTo(context, destinationType);
		}

		public override bool CanConvertFrom
			(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string))
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom
			(ITypeDescriptorContext context, CultureInfo culture, object text_obj)
		{
			if (text_obj is string)
			{
				string text = (string) text_obj;
				TestObject tb = new TestObject();
				string[] split = text.Split(new char[] {'_'});
				tb.Name = split[0];
				tb.Age = int.Parse(split[1]);
				return tb;
			}
			return base.ConvertFrom(context, culture, text_obj);
		}

		public override object ConvertTo(
			ITypeDescriptorContext context, CultureInfo culture, object param, Type destinationType)
		{
			return base.ConvertTo(context, culture, param, destinationType);
		}
	}
}