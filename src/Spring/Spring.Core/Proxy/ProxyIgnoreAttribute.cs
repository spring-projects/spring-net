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

#region Imports

#endregion

namespace Spring.Proxy
{
    /// <summary>
    /// This attribute can be used to mark interfaces that should not be proxied
    /// </summary>
    /// <author>Bruno Baia</author>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    [Serializable]
    public sealed class ProxyIgnoreAttribute : Attribute
    {
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Proxy.ProxyIgnoreAttribute"/> class.
        /// </summary>
        public ProxyIgnoreAttribute()
        {
        }
    }
}
