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

using Spring.Objects;

namespace Spring.Context.Support
{
	/// <summary>
	/// Convenient superclass for application objects that want to be aware of
	/// the application context, e.g. for custom lookup of collaborating object
	/// or for context-specific resource access.
	/// </summary>
	/// <remarks>
	/// <p>
	/// It saves the application context reference and provides an
	/// initialization callback method. Furthermore, it offers numerous
	/// convenience methods for message lookup.
	/// </p>
	/// <p>
	/// There is no requirement to subclass this class: it just makes things
	/// a little easier if you need access to the context, e.g. for access to
	/// file resources or to the message source. Note that many application
	/// objects do not need to be aware of the application context at all,
	/// as they can receive collaborating objects via object references.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Griffin Caprio (.NET)</author>
	public abstract class ApplicationObjectSupport : IApplicationContextAware
	{
		private IApplicationContext _applicationContext;
		private MessageSourceAccessor _messageSourceAccessor;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Context.Support.ApplicationObjectSupport"/> class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is an <see langword="abstract"/> class, and as such exposes no
		/// public constructors.
		/// </p>
		/// </remarks>
		protected ApplicationObjectSupport()
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Context.Support.ApplicationObjectSupport"/> class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is an <see langword="abstract"/> class, and as such exposes no
		/// public constructors.
		/// </p>
		/// </remarks>
		/// <param name="applicationContext">
		/// The <see cref="Spring.Context.IApplicationContext"/> that this
		/// object runs in.
		/// </param>
		protected ApplicationObjectSupport(
			IApplicationContext applicationContext)
		{
			_applicationContext = applicationContext;
		}

		/// <summary>
		/// The context class that any context passed to the
		/// <see cref="Spring.Context.Support.ApplicationObjectSupport.ApplicationContext"/>
		/// must be an instance of.
		/// </summary>
		/// <value>
		/// The <see cref="Spring.Context.IApplicationContext"/>
		/// <see cref="System.Type"/>.
		/// </value>
		protected virtual Type RequiredType
		{
			get { return typeof (IApplicationContext); }
		}

		/// <summary>
		/// Intializes the wrapped
		/// <see cref="Spring.Context.IApplicationContext"/>.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is a template method that subclasses can override for custom
		/// initialization behavior.
		/// </p>
		/// <p>
		/// Gets called by the
		/// <see cref="Spring.Context.Support.ApplicationObjectSupport.ApplicationContext"/>
		/// instance directly after setting the context instance.
		/// </p>
		/// <note type="caution">
		/// Does not get called on reinitialization of the context.
		/// </note>
		/// </remarks>
		/// <exception cref="ApplicationContextException">
		/// In the case of any initialization errors.
		/// </exception>
		/// <exception cref="ObjectsException">
		/// If thrown by application context methods.
		/// </exception>
		protected virtual void InitApplicationContext()
		{
		}

		/// <summary>
		/// Return a <see cref="Spring.Context.Support.MessageSourceAccessor"/> for the
		/// application context used by this object, for easy message access.
		/// </summary>
		public MessageSourceAccessor MessageSourceAccessor
		{
			get { return _messageSourceAccessor; }
		}

		#region IApplicationContextAware Members

		/// <summary>
		/// Gets or sets the <see cref="Spring.Context.IApplicationContext"/> that this
		/// object runs in.
		/// </summary>
		/// <exception cref="Spring.Context.ApplicationContextException">
		/// When passed an unexpected
		/// <see cref="Spring.Context.IApplicationContext"/> implementation
		/// instance that is not compatible with the <see cref="System.Type"/>
		/// defined by the value of the
		/// <see cref="Spring.Context.Support.ApplicationObjectSupport.RequiredType"/>.
		/// property. Also, thrown when trying to re-initialize with a
		/// different <see cref="Spring.Context.IApplicationContext"/> than was
		/// originally used.
		/// </exception>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// If thrown by any application context methods.
		/// </exception>
		/// <exception cref="Spring.Objects.Factory.ObjectInitializationException"/>
		/// <seealso cref="Spring.Context.IApplicationContextAware.ApplicationContext"/>
		public IApplicationContext ApplicationContext
		{
			get { return _applicationContext; }
			set
			{
				if (_applicationContext == null)
				{
					if (! isValueOfRequiredType(value))
					{
						throw new ApplicationContextException(
							"Invalid application context: needs to by of type '" + RequiredType.DeclaringType + "'");
					}
					_applicationContext = value;
					_messageSourceAccessor = new MessageSourceAccessor(value);
					InitApplicationContext();
				}
				else
				{
					if (_applicationContext != value)
					{
						throw new ApplicationContextException("Cannot reinitialize with different application context");
					}
				}
			}
		}

		private bool isValueOfRequiredType(IApplicationContext value)
		{
			if (value.GetType() == RequiredType)
			{
				return true;
			}
			Type[] implementedTypes = value.GetType().GetInterfaces();
			for (int i = 0; i < implementedTypes.Length; i++)
			{
				if (implementedTypes[i] == RequiredType)
				{
					return true;
				}
			}
			return false;
		}

		#endregion
	}
}
