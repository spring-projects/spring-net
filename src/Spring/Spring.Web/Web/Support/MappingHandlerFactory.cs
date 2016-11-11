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

#region Imports

using System.Collections;
using System.Net;
using System.Web;
using Spring.Context;

#endregion

namespace Spring.Web.Support
{
	/// <summary>
	/// MappingHandleryFactory allows for full Spring-managed &lt;httpHandlers&gt; configuration. 
	/// It uses regular expressions for url matching.
	/// </summary>
	/// <remarks>
	/// <example>
	/// The example below shows, how to map all url requests to spring and let
	/// <see cref="MappingHandlerFactory"/> resolve urls to container-managed <see cref="IHttpHandlerFactory"/> or <see cref="IHttpHandler"/> objects.
	/// <code>
    /// // web.config
    /// 
    /// &lt;httpHandlers&gt;
    ///   &lt;!-- map all requests to spring (just for demo - don't do this at home!) --&gt;
    ///   &lt;add verb=&quot;*&quot; path=&quot;*.*&quot; type=&quot;Spring.Web.Support.MappingHandlerFactory, Spring.Web&quot; /&gt;
    /// &lt;/httpHandlers&gt;
    /// 
    /// // spring-objects.config
    /// 
    /// &lt;object type=&quot;Spring.Web.Support.MappingHandlerFactoryConfigurer, Spring.Web&quot;&gt;
    ///   &lt;property name=&quot;HandlerMap&quot;&gt;
    /// 	  &lt;dictionary&gt;
    /// 	      &lt;entry key=&quot;\.ashx$&quot; value=&quot;standardHandlerFactory&quot; /&gt;
    /// 	      &lt;!-- map any request ending with *.whatever to standardHandlerFactory --&gt;
    /// 	      &lt;entry key=&quot;\.whatever$&quot; value=&quot;specialHandlerFactory&quot; /&gt;
    /// 	  &lt;/dictionary&gt;
    ///   &lt;/property&gt;
    /// &lt;/object&gt;
    /// 
    /// &lt;object name=&quot;standardHandlerFactory&quot; type=&quot;Spring.Web.Support.DefaultHandlerFactory, Spring.Web&quot; /&gt;
    /// 
    /// &lt;object name=&quot;specialHandlerFactory&quot; type=&quot;MySpecialHandlerFactoryImpl&quot; /&gt;
	/// </code>
	/// </example>
	/// </remarks>
	/// <seealso cref="IHttpHandlerFactory"/>
	/// <seealso cref="IHttpHandler"/>
	/// <seealso cref="HandlerMap"/>
	/// <seealso cref="MappingHandlerFactoryConfigurer"/>
	/// <author>Erich Eichinger</author>
	public class MappingHandlerFactory : AbstractHandlerFactory
	{
		private static readonly HandlerMap s_handlerMap = new HandlerMap();

        /// <summary>
        /// Holds the global list of mappings from url patterns to handler names.
        /// </summary>
		public static HandlerMap HandlerMap
		{
			get { return s_handlerMap; }
		}

        /// <summary>
        /// Holds the cache of handler/factory pairs handed out by this factory. This is required 
        /// for proper handling of <see cref="IHttpHandlerFactory.ReleaseHandler"/>.
        /// </summary>
		private readonly Hashtable _handlerWithFactoryTable = new Hashtable();

	    /// <summary>
	    /// Create a handler instance for the given URL. Will try to find a match of <paramref name="rawUrl"/> onto patterns in <see cref="HandlerMap"/>. 
	    /// If a match is found, delegates the call to the matching <see cref="IHttpHandlerFactory.GetHandler"/> method.
	    /// </summary>
	    /// <param name="appContext">the application context corresponding to the current request</param>
	    /// <param name="context">The <see cref="HttpContext"/> instance for this request.</param>
	    /// <param name="requestType">The HTTP data transfer method (GET, POST, ...)</param>
	    /// <param name="rawUrl">The requested <see cref="HttpRequest.RawUrl"/>.</param>
	    /// <param name="physicalPath">The physical path of the requested resource.</param>
	    /// <returns>A handler instance for processing the current request.</returns>
	    protected override IHttpHandler CreateHandlerInstance(IConfigurableApplicationContext appContext, HttpContext context, string requestType, string rawUrl, string physicalPath)
	    {
	        return MapHandlerInstance(appContext, context, requestType, rawUrl, physicalPath, s_handlerMap, _handlerWithFactoryTable);
	    }

