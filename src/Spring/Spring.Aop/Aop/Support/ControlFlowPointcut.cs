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

using System.Reflection;
using System.Runtime.Serialization;

using Spring.Core;

namespace Spring.Aop.Support
{
	/// <summary>
	/// Pointcut and method matcher for use in simple <b>cflow</b>-style
	/// pointcuts.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Evaluating such pointcuts is slower than evaluating normal pointcuts,
	/// but can nevertheless be useful in some cases. Of course, your mileage
	/// may vary as to what 'slower' actually means.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Simon White (.NET)</author>
	[Serializable]
	public class ControlFlowPointcut : IPointcut, ITypeFilter, IMethodMatcher, ISerializable
	{
	    private Type _type;
		private string _methodName;
		private int _evaluationCount;

	    /// <summary>
		/// Creates a new instance of the <see cref="ControlFlowPointcut"/>
		/// class.
		/// </summary>
		/// <param name="type">
		/// The class under which <b>all</b> control flows are to be matched.
		/// </param>
		public ControlFlowPointcut(Type type) : this(type, null)
		{
		}

		/// <summary>
		/// Construct a new pointcut that matches all calls below the
		/// given method in the given class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// If the supplied <paramref name="methodName"/> is
		/// <see langword="null"/>, <b>all</b> control flows below the given
		/// class will be successfully matched.
		/// </p>
		/// </remarks>
		/// <param name="type">
		/// The class under which <b>all</b> control flows are to be matched.
		/// </param>
		/// <param name="methodName">
		/// The method name under which <b>all</b> control flows are to be matched.
		/// </param>
		public ControlFlowPointcut(Type type, string methodName)
		{
			_type = type;
			_methodName = methodName;
		}

	    /// <inheritdoc />
	    private ControlFlowPointcut(SerializationInfo info, StreamingContext context)
	    {
	        var type = info.GetString("Type");
	        _type = type != null ? Type.GetType(type) : null;
	        _methodName = info.GetString("MethodName");
	        _evaluationCount = info.GetInt32("EvaluationCount");
	    }

	    /// <inheritdoc />
	    public void GetObjectData(SerializationInfo info, StreamingContext context)
	    {
	        info.AddValue("Type", _type?.AssemblyQualifiedName);
	        info.AddValue("MethodName", _methodName);
	        info.AddValue("EvaluationCount", _evaluationCount);
	    }

	    /// <summary>
		/// The <see cref="Spring.Aop.ITypeFilter"/> for this pointcut.
		/// </summary>
		/// <value>
		/// The current <see cref="Spring.Aop.ITypeFilter"/>.
		/// </value>
		public ITypeFilter TypeFilter
		{
			get { return this; }
		}

		/// <summary>
		/// Gets the number of times this pointcut has been <i>evaluated</i>.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Useful as a debugging aid.
		/// </p>
		/// <p>
		/// Note that this value is distinct from the number of times that this
		/// pointcut sucessfully matches a target method, in that a
		/// <see cref="ControlFlowPointcut"/> may be evaluated many times but
		/// never actually match even once.
		/// </p>
		/// </remarks>
		/// <value>
		/// The number of times this pointcut has been <i>evaluated</i>.
		/// </value>
		public int EvaluationCount
		{
			get { return _evaluationCount; }
		}

		/// <summary>
		/// The <see cref="Spring.Aop.IMethodMatcher"/> for this pointcut.
		/// </summary>
		/// <value>
		/// The current <see cref="Spring.Aop.IMethodMatcher"/>.
		/// </value>
		public IMethodMatcher MethodMatcher
		{
			get { return this; }
		}

		/// <summary>
		/// Is this a runtime pointcut?
		/// </summary>
		/// <remarks>
		/// <p>
		/// This implementation is a runtime pointcut, and so always returns
		/// <see langword="true"/>.
		/// </p>
		/// </remarks>
		/// <value>
		/// <see langword="true"/> if this is a runtime pointcut.
		/// </value>
		/// <seealso cref="Spring.Aop.IMethodMatcher.IsRuntime"/>
		public bool IsRuntime
		{
			get { return true; }
		}

	    /// <summary>
		/// Should the pointcut apply to the supplied <see cref="System.Type"/>?
		/// </summary>
		/// <remarks>
		/// <p>
		/// Subclasses are encouraged to override this method for greater
		/// filtering (and performance).
		/// </p>
		/// <p>
		/// This, the default, implementation always matches (returns
		/// <see langword="true"/>).
		/// </p>
		/// </remarks>
		/// <param name="type">The candidate target class.</param>
		/// <returns>
		/// <see langword="true"/> if the advice should apply to the supplied
		/// <paramref name="type"/>
		/// </returns>
		public virtual bool Matches(Type type)
		{
			return true;
		}

		/// <summary>
		/// Does the supplied <paramref name="method"/> satisfy this matcher?
		/// Perform static checking. If this returns false, or if the isRuntime() method
		/// returns false, no runtime check will be made.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Subclasses are encouraged to override this method if it is possible
		/// to filter out some candidate classes.
		/// </p>
		/// <p>
		/// This, the default, implementation always matches (returns
		/// <see langword="true"/>). This means that the three argument
		/// <see cref="Spring.Aop.IMethodMatcher.Matches(MethodInfo, Type, object[])"/>
		/// method will always be invoked.
		/// </p>
		/// </remarks>
		/// <param name="method">The candidate method.</param>
		/// <param name="targetType">
		/// The target class (may be <see langword="null"/>, in which case the
		/// candidate class must be taken to be the <paramref name="method"/>'s
		/// declaring class).
		/// </param>
		/// <returns>
		/// <see langword="true"/> if this this method matches statically.
		/// </returns>
		/// <seealso cref="Spring.Aop.IMethodMatcher.Matches(MethodInfo, Type)"/>
		public virtual bool Matches(MethodInfo method, Type targetType)
		{
			return true;
		}

		/// <summary>
		/// Is there a runtime (dynamic) match for the supplied
		/// <paramref name="method"/>?
		/// </summary>
		/// <remarks>
		/// <p>
		/// Subclasses are encouraged to override this method if it is possible
		/// to filter out some candidate classes.
		/// </p>
		/// </remarks>
		/// <param name="method">The candidate method.</param>
		/// <param name="targetType">The target class.</param>
		/// <param name="args">The arguments to the method</param>
		/// <returns>
		/// <see langword="true"/> if there is a runtime match.</returns>
		/// <seealso cref="Spring.Aop.IMethodMatcher.Matches(MethodInfo, Type, object[])"/>
		public virtual bool Matches(MethodInfo method, Type targetType, object[] args)
		{
			++_evaluationCount;
			IControlFlow cflow = ControlFlowFactory.CreateControlFlow();
			return (_methodName != null)
				? cflow.Under(_type, _methodName)
				: cflow.Under(_type);
		}
	}
}
