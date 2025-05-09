﻿/*
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

using Spring.Core.IO;

namespace Spring.Objects.Factory.Parsing;

public class Location
{
    private IResource resource;
    private object source;

    /// <summary>
    /// Initializes a new instance of the Location class.
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="source"></param>
    public Location(IResource resource, object source)
    {
        //TODO: look into re-enabling this since resource *is* NULL when parsing config classes vs. acquiring IResources
        //AssertUtils.ArgumentNotNull(resource, "resource");
        this.resource = resource;
        this.source = source;
    }

    /// <summary>
    /// Initializes a new instance of the Location class.
    /// </summary>
    /// <param name="resource"></param>
    public Location(IResource resource)
        : this(resource, null)
    {
    }

    public IResource Resource
    {
        get
        {
            return resource;
        }
    }

    public object Source
    {
        get
        {
            return source;
        }
    }
}