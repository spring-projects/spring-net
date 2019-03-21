#region License

/*
 * Copyright � 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

#region Imports

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// Configures <see cref="MappingHandlerFactory"/>.
    /// </summary>
    /// <author>Erich Eichinger</author>
	public class MappingHandlerFactoryConfigurer 
//        : Spring.Objects.Factory.Config.IObjectPostProcessor // just a trick to have
	{
        /// <summary>
        /// Contains mappings of url patterns to handler objects.
        /// </summary>
		public HandlerMap HandlerMap
		{
			get
			{
				return MappingHandlerFactory.HandlerMap;
			}
//			set
//			{
//				AssertUtils.ArgumentNotNull(value, "HandlerMap");
//				foreach(HandlerMapEntry mapEntry in value)
//				{
//					GenericHandlerFactory.HandlerMap[mapEntry.UrlPattern] = mapEntry;
//				}
//			}
        }

        #region IObjectPostProcessor implementation

//        object IObjectPostProcessor.PostProcessBeforeInitialization(object instance, string name)
//	    {
//	        return instance;
//	    }
//
//	    object IObjectPostProcessor.PostProcessAfterInitialization(object instance, string objectName)
//	    {
//	        return instance;
//        }

        #endregion
    }
}