#region License

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

#endregion

namespace Spring.DataBinding
{
    /// <summary>
    /// Interface that should be implemented by data bound objects, such as 
    /// web pages, user controls, windows forms, etc.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public interface IDataBound
    {
        /// <summary>
        /// Gets the binding manager.
        /// </summary>
        /// <value>The binding manager.</value>
        IBindingContainer BindingManager { get; }
    }
}