#region License

/*
 * Copyright 2002-2009 the original author or authors.
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

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// A generic implementation of an <see cref="IObjectFactoryPostProcessor"/>, that delegates post processing to a passed delegate
    /// </summary>
    /// <remarks>
    /// This comes in handy when you want to perform specific tasks on an object factory, e.g. doing special initialization.
    /// </remarks>
    /// <example>
    /// The example below is taken from a unit test. The snippet causes 'someObject' to be registered each time <see cref="Spring.Context.Support.AbstractApplicationContext.Refresh"/> is called on 
    /// the context instance:
    /// <code>
    /// IConfigurableApplicationContext ctx = new XmlApplicationContext(false, &quot;name&quot;, false, null);
    /// ctx.AddObjectFactoryPostProcessor(new DelegateObjectFactoryConfigurer( of =&gt;
    ///     {
    ///         of.RegisterSingleton(&quot;someObject&quot;, someObject);
    ///     }));
    /// </code>
    /// </example>
    /// <author>Erich Eichinger</author>
    public class DelegateObjectFactoryConfigurer : IObjectFactoryPostProcessor
    {
        public delegate void ObjectFactoryConfigurationHandler(IConfigurableListableObjectFactory objectFactory);

        private ObjectFactoryConfigurationHandler _configurationHandler;

        /// <summary>
        /// Get or Set the handler to delegate configuration to
        /// </summary>
        public ObjectFactoryConfigurationHandler ConfigurationHandler
        {
            get { return _configurationHandler; }
            set { _configurationHandler = value; }
        }

        public DelegateObjectFactoryConfigurer()
        { }

        public DelegateObjectFactoryConfigurer(ObjectFactoryConfigurationHandler configurationHandler)
        {
            _configurationHandler = configurationHandler;
        }

        public void PostProcessObjectFactory(IConfigurableListableObjectFactory factory)
        {
            if (_configurationHandler != null)
            {
                _configurationHandler(factory);
            }
        }
    }
}