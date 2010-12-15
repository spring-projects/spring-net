#region License

/*
 * Copyright © 2002-2010 the original author or authors.
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

using System;
using Spring.Core.IO;

namespace Spring.Context.Support
{
    public abstract class AbstractXmlApplicationContextArgs
    {
        private bool _caseSensitive;

        private string[] _configurationLocations;

        private IResource[] _configurationResources;

        private string _name;

        private IApplicationContext _parentContext;

        private bool _refresh;

        public virtual bool CaseSensitive
        {
            get
            {
                return _caseSensitive;
            }
            set
            {
                _caseSensitive = value;
            }
        }

        public virtual string[] ConfigurationLocations
        {
            get
            {
                if (_configurationLocations == null)
                    _configurationLocations = new string[0];

                return _configurationLocations;
            }
            set
            {
                _configurationLocations = value;
            }
        }

        public virtual IResource[] ConfigurationResources
        {
            get
            {
                if (_configurationResources == null)
                    _configurationResources = new IResource[0];

                return _configurationResources;
            }
            set
            {
                _configurationResources = value;
            }
        }

        public virtual string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public virtual IApplicationContext ParentContext
        {
            get
            {
                return _parentContext;
            }
            set
            {
                _parentContext = value;
            }
        }

        public virtual bool Refresh
        {
            get
            {
                return _refresh;
            }
            set
            {
                _refresh = value;
            }
        }

    }
}
