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

using System.Collections;

using Common.Logging;
using Spring.Core;
using Spring.Util;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Configure all ISharedStateAware objects, delegating concrete handling to the list of <see cref="SharedStateFactories"/>.
    /// </summary>
    public class SharedStateAwareProcessor : IObjectPostProcessor, IOrdered
    {
        // holds the logger for this processor instance
        private readonly ILog Log = LogManager.GetLogger( typeof( SharedStateAwareProcessor ) );
        // holds a list of ISharedStateProvider instances (if any)
        private ISharedStateFactory[] _sharedStateFactories = new ISharedStateFactory[0];
        // holds prio
        private int _order = Int32.MaxValue;

        /// <summary>
        /// Return the order value of this object, where a higher value means greater in
        /// terms of sorting.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Normally starting with 0 or 1, with <see cref="System.Int32.MaxValue"/> indicating
        /// greatest. Same order values will result in arbitrary positions for the affected
        /// objects.
        /// </p>
        /// <p>
        /// Higher value can be interpreted as lower priority, consequently the first object
        /// has highest priority.
        /// </p>
        /// </remarks>
        /// <returns>The order value.</returns>
        public int Order
        {
            get { return _order; }
            set { _order = value; }
        }

        /// <summary>
        /// Get/Set the (already ordererd!) list of <see cref="ISharedStateFactory"/> instances.
        /// </summary>
        /// <remarks>
        /// If this list is not set, the containing object factory will automatically
        /// be scanned for <see cref="ISharedStateFactory"/> instances.
        /// </remarks>
        public ISharedStateFactory[] SharedStateFactories
        {
            get { return _sharedStateFactories; }
            set
            {
                AssertUtils.ArgumentHasElements( value, "SharedStateFactories" );
                _sharedStateFactories = value;
            }
        }

        /// <summary>
        /// Creates a new empty instance.
        /// </summary>
        public SharedStateAwareProcessor()
        { }

        /// <summary>
        /// Creates a new preconfigured instance.
        /// </summary>
        /// <param name="sharedStateFactories"></param>
        /// <param name="order">priority value affecting order of invocation of this processor. See <see cref="IOrdered"/> interface.</param>
        public SharedStateAwareProcessor( ISharedStateFactory[] sharedStateFactories, int order )
        {
            SharedStateFactories = sharedStateFactories;
        }

        /// <summary>
        /// Iterates over configured list of <see cref="ISharedStateFactory"/>s until
        /// the first provider is found that<br/>
        /// a) true == provider.CanProvideState( instance, name )<br/>
        /// b) null != provider.GetSharedState( instance, name )<br/>
        /// </summary>
        public object PostProcessBeforeInitialization( object instance, string name )
        {
            if (SharedStateFactories.Length == 0)
            {
                return instance;
            }

            ISharedStateAware ssa = instance as ISharedStateAware;
            if (ssa != null && ssa.SharedState == null)
            {
                // probe for first factory willing to serve shared state
                foreach (ISharedStateFactory ssf in _sharedStateFactories)
                {
                    if (ssf.CanProvideState( ssa, name ))
                    {
                        IDictionary sharedState = ssf.GetSharedStateFor( ssa, name );
                        if (sharedState != null)
                        {
                            ssa.SharedState = sharedState;
                            break;
                        }
                    }
                }
            }
            return instance;
        }

        /// <summary>
        /// A NoOp for this processor
        /// </summary>
        /// <param name="instance">
        /// The new object instance.
        /// </param>
        /// <param name="name">
        /// The name of the object.
        /// </param>
        /// <returns>
        /// the original <paramref name="instance"/>.
        /// </returns>
        public object PostProcessAfterInitialization( object instance, string name )
        {
            return instance;
        }
    }
}
