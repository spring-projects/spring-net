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

using Spring.Objects.Factory.Config;

#endregion

namespace Spring.Objects.Factory {

	/// <summary>
    /// Simple test of IObjectFactory initialization and lifecycle callbacks.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Rick Evans (.NET)</author>
    public class LifecycleObject :
        IObjectNameAware,
        IInitializingObject,
        IObjectFactoryAware,
        IDisposable {

        #region Methods
        public virtual void PostProcessBeforeInit ()
        {
            if (inited)
            {
                throw new ApplicationException ("Factory called PostProcessBeforeInit after AfterPropertiesSet");
            }
            if (postProcessedBeforeInit)
            {
                throw new SystemException("Factory called PostProcessBeforeInit twice");
            }
            postProcessedBeforeInit = true;
        }

        /// <summary>
        /// Dummy business method that will fail unless the factory
        /// managed the object's lifecycle correctly
        /// </summary>
        public virtual void BusinessMethod ()
        {
            if (!inited || !postProcessedAfterInit)
            {
                throw new SystemException ("Factory didn't initialize lifecycle object correctly");
            }
        }
		
        public virtual void PostProcessAfterInit ()
        {
            if (!inited)
            {
                throw new SystemException("Factory called PostProcessAfterInit before AfterPropertiesSet");
            }
            if (postProcessedAfterInit)
            {
                throw new SystemException("Factory called PostProcessAfterInit twice");
            }
            postProcessedAfterInit = true;
        }
        #endregion

        #region Properties
        public bool Destroyed 
        {
            get 
            {
                return destroyed;
            }
            set 
            {
                destroyed = value;
            }
        }
        #endregion

        #region Fields
        private string objectName;
        private IObjectFactory owningFactory;
        private bool postProcessedBeforeInit;
        private bool inited;
        private bool postProcessedAfterInit;
        private bool destroyed;
        #endregion

        #region IObjectNameAware Members
        public string ObjectName
        {
            get 
            {
                return objectName;
            }
            set
            {
                objectName = value;
            }
        }
        #endregion

        #region IInitializingObject Members
        public void AfterPropertiesSet ()
        {
            if (owningFactory == null)
            {
                throw new SystemException ("Factory didn't call ObjectFactory before AfterPropertiesSet on lifecycle object");
            }
            if (!postProcessedBeforeInit)
            {
                throw new SystemException ("Factory didn't call PostProcessBeforeInit before AfterPropertiesSet on lifecycle object");
            }
            if (inited)
            {
                throw new SystemException ("Factory called AfterPropertiesSet twice");
            }
            inited = true;
        }
        #endregion

        #region IObjectFactoryAware Members
        public IObjectFactory ObjectFactory
        {
            get 
            {
                return owningFactory;
            }
            set
            {
                owningFactory = value;
            }
        }
        #endregion

        #region IDisposable Members
        public void Dispose ()
        {
            if (destroyed)
            {
                throw new SystemException ("Already destroyed...");
            }
            destroyed = true;
        }
        #endregion

        #region Inner Class : PostProcessor
        public class PostProcessor : IObjectPostProcessor, IObjectFactoryAware
        {
			
            public object PostProcessBeforeInitialization (object obj, string name)
            {
                if (obj is LifecycleObject)
                {
                    ((LifecycleObject) obj).PostProcessBeforeInit ();
                }
                return obj;
            }
			
            public object PostProcessAfterInitialization (object obj, string objectName)
            {
                if (obj is LifecycleObject)
                {
                    ((LifecycleObject) obj).PostProcessAfterInit ();
                }
                return obj;
            }

            private IObjectFactory objectFactory;

            public IObjectFactory ObjectFactory
            {
                set { objectFactory=value; }
                get { return objectFactory; }
            }
        }
        #endregion
    }
}
