#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using System;

namespace Spring.Context.Support
{  
    /// <author>Mark Pollack</author>
    public class Service : IApplicationContextAware, IMessageSourceAware, IDisposable
    {
        private IApplicationContext applicationContext;
        private IMessageSource messageSource;
        private IResourceLoaderAware[] resources;
        private bool properlyDestroyed = false;

        public IApplicationContext ApplicationContext
        {
            get { return applicationContext; }
            set { applicationContext = value; }
        }

        public IMessageSource MessageSource
        {
            set
            {
                if (messageSource != null)
                {
                    throw new InvalidOperationException("MessageSource should not be set twice");
                }
                messageSource = value;
            }
            get
            {
                return messageSource;
            }

        }


        public IResourceLoaderAware[] Resources
        {
            get { return resources; }
            set { resources = value; }
        }

        public bool ProperlyDestroyed
        {
            get { return properlyDestroyed; }
        }

        public void Dispose()
        {
            properlyDestroyed = true;
            //TODO - try to get object while destroying, expect ObjectCreationNotAllowedException
        }
    }
}