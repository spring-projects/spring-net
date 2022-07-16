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

using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;
using System.Web.UI;
using Spring.Reflection.Dynamic;
using Spring.Util;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// Helper class for easier access to reflected Control members.
    /// </summary>
    /// <author>Erich Eichinger</author>
    internal class ControlAccessor
    {
        private delegate ControlCollection CreateControlCollectionDelegate(Control target);
        private delegate void AddedControlDelegate(Control target, Control control, int index);
        private delegate void RemovedControlDelegate(Control target, Control control);
        private delegate void VoidMethodDelegate(Control target);

        private static readonly MethodInfo s_miClear;
        private static readonly SafeField ControlsArrayField;

        private static readonly CreateControlCollectionDelegate BaseCreateControlCollection;
        private static readonly AddedControlDelegate BaseAddedControl;
        private static readonly RemovedControlDelegate BaseRemovedControl;
        private static readonly VoidMethodDelegate BaseClearNamingContainer;

        static ControlAccessor()
        {
            SafeField fldControls = null;
            MethodInfo fnClear = null;

            SecurityCritical.ExecutePrivileged(new PermissionSet(PermissionState.Unrestricted), delegate
            {
                fnClear = GetMethod("Clear");
                fldControls = new SafeField(typeof(ControlCollection).GetField("_controls", BindingFlags.Instance | BindingFlags.NonPublic));
            });

            s_miClear = fnClear;
            ControlsArrayField = fldControls;

            CreateControlCollectionDelegate fnBaseCreateControlCollection = null;
            AddedControlDelegate fnBaseAddedControl = null;
            RemovedControlDelegate fnBaseRemovedControl = null;
            VoidMethodDelegate fnBaseClearNamingContainer = null;

            SecurityCritical.ExecutePrivileged(new PermissionSet(PermissionState.Unrestricted), delegate
            {
                fnBaseCreateControlCollection = (CreateControlCollectionDelegate)Delegate.CreateDelegate(typeof(CreateControlCollectionDelegate), GetMethod("CreateControlCollection"));
                fnBaseAddedControl = (AddedControlDelegate)Delegate.CreateDelegate(typeof(AddedControlDelegate), GetMethod("AddedControl"));
                fnBaseRemovedControl = (RemovedControlDelegate)Delegate.CreateDelegate(typeof(RemovedControlDelegate), GetMethod("RemovedControl"));
                fnBaseClearNamingContainer = (VoidMethodDelegate)Delegate.CreateDelegate(typeof(VoidMethodDelegate), GetMethod("ClearNamingContainer"));
            });

            BaseCreateControlCollection = fnBaseCreateControlCollection;
            BaseAddedControl = fnBaseAddedControl;
            BaseRemovedControl = fnBaseRemovedControl;
            BaseClearNamingContainer = fnBaseClearNamingContainer;
        }

        private static MethodInfo GetMethod(string name)
        {
            return typeof(Control).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }
        private static FieldInfo GetField(string name)
        {
            return typeof(Control).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }


        private readonly Control _targetControl;

        /// <summary>
        /// Instantiates a new Accessor.
        /// </summary>
        /// <param name="control"></param>
        public ControlAccessor(Control control)
        {
            this._targetControl = control;
        }

        /// <summary>
        /// Returns the underlying ControlCollection instance.
        /// </summary>
        public Control GetTarget()
        {
            return _targetControl;
        }

        /// <summary>
        /// Gets or sets the ControlCollection of the target without accessing the target's <see cref="Control.Controls"/> property.
        /// </summary>
        /// <remarks>
        /// If the underlying collection is null, it is automatically created.
        /// </remarks>
        public ControlCollection Controls
        {
            get
            {
                ControlCollection controls = GetChildControlCollection();
                if (controls == null)
                {
                    controls = InterceptControlCollectionStrategy.TryCreateCollection(_targetControl);
                    if (controls == null)
                    {
                        controls = BaseCreateControlCollection(_targetControl);
                    }
                    SetChildControlCollection(controls);
                }
                return controls;
            }
            set { SetChildControlCollection(value); }
        }

        public void AddedControl(Control control, int index)
        {
            BaseAddedControl(_targetControl, control, index);
        }

        public void RemovedControl(Control control)
        {
            BaseRemovedControl(_targetControl, control);

            if (!_targetControl.HasControls())
            {
                // clear naming table etc. if collection has been cleared
                // this is because we can't intercept Control.ClearNamingTable(),
                // which is called by ControlCollection.Clear() after removing all controls
                StackFrame frame = new StackFrame(3, false);
                if (frame.GetMethod() == s_miClear)
                {
                    if (_targetControl is INamingContainer)
                    {
                        //s_miClearNamingContainer.Invoke(_targetControl, null);
                        BaseClearNamingContainer(_targetControl);
                    }
                }
            }
        }

        public void SetControlAt(Control control, int index)
        {
            Control[] controls = (Control[]) ControlsArrayField.GetValue(this.Controls);
            controls[index] = control;
        }

        private delegate ControlCollection GetControlsDelegate(Control target);
        private delegate void SetControlsDelegate(Control target, ControlCollection controls);

        private static readonly GetControlsDelegate GetChildControlCollectionInternal = GetGetControlsDelegate();
        private static readonly SetControlsDelegate SetChildControlCollectionInternal = GetSetControlsDelegate();

        private ControlCollection GetChildControlCollection()
        {
            return GetChildControlCollectionInternal(_targetControl);
        }

        private void SetChildControlCollection(ControlCollection controls)
        {
            SetChildControlCollectionInternal(_targetControl, controls);
        }

        private static GetControlsDelegate GetGetControlsDelegate()
        {
            GetControlsDelegate handler = null;
            FieldInfo controls = GetField("_controls");

            SecurityCritical.ExecutePrivileged(new PermissionSet(PermissionState.Unrestricted), delegate
            {
                System.Reflection.Emit.DynamicMethod dm = new System.Reflection.Emit.DynamicMethod("get_Controls", typeof(ControlCollection), new Type[] {typeof(Control)}, typeof(Control).Module, true);
                ILGenerator il = dm.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, controls);
                il.Emit(OpCodes.Ret);
                handler = (GetControlsDelegate) dm.CreateDelegate(typeof(GetControlsDelegate));
            });
            return handler;
        }

        private static SetControlsDelegate GetSetControlsDelegate()
        {
            SetControlsDelegate handler = null;
            FieldInfo controls = GetField("_controls");
            FieldInfo occasionalFields = GetField("_occasionalFields");
            MethodInfo ensureOccasionalFields = GetMethod("EnsureOccasionalFields");

            SecurityCritical.ExecutePrivileged(new PermissionSet(PermissionState.Unrestricted), delegate
            {
                System.Reflection.Emit.DynamicMethod dm = new System.Reflection.Emit.DynamicMethod("set_Controls ", null, new Type[] {typeof(Control), typeof(ControlCollection)}, typeof(Control).Module, true);
                ILGenerator il = dm.GetILGenerator();
                Label occFieldsNull = il.DefineLabel();
                Label setControls = il.DefineLabel();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, occasionalFields);
                il.Emit(OpCodes.Brfalse_S, occFieldsNull);
                il.MarkLabel(setControls);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, controls);
                il.Emit(OpCodes.Ret);
                il.MarkLabel(occFieldsNull);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, ensureOccasionalFields);
                il.Emit(OpCodes.Br, setControls);
                handler = (SetControlsDelegate) dm.CreateDelegate(typeof(SetControlsDelegate));
            });
            return handler;
        }
    }
}
