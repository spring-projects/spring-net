#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Specifies how instances of the
	/// <see cref="Spring.Objects.Factory.Config.PropertyPlaceholderConfigurer"/>
	/// class must apply environment variables when replacing values.
	/// </summary>
	/// <author>Mark Pollack</author>
    [Serializable]
    public enum EnvironmentVariableMode
	{
		/// <summary>
		/// Never replace environment variables.
		/// </summary>
		Never = 1,

		/// <summary>
		/// If properties are not specified via a resource, 
		/// then resolve using environment variables.
		/// </summary>
		Fallback = 2,

		/// <summary>
		/// Apply environment variables first before applying properties from a
		/// resource.
		/// </summary>
		Override = 3
	}
}
