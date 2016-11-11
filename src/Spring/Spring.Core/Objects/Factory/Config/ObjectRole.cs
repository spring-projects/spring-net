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

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// </summary>
    /// <author>Erich Eichinger</author>
    public enum ObjectRole
    {
        // TODO (EE): ComponentDefinitions are part of Spring/J 2.5

	    /// <summary>
	    /// Role hint indicating that a <see cref="IObjectDefinition"/> is a major part of the application. Typically corresponds to a user-defined object.
	    /// </summary>
	    ROLE_APPLICATION = 0,

        /// <summary>
        /// Role hint indicating that a <see cref="IObjectDefinition"/> is a supporting
        /// part of some larger configuration, typically an outer ComponentDefinition
        /// <code>SUPPORT</code> objects are considered important enough to be aware
        /// of when looking more closely at a particular ComponentDefinition,
        /// but not when looking at the overall configuration of an application.
        /// </summary>
        ROLE_SUPPORT = 1,

        /// <summary>
        /// Role hint indicating that a <see cref="IObjectDefinition"/> is providing an
        /// entirely background role and has no relevance to the end-user. This hint is
        /// used when registering objects that are completely part of the internal workings
        /// of a <code>ComponentDefinition</code>.
        /// </summary>
        ROLE_INFRASTRUCTURE = 2
    }
}