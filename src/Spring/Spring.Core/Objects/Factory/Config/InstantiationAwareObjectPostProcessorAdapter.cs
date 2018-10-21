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

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Adapter that implements all methods on <see cref="SmartInstantiationAwareObjectPostProcessor"/>
    /// as no-ops, which will not change normal processing of each object instantiated
    /// by the container. Subclasses may override merely those methods that they are
    /// actually interested in.
    /// </summary>
    /// <remarks>
    /// Note that this base class is only recommendable if you actually require
    /// <see cref="IInstantiationAwareObjectPostProcessor"/> functionality.  If all you need
    /// is plain <see cref="IObjectPostProcessor"/> functionality, prefer a straight
    /// implementation of that (simpler) interface.
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public abstract class InstantiationAwareObjectPostProcessorAdapter : SmartInstantiationAwareObjectPostProcessor
    {
        /// <summary>
        /// Predicts the type of the object to be eventually returned from this
        /// processors PostProcessBeforeInstantiation callback.
        /// </summary>
        /// <param name="objectType">The raw Type of the object.</param>
        /// <param name="objectName">Name of the object.</param>
        /// <returns>The type of the object, or <code>null</code> if not predictable.</returns>
        /// <exception cref="ObjectsException">in case of errors</exception>
        public virtual Type PredictObjectType(Type objectType, string objectName)
        {
            return null;
        }

        /// <summary>
        /// Determines the candidate constructors to use for the given object.
        /// </summary>
        /// <param name="objectType">The raw Type of the object.</param>
        /// <param name="objectName">Name of the object.</param>
        /// <returns>The candidate constructors, or <code>null</code> if none specified</returns>
        /// <exception cref="ObjectsException">in case of errors</exception>
        public virtual ConstructorInfo[] DetermineCandidateConstructors(Type objectType, string objectName)
        {
            return null;
        }

        /// <summary>
        /// Apply this
        /// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>
        /// <i>before the target object gets instantiated</i>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The returned object may be a proxy to use instead of the target
        /// object, effectively suppressing the default instantiation of the
        /// target object.
        /// </p>
        /// <p>
        /// If the object is returned by this method is not
        /// <see langword="null"/>, the object creation process will be
        /// short-circuited. The returned object will not be processed any
        /// further; in particular, no further
        /// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>
        /// callbacks will be applied to it. This mechanism is mainly intended
        /// for exposing a proxy instead of an actual target object.
        /// </p>
        /// <p>
        /// This callback will only be applied to object definitions with an
        /// object class. In particular, it will <b>not</b> be applied to
        /// objects with a "factory-method" (i.e. objects that are to be
        /// instantiated via a layer of indirection anyway).
        /// </p>
        /// </remarks>
        /// <param name="objectType">
        /// The <see cref="System.Type"/> of the target object that is to be
        /// instantiated.
        /// </param>
        /// <param name="objectName">
        /// The name of the target object.
        /// </param>
        /// <returns>
        /// The object to expose instead of a default instance of the target
        /// object.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In the case of any errors.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.Support.AbstractObjectDefinition.HasObjectType"/>
        /// <seealso cref="Spring.Objects.Factory.Support.IConfigurableObjectDefinition.FactoryMethodName"/>
        public virtual object PostProcessBeforeInstantiation(Type objectType, string objectName)
        {
            return null;
        }

        /// <summary>
        /// Perform operations after the object has been instantiated, via a constructor or factory method,
        /// but before Spring property population (from explicit properties or autowiring) occurs.
        /// </summary>
        /// <param name="objectInstance">The object instance created, but whose properties have not yet been set</param>
        /// <param name="objectName">Name of the object.</param>
        /// <returns>true if properties should be set on the object; false if property population
        /// should be skipped.  Normal implementations should return true.  Returning false will also
        /// prevent any subsequent InstantiationAwareObjectPostProcessor instances from being 
        /// invoked on this object instance.</returns>
        public virtual bool PostProcessAfterInstantiation(object objectInstance, string objectName)
        {
            return true;
        }

        /// <summary>
        /// Post-process the given property values before the factory applies them
        /// to the given object.
        /// </summary>
        /// <remarks>Allows for checking whether all dependencies have been
        /// satisfied, for example based on a "Required" annotation on bean property setters.
        /// <para>Also allows for replacing the property values to apply, typically through
        /// creating a new MutablePropertyValues instance based on the original PropertyValues,
        /// adding or removing specific values.
        /// </para>
        /// </remarks>
        /// <param name="pvs">The property values that the factory is about to apply (never <code>null</code>).</param>
        /// <param name="pis">he relevant property infos for the target object (with ignored
        /// dependency types - which the factory handles specifically - already filtered out)</param>
        /// <param name="objectInstance">The object instance created, but whose properties have not yet 
        /// been set.</param>
        /// <param name="objectName">Name of the object.</param>
        /// <returns>The actual property values to apply to the given object (can be the 
        /// passed-in PropertyValues instances0 or null to skip property population.</returns>
        public virtual IPropertyValues PostProcessPropertyValues(IPropertyValues pvs, IList<PropertyInfo> pis, object objectInstance, string objectName)
        {
            return pvs;
        }

        /// <summary>
        /// Apply this <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>
        /// to the given new object instance <i>before</i> any object initialization callbacks.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The object will already be populated with property values.
        /// The returned object instance may be a wrapper around the original.
        /// </p>
        /// </remarks>
        /// <param name="instance">
        /// The new object instance.
        /// </param>
        /// <param name="name">
        /// The name of the object.
        /// </param>
        /// <returns>
        /// The object instance to use, either the original or a wrapped one.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        public virtual object PostProcessBeforeInitialization(object instance, string name)
        {
            return instance;
        }

        /// <summary>
        /// Apply this <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/> to the
        /// given new object instance <i>after</i> any object initialization callbacks.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The object will already be populated with property values. The returned object
        /// instance may be a wrapper around the original.
        /// </p>
        /// </remarks>
        /// <param name="instance">
        /// The new object instance.
        /// </param>
        /// <param name="objectName">
        /// The name of the object.
        /// </param>
        /// <returns>
        /// The object instance to use, either the original or a wrapped one.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        public virtual object PostProcessAfterInitialization(object instance, string objectName)
        {
            return instance;
        }
    }
}