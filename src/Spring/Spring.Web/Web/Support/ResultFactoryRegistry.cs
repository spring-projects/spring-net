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

#region Imports

using System.Collections;
using Spring.Collections;
using Spring.Util;
using Spring.Web.UI;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// A result factory is responsible for create an <see cref="IResult"/> instance from a given string representation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Factories get registered with the <see cref="ResultFactoryRegistry"/> for a certain <i>resultMode</i> string.
    /// <see cref="DefaultResultWebNavigator"/> uses <see cref="ResultFactoryRegistry"/> for converting strings into <see cref="IResult"/> instances
    /// implementing the corresponding navigation logic.
    /// </para>
    /// <para>
    /// Result string representations are always of the form:<br/>
    /// <c>&quot;&lt;resultmode&gt;:&lt;textual result representation&gt;&quot;</c><br/>
    /// Calling <see cref="CreateResult"/> on the registry will cause the registry to first extract the leading <c>resultmode</c> to obtain
    /// the corresponding <see cref="IResultFactory"/> instance and handle the actual <see cref="IResult"/> instantiation by delegating to
    /// <see cref="IResultFactory.CreateResult"/>.
    /// </para>
    /// <example>
    /// The following example illustrates the usual flow:
    /// <code>
    /// class MySpecialResultLogic : IResult
    /// {
    ///   ...
    /// }
    ///
    /// class MySpecialResultLogicFactory : IResultFactory
    /// {
    ///    IResult Create( string mode, string expression ) { /* ... convert 'expression' into
    /// MySpecialResultLogic */ }
    /// }
    ///
    /// // register with global factory
    /// ResultFactoryRegistry.RegisterResultFactory( &quot;mySpecialMode&quot;, new MySpecialResultLogicFactory );
    ///
    /// // configure your Results
    /// &lt;object type=&quot;mypage.aspx&quot;&gt;
    ///    &lt;property name=&quot;Results&quot;&gt;
    ///       &lt;dictionary&gt;
    ///          &lt;entry key=&quot;continue&quot; value=&quot;mySpecialMode:&lt;some MySpecialResultLogic string representation&gt;&quot; /&gt;
    ///       &lt;/dictionary&gt;
    ///    &lt;/property&gt;
    ///
    /// // on your page call
    /// myPage.SetResult(&quot;continue&quot;);
    /// </code>
    /// </example>
    /// </remarks>
    /// <seealso cref="ResultFactoryRegistry"/>
    /// <seealso cref="IResult"/>
    /// <seealso cref="Result"/>
    /// <seealso cref="DefaultResultWebNavigator"/>
    /// <seealso cref="Page.SetResult(string, object)"/>
    /// <seealso cref="UserControl.SetResult(string, object)"/>
    /// <author>Erich Eichinger</author>
    public class ResultFactoryRegistry
    {
        private static readonly IDictionary s_registeredFactories = new CaseInsensitiveHashtable();
        private static volatile IResultFactory s_defaultFactory;

        static ResultFactoryRegistry()
        {
            Reset();
        }

        /// <summary>
        /// Resets the factory registry to its defaults. Mainly used for unit testing.
        /// </summary>
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

        /// <summary>
        /// Returns the current <see cref="IResultFactory"/> set by <see cref="SetDefaultFactory"/>. Will never be null.
        /// </summary>
        /// <remarks>
        /// The default factory is responsible for handling any unknown result modes.
        /// </remarks>
        public static IResultFactory DefaultResultFactory
        {
            get { return s_defaultFactory; }
        }

        /// <summary>
        /// Set a new default factory
        /// </summary>
        /// <param name="resultFactory">the new default factory instance. Must not be null.</param>
        /// <returns>the previous default factory.</returns>
        public static IResultFactory SetDefaultFactory(IResultFactory resultFactory)
        {
            AssertUtils.ArgumentNotNull(resultFactory, "resultFactory");

            IResultFactory prevFactory = s_defaultFactory;
            s_defaultFactory = resultFactory;
            return prevFactory;
        }

        /// <summary>
        /// Registers a <see cref="IResultFactory"/> for the specified <paramref name="resultMode"/>.
        /// </summary>
        /// <param name="resultMode">the resultMode. Must not be null.</param>
        /// <param name="resultFactory">the factory respponsible for handling <paramref name="resultMode"/> results. Must not be null.</param>
        /// <returns>the factory previously registered for the specified <paramref name="resultMode"/>, if any.</returns>
        /// <remarks>
        /// See <see cref="ResultFactoryRegistry"/> overview for more information.
        /// </remarks>
        public static IResultFactory RegisterResultMode( string resultMode, IResultFactory resultFactory )
        {
            AssertUtils.ArgumentHasText(resultMode, "resultMode");
            AssertUtils.ArgumentNotNull(resultFactory, "resultFactory");

            lock (s_registeredFactories.SyncRoot)
            {
                IResultFactory prevFactory = (IResultFactory) s_registeredFactories[resultMode];
                s_registeredFactories[resultMode] = resultFactory;
                return prevFactory;
            }
        }

        /// <summary>
        /// Creates a result from the specified <paramref name="resultText"/> by extracting the result mode from
        /// the text and delegating to a corresponding <see cref="IResultFactory"/>, if any.
        /// </summary>
        /// <param name="resultText">the 'resultmode'-prefixed textual representation of the result instance to create.</param>
        /// <returns>
        /// the <see cref="IResult"/> instance corresponding to the textual <paramref name="resultText"/> represenation,
        /// created by the <see cref="IResultFactory"/>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// if either <paramref name="resultText"/> is null or <see cref="IResultFactory.CreateResult"/> returned null.</exception>
        /// <remarks>
        /// This method guarantees that the return value will always be non-null.<br/>
        /// <paramref name="resultText"/> must always be of the form <c>&quot;&lt;resultmode&gt;:&lt;textual result representation&gt;&quot;</c>.
        /// The <c>resultmode</c> will be extracted and the corresponding <see cref="IResultFactory"/> (previously registered
        /// using <see cref="RegisterResultMode"/>) is called to actually create the <see cref="IResult"/> instance. If no factory matches
        /// <c>resultmode</c>, the call is handled to the <see cref="DefaultResultFactory"/>.
        /// </remarks>
        public static IResult CreateResult( string resultText )
        {
            AssertUtils.ArgumentNotNull(resultText, "resultText");

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
