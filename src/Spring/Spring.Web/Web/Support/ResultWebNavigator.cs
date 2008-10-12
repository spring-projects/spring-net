#region License

/*
 * Copyright 2002-2008 the original author or authors.
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
using System.Collections;
using Spring.Collections;
using Spring.Core;
using Spring.Util;

#endregion

namespace Spring.Web.Support
{
    public class ResultWebNavigator : IResultWebNavigator
    {
        private IWebNavigator _parentNavigator;
        private bool _ignoreCase;
        private IDictionary _results;

        public bool IsCaseSensitive
        {
            get { return !_ignoreCase; }
        }

        public virtual IWebNavigator ParentNavigator
        {
            get { return _parentNavigator; }
            set
            {
                if (_parentNavigator != null)
                {
                    AssertUtils.ArgumentNotNull( _parentNavigator, "Parent", "Navigator already has a parent" );
                }
                _parentNavigator = value;
            }
        }

        /// <summary>
        /// Gets or sets map of result names to target URLs
        /// </summary>
        public IDictionary Results
        {
            get
            {
                return _results;
            }
            set
            {
                _results = CreateResultsDictionary(value);
            }
        }

        public ResultWebNavigator()
            :this(null, null, true)
        {}

        public ResultWebNavigator( IWebNavigator parent, IDictionary results, bool ignoreCase )
        {
            this._parentNavigator = parent;
            this._ignoreCase = ignoreCase;
            this._results = CreateResultsDictionary( results );
        }

        protected virtual IDictionary CreateResultsDictionary( IDictionary initialResults )
        {
            IDictionary newResults = (_ignoreCase) ? new CaseInsensitiveHashtable() : new Hashtable();
            if (initialResults != null)
            {
                foreach(DictionaryEntry entry in initialResults)
                {
                    newResults[entry.Key.ToString()] = entry.Value;
                }
            }
            return newResults;
        }

        public virtual bool CanNavigateTo( string resultName )
        {
            if (_results.Contains( resultName ))
            {
                return true;
            }
            return (ParentNavigator != null) ? ParentNavigator.CanNavigateTo( resultName ) : false;
        }

        /// <summary>
        /// Redirects user to a URL mapped to specified result name.
        /// </summary>
        /// <param name="resultName">Name of the result.</param>
        /// <param name="context">The context to use for evaluating the SpEL expression in the Result.</param>
        public virtual void NavigateTo( string resultName, object context )
        {
            IResult result = GetResult( resultName );
            if (result == null)
            {
                if (ParentNavigator != null)
                {
                    ParentNavigator.NavigateTo( resultName, context );
                    return;
                }
                throw new ArgumentException( string.Format( "No result mapping found for the specified name '{0}'.", resultName ), "resultName" );
            }
            result.Navigate( context );
        }

        /// <summary>
        /// Returns a redirect url string that points to the 
        /// <see cref="Spring.Web.Support.Result.TargetPage"/> defined by this
        /// result evaluated using this Page for expression 
        /// </summary>
        /// <param name="resultName">Name of the result.</param>
        /// <param name="context">The context to use for evaluating the SpEL expression in the Result</param>
        /// <returns>A redirect url string.</returns>
        public virtual string GetResultUri( string resultName, object context )
        {
            IResult result = GetResult( resultName );
            if (result == null)
            {
                if (ParentNavigator != null)
                {
                    return result.GetRedirectUri( context );
                }
                throw new ArgumentException( string.Format( "No result mapping found for the specified name '{0}'.", resultName ), "resultName" );
            }
            return result.GetRedirectUri( context );
        }

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
            else
            {
                throw new TypeMismatchException(
                    "Unable to create result object. Please use either String or Result instances to define results." );
            }
        }
    }
}