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

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Convenience implementation of the <see cref="IFactoryObject"/> interface that
    /// delegates to an arbitrary object + method to perform the object construction.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Because this <see cref="IFactoryObject"/> implementation requires a delegate
    /// passed to its ctor, its only possible to configure this object and register
    /// it with the <see cref="IObjectFactory"/> via code rather than via XML.
    /// </para>
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class DelegateInvokingFactoryObject<T> : IFactoryObject
    {
        private readonly bool _isSingleton;
        private readonly Func<T> _builderDelegate;


        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateInvokingFactoryObject{T}"/> class.
        /// </summary>
        /// <param name="builderDelegate">The builder delegate.</param>
        /// <param name="isSingleton">if set to <c>true</c> [is singleton].</param>
        public DelegateInvokingFactoryObject(Func<T> builderDelegate, bool isSingleton)
        {
            _builderDelegate = builderDelegate;
            _isSingleton = isSingleton;
        }


        /// <summary>
        /// Return an instance (possibly shared or independent) of the object
        ///             managed by this factory.
        /// </summary>
        /// <remarks>
        /// <note type="caution">If this method is being called in the context of an enclosing IoC container and
        ///             returns <see langword="null"/>, the IoC container will consider this factory
        ///             object as not being fully initialized and throw a corresponding (and most
        ///             probably fatal) exception.
        ///             </note>
        /// </remarks>
        /// <returns>
        /// An instance (possibly shared or independent) of the object managed by
        ///             this factory.
        /// </returns>
        public object GetObject()
        {
            return _builderDelegate.Invoke();
        }

        /// <summary>
        /// Return the <see cref="T:System.Type"/> of object that this
        ///             <see cref="T:Spring.Objects.Factory.IFactoryObject"/> creates, or
        ///             <see langword="null"/> if not known in advance.
        /// </summary>
        public Type ObjectType
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// Is the object managed by this factory a singleton or a prototype?
        /// </summary>
        public bool IsSingleton
        {
            get { return _isSingleton; }
        }
    }
}
