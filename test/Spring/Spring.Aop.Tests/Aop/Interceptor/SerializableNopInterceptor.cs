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

#region Imports
using System;
using System.Runtime.Serialization;
#endregion

namespace Spring.Aop.Interceptor
{
	/// <summary>
	/// Subclass of NopInterceptor that is serializable and
	/// can be used to test proxy serialization.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Simon White (.NET)</author>
	[Serializable]
	public sealed class SerializableNopInterceptor : NopInterceptor, ISerializable
	{
		public SerializableNopInterceptor()
		{
		}

		public SerializableNopInterceptor(SerializationInfo info, StreamingContext ctxt)
		{
			this.instanceId = (int) info.GetValue("InstanceId", typeof(int));
            this.count = (int)info.GetValue("Count", typeof(int));
        }

		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
            info.AddValue("InstanceId", InstanceId);
			info.AddValue("Count", Count);
		}

	}
}
