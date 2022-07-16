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
	/// Enumeration that defines possible values for data binding direction.
	/// </summary>
    /// <author>Aleksandar Seovic</author>
    [Flags]
    public enum BindingDirection
	{
        /// <summary>
        /// Specifies that value from the control property should be bound to a data model.
        /// </summary>
        SourceToTarget = 0x0001,

        /// <summary>
        /// Specifies that value from the data model should be bound to control property.
        /// </summary>
        TargetToSource = 0x0002,

        /// <summary>
        /// Specifies that binding is bidirectional.
        /// </summary>
        Bidirectional = SourceToTarget | TargetToSource
	}
}
