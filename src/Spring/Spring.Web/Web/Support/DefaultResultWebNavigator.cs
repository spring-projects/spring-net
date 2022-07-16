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

#region Imports

using System.Collections;
using Spring.Collections;
using Spring.Core;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// The default implementation of the <see cref="IResultWebNavigator"/> interface.
    /// </summary>
    public class DefaultResultWebNavigator : IResultWebNavigator
    {
        private IWebNavigator _parentNavigator;
        private bool _ignoreCase;
        private IDictionary _results;

        /// <summary>
        /// Indicates, whether result names are treated case sensitive by this navigator.
        /// </summary>
        public bool IsCaseSensitive
        {
            get { return !_ignoreCase; }
        }

        /// <summary>
        /// Get/Set the parent of this navigator.
        /// </summary>
        /// <exception cref="InvalidOperationException">if this navigator already has a parent.</exception>
        public virtual IWebNavigator ParentNavigator
        {
            get { return _parentNavigator; }
            set
            {
                if (_parentNavigator != null)
                {
                    throw new InvalidOperationException("Can't set parent navigator because this navigator already has a parent");
                }
                _parentNavigator = value;
            }
        }

        /// <summary>
        /// Gets or sets map of result names to <see cref="IResult"/> instances or their textual representations.
        /// See <see cref="ResultFactoryRegistry"/> for information on parsing textual <see cref="IResult"/> representations.
        /// </summary>
        /// <seealso cref="IResult"/>
        /// <seealso cref="ResultFactoryRegistry"/>
        /// <seealso cref="IResultFactory"/>
        /// <seealso cref="DefaultResultFactory"/>
        public IDictionary Results
        {
            get
            {
                return _results;
            }
            set
            {
                _results = CreateResultsDictionary( value );
            }
        }

        /// <summary>
        /// Creates and initializes a new instance.
        /// </summary>
        public DefaultResultWebNavigator()
            : this( null, null, true )
        { }

        /// <summary>
        /// Creates and initializes a new instance.
        /// </summary>
        /// <param name="parent">the parent of this instance. May be null.</param>
        /// <param name="initialResults">a dictionary of result name to result mappings. May be null.</param>
        /// <param name="ignoreCase">sets, how this navigator treats case sensitivity of result names</param>
        public DefaultResultWebNavigator( IWebNavigator parent, IDictionary initialResults, bool ignoreCase )
        {
            this._parentNavigator = parent;
            this._ignoreCase = ignoreCase;
            this._results = CreateResultsDictionary( initialResults );
        }

        /// <summary>
        /// Create the dictionary instance to be used by this navigator component.
        /// </summary>
        /// <param name="initialResults">a dictionary of intitial result mappings</param>
        /// <returns>the dictionary, that will be used by this navigator.</returns>
        /// <remarks>
        /// Implementors may override this for creating custom dictionaries.
        /// </remarks>
        protected virtual IDictionary CreateResultsDictionary( IDictionary initialResults )
        {
            IDictionary newResults = (_ignoreCase) ? new CaseInsensitiveHashtable() : new Hashtable();
            if (initialResults != null)
            {
                foreach (DictionaryEntry entry in initialResults)
                {
                    newResults[entry.Key.ToString()] = entry.Value;
                }
            }
            return newResults;
        }

        /// <summary>
        /// Determines, whether this navigator or one of its parents can
        /// navigate to the destination specified in <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">the name of the navigation destination</param>
        /// <returns>true, if this navigator can navigate to the destination.</returns>
        public virtual bool CanNavigateTo( string destination )
        {
            if (_results.Contains( destination ))
            {
                return true;
            }
            return (ParentNavigator != null) ? ParentNavigator.CanNavigateTo( destination ) : false;
        }

        /// <summary>
        /// Redirects user to a URL mapped to specified result name.
        /// </summary>
        /// <param name="destination">Name of the result.</param>
        /// <param name="sender">the instance that issued this request</param>
        /// <param name="context">The context to use for evaluating the SpEL expression in the Result.</param>
        public virtual void NavigateTo( string destination, object sender, object context )
        {
            IResult result = GetResult( destination );
            if (result == null)
            {
                if (ParentNavigator != null)
                {
                    ParentNavigator.NavigateTo( destination, sender, context );
                    return;
                }
                HandleUnknownDestination(destination, sender, context);
                return;
            }

            // If no context, 'sender' is context
            if (context == null)
            {
                context = sender;
            }
            result.Navigate( context );
        }

        /// <summary>
        /// Returns a redirect url string that points to the
        /// <see cref="Spring.Web.Support.Result.TargetPage"/> defined by this
        /// result evaluated using this Page for expression
        /// </summary>
        /// <param name="resultName">Name of the result.</param>
        /// <param name="sender">the instance that issued this request</param>
        /// <param name="context">The context to use for evaluating the SpEL expression in the Result</param>
        /// <returns>A redirect url string.</returns>
        public virtual string GetResultUri( string resultName, object sender, object context )
        {
            IResult result = GetResult( resultName );
            if (result == null)
            {
                if (ParentNavigator != null)
                {
                    return result.GetRedirectUri( context );
                }
                return HandleUnknownDestination( resultName, sender, context );
            }

            // If no context, 'sender' is context
            if (context == null)
            {
                context = sender;
            }
            return result.GetRedirectUri( context );
        }

        /// <summary>
        /// Obtain the named result instance from the <see cref="Results"/> dictionary. If necessary, the actual representation of the result
        /// will be converted to an <see cref="IResult"/> instance by this method.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected IResult GetResult( string name )
        {
            object val = _results[name];
            if (val == null)
            {
                return null;
            }
            else if (val is IResult)
            {
                return (IResult)val;
            }
            else if (val is String)
            {
                return ResultFactoryRegistry.CreateResult( (string)val );
            }

            return HandleUnknownResultType(name, val);
        }

        /// <summary>
        /// Handle an unknown result object.
        /// </summary>
        /// <param name="name">the name of the result</param>
        /// <param name="val">the result instance obtained from the <see cref="Results"/> dictionary</param>
        /// <remarks>
        /// By default, this method throws a <see cref="TypeMismatchException"/>.
        /// </remarks>
        protected virtual IResult HandleUnknownResultType(string name, object val)
        {
            throw new TypeMismatchException("Unable to create result object. Please use either String or Result instances to define results." );
        }

        /// <summary>
        /// Handle an unknown destination.
        /// </summary>
        /// <param name="destination">the destination that could not be resolved.</param>
        /// <param name="sender">the sender that issued the request</param>
        /// <param name="context">the context to be used for evaluating any dynamic parts of the destination</param>
        /// <returns>the uri as being returned from <see cref="GetResultUri"/></returns>
        /// <remarks>
        /// By default, this method throws a <see cref="ArgumentOutOfRangeException"/>.
        /// </remarks>
        protected virtual string HandleUnknownDestination( string destination, object sender, object context )
        {
            throw new ArgumentOutOfRangeException( "destination", string.Format( "No mapping found for the specified destination '{0}'.", destination ) );
        }
    }
}
