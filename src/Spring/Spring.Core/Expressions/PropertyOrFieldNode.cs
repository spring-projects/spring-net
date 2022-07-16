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

using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Runtime.Serialization;

using Spring.Collections;
using Spring.Core;
using Spring.Core.TypeConversion;
using Spring.Core.TypeResolution;
using Spring.Util;
using Spring.Reflection.Dynamic;

namespace Spring.Expressions
{
    /// <summary>
    /// Represents node that navigates to object's property or public field.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class PropertyOrFieldNode : BaseNode
    {
        private const BindingFlags BINDING_FLAGS =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.IgnoreCase;

        private string memberName;
        private IValueAccessor accessor;

        /// <summary>
        /// Create a new instance
        /// </summary>
        public PropertyOrFieldNode()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected PropertyOrFieldNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes the node.
        /// </summary>
        /// <param name="context">The parent.</param>
        private void InitializeNode(object context)
        {
            Type contextType = (context == null || context is Type ? context as Type : context.GetType());

            if (accessor == null || accessor.RequiresRefresh(contextType))
            {
                memberName = this.getText();

                // clear cached member info if context type has changed (for example, when ASP.NET page is recompiled)
                if (accessor != null && accessor.RequiresRefresh(contextType))
                {
                    accessor = null;
                }

                // initialize this node if necessary
                if (contextType != null && accessor == null)
                {
                    // try to initialize node as ExpandoObject value
                    if (contextType == typeof(System.Dynamic.ExpandoObject))
                    {
                        accessor = new ExpandoObjectValueAccessor(memberName);
                    }
                    // try to initialize node as DynamicObject value
                    else if (contextType.IsSubclassOf(typeof(System.Dynamic.DynamicObject)))
                    {
                        accessor = new DynamicObjectValueAccessor(memberName);
                    }
                    // try to initialize node as enum value first
                    else if (contextType.IsEnum)
                    {
                        try
                        {
                            accessor = new EnumValueAccessor(Enum.Parse(contextType, memberName, true));
                        }
                        catch (ArgumentException)
                        {
                            // ArgumentException will be thrown if specified member name is not a valid
                            // enum value for the context type. We should just ignore it and continue processing,
                            // because the specified member could be a property of a Type class (i.e. EnumType.FullName)
                        }
                    }

                    // then try to initialize node as property or field value
                    if (accessor == null)
                    {
                        // check the context type first
                        accessor = GetPropertyOrFieldAccessor(contextType, memberName, BINDING_FLAGS);

                        // if not found, probe the Type type
                        if (accessor == null && context is Type)
                        {
                            accessor = GetPropertyOrFieldAccessor(typeof(Type), memberName, BINDING_FLAGS);
                        }
                    }
                }

                // if there is still no match, try to initialize node as type accessor
                if (accessor == null)
                {
                    try
                    {
                        accessor = new TypeValueAccessor(TypeResolutionUtils.ResolveType(memberName));
                    }
                    catch (TypeLoadException)
                    {
                        if (context == null)
                        {
                            throw new NullValueInNestedPathException("Cannot initialize property or field node '" +
                                                                     memberName +
                                                                     "' because the specified context is null.");
                        }
                        else
                        {
                            throw new InvalidPropertyException(contextType, memberName,
                                                               "'" + memberName +
                                                               "' node cannot be resolved for the specified context [" +
                                                               context + "].");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to resolve property or field.
        /// </summary>
        /// <param name="contextType">
        /// Type to search for a property or a field.
        /// </param>
        /// <param name="memberName">
        /// Property or field name.
        /// </param>
        /// <param name="bindingFlags">
        /// Binding flags to use.
        /// </param>
        /// <returns>
        /// Resolved property or field accessor, or <c>null</c>
        /// if specified <paramref name="memberName"/> cannot be resolved.
        /// </returns>
        private static IValueAccessor GetPropertyOrFieldAccessor(Type contextType, string memberName, BindingFlags bindingFlags)
        {
            try
            {
                PropertyInfo pi = contextType.GetProperty(memberName, bindingFlags);
                if (pi == null)
                {
                    FieldInfo fi = contextType.GetField(memberName, bindingFlags);
                    if (fi != null)
                    {
                        return new FieldValueAccessor(fi);
                    }
                }
                else
                {
                    return new PropertyValueAccessor(pi);
                }
            }
            catch (AmbiguousMatchException)
            {
                PropertyInfo pi = null;

                // search type hierarchy
                while (contextType != typeof(object))
                {
                    pi = contextType.GetProperty(memberName, bindingFlags | BindingFlags.DeclaredOnly);
                    if (pi == null)
                    {
                        FieldInfo fi = contextType.GetField(memberName, bindingFlags | BindingFlags.DeclaredOnly);
                        if (fi != null)
                        {
                            return new FieldValueAccessor(fi);
                        }
                    }
                    else
                    {
                        return new PropertyValueAccessor(pi);
                    }
                    contextType = contextType.BaseType;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns node's value for the given context.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            lock (this)
            {
                InitializeNode(context);

                if (context == null && accessor.RequiresContext)
                {
                    throw new NullValueInNestedPathException(
                        "Cannot retrieve the value of a field or property '" + this.memberName
                        + "', because context for its resolution is null.");
                }
                if (IsProperty || IsField)
                {
                    return GetPropertyOrFieldValue(context, evalContext);
                }
                else
                {
                    return accessor.Get(context);
                }
            }
        }

        /// <summary>
        /// Sets node's value for the given context.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <param name="newValue">New value for this node.</param>
        protected override void Set(object context, EvaluationContext evalContext, object newValue)
        {
            lock (this)
            {
                InitializeNode(context);

                if (context == null && accessor.RequiresContext)
                {
                    throw new NullValueInNestedPathException(
                        "Cannot set the value of a field or property '" + this.memberName
                        + "', because context for its resolution is null.");
                }
                if (IsProperty || IsField)
                {
                    SetPropertyOrFieldValue(context, evalContext, newValue);
                }
                else
                {
                    accessor.Set(context, newValue);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this node represents a property.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this node is a property; otherwise, <c>false</c>.
        /// </value>
        private bool IsProperty
        {
            get { return accessor is PropertyValueAccessor; }
        }

        /// <summary>
        /// Gets a value indicating whether this node represents a field.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this node is a field; otherwise, <c>false</c>.
        /// </value>
        private bool IsField
        {
            get { return accessor is FieldValueAccessor; }
        }

        /// <summary>
        /// Retrieves property or field value.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Property or field value.</returns>
        private object GetPropertyOrFieldValue(object context, EvaluationContext evalContext)
        {
            try
            {
                return accessor.Get(context);
            }
            catch (InvalidOperationException)
            {
                throw new NotReadablePropertyException(evalContext.RootContextType, this.memberName);
            }
            catch (TargetInvocationException e)
            {
                throw new InvalidPropertyException(evalContext.RootContextType, this.memberName,
                                                   "Getter for property '" + this.memberName + "' threw an exception.",
                                                   e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new InvalidPropertyException(evalContext.RootContextType, this.memberName,
                                                   "Illegal attempt to get value for the property '" + this.memberName +
                                                   "'.", e);
            }
        }

        /// <summary>
        /// Sets property value, doing any type conversions that are necessary along the way.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <param name="newValue">New value for this node.</param>
        private void SetPropertyOrFieldValue(object context, EvaluationContext evalContext, object newValue)
        {
            bool isWriteable = accessor.IsWriteable;
            Type targetType = accessor.TargetType;

            try
            {
                if (!isWriteable)
                {
                    if (!AddToCollections(context, evalContext, newValue))
                    {
                        throw new NotWritablePropertyException(
                            "Can't change the value of the read-only property or field '" + this.memberName + "'.");
                    }
                }
                else if (targetType.IsPrimitive && (newValue == null || String.Empty.Equals(newValue)))
                {
                    throw new ArgumentException("Invalid value [" + newValue + "] for property or field '" +
                                                this.memberName + "' of primitive type ["
                                                + targetType + "]");
                }
                else if (newValue == null || ObjectUtils.IsAssignable(targetType, newValue)) // targetType.IsAssignableFrom(newValue.GetType())
                {
                    SetPropertyOrFieldValueInternal(context, newValue);
                }
                else if (!RemotingServices.IsTransparentProxy(newValue) &&
                         (newValue is IList || newValue is IDictionary || newValue is ISet))
                {
                    if (!AddToCollections(context, evalContext, newValue))
                    {
                        object tmpValue =
                            TypeConversionUtils.ConvertValueIfNecessary(targetType, newValue, this.memberName);
                        SetPropertyOrFieldValueInternal(context, tmpValue);
                    }
                }
                else
                {
                    object tmpValue = TypeConversionUtils.ConvertValueIfNecessary(targetType, newValue, this.memberName);
                    SetPropertyOrFieldValueInternal(context, tmpValue);
                }
            }
            catch (TargetInvocationException ex)
            {
                PropertyChangeEventArgs propertyChangeEvent =
                    new PropertyChangeEventArgs(this.memberName, null, newValue);
                if (ex.GetBaseException() is InvalidCastException)
                {
                    throw new TypeMismatchException(propertyChangeEvent, targetType, ex.GetBaseException());
                }
                else
                {
                    throw new MethodInvocationException(ex.GetBaseException(), propertyChangeEvent);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new FatalReflectionException("Illegal attempt to set property '" + this.memberName + "'", ex);
            }
            catch (NotWritablePropertyException)
            {
                throw;
            }
            catch (NotReadablePropertyException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                PropertyChangeEventArgs propertyChangeEvent =
                    new PropertyChangeEventArgs(this.memberName, null, newValue);
                throw new TypeMismatchException(propertyChangeEvent, targetType, ex);
            }
        }

        /// <summary>
        /// Sets property or field value using either dynamic or standard reflection.
        /// </summary>
        /// <param name="context">Object to evaluate node against.</param>
        /// <param name="newValue">New value for this node, converted to appropriate type.</param>
        private void SetPropertyOrFieldValueInternal(object context, object newValue)
        {
            accessor.Set(context, newValue);
        }

        /// <summary>
        /// In the case of read only collections or custom collections that are not assignable from
        /// IList, try to add to the collection.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <param name="newValue">New value for this node.</param>
        /// <returns>true if was able add to IList, IDictionary, or ISet</returns>
        private bool AddToCollections(object context, EvaluationContext evalContext, object newValue)
        {
            // short-circuit if accessor is not readable or if we have an array
            if (!this.accessor.IsReadable || this.accessor.TargetType.IsArray)
            {
                return false;
            }

            bool added = false;

            // try adding values if property is a list...
            if (newValue is IList && !RemotingServices.IsTransparentProxy(newValue))
            {
                IList currentValue = (IList)Get(context, evalContext);
                if (currentValue != null && !currentValue.IsFixedSize && !currentValue.IsReadOnly)
                {
                    foreach (object el in (IList)newValue)
                    {
                        currentValue.Add(el);
                    }
                    added = true;
                }
            }
            // try adding values if property is a dictionary...
            else if (newValue is IDictionary && !RemotingServices.IsTransparentProxy(newValue))
            {
                IDictionary currentValue = (IDictionary)Get(context, evalContext);
                if (currentValue != null && !currentValue.IsFixedSize && !currentValue.IsReadOnly)
                {
                    foreach (DictionaryEntry entry in (IDictionary)newValue)
                    {
                        currentValue[entry.Key] = entry.Value;
                    }
                    added = true;
                }
            }
            // try adding values if property is a set...
            else if (newValue is ISet && !RemotingServices.IsTransparentProxy(newValue))
            {
                ISet currentValue = (ISet)Get(context, evalContext);
                if (currentValue != null)
                {
                    currentValue.AddAll((ICollection)newValue);
                    added = true;
                }
            }
            return added;
        }

        /// <summary>
        /// Utility method that is needed by ObjectWrapper and AbstractAutowireCapableObjectFactory.
        /// We try as hard as we can, but there are instances when we won't be able to obtain PropertyInfo...
        /// </summary>
        /// <param name="context">Context to resolve property against.</param>
        /// <returns>PropertyInfo for this node.</returns>
        internal MemberInfo GetMemberInfo(object context)
        {
            lock (this)
            {
                InitializeNode(context);
            }
            return accessor.MemberInfo;

            //if (IsProperty)
            //{
            //    return (((PropertyValueAccessor) accessor).MemberInfo);
            //}
            //else
            //{
            //    throw new FatalObjectException(
            //        "Cannot obtain PropertyInfo from an expression that does not resolve to a property.");
            //}
        }

        #region IValueAccessor interface

        private interface IValueAccessor
        {
            object Get(object context);
            void Set(object context, object value);

            bool IsReadable { get; }
            bool IsWriteable { get; }
            bool RequiresContext { get; }
            Type TargetType { get; }
            MemberInfo MemberInfo { get; }
            bool RequiresRefresh(Type contextType);
        }

        #endregion

        #region BaseValueAccessor implementation

        private abstract class BaseValueAccessor : IValueAccessor
        {
            public abstract object Get(object context);

            public abstract void Set(object context, object value);

            public virtual bool IsReadable
            {
                get { return true; }
            }

            public virtual bool IsWriteable
            {
                get { return false; }
            }

            public virtual bool RequiresContext
            {
                get { return false; }
            }

            public virtual Type TargetType
            {
                get { throw new NotSupportedException(); }
            }

            public virtual MemberInfo MemberInfo
            {
                get { throw new NotSupportedException(); }
            }

            public virtual bool RequiresRefresh(Type contextType)
            {
                return false;
            }
        }

        #endregion

        #region PropertyValueAccessor implementation

        private class PropertyValueAccessor : BaseValueAccessor
        {
            private SafeProperty property;
            private string name;
            private bool isReadable;
            private bool isWriteable;
            private Type targetType;
            private Type contextType;

            public PropertyValueAccessor(PropertyInfo propertyInfo)
            {
                this.name = propertyInfo.Name;
                this.isReadable = propertyInfo.CanRead;
                this.isWriteable = propertyInfo.CanWrite;
                this.targetType = propertyInfo.PropertyType;
                this.contextType = propertyInfo.DeclaringType;
                this.property = new SafeProperty(propertyInfo);
            }

            public override object Get(object context)
            {
                if (!isReadable)
                {
                    throw new NotReadablePropertyException("Cannot get a non-readable property [" + name + "]");
                }
                return property.GetValue(context);
            }

            public override void Set(object context, object value)
            {
                if (!isWriteable)
                {
                    throw new NotWritablePropertyException("Cannot set a read-only property [" + name + "]");
                }
                property.SetValue(context, value);
            }

            public override bool IsReadable
            {
                get { return isReadable; }
            }

            public override bool IsWriteable
            {
                get { return isWriteable; }
            }

            public override bool RequiresContext
            {
                get { return true; }
            }

            public override Type TargetType
            {
                get { return targetType; }
            }

            public override MemberInfo MemberInfo
            {
                get { return property.PropertyInfo; }
            }

            public override bool RequiresRefresh(Type contextType)
            {
                return this.contextType != contextType;
            }
        }

        #endregion

        #region FieldValueAccessor implementation

        private class FieldValueAccessor : BaseValueAccessor
        {
            private SafeField field;
            private bool isWriteable;
            private Type targetType;
            private Type contextType;

            public FieldValueAccessor(FieldInfo fieldInfo)
            {
                this.field = new SafeField(fieldInfo);
                this.isWriteable = !(fieldInfo.IsInitOnly || fieldInfo.IsLiteral);
                this.targetType = fieldInfo.FieldType;
                this.contextType = fieldInfo.DeclaringType;
            }

            public override object Get(object context)
            {
                return field.GetValue(context);
            }

            public override void Set(object context, object value)
            {
                field.SetValue(context, value);
            }

            public override bool IsWriteable
            {
                get { return isWriteable; }
            }

            public override bool RequiresContext
            {
                get { return true; }
            }

            public override Type TargetType
            {
                get { return targetType; }
            }

            public override MemberInfo MemberInfo
            {
                get { return field.FieldInfo; }
            }

            public override bool RequiresRefresh(Type contextType)
            {
                return this.contextType != contextType;
            }
        }

        #endregion

        #region EnumValueAccessor implementation

        private class EnumValueAccessor : BaseValueAccessor
        {
            private object enumValue;

            public EnumValueAccessor(object enumValue)
            {
                this.enumValue = enumValue;
            }

            public override object Get(object context)
            {
                return enumValue;
            }

            public override void Set(object context, object value)
            {
                throw new NotSupportedException("Cannot set the value of an enum.");
            }
        }

        #endregion

        #region ExpandoObjectValueAccessor implementation

        private class ExpandoObjectValueAccessor : BaseValueAccessor
        {
            private string memberName;

            public ExpandoObjectValueAccessor(string memberName)
            {
                this.memberName = memberName;
            }

            public override object Get(object context)
            {
                var dictionary = context as IDictionary<string, object>;

                object value;
                if (dictionary.TryGetValue(memberName, out value))
                    return value;
                throw new InvalidPropertyException(typeof(System.Dynamic.ExpandoObject), memberName,
                                                  "'" + memberName +
                                                  "' node cannot be resolved for the specified context [" +
                                                  context + "].");
            }

            public override void Set(object context, object value)
            {
                throw new NotSupportedException("Cannot set the value of an expando object.");
            }
        }

        #endregion

        #region DynamicObjectValueAccessor implementation

        private class DynamicObjectValueAccessor : BaseValueAccessor
        {
            private string memberName;

            public DynamicObjectValueAccessor(string memberName)
            {
                this.memberName = memberName;
            }

            public override object Get(object context)
            {
                var dynamicObject = context as System.Dynamic.DynamicObject;

                try
                {
                    var binder = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(
                        Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags.None,
                        memberName,
                        dynamicObject.GetType(),
                        new List<Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo>
                        {
                            Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags.None, null)
                        }
                    );

                    var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);

                    return callsite.Target(callsite, dynamicObject);
                }
                catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException runtimeBinderException)
                {
                    throw new InvalidPropertyException(
                        typeof(System.Dynamic.DynamicObject),
                        memberName,
                        "'" + memberName + "' node cannot be resolved for the specified context [" + context + "].",
                        runtimeBinderException
                    );
                }
            }

            public override void Set(object context, object value)
            {
                throw new NotSupportedException("Cannot set the value of an dynamic object.");
            }
        }

        #endregion

        #region TypeValueAccessor implementation

        private class TypeValueAccessor : BaseValueAccessor
        {
            private Type type;

            public TypeValueAccessor(Type type)
            {
                this.type = type;
            }

            public override object Get(object context)
            {
                return type;
            }

            public override void Set(object context, object value)
            {
                throw new NotSupportedException("Cannot set the value of a type.");
            }
        }

        #endregion
    }
}
