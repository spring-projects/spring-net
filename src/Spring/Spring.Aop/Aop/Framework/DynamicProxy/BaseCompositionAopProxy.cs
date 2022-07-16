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

using System.Runtime.Serialization;

#endregion

namespace Spring.Aop.Framework.DynamicProxy
{
	/// <summary>
	/// Base class that each dynamic composition proxy has to extend.
	/// </summary>
	/// <author>Aleksandar Seovic</author>
    /// <author>Bruno Baia</author>
    [Serializable]
    public abstract class BaseCompositionAopProxy : AdvisedProxy, IAopProxy, ISerializable
    {
        #region Constructor (s) / Destructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BaseCompositionAopProxy()
        {}

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Aop.Framework.DynamicProxy.BaseCompositionAopProxy"/> class.
        /// </summary>
        /// <param name="advised">The proxy configuration.</param>
        public BaseCompositionAopProxy(IAdvised advised) : base(advised)
		{
            base.Initialize(advised, this);
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">Serialization data.</param>
        /// <param name="context">Serialization context.</param>
        protected BaseCompositionAopProxy(SerializationInfo info, StreamingContext context) : base(info, context)
        {}

	    ///<summary>
	    ///Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> with the data needed to serialize the target object.
	    ///</summary>
	    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion

		#region IAopProxy Members

		/// <summary>
		/// Returns this proxy instance
		/// </summary>
		/// <returns></returns>
		object IAopProxy.GetProxy()
		{
			return this;
		}

		#endregion

        #region Equal, HashCode and ToString overrides

        /// <summary>
        /// Delegate to target object handling of equals method.
        /// </summary>
        /// <param name="obj">The object to compare with the current target object</param>
        /// <returns>true if the specified Object is equal to the current target object; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            bool equals = false;
            object target = m_targetSource.GetTarget();
            AdvisedProxy otherProxy = obj as AdvisedProxy;
            object otherTarget = null;
            if (otherProxy != null)
            {
                otherTarget = otherProxy.m_targetSource.GetTarget();
                if (target == null)
                {
                    equals = (otherTarget == null);
                }
                else
                {
                    equals = target.Equals(otherTarget);
                }
            }
            else if (target == null)
            {
                equals = (obj == null);
            }
            else
            {
                equals = target.Equals(obj);
            }
            m_targetSource.ReleaseTarget(target);
            if (otherProxy != null)
            {
                otherProxy.m_targetSource.ReleaseTarget(otherTarget);
            }
            return equals;
        }

        /// <summary>
        /// Delgate to the target object generation of the hash code.
        /// </summary>
        /// <returns>A hash code for the target object.</returns>
        public override int GetHashCode()
        {
            int hashCode = 0;
            object target = m_targetSource.GetTarget();
            if (target != null)
            {
                hashCode = target.GetHashCode();
            }
            m_targetSource.ReleaseTarget(target);
            return hashCode;
        }

        /// <summary>
        /// Returns a String the represents the target object.
        /// </summary>
        /// <returns>A String that represents the target object</returns>
        public override string ToString()
        {
            string str;
            object target = m_targetSource.GetTarget();
            if (target != null)
            {
                str = target.ToString();
            }
            else
            {
                str = base.ToString();
            }
            m_targetSource.ReleaseTarget(target);
            return str;
        }

        #endregion
    }
}
