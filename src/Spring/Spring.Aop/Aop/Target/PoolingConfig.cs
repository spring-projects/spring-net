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

namespace Spring.Aop.Target
{
	/// <summary>
	/// Configuration interface for a pooling invoker.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	public interface PoolingConfig
	{
		/// <summary>
		/// The number of active object instances in a pool.
		/// </summary>
		int Active { get; }

		/// <summary>
		/// The number of free object instances in a pool.
		/// </summary>
		int Free { get; }

		/// <summary>
		/// The maximum number of object instances in a pool.
		/// </summary>
		int MaxSize { get; }
	}
}