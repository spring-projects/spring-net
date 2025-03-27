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

namespace Spring.Objects
{
    /// <summary>
    /// Adds some extra interfaces and properties to the base clas
    /// so that we can test extra odd corner cases and suchlike.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    public class DerivedTestObject : TestObject, IDisposable
    {
        public const string NicknamePrefix = "#";

        private bool destroyed;
        private string nickers;

        public virtual void Dispose()
        {
            destroyed = true;
        }

        public virtual bool WasDestroyed()
        {
            return destroyed;
        }

        public new string Nickname
        {
            get { return this.nickers; }
            set { this.nickers = NicknamePrefix + value; }
        }
    }
}