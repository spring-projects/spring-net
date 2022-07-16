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

using Spring.Util;

namespace Spring.Transaction.Interceptor
{
	/// <summary>
	/// Rule determining whether or not a given exception (and any subclasses) should
	/// cause a rollback.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Multiple such rules can be applied to determine whether a transaction should commit
	/// or rollback after an exception has been thrown.
	/// </p>
	/// </remarks>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
	public class RollbackRuleAttribute : Attribute
	{
        /// <summary>
        /// Could hold exception, resolving class name but would always require FQN.
        /// This way does multiple string comparisons, but how often do we decide
        /// whether to roll back a transaction following an exception?
        /// </summary>
		private string _exceptionName;

		/// <summary>
		/// Canonical instance representing default behavior for rolling back on
		/// all <see cref="System.Exception"/>s.
		/// </summary>
		public static RollbackRuleAttribute RollbackOnSystemExceptions
			= new RollbackRuleAttribute(typeof (Exception).Name);

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.Interceptor.RollbackRuleAttribute"/> class
		/// for the named <paramref name="exceptionName"/>.
		/// </summary>
		/// <param name="exceptionName">The exception name.</param>
		/// <remarks>
		/// <p>
		/// As always, the <paramref name="exceptionName"/> should be the full
		/// assembly qualified version.
		/// </p>
		/// </remarks>
		public RollbackRuleAttribute( string exceptionName )
		{
            AssertUtils.ArgumentHasText(exceptionName, "exceptionName");
			_exceptionName = exceptionName;
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.Interceptor.RollbackRuleAttribute"/> class
		/// for the suplied <paramref name="exceptionType"/>.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The exception class must be <see cref="System.Exception"/> or a subclass.
		/// </p>
		/// <p>
		/// This is the preferred way to construct a
		/// <see cref="Spring.Transaction.Interceptor.RollbackRuleAttribute"/>,
		/// matching the exception class and subclasses.
		/// </p>
		/// </remarks>
		/// <param name="exceptionType">
		/// The <see cref="System.Exception"/> class that will trigger a rollback.
		/// </param>
		public RollbackRuleAttribute( Type exceptionType )
		{
		    AssertUtils.ArgumentNotNull(exceptionType, "exceptionType");
			if ( ! typeof(Exception).IsAssignableFrom( exceptionType ) )
			{
				throw new ArgumentException("Cannot construct rollback rule from " + exceptionType + "; " + "It's not an Exception");
			}
			_exceptionName = exceptionType.Name;
		}

		/// <summary>
		/// Returns the name of the exception.
		/// </summary>
		public string ExceptionName
		{
			get { return _exceptionName; }
		}

		/// <summary>
		/// Return the depth to the matching superclass execption <see cref="System.Type"/>.
		/// </summary>
		/// <remarks>
		/// A return value of 0 means that the <paramref name="exceptionType"/> matches.
		/// </remarks>
		/// <param name="exceptionType">
		/// The <see cref="System.Type"/> of exception to find.
		/// </param>
		/// <returns>
		/// Return -1 if there's no match. Otherwise, return depth. Lowest depth wins.
		/// </returns>
		public int GetDepth( Type exceptionType )
		{
			return getDepth( exceptionType, 0 );
		}

		/// <summary>
		/// Return the depth to the matching superclass execption <see cref="System.Type"/>.
		/// </summary>
		/// <remarks>
		/// A return value of 0 means that the <paramref name="exception"/>s
		/// <see cref="System.Type"/> matches.
		/// </remarks>
		/// <param name="exception">
		/// The exception object to find.
		/// </param>
		/// <returns>
		/// Return -1 if there's no match. Otherwise, return depth. Lowest depth wins.
		/// </returns>
		public int GetDepth( object exception )
		{
			return GetDepth( exception.GetType() );
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> representation of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> containing the exception covered by this instance.
		/// </returns>
		public override string ToString()
		{
			return "RollbackRule with pattern '" + ExceptionName + "'";
		}

		/// <summary>
		/// Override of <see cref="System.Object.GetHashCode"/>.
		/// </summary>
		/// <returns>The hashcode of the exception name covered by this instance.</returns>
		public override int GetHashCode()
		{
		    return base.GetHashCode();
		}


		/// <summary>
		/// Override of <see cref="System.Object.Equals(object)"/>.
		/// </summary>
		/// <param name="obj">The object to compare.</param>
		/// <returns><b>True</b> if the input object is equal to this instance.</returns>
		public override bool Equals(object obj)
		{
            if (ReferenceEquals(this, obj)) return true;
			RollbackRuleAttribute rollbackRuleAttribute = obj as RollbackRuleAttribute;
			if ( rollbackRuleAttribute == null )
			{
				return false;
			}
			return Equals( rollbackRuleAttribute );
		}

	    /// <summary>
		/// Strongly typed <c>Equals()</c> implementation.
		/// </summary>
		/// <param name="rollbackRuleAttribute">
		/// The <see cref="Spring.Transaction.Interceptor.RollbackRuleAttribute"/> to compare.
		/// </param>
		/// <returns>
		/// <b>True</b> if the input object is equal to the supplied <paramref name="rollbackRuleAttribute"/>.
		/// </returns>
		public bool Equals( RollbackRuleAttribute rollbackRuleAttribute )
		{
		    return base.Equals(rollbackRuleAttribute);
		}

		private int getDepth( Type exceptionType, int depth )
		{
			if ( ( exceptionType.Name.IndexOf( ExceptionName ) != -1 ) || ( exceptionType.FullName.IndexOf( ExceptionName ) != -1 ) )
			{
				return depth;
			}
			if ( exceptionType == typeof(Exception))
			{
				return -1;
			}
			return getDepth( exceptionType.BaseType, depth + 1 );
		}
	}
}