        /// <summary>
        /// Obtains a handler by mapping <paramref name="rawUrl"/> to the list of patterns in <paramref name="handlerMappings"/>.
        /// </summary>
	    /// <param name="appContext">the application context corresponding to the current request</param>
	    /// <param name="context">The <see cref="HttpContext"/> instance for this request.</param>
	    /// <param name="requestType">The HTTP data transfer method (GET, POST, ...)</param>
	    /// <param name="rawUrl">The requested <see cref="HttpRequest.RawUrl"/>.</param>
	    /// <param name="physicalPath">The physical path of the requested resource.</param>
        /// <param name="handlerMappings"></param>
        /// <param name="handlerWithFactoryTable"></param>
	    /// <returns>A handler instance for processing the current request.</returns>
	    protected IHttpHandler MapHandlerInstance(IConfigurableApplicationContext appContext, HttpContext context, string requestType, string rawUrl, string physicalPath, HandlerMap handlerMappings, IDictionary handlerWithFactoryTable)
	    {
            // resolve handler instance by mapping the url to the list of patterns
	        HandlerMapEntry handlerMapEntry = handlerMappings.MapPath(rawUrl);
	        if (handlerMapEntry == null)
	        {
	            throw new HttpException(404, HttpStatusCode.NotFound.ToString());
	        }
	        object handlerObject = appContext.GetObject(handlerMapEntry.HandlerObjectName);

	        if (handlerObject is IHttpHandler)
	        {
	            return (IHttpHandler) handlerObject;
	        }
	        else if (handlerObject is IHttpHandlerFactory)
	        {
                // keep a reference to the issuing factory for later ReleaseHandler call
	            IHttpHandlerFactory factory = (IHttpHandlerFactory) handlerObject;
	            IHttpHandler handler = factory.GetHandler(context, requestType, rawUrl, physicalPath);
	            lock(handlerWithFactoryTable.SyncRoot)
	            {
	                handlerWithFactoryTable.Add(handler, factory);
	            }
	            return handler;
	        }

	        throw new HttpException((int)HttpStatusCode.NotFound, HttpStatusCode.NotFound.ToString());
	    }

	    /// <summary>
	    /// Enables a factory to release an existing
	    /// <see cref="System.Web.IHttpHandler"/> instance.
	    /// </summary>
	    /// <param name="handler">
	    /// The <see cref="System.Web.IHttpHandler"/> object to release.
	    /// </param>
	    public override void ReleaseHandler(IHttpHandler handler)
		{
            ReleaseHandler(handler, this._handlerWithFactoryTable);
		}

        /// <summary>
        /// Removes the handler from the handler/factory dictionary and releases the handler.
        /// </summary>
        /// <param name="handler">the handler to be released</param>
        /// <param name="_handlerWithFactoryTable">a dictionary containing (<see cref="IHttpHandler"/>, <see cref="IHttpHandlerFactory"/>) entries.</param>
        protected void ReleaseHandler( IHttpHandler handler, IDictionary _handlerWithFactoryTable )
        {
		    lock (_handlerWithFactoryTable.SyncRoot)
		    {
		        IHttpHandlerFactory factory = _handlerWithFactoryTable[handler] as IHttpHandlerFactory;
		        if (factory != null)
		        {
		            _handlerWithFactoryTable.Remove(handler);
		            factory.ReleaseHandler(handler);
		        }		        
		    }            
        }
	}
}
