#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using NVelocity.App;
using Spring.Context;
using Spring.Objects.Factory;

namespace Spring.Template.Velocity
{
    /// <summary>
    /// FactoryObject implementation that configures a VelocityEngine and provides it
    /// as an object reference. This object is intended for any kind of usage of Velocity in
    /// application code, e.g. for generating email content.
    ///
    /// See the base class VelocityEngineFactory for configuration details.
    /// </summary>
    /// <see cref="VelocityEngineFactory"/>
    /// <author>Erez Mazor</author>
    public class VelocityEngineFactoryObject : VelocityEngineFactory, IFactoryObject, IInitializingObject, IResourceLoaderAware
    {
        private VelocityEngine velocityEngine;
        /// <summary>
        /// Get the velocity engine underlying object
        /// </summary>
        /// <returns>An instance of a configured VelocityEngine</returns>
        /// <see cref="IFactoryObject"/>
        public object GetObject() {
            return velocityEngine;
        }

        /// <summary>
        /// Get the type of the velocity engine
        /// </summary>
        public Type ObjectType {
            get { return velocityEngine.GetType(); }
        }

        /// <summary>
        /// Singleton
        /// </summary>
        public bool IsSingleton {
            get { return true; }
        }

        /// <summary>
        /// Facilitate the creation of the velocity engine object
        /// </summary>
        public void AfterPropertiesSet() {
            velocityEngine = CreateVelocityEngine();
        }
    }
}
