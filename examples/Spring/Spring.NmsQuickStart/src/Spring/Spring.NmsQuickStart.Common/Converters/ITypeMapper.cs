 

#region License

/*
 * Copyright 2002-2008 the original author or authors.
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

namespace Spring.NmsQuickStart.Common.Converters
{
    /// <summary>
    /// Provides a layer of indirection when adding the 'type' of the object as a message property.
    /// </summary>
    /// <author>Mark Pollack</author>
    public interface ITypeMapper
    {
        string TypeIdFieldName
        {
            get;
        }
        
        string FromType(Type typeOfObjectToConvert);

        Type ToType(string typeId);
    }
}