#region License
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
#endregion

#region Imports

using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Web.UI;
using Spring.Reflection.Dynamic;
using Spring.Util;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// Helper class for easier access to reflected ControlCollection members.
    /// </summary>
    /// <author>Erich Eichinger</author>
    internal class ControlCollectionAccessor
    {
        private static readonly IDynamicField _owner;
        static ControlCollectionAccessor()
        {
            IDynamicField owner = null;

            SecurityCritical.ExecutePrivileged( new PermissionSet(PermissionState.Unrestricted), delegate 
            {
#if MONO_2_0
                owner = new SafeField(typeof (ControlCollection).GetField("owner", BindingFlags.Instance | BindingFlags.NonPublic));
#else
                owner = new SafeField(typeof (ControlCollection).GetField("_owner", BindingFlags.Instance | BindingFlags.NonPublic));
#endif
            });
            _owner = owner;
        }

        private readonly ControlCollection _controls;
        private readonly Type _controlsType;

        /// <summary>
        /// Returns the underlying ControlCollection instance.
        /// </summary>
        public ControlCollection GetTarget()
        {
            return _controls;
        }

        /// <summary>
        /// Returns the type of the underlying ControlCollection instance.
        /// </summary>
        public Type GetTargetType()
        {
            return _controlsType;
        }

        /// <summary>
        /// Creates a new Accessor for a given <see cref="ControlCollection"/>.
        /// </summary>
        /// <param name="controls">The <see cref="ControlCollection"/> to be accessed</param>
        public ControlCollectionAccessor(ControlCollection controls)
        {
            _controls = controls;
            _controlsType = controls.GetType();
        }

        /// <summary>
        /// Gets or sets the owner of the underlying ControlCollection.
        /// </summary>
        public Control Owner
        {
            get { return (Control) _owner.GetValue(_controls); }
            set { _owner.SetValue(_controls, value); }
        }
    }
}
