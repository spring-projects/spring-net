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
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

namespace Spring.Objects.Factory
{
	internal class UnsupportedObjectDefinitionImplementation : IObjectDefinition
	{
		public MutablePropertyValues PropertyValues
		{
			get { throw new NotImplementedException(); }
		}

		public ConstructorArgumentValues ConstructorArgumentValues
		{
			get { throw new NotImplementedException(); }
		}

		public MethodOverrides MethodOverrides
		{
			get { throw new NotImplementedException(); }
		}

		public EventValues EventHandlerValues
		{
			get { throw new NotImplementedException(); }
		}

		public string ResourceDescription
		{
			get { return "UnsupportedObjectDefinitionImplementation_Resource"; }
		}

        public bool IsTemplate
        {
            get { throw new NotImplementedException(); }
        }

		public bool IsAbstract
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsSingleton
		{
			get { throw new NotImplementedException(); }
		}

	    public bool IsLazyInit
	    {
	        get { throw new NotImplementedException(); }
	    }

	    public Type ObjectType
		{
			get { throw new NotImplementedException(); }
		}

	    public string ObjectTypeName
	    {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
	    }

	    public AutoWiringMode AutowireMode
	    {
	        get { throw new NotImplementedException(); }
	    }

	    public DependencyCheckingMode DependencyCheck
	    {
	        get { throw new NotImplementedException(); }
	    }

	    public string[] DependsOn
	    {
	        get { throw new NotImplementedException(); }
	    }

	    public string InitMethodName
	    {
	        get { throw new NotImplementedException(); }
	    }

	    public string DestroyMethodName
	    {
	        get { throw new NotImplementedException(); }
	    }

	    public string FactoryMethodName
	    {
	        get { throw new NotImplementedException(); }
	    }

	    public string FactoryObjectName
	    {
	        get { throw new NotImplementedException(); }
	    }

	    public bool IsAutowireCandidate
	    {
	        get { throw new NotImplementedException(); }
	    }
	}
}