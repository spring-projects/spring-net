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

using Common.Logging;
using Spring.Util;

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// <see cref="Spring.Objects.Factory.IFactoryObject"/> implementation that
	/// creates instances of the <see cref="Common.Logging.ILog"/> class.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Typically used for retrieving shared <see cref="Common.Logging.ILog"/>
	/// instances for common topics (such as the 'DAL', 'BLL', etc). The
	/// <see cref="LogFactoryObject.LogName"/>
	/// property determines the name of the
    /// <a href="http://netcommon.sourceforge.net/">Common.Logging</a> logger.
	/// </p>
	/// </remarks>
	/// <author>Rick Evans</author>
	/// <seealso cref="Common.Logging.LogManager.GetLogger(string)"/>
    [Serializable]
    public class LogFactoryObject : IFactoryObject, IInitializingObject
	{
		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="LogFactoryObject"/>
		/// class.
		/// </summary>
		public LogFactoryObject()
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="LogFactoryObject"/>
		/// class.
		/// </summary>
		/// <param name="logName">
		/// The name of the <see cref="Common.Logging.ILog"/> instance served up by
		/// this factory.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="logName"/> is
		/// <see langword="null"/> or contains only whitespace character(s).
		/// </exception>
		public LogFactoryObject(string logName)
		{
			LogName = logName;
		}

		#endregion

		/// <summary>
		/// The name of the <see cref="Common.Logging.ILog"/> instance served up by
		/// this factory.
		/// </summary>
		/// <value>
		/// The name of the <see cref="Common.Logging.ILog"/> instance served up by
		/// this factory.
		/// </value>
		/// <exception cref="System.ArgumentNullException">
		/// If the <see langword="value"/> supplied to the setter is
		/// <see langword="null"/> or contains only whitespace character(s).
		/// </exception>
		public string LogName
		{
			get { return this.logName; }
			set
			{
				AssertUtils.ArgumentHasText(value, "The 'LogName' property must have a value.");
				this.logName = value.Trim();
			}
		}

		/// <summary>
		/// Return an instance (possibly shared or independent) of the object
		/// managed by this factory.
		/// </summary>
		/// <returns>
		/// An instance (possibly shared or independent) of the object
		/// managed by this factory.
		/// </returns>
		/// <seealso cref="Spring.Objects.Factory.IFactoryObject.GetObject"/>
		public object GetObject()
		{
			if (this.log == null)
			{
				ValidateProperties();
				this.log = LogManager.GetLogger(LogName);
			}
			return this.log;
		}

		/// <summary>
		/// Return the type of object that this
		/// <see cref="Spring.Objects.Factory.IFactoryObject"/> creates, or
		/// <cref lang="null"/> if not known in advance.
		/// </summary>
		/// <seealso cref="Spring.Objects.Factory.IFactoryObject.ObjectType"/>
		public Type ObjectType
		{
			get { return typeof(ILog); }
		}

		/// <summary>
		/// Is the object managed by this factory a singleton or a prototype?
		/// </summary>
		/// <seealso cref="Spring.Objects.Factory.IFactoryObject.IsSingleton"/>
		public bool IsSingleton
		{
			get { return true; }
		}

		/// <summary>
		/// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// after it has set all object properties supplied
		/// (and satisfied the
		/// <see cref="Spring.Objects.Factory.IObjectFactoryAware"/>
		/// and <see cref="Spring.Context.IApplicationContextAware"/>
		/// interfaces).
		/// </summary>
		/// <exception cref="System.Exception">
		/// In the event of misconfiguration (such as failure to set an essential
		/// property) or if initialization fails.
		/// </exception>
		/// <seealso cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
		public void AfterPropertiesSet()
		{
			ValidateProperties();
		}

		private void ValidateProperties()
		{
			if (StringUtils.IsNullOrEmpty(LogName))
			{
				throw new ArgumentException("The 'LogName' property has not been set.");
			}
		}

		private ILog log;
		private string logName;
	}
}
