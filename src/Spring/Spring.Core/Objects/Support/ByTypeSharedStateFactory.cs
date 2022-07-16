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

namespace Spring.Objects.Support
{
    /// <summary>
    /// Serves shared state on a by-type basis.
    /// </summary>
    public class ByTypeSharedStateFactory : AbstractSharedStateFactory
    {
        private Type[] typeFilter;

        /// <summary>
        /// Limit object types to be served by this state manager.
        /// </summary>
        /// <remarks>
        /// Only objects assignable to one of the types in this list
        /// will be served state by this manager.
        /// </remarks>
        public Type[] TypeFilter
        {
            set { typeFilter = value; }
        }

        /// <summary>
        /// Creates a new instance matching all types by default.
        /// </summary>
        public ByTypeSharedStateFactory()
        {}

        /// <summary>
        /// Creates a new instance matching only specified list of types.
        /// </summary>
        /// <param name="typeFilter">the list of types to serve.</param>
        public ByTypeSharedStateFactory(Type[] typeFilter)
        {
            this.typeFilter = typeFilter;
        }

        /// <summary>
        /// Indicate, whether the given instance will be served by this provider
        /// </summary>
        /// <param name="instance">the instance to serve state</param>
        /// <param name="name">the name of the instance</param>
        /// <returns>
        /// a boolean value indicating, whether state shall
        /// be resolved for the given instance or not.
        /// </returns>
        public override bool CanProvideState( object instance, string name )
        {
            if (instance == null)
                return false;

            if (typeFilter == null)
                return true;

            Type instanceType = instance.GetType();
            foreach (Type type in typeFilter)
            {
                if (type.IsAssignableFrom( instanceType ))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the <see cref="Type"/> for the given <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">the instance to obtain the key for.</param>
        /// <param name="name">the name of the instance (ignored by this provider)</param>
        /// <returns>instance.GetType() if it matches the <see cref="TypeFilter"/> list. Null otherwise.</returns>
        /// <remarks>
        /// This method will only be called if <see cref="CanProvideState"/> returned true previously.
        /// </remarks>
        protected override object GetKey( object instance, string name )
        {
            Type key = instance.GetType();
            return key;
        }
    }
}
