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

using System.Web.UI;

namespace Spring.Web.UI;

/// <summary>
/// Abstracts storage strategy for storing model instances between requests. 
/// All storage providers participating in UI model management must implement this interface.
/// </summary>
/// <seealso cref="SessionModelPersistenceMedium"/>
/// <author>Erich Eichinger</author>
public interface IModelPersistenceMedium
{
    /// <summary>
    /// Load the model for the specified control context.
    /// </summary>
    /// <param name="context">the control context.</param>
    /// <returns>the model for the specified control context.</returns>
    object LoadFromMedium(Control context);

    /// <summary>
    /// Save the specified model object.
    /// </summary>
    /// <param name="context">the control context.</param>
    /// <param name="modelToSave">the model to save.</param>
    void SaveToMedium(Control context, object modelToSave);
}