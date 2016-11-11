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

#endregion

namespace Spring.Aop.Framework.DynamicProxy
{
    /// <summary>
    /// Useful <see cref="Spring.Aop.ITargetSource"/> implementation 
    /// that checks calls to GetTarget and ReleaseTarget.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Bruno Baia (.NET)</author>
    public class MockTargetSource : ITargetSource
    {
        private object _target;

        public int gets;
        public int releases;

        public void Reset()
        {
            this._target = null;
            gets = releases = 0;
        }

        public void SetTarget(Object target)
        {
            this._target = target;
        }

        public void Verify()
        {
            if (gets != releases)
                throw new Exception("Expectation failed: " + gets + " gets and " + releases + " releases");
        }

        #region ITargetSource Members

        public Type TargetType
        {
            get { return _target.GetType(); }
        }

        public bool IsStatic
        {
            get { return false; }
        }

        public object GetTarget()
        {
            ++gets;
            return _target;
        }

        public void ReleaseTarget(object target)
        {
            if (target != this._target)
                throw new Exception("Released wrong target");
            ++releases;
        }

        #endregion
    }
}
