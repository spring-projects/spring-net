#region License

/*
 * Copyright 2002-2004 the original author or authors.
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

using System;
using System.Collections;
using System.ComponentModel;
using DotNetMock;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;

namespace Spring
{
	public class MockConfigurableObjectFactory : MockObject, IConfigurableObjectFactory
	{
		private ExpectationCounter _destroyCalls = new ExpectationCounter("MockConfigurableObjectFactory.DestroySingletonCalls");

		public void SetDisposeCalls(int expectedCalls)
		{
			_destroyCalls.Expected = expectedCalls;
		}

		#region IConfigurableObjectFactory Members

		public void RegisterSingleton(string objectName, object singletonObject)
		{
		}

	    public object GetSingleton(string objectName)
	    {
	        throw new NotImplementedException();
	    }

	    public void Dispose()
		{
			_destroyCalls.Inc();
		}

		public void IgnoreDependencyType(Type type)
		{
		}


	    public bool IsCurrentlyInCreation(string objectName)
	    {
	        throw new NotImplementedException();
	    }

	    public void AddObjectPostProcessor(IObjectPostProcessor objectPostProcessor)
		{
		}

		public int ObjectPostProcessorCount
		{
			get { throw new NotImplementedException(); }
		}

		public void RegisterAlias(string objectName, string alias)
		{
		}

		public void RegisterCustomConverter(Type requiredType, TypeConverter converter)
		{
		}

		public bool ContainsSingleton(string name)
		{
			throw new NotImplementedException();
		}

	    public string[] SingletonNames
	    {
	        get { throw new NotImplementedException(); }
	    }

	    #region ISingletonObjectRegistry Members

	    public int SingletonCount
	    {
	        get { throw new NotImplementedException(); }
	    }

	    #endregion

	    public IObjectDefinition GetObjectDefinition(string objectName)
		{
			return null;
		}

		public IObjectFactory ParentObjectFactory
		{
			set
			{
			}
		}

		#endregion

		#region IHierarchicalObjectFactory Members

		IObjectFactory IHierarchicalObjectFactory.ParentObjectFactory
		{
			get { return null; }
		}

		#endregion

		#region IObjectFactory Members

		public object this[string name]
		{
			get { return null; }
		}

		public bool ContainsObject(string name)
		{
			return false;
		}

		public string[] GetAliases(string name)
		{
			return null;
		}

		public object GetObject(string name, Type requiredType)
		{
			return null;
		}

		object IObjectFactory.GetObject(string name)
		{
			return null;
		}

	    public object GetObject(string name, object[] arguments)
	    {
	        return null;
	    }

	    public object GetObject(string name, Type requiredType, object[] arguments)
	    {
	        return null;
	    }

	    public bool IsSingleton(string name)
		{
			return false;
		}


	    public bool IsPrototype(string name)
	    {
	        return false;
	    }

	    public Type GetType(string name)
		{
			return null;
		}



	    public bool IsTypeMatch(string name, Type targetType)
	    {
	        return false;
	    }

	    public object ConfigureObject(object target)
		{
		    return null;
		}

        public object ConfigureObject(object target, string name)
		{
            return null;
        }

		#endregion
	}

	public class MockObjectFactory : MockObject, IObjectFactory
	{
		private Exception _exceptionToThrow = null;

		public Exception ExceptionToThrow
		{
			get { return _exceptionToThrow; }
			set { _exceptionToThrow = value; }
		}

		#region IObjectFactory Members

		public object this[string name]
		{
			get
			{
				innerExecute();
				return null;
			}
		}

		public bool ContainsObject(string name)
		{
			innerExecute();
			return false;
		}

		public string[] GetAliases(string name)
		{
			innerExecute();
			return null;
		}

		public object GetObject(string name, Type requiredType)
		{
			innerExecute();
			return null;
		}

		object IObjectFactory.GetObject(string name)
		{
			innerExecute();
			return null;
		}

	    public object GetObject(string name, object[] arguments)
	    {
            innerExecute();
            return null;
	    }

	    public object GetObject(string name, Type requiredType, object[] arguments)
	    {
            innerExecute();
            return null;
	    }

	    public bool IsSingleton(string name)
		{
			innerExecute();
			return false;
		}


	    public bool IsPrototype(string name)
	    {
            innerExecute();
	        return false;
	    }

	    public Type GetType(string name)
		{
			innerExecute();
			return null;
		}


	    public bool IsTypeMatch(string name, Type targetType)
	    {
	        innerExecute();
	        return false;
	    }

	    public object ConfigureObject(object target)
		{
            return null;
        }

        public object ConfigureObject(object target, string name)
		{
            return null;
        }

		#endregion

		private void innerExecute()
		{
			if (_exceptionToThrow != null)
			{
				throw _exceptionToThrow;
			}
		}

		public void Dispose()
		{
		}
	}

    public class Inventor
    {
        public string Name;
        public string Nationality;
        public string[] Inventions;
#if NET_2_0
        public DateTime? DateOfGraduation;
#endif
        private DateTime dob;
        private Place pob;

        public Inventor() : this(null, DateTime.MinValue, null)
        {}

        public Inventor(string name, DateTime dateOfBirth, string nationality)
        {
            this.Name = name;
            this.dob = dateOfBirth;
            this.Nationality = nationality;
            this.pob = new Place();
        }

        public DateTime DOB
        {
            get { return dob; }
            set { dob = value; }
        }

        public Place PlaceOfBirth
        {
            get { return pob; }
        }

        public int GetAge(DateTime on)
        {
            // not very accurate, but it will do the job ;-)
            return on.Year - dob.Year;
        }
    }

    public class Place
    {
        public string City;
        public string Country;
    }

    public class Society
    {
        public string Name = "League of Extraordinary Gentlemen";
        public static string Advisors = "advisors";
        public static string President = "president";

        private IList members = new ArrayList();
        private IDictionary officers = new Hashtable();

        public IList Members
        {
            get { return members; }
        }

        public IDictionary Officers
        {
            get { return officers; }
        }

        public bool IsMember(string name)
        {
            bool found = false;
            foreach (Inventor inventor in members)
            {
                if (inventor.Name == name)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }
    }


}