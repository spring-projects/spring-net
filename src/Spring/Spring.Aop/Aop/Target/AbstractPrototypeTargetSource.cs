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

using Common.Logging;

using Spring.Objects.Factory;
using Spring.Util;

#endregion

namespace Spring.Aop.Target
{
	/// <summary>
	/// Base class for dynamic <see cref="Spring.Aop.ITargetSource"/>
	/// implementations that can create new prototype object instances to
	/// support a pooling or new-instance-per-invocation strategy.
	/// </summary>
	/// <remarks>
	/// <p>
	/// All such <see cref="Spring.Aop.ITargetSource"/>s must run in an
	/// <see cref="Spring.Objects.Factory.IObjectFactory"/>, as they need to
	/// call the <see cref="Spring.Objects.Factory.IObjectFactory.GetObject(string)"/>
	/// method to create a new prototype instance.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Federico Spinazzi (.NET)</author>
    public abstract class AbstractPrototypeTargetSource
		: ITargetSource, IObjectFactoryAware, IInitializingObject
	{
		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Target.AbstractPrototypeTargetSource"/>
		/// class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is an <see langword="abstract"/> class, and as such exposes no
		/// public constructors.
		/// </p>
		/// </remarks>
		protected AbstractPrototypeTargetSource()
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// The name of the target object to be created on each invocation.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This object should be a prototype, or the same instance will always
		/// be obtained from the owning <see cref="ObjectFactory"/>.
		/// </p>
		/// </remarks>
		public virtual string TargetObjectName
		{
			get { return _targetObjectName; }
			set {
				_targetObjectName = value;
			}
		}

		/// <summary>
		/// The <see cref="System.Type"/> of the target object.
		/// </summary>
		public virtual Type TargetType
		{
			get { return _targetType; }
		}

		/// <summary>
		/// Is the target source static?
		/// </summary>
		/// <value>
		/// <see langword="true"/> if the target source is static.
		/// </value>
		public virtual bool IsStatic
		{
			get { return false; }
		}

		/// <summary>
		/// The target factory that will be used to perform the lookup
		/// of the object referred to by the <see cref="TargetObjectName"/>
		/// property.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Needed so that prototype instances can be created as necessary.
		/// </p>
		/// </remarks>
		/// <value>
		/// The owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// (will never be <see langword="null"/>).
		/// </value>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// In case of initialization errors.
		/// </exception>
		/// <seealso cref="Spring.Objects.Factory.IObjectFactoryAware.ObjectFactory"/>
		public virtual IObjectFactory ObjectFactory
		{
			get { return _owningObjectFactory; }
			set
			{
				_owningObjectFactory = value;
				if (!value.IsPrototype(TargetObjectName))
				{
					throw new ObjectDefinitionStoreException(
						"Cannot use PrototypeTargetSource against a " +
						"Singleton object; instances would not be independent.");
				}

				#region Instrumentation

				if (logger.IsDebugEnabled)
				{
					logger.Debug(string.Format(
						"Getting object with name '{0}' to determine class.",
						TargetObjectName));
				}

				#endregion

                _targetType = _owningObjectFactory.GetType(TargetObjectName);
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Subclasses should use this method to create a new prototype instance.
		/// </summary>
		protected virtual object NewPrototypeInstance()
		{
			#region Instrumentation

			if (logger.IsDebugEnabled)
			{
				logger.Debug(string.Format(
					"Creating new target from object '{0}'.",
					TargetObjectName));
			}

			#endregion

			return ObjectFactory.GetObject(TargetObjectName);
		}

		/// <summary>
		/// Returns the target object.
		/// </summary>
		/// <returns>The target object.</returns>
		/// <exception cref="System.Exception">
		/// If unable to obtain the target object.
		/// </exception>
		public abstract object GetTarget();

		/// <summary>
		/// Releases the target object.
		/// </summary>
		/// <param name="target">The target object to release.</param>
		public virtual void ReleaseTarget(object target)
		{
		}

		/// <summary>
		/// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// after it has set all object properties supplied
		/// (and satisfied the
		/// <see cref="Spring.Objects.Factory.IObjectFactoryAware"/>
		/// and <see cref="Spring.Context.IApplicationContextAware"/>
		/// interfaces).
		/// </summary>
		/// <remarks>
		/// <p>
		/// Ensures that the <see cref="TargetObjectName"/> property has been
		/// set to a valid value (i.e. is not <see langword="null"/> or a string
		/// that consists solely of whitespace).
		/// </p>
		/// </remarks>
		/// <exception cref="System.Exception">
		/// In the event of misconfiguration (such as failure to set an essential
		/// property) or if initialization fails.
		/// </exception>
		/// <seealso cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
		public virtual void AfterPropertiesSet()
		{
			AssertUtils.ArgumentHasText(
				TargetObjectName, "TargetObjectName",
				"The 'TargetObjectName' property must have a value.");
		}

		#endregion

		#region Fields

        /// <summary>
        /// Returns a textual representation of this target source instance.
        /// This implementation returns <see cref="GetDescription"/>
        /// </summary>
        public override string ToString()
        {
            return GetDescription();
        }

        /// <summary>
        /// Returns a textual representation of this target source instance
        /// </summary>
        protected virtual string GetDescription()
        {
            return string.Format("[{0}:{1}]", this.GetType().Name, this.TargetObjectName);
        }

		/// <summary>
		/// The shared <see cref="Common.Logging.ILog"/> instance for this class (and derived classes).
		/// </summary>
		protected readonly ILog logger = LogManager.GetLogger(typeof (AbstractPrototypeTargetSource));

		private String _targetObjectName;
		private IObjectFactory _owningObjectFactory;
		private Type _targetType;

		#endregion
	}
}
