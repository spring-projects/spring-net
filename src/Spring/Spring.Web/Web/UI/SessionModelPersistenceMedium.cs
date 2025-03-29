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

using System.Web.SessionState;
using System.Web.UI;

namespace Spring.Web.UI;

/// <summary>
/// <see cref="SessionModelPersistenceMedium"/> implements <see cref="HttpSessionState"/>-based storage for 
/// UI model management.
/// </summary>
/// <author>Erich Eichinger</author>
public class SessionModelPersistenceMedium : IModelPersistenceMedium
{
    /// <summary>
    /// Load the model for the specified control context.
    /// </summary>
    /// <remarks>
    /// The key used for loading the model from the session dictionary is obtained by calling <see cref="GetKey"/>
    /// </remarks>
    /// <param name="context">the control context.</param>
    /// <returns>the model for the specified control context.</returns>
    /// <seealso cref="GetKey"/>
    public object LoadFromMedium(Control context)
    {
        return GetItem(context, GetKey(context));
    }

    /// <summary>
    /// Save the specified model object to session.
    /// </summary>
    /// <remarks>
    /// The key used for storing the model into the session dictionary is obtained by calling <see cref="GetKey"/>
    /// </remarks>
    /// <param name="context">the control context.</param>
    /// <param name="modelToSave">the model to save.</param>
    /// <seealso cref="GetKey"/>
    public void SaveToMedium(Control context, object modelToSave)
    {
        SetItem(context, GetKey(context), modelToSave);
    }

    /// <summary>
    /// Create the key to be used for accessing the <see cref="HttpSessionState"/> dictionary.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected virtual string GetKey(Control context)
    {
        return context.Page.Request.CurrentExecutionFilePath + context.UniqueID + ".Model";
    }

    /// <summary>
    /// Abstracts session access for unit testing.
    /// </summary>
    protected virtual object GetItem(Control context, string key)
    {
        return context.Page.Session[key];
    }

    /// <summary>
    /// Abstracts session access for unit testing.
    /// </summary>
    protected virtual void SetItem(Control context, string key, object item)
    {
        context.Page.Session[key] = item;
    }
}
