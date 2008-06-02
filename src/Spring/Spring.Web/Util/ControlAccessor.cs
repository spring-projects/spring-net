#region License
/*
 * Copyright © 2002-2006 the original author or authors.
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

using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Web.UI;
using Spring.Reflection.Dynamic;
using Spring.Web.Support;

#endregion

namespace Spring.Util
{
	/// <summary>
	/// Helper class for easier access to reflected Control members.
	/// </summary>
	/// <author>Erich Eichinger</author>
	internal class ControlAccessor
	{
		private static readonly MethodInfo s_miClear = GetMethod("Clear");
        private static MethodInfo GetMethod(string name)
        {
            return typeof(Control).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }
        private static FieldInfo GetField(string name)
        {
            return typeof(Control).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        private delegate ControlCollection CreateControlCollectionDelegate(Control target);
        private delegate void AddedControlDelegate(Control target, Control control, int index);
        private delegate void RemovedControlDelegate(Control target, Control control);
        private delegate void VoidMethodDelegate(Control target);

#if NET_2_0
        private static readonly CreateControlCollectionDelegate BaseCreateControlCollection = (CreateControlCollectionDelegate) Delegate.CreateDelegate(typeof(CreateControlCollectionDelegate), GetMethod("CreateControlCollection"));
	    private static readonly AddedControlDelegate BaseAddedControl = (AddedControlDelegate) Delegate.CreateDelegate(typeof (AddedControlDelegate), GetMethod("AddedControl"));
	    private static readonly RemovedControlDelegate BaseRemovedControl = (RemovedControlDelegate) Delegate.CreateDelegate(typeof (RemovedControlDelegate), GetMethod("RemovedControl"));
	    private static readonly VoidMethodDelegate BaseClearNamingContainer = (VoidMethodDelegate) Delegate.CreateDelegate(typeof (VoidMethodDelegate), GetMethod("ClearNamingContainer"));
#else
        private class DynamicMethodWrapper
        {
            private IDynamicMethod _method;

            public DynamicMethodWrapper(IDynamicMethod method)
            {
                _method = method;
            }

            public static CreateControlCollectionDelegate CreateControlCollection(MethodInfo methodInfo)
            {
                IDynamicMethod method = new SafeMethod(methodInfo);
                return new CreateControlCollectionDelegate(new DynamicMethodWrapper(method).CreateControlCollectionInternal);
            }

            public static AddedControlDelegate AddedControl(MethodInfo methodInfo)
            {
                IDynamicMethod method = new SafeMethod(methodInfo);
                return new AddedControlDelegate(new DynamicMethodWrapper(method).AddedControlInternal);
            }

            public static RemovedControlDelegate RemovedControl(MethodInfo methodInfo)
            {
                IDynamicMethod method = new SafeMethod(methodInfo);
                return new RemovedControlDelegate(new DynamicMethodWrapper(method).RemovedControlInternal);
            }

            public static VoidMethodDelegate ClearNamingContainer(MethodInfo methodInfo)
            {
                IDynamicMethod method = new SafeMethod(methodInfo);
                return new VoidMethodDelegate(new DynamicMethodWrapper(method).ClearNamingContainerInternal);
            }

            private ControlCollection CreateControlCollectionInternal(Control target)
            {
                return (ControlCollection) _method.Invoke(target, null);
            }

            private void AddedControlInternal(Control target, Control control, int index)
            {
                _method.Invoke(target, new object[] { control, index });
            }

            private void RemovedControlInternal(Control target, Control control)
            {
                _method.Invoke(target, new object[] { control });
            }

            private void ClearNamingContainerInternal(Control target)
            {
                _method.Invoke(target, null);
            }
        }

        private static readonly CreateControlCollectionDelegate BaseCreateControlCollection = DynamicMethodWrapper.CreateControlCollection(GetMethod("CreateControlCollection"));
        private static readonly AddedControlDelegate BaseAddedControl = DynamicMethodWrapper.AddedControl(GetMethod("AddedControl"));
        private static readonly RemovedControlDelegate BaseRemovedControl = DynamicMethodWrapper.RemovedControl(GetMethod("RemovedControl"));
        private static readonly VoidMethodDelegate BaseClearNamingContainer = DynamicMethodWrapper.ClearNamingContainer(GetMethod("ClearNamingContainer"));
#endif

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

#if NET_2_0
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
            FieldInfo occasionalFields = GetField("_occasionalFields");
            MethodInfo ensureOccasionalFields = GetMethod("EnsureOccasionalFields");
            FieldInfo controls = occasionalFields.FieldType.GetField("Controls");

            System.Reflection.Emit.DynamicMethod dm = new System.Reflection.Emit.DynamicMethod("get_Controls", typeof(ControlCollection), new Type[] { typeof(Control) }, typeof(Control).Module, true);
            ILGenerator il = dm.GetILGenerator();
            Label occFieldsNull = il.DefineLabel();
            Label retControls = il.DefineLabel();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, occasionalFields);
            il.Emit(OpCodes.Brfalse_S, occFieldsNull);
            il.MarkLabel(retControls);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, occasionalFields);
            il.Emit(OpCodes.Ldfld, controls);
            il.Emit(OpCodes.Ret);
            il.MarkLabel(occFieldsNull);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, ensureOccasionalFields);
            il.Emit(OpCodes.Br, retControls);

            return (GetControlsDelegate) dm.CreateDelegate(typeof(GetControlsDelegate));
        }

        private static SetControlsDelegate GetSetControlsDelegate()
        {
            FieldInfo occasionalFields = GetField("_occasionalFields");
            MethodInfo ensureOccasionalFields = GetMethod("EnsureOccasionalFields");
            FieldInfo controls = occasionalFields.FieldType.GetField("Controls");

            System.Reflection.Emit.DynamicMethod dm = new System.Reflection.Emit.DynamicMethod("set_Controls", null, new Type[] { typeof(Control), typeof(ControlCollection) }, typeof(Control).Module, true);
            ILGenerator il = dm.GetILGenerator();
            Label occFieldsNull = il.DefineLabel();
            Label setControls = il.DefineLabel();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, occasionalFields);
            il.Emit(OpCodes.Brfalse_S, occFieldsNull);
            il.MarkLabel(setControls);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, occasionalFields);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, controls);
            il.Emit(OpCodes.Ret);
            il.MarkLabel(occFieldsNull);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, ensureOccasionalFields);
            il.Emit(OpCodes.Br, setControls);

            return (SetControlsDelegate) dm.CreateDelegate(typeof(SetControlsDelegate));            
        }

//		private static readonly Type s_tOccasionalFields = typeof(Control).GetNestedType("OccasionalFields", BindingFlags.NonPublic);
//
//	    private static readonly VoidMethodDelegate EnsureOccasionalFields = (VoidMethodDelegate) Delegate.CreateDelegate(typeof (VoidMethodDelegate), GetMethod("EnsureOccasionalFields"));
//        private static readonly IDynamicField _occasionalFields = SafeField.CreateFrom(GetField("_occasionalFields"));
//	    private static readonly IDynamicField _controls = SafeField.CreateFrom(s_tOccasionalFields.GetField("Controls"));
//
//		private ControlCollection GetChildControlCollection()
//		{
//			// we *must not* simply call control.Controls here! 
//			// Some controls (e.g. Repeater overload this property and call Control.EnsureChildControls() 
//			// which causes Control.ChildControlsCreated flag to be set and prevents children from being created after loading viewstate!
//
//		    EnsureOccasionalFields(_targetControl);
//
//			object occasionalFields = _occasionalFields.GetValue(_targetControl);
//			object childControls =  _controls.GetValue(occasionalFields);
//			return (ControlCollection) childControls;
//		}
//
//		private void SetChildControlCollection( ControlCollection controls )
//		{
//		    EnsureOccasionalFields(_targetControl);
//
//			object occasionalFields = _occasionalFields.GetValue(_targetControl);
//            _controls.SetValue(occasionalFields, controls);
//		}
#else
	    private static readonly IDynamicField fControls = SafeField.CreateFrom(GetField("_controls"));

		private ControlCollection GetChildControlCollection()
		{
		    return (ControlCollection)fControls.GetValue(_targetControl);
		}

		private void SetChildControlCollection(ControlCollection controls)
		{
            fControls.SetValue(_targetControl, controls);
		}
#endif
	}
}