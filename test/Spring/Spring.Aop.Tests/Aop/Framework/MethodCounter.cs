#region License
/*
 * Copyright 2002-2010 the original author or authors.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      https://www.apache.org/licenses/LICENSE-2.0
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
using System.Reflection;
#endregion

namespace Spring.Aop.Framework
{
	/// <summary>
	/// Useful base class for counting advices etc.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Choy Rim (.NET)</author>
	[Serializable]
    public class MethodCounter
	{
		/// <summary>Method name --> count, does not understand overloading </summary>
		private Hashtable map = new Hashtable();
		private int allCount;
		
		protected internal virtual void Count(MethodBase m)
		{
			Count(m.Name);
		}
		
		protected internal virtual void Count(string methodName)
		{
			int count = GetCalls(methodName);
			++count;
			map[methodName] = count;
			++allCount;
		}
		
		public virtual int GetCalls(string methodName)
		{
			int count = 0;
			if ( map.ContainsKey(methodName) ) 
			{
				count = (int) map[methodName];
			}
			return count;
		}
		
		public virtual int GetCalls()
		{
			return allCount;
		}
	}
}
