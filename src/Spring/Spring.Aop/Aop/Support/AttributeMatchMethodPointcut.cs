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

#region Imports

using System.Reflection;
using System.Runtime.Serialization;

using Spring.Util;

#endregion

namespace Spring.Aop.Support
{
	/// <summary>
	/// <see cref="Spring.Aop.IPointcut"/> implementation that matches methods
	/// that have been decorated with a specified <see cref="System.Attribute"/>.
	/// </summary>
	/// <author>Aleksandar Seovic</author>
    /// <author>Ronald Wildenberg</author>
	[Serializable]
    public class AttributeMatchMethodPointcut : StaticMethodMatcherPointcut, ISerializable
	{
		private Type _attribute;
		private bool _inherit = true;
        private bool _checkInterfaces = false;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Support.AttributeMatchMethodPointcut"/> class.
		/// </summary>
		public AttributeMatchMethodPointcut()
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Support.AttributeMatchMethodPointcut"/> class.
		/// </summary>
		/// <param name="attribute">
		/// The <see cref="System.Attribute"/> to match.
		/// </param>
        public AttributeMatchMethodPointcut(Type attribute)
            : this(attribute, true, false)
        {
        }

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Support.AttributeMatchMethodPointcut"/>
		/// class.
		/// </summary>
		/// <param name="attribute">
		/// The <see cref="System.Attribute"/> to match.
		/// </param>
		/// <param name="inherit">
		/// Flag that controls whether or not the inheritance tree of the
		/// method to be included in the search for the <see cref="Attribute"/>?
		/// </param>
		public AttributeMatchMethodPointcut(Type attribute, bool inherit)
            : this(attribute, inherit, false)
		{
		}

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Aop.Support.AttributeMatchMethodPointcut"/>
        /// class.
        /// </summary>
        /// <param name="attribute">
        /// The <see cref="System.Attribute"/> to match.
        /// </param>
        /// <param name="inherit">
        /// Flag that controls whether or not the inheritance tree of the
        /// method to be included in the search for the <see cref="Attribute"/>?
        /// </param>
        /// <param name="checkInterfaces">
        /// Flag that controls whether or not interfaces attributes of the
        /// method to be included in the search for the <see cref="Attribute"/>?
        /// </param>
        public AttributeMatchMethodPointcut(Type attribute, bool inherit, bool checkInterfaces)
        {
            Attribute = attribute;
            Inherit = inherit;
            CheckInterfaces = checkInterfaces;
        }

	    /// <inheritdoc />
	    protected AttributeMatchMethodPointcut(SerializationInfo info, StreamingContext context)
	    {
	        Inherit = info.GetBoolean("Inherit");
	        CheckInterfaces = info.GetBoolean("CheckInterfaces");
	        var type = info.GetString("Attribute");
	        Attribute = type != null ? Type.GetType(type) : null;
	    }

	    /// <inheritdoc />
	    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	    {
	        info.AddValue("Attribute", Attribute?.AssemblyQualifiedName);
	        info.AddValue("Inherit", Inherit);
	        info.AddValue("CheckInterfaces", CheckInterfaces);
	    }

		/// <summary>
		/// The <see cref="System.Attribute"/> to match.
		/// </summary>
		/// <exception cref="System.ArgumentException">
		/// If the supplied value is not a <see cref="System.Type"/> that
		/// derives from the <see cref="System.Attribute"/> class.
		/// </exception>
		public virtual Type Attribute
		{
            get { return _attribute; }
			set
			{
				if (value != null)
				{
					if (!typeof (Attribute).IsAssignableFrom(value))
					{
						throw new ArgumentException(
							string.Format(
								"The [{0}] Type must be derived from the [System.Attribute] class.",
								value));
					}
				}
				_attribute = value;
			}
		}

		/// <summary>
		/// Is the inheritance tree of the method to be included in the search for the
		/// <see cref="Attribute"/>?
		/// </summary>
		/// <remarks>
		/// <p>
		/// The default is <see langword="true"/>.
		/// </p>
		/// </remarks>
		public virtual bool Inherit
		{
			get { return _inherit; }
			set { _inherit = value; }
		}

        /// <summary>
        /// Is the interfaces attributes of the method to be included in the search for theg
        /// <see cref="Attribute"/>?
        /// </summary>
        /// <remarks>
        /// <p>
        /// The default is <see langword="false"/>.
        /// </p>
        /// </remarks>
        public virtual bool CheckInterfaces
        {
            get { return _checkInterfaces; }
            set { _checkInterfaces = value; }
        }

		/// <summary>
		/// Does the supplied <paramref name="method"/> satisfy this matcher?
		/// </summary>
		/// <param name="method">The candidate method.</param>
		/// <param name="targetType">
		/// The target <see cref="System.Type"/> (may be <see langword="null"/>,
		/// in which case the candidate <see cref="System.Type"/> must be taken
		/// to be the <paramref name="method"/>'s declaring class).
		/// </param>
		/// <returns>
		/// <see langword="true"/> if this this method matches statically.
		/// </returns>
		public override bool Matches(MethodInfo method, Type targetType)
		{
            if (method.IsDefined(Attribute, Inherit))
            {
                // Checks whether the attribute is defined on the method or a super definition of the method
                // but does not check attributes on implemented interfaces.
                return true;
            }
            else
            {
                if (CheckInterfaces)
                {
                    // Also check whether the attribute is defined on a method implemented from an interface.
                    // First find all interfaces for the type that contains the method.
                    // Next, check each interface for the presence of the attribute on the corresponding
                    // method from the interface.
                    Type[] parameterTypes = ReflectionUtils.GetParameterTypes(method);
                    foreach (Type interfaceType in method.DeclaringType.GetInterfaces())
                    {
                        // The method may be implemented explicitly, so the method name
                        // will include the interface name also
                        string methodName = method.Name;
                        if (methodName.IndexOf('.') != -1)
                        {
                            if (methodName.StartsWith(interfaceType.FullName.Replace('+', '.')))
                            {
                                methodName = methodName.Remove(0, interfaceType.FullName.Length + 1);
                            }
                        }

                        MethodInfo intfMethod = interfaceType.GetMethod(methodName, parameterTypes);
                        if (intfMethod != null && intfMethod.IsDefined(Attribute, Inherit))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
		}
	}
}
