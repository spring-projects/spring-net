#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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
using Spring.Util;

#endregion

namespace Spring.Web.Support
{
    public class ResultFactoryRegistry
    {
        private static readonly IDictionary s_registeredFactories = new CaseInsensitiveHashtable();
        private static volatile IResultFactory s_defaultFactory;

        static ResultFactoryRegistry()
        {
            Reset();
        }

        public static void Reset()
        {
            s_defaultFactory = null;
            s_registeredFactories.Clear();

            SetDefaultFactory( new DefaultResultFactory() );

            foreach(string resultMode in Enum.GetNames(typeof(ResultMode)))
            {
                RegisterResultMode( resultMode, DefaultResultFactory );
            }
        }

        public static IResultFactory DefaultResultFactory
        {
            get { return s_defaultFactory; }
        }

        public static IResultFactory SetDefaultFactory(IResultFactory resultFactory)
        {
            AssertUtils.ArgumentNotNull(resultFactory, "resultFactory");
            IResultFactory prevFactory = s_defaultFactory;
            s_defaultFactory = resultFactory;
            return prevFactory;
        }

        public static IResultFactory RegisterResultMode( string resultMode, IResultFactory resultFactory )
        {
            lock (s_registeredFactories.SyncRoot)
            {
                IResultFactory prevFactory = (IResultFactory) s_registeredFactories[resultMode];
                s_registeredFactories[resultMode] = resultFactory;
                return prevFactory;
            }
        }

        public static IResult CreateResult( string resultText )
        {
            IResultFactory resultFactory = null;
            string resultMode = null;

            int indexOfResultModeDelimiter = resultText.IndexOf( ':' );
            if (indexOfResultModeDelimiter > 0)
            {
                resultMode = resultText.Substring( 0, indexOfResultModeDelimiter ).Trim();
                resultFactory = (IResultFactory) s_registeredFactories[resultMode];
                resultText = resultText.Substring( indexOfResultModeDelimiter + 1 );
            }

            if (resultFactory == null)
            {
                resultFactory = s_defaultFactory;
            }

            IResult result = resultFactory.CreateResult( resultMode, resultText );
            AssertUtils.ArgumentNotNull(result, "ResultFactories must not return null results");
            return result;            
        }
    }
}