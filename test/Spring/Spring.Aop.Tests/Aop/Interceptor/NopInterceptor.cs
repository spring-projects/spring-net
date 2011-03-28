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

#endregion

namespace Spring.Aop.Interceptor
{
	/// <summary>
	/// Trivial interceptor that can be introduced into an interceptor chain to
	/// aid in debugging.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Choy Rim (.NET)</author>
	public class NopInterceptor : IMethodBeforeAdvice
	{
	    protected int instanceId;
		protected int count;


	    public NopInterceptor() : this(0)
	    {
	    }

        public NopInterceptor(int instanceId)
	    {
	        this.instanceId = instanceId;
	    }

	    public int InstanceId
	    {
	        get { return instanceId; }
	    }

	    public int Count
		{
			get { return this.count; }
		}

		public void Before(MethodInfo method, object[] args, object target)
		{
			++count;
		}

		public override bool Equals(Object other)
		{
			if (!(other is NopInterceptor))
			{
				return false;
			}
			if (this == other)
			{
				return true;
			}
			return (instanceId == ((NopInterceptor) other).InstanceId) 
                && (count == ((NopInterceptor) other).count);
		}

		public override int GetHashCode()
		{
			return instanceId + 13 * count;
		}
	}
}