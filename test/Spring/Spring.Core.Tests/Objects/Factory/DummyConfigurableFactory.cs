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

using Spring.Objects.Factory.Config;

namespace Spring.Objects.Factory;

/// <summary>
/// Simple factory based on DummyFactory to allow testing
/// of IConfigurableFactoryObject support in AbstractObjectFactory.
/// </summary>
/// <author>Bruno Baia</author>
public class DummyConfigurableFactory : DummyFactory, IConfigurableFactoryObject
{
    private IObjectDefinition productTemplate;

    public IObjectDefinition ProductTemplate
    {
        get { return productTemplate; }
        set { productTemplate = value; }
    }
}
