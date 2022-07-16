#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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
using System.Globalization;
using System.Reflection;

using Spring.Core;
using Spring.Core.TypeResolution;

namespace Spring.Objects.Support
{
	/// <summary>
	/// Helper class allowing one to declaratively specify a method call for later invocation.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Typically not used directly but via its subclasses such as
	/// <see cref="Spring.Objects.Factory.Config.MethodInvokingFactoryObject"/>.
	/// </p>
	/// <p>
	/// Usage: specify either the <see cref="MethodInvoker.TargetType"/> and
	/// <see cref="MethodInvoker.TargetMethod"/> or the
	/// <see cref="MethodInvoker.TargetObject"/> and
	/// <see cref="MethodInvoker.TargetMethod"/> properties respectively, and
	/// (optionally) any arguments to the method. Then call the
	/// <see cref="MethodInvoker.Prepare"/> method to prepare the invoker.
	/// Once prepared, the invoker can be invoked any number of times.
	/// </p>
	/// </remarks>
	/// <example>
	/// <p>
	/// The following example uses the <see cref="MethodInvoker"/> class to invoke the
	/// <c>ToString()</c> method on the <c>Foo</c> class using a mixture of both named and unnamed
	/// arguments.
	/// </p>
	/// <code language="C#">
	/// public class Foo
	/// {
	///     public string ToString(string name, int age, string address)
	///     {
	///			return string.Format("{0}, {1} years old, {2}", name, age, address);
	///     }
	///
	///     public static void Main()
	///     {
	///			Foo foo = new Foo();
	///			MethodInvoker invoker = new MethodInvoker();
	///			invoker.Arguments = new object [] {"Kaneda", "18 Kaosu Gardens, Nakatani Drive, Okinanawa"};
	///			invoker.AddNamedArgument("age", 29);
	///			invoker.Prepare();
	///			// at this point, the arguments that will be passed to the method invocation
	///			// will have been resolved into the following ordered array : {"Kaneda", 29, "18 Kaosu Gardens, Nakatani Drive, Okinanawa"}
	///			string details = (string) invoker.Invoke();
	///			Console.WriteLine (details);
	///			// will print out 'Kaneda, 29 years old, 18 Kaosu Gardens, Nakatani Drive, Okinanawa'
	///     }
	/// }
	/// </code>
	/// </example>
	/// <author>Colin Sampaleanu</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Simon White (.NET)</author>
	public class MethodInvoker
	{
		#region Fields

		/// <summary>
		/// The value returned from the invocation of a method that returns void.
		/// </summary>
		public static readonly Missing Void = Missing.Value;

		private Type _targetType;
		private object _targetObject;
		private string _targetMethod;
		private object[] _arguments;
		private IDictionary _namedArguments;
		private object[] _preparedArguments;

		/// <summary>
		/// The method that will be invoked.
		/// </summary>
		private MethodInfo _methodObject;

		/// <summary>
		/// The <see cref="System.Reflection.BindingFlags"/> used to search for
		/// the method to be invoked.
		/// </summary>
		private const BindingFlags MethodSearchingFlags =
			BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic |
				BindingFlags.IgnoreCase;

		#endregion

		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the <see cref="Spring.Objects.Support.MethodInvoker"/> class.
		/// </summary>
		public MethodInvoker()
		{
			Arguments = new object[] {};
			NamedArguments = new Hashtable();
			PreparedArguments = new object[] {};
		}

		#endregion

		#region Properties

		/// <summary>
		/// The target <see cref="System.Type"/> on which to call the target method.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Only necessary when the target method is <see langword="static"/>;
		/// else, a target object needs to be specified.
		/// </p>
		/// </remarks>
		public Type TargetType
		{
			get { return _targetType; }
			set { this._targetType = value; }
		}

		/// <summary>
		/// The target object on which to call the target method.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Only necessary when the target method is not <see langword="static"/>;
		/// else, a target class is sufficient.
		/// </p>
		/// </remarks>
		public object TargetObject
		{
			get { return _targetObject; }
			set { this._targetObject = value; }
		}

		/// <summary>
		/// The name of the method to be invoked.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Refers to either a <see langword="static"/> method
		/// or a non-<see langword="static"/> method, depending on
		/// whether or not a target object has been set.
		/// </p>
		/// </remarks>
		/// <seealso cref="Spring.Objects.Support.MethodInvoker.TargetObject"/>
		public string TargetMethod
		{
			get { return _targetMethod; }
			set { this._targetMethod = value; }
		}

		/// <summary>
		/// Arguments for the method invocation.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Ordering <b>is</b> significant... the order of the arguments in this
		/// property must match the ordering of the various parameters on the target
		/// method. There does however exist a small possibility for confusion when
		/// the arguments in this property are supplied in addition to one or more named
		/// arguments. In this case, each named argument is slotted into the index position
		/// corresponding to the named argument... once once all named arguments have been
		/// resolved, the arguments in this property are slotted into any remaining (empty)
		/// slots in the method parameter list (see the example in the overview of the
		/// <see cref="Spring.Objects.Support.MethodInvoker"/> class if this is not clear).
		/// </p>
		/// <p>
		/// If this property is not set, or the value passed to the setter invocation
		/// is <see langword="null"/> or a zero-length array, a method with no (un-named) arguments is assumed.
		/// </p>
		/// </remarks>
		/// <seealso cref="Spring.Objects.Support.MethodInvoker.NamedArguments"/>
		public object[] Arguments
		{
			get { return _arguments; }
			set
			{
				if (value != null)
				{
					this._arguments = value;
				}
				else
				{
					this._arguments = new object[] {};
				}
			}
		}

		/// <summary>
		/// The resolved arguments for the method invocation.
		/// </summary>
		/// <remarks>
		/// <note type="caution">
		/// This property is not set until the target method has been resolved via a call to the
		/// <see cref="Spring.Objects.Support.MethodInvoker.Prepare"/> method). It is a combination of the
		/// named and plain vanilla arguments properties, and it is this object array that
		/// will actually be passed to the invocation of the target method.
		/// </note>
		/// <p>
		/// Setting the value of this property to <see langword="null"/> results in basically clearing out any
		/// previously prepared arguments... another call to the <see cref="Spring.Objects.Support.MethodInvoker.Prepare"/>
		/// method will then be required to prepare the arguments again (or the prepared arguments
		/// can be set explicitly if so desired).
		/// </p>
		/// </remarks>
		/// <seealso cref="Spring.Objects.Support.MethodInvoker.Arguments"/>
		/// <seealso cref="Spring.Objects.Support.MethodInvoker.NamedArguments"/>
		protected object[] PreparedArguments
		{
			get { return _preparedArguments; }
			set
			{
				if (value != null)
				{
					this._preparedArguments = value;
				}
				else
				{
					this._preparedArguments = new object[] {};
				}
			}
		}

		/// <summary>
		/// Named arguments for the method invocation.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The keys of this dictionary are the (<see cref="System.String"/>) names of the
		/// method arguments, and the (<see cref="System.Object"/>) values are the actual
		/// argument values themselves.
		/// </p>
		/// <p>
		/// If this property is not set, or the value passed to the setter invocation
		/// is a <see langword="null"/> reference, a method with no named arguments is assumed.
		/// </p>
		/// </remarks>
		/// <seealso cref="Spring.Objects.Support.MethodInvoker.Arguments"/>
		public IDictionary NamedArguments
		{
			get { return _namedArguments; }
			set
			{
				if (value != null)
				{
					this._namedArguments = value;
				}
				else
				{
					this._namedArguments.Clear();
				}
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Prepare the specified method.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The method can be invoked any number of times afterwards.
		/// </p>
		/// </remarks>
		/// <exception cref="System.ArgumentException">
		/// If all required properties are not set, or a matching argument could not be found
		/// for a named argument (typically down to a typo).
		/// </exception>
		/// <exception cref="System.MissingMethodException">
		/// If the specified method could not be found.
		/// </exception>
		public virtual void Prepare()
		{
			if (_targetMethod == null)
			{
				throw new ArgumentException("The 'TargetMethod' property is required.");
			}
			if (_targetType == null && _targetObject == null)
			{
				throw new ArgumentException("One of either the 'TargetType' or 'TargetObject' properties is required.");
			}
			_methodObject = FindTheMethodToInvoke();
			if (TargetObject == null && !_methodObject.IsStatic)
			{
				throw new ArgumentException(
					"The target method cannot be an instance method without a corresponding target instance on which to invoke it.");
			}
			PrepareArguments();
		}

		private void PrepareArguments()
		{
			_preparedArguments = new object[ArgumentCount];
			// ok, lets prepare any named arguments first...
			if (NamedArguments.Count > 0)
			{
				// lets slot in all of the named arguments first...
				ParameterInfo[] parameters = _methodObject.GetParameters();
				// lets figure out the index og each of the method parameters...
				IDictionary<string, int> argumentNamesToIndexes = new Dictionary<string, int>();
				for (int i = 0; i < parameters.Length; ++i)
				{
					ParameterInfo parameter = parameters[i];
					argumentNamesToIndexes[parameter.Name.ToLower(CultureInfo.InvariantCulture)] = i;
				}
				int THE_ARGUMENT_IS_PREPARED = -12;
				foreach (DictionaryEntry namedArgument in NamedArguments)
				{
					string argumentName = ((string) namedArgument.Key).ToLower(CultureInfo.InvariantCulture);
					object argumentValue = namedArgument.Value;
					if (!argumentNamesToIndexes.ContainsKey(argumentName))
					{
						// whoa (Nelly); the named argument does not exist on the method...
						throw new ArgumentException(string.Format(
							CultureInfo.InvariantCulture,
							"The named argument '{0}' could not be found on the '{1}' method of class [{2}].",
							argumentName, _methodObject.Name, _methodObject.DeclaringType.FullName));

					}
					// look up the index of where in the prepared args array we're gonna stick the named argument value
					int namedArgumentsIndex = argumentNamesToIndexes[argumentName];
					PreparedArguments[namedArgumentsIndex] = argumentValue;
					// we've prepped this index position, so mark it as so...
					argumentNamesToIndexes[argumentName] = THE_ARGUMENT_IS_PREPARED;
				}
				// and then fill in any remaining blanks with the plain vanilla arguments...
				int plainVanillaIndex = 0;
				int[] sortedIndexes = new List<int>(argumentNamesToIndexes.Values).ToArray();
				Array.Sort(sortedIndexes);
				foreach (int argumentIndex in sortedIndexes)
				{
					// have we previously prepped a named argument at this index position?
					if (argumentIndex == THE_ARGUMENT_IS_PREPARED)
					{
						continue;
					}
					// lets stick a plain vanilla argument in at this index position (in the order that they have been supplied)...
					PreparedArguments[argumentIndex] = Arguments[plainVanillaIndex++];
				}
			}
			else
			{
				PreparedArguments = Arguments;
			}
		}

		/// <summary>
		/// Searches for and returns the method that is to be invoked.
		/// </summary>
		/// <remarks>
		/// The return value of this method call will subsequently be returned from the
		/// <see cref="Spring.Objects.Support.MethodInvoker.GetPreparedMethod()"/>.
		/// </remarks>
		/// <returns>The method that is to be invoked.</returns>
		/// <exception cref="System.MissingMethodException">
		/// If no method could be found.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// If more than one method was found.
		/// </exception>
        protected virtual MethodInfo FindTheMethodToInvoke()
        {
            MethodInfo theMethod = null;
            Type targetType = (TargetObject != null) ? TargetObject.GetType() : TargetType;
            GenericArgumentsHolder genericInfo = new GenericArgumentsHolder(TargetMethod);

            // if we don't have any named arguments, we can try to get the exact method first...
			if (NamedArguments.Count == 0)
			{
                ComposedCriteria searchCriteria = new ComposedCriteria();
                searchCriteria.Add(new MethodNameMatchCriteria(genericInfo.GenericMethodName));
                searchCriteria.Add(new MethodParametersCountCriteria(ArgumentCount));
                searchCriteria.Add(new MethodGenericArgumentsCountCriteria(genericInfo.GetGenericArguments().Length));
                searchCriteria.Add(new MethodArgumentsCriteria(Arguments));

                MemberInfo[] matchingMethods = targetType.FindMembers(
                    MemberTypes.Method,
                    MethodSearchingFlags,
                    new MemberFilter(new CriteriaMemberFilter().FilterMemberByCriteria),
                    searchCriteria);

                if (matchingMethods != null && matchingMethods.Length == 1)
                {
                    theMethod = matchingMethods[0] as MethodInfo;
                }
			}
            if (theMethod == null)
            {
                // search for a method with a matching signature...
                ComposedCriteria searchCriteria = new ComposedCriteria();
                searchCriteria.Add(new MethodNameMatchCriteria(genericInfo.GenericMethodName));
                searchCriteria.Add(new MethodParametersCountCriteria(ArgumentCount));
                searchCriteria.Add(new MethodGenericArgumentsCountCriteria(genericInfo.GetGenericArguments().Length));

                MemberInfo[] matchingMethods = targetType.FindMembers(
                    MemberTypes.Method,
                    MethodSearchingFlags,
                    new MemberFilter(new CriteriaMemberFilter().FilterMemberByCriteria),
                    searchCriteria);

                if (matchingMethods.Length == 0)
                {
                    throw new MissingMethodException(targetType.Name, TargetMethod);
                }
                if (matchingMethods.Length > 1)
                {
                    throw new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        "Unable to determine which exact method to call; found '{0}' matches.",
                        matchingMethods.Length));
                }
                theMethod = matchingMethods[0] as MethodInfo;
            }

            if (genericInfo.ContainsGenericArguments)
            {
                string[] unresolvedGenericArgs = genericInfo.GetGenericArguments();
                Type[] genericArgs = new Type[unresolvedGenericArgs.Length];
                for (int j = 0; j < unresolvedGenericArgs.Length; j++)
                {
                    genericArgs[j] = TypeResolutionUtils.ResolveType(unresolvedGenericArgs[j]);
                }
                theMethod = theMethod.MakeGenericMethod(genericArgs);
            }

            return theMethod;
        }

		/// <summary>
		/// Adds the named argument to this instances mapping of argument names to argument values.
		/// </summary>
		/// <param name="argumentName">
		/// The name of an argument on the method that is to be invoked.
		/// </param>
		/// <param name="argument">
		/// The value of the named argument on the method that is to be invoked.
		/// </param>
		public void AddNamedArgument(string argumentName, object argument)
		{
			if (NamedArguments.Contains(argumentName))
			{
				NamedArguments.Remove(argumentName);
			}
			NamedArguments.Add(argumentName, argument);
		}

		private int ArgumentCount
		{
			get { return Arguments.Length + NamedArguments.Count; }
		}

		/// <summary>
		/// Returns the prepared <see cref="System.Reflection.MethodInfo"/> object that
		/// will be invoked.
		/// </summary>
		/// <remarks>
		/// <p>
		/// A possible use case is to determine the return <see cref="System.Type"/> of the method.
		/// </p>
		/// </remarks>
		/// <returns>
		/// The prepared <see cref="System.Reflection.MethodInfo"/> object that
		/// will be invoked.
		/// </returns>
		public MethodInfo GetPreparedMethod()
		{
			return this._methodObject;
		}

		/// <summary>
		/// Invoke the specified method.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The invoker needs to have been prepared beforehand (via a call to the
		/// <see cref="Spring.Objects.Support.MethodInvoker.Prepare"/> method).
		/// </p>
		/// </remarks>
		/// <returns>
		/// The object returned by the method invocation, or
		/// <see cref="Spring.Objects.Support.MethodInvoker.Void"/> if the method returns void.
		/// </returns>
		/// <exception cref="MethodInvocationException">
		/// If at least one of the arguments passed to this <see cref="Spring.Objects.Support.MethodInvoker"/>
		/// was incompatible with the signature of the invoked method.
		/// </exception>
		public virtual object Invoke()
		{
			object result = null;
			try
			{
				result = this._methodObject.Invoke(TargetObject, PreparedArguments);
			}
			catch (ArgumentException ex)
			{
				throw new MethodInvocationException(
					string.Format(CultureInfo.InvariantCulture,
					              "At least one of the arguments passed to this {0} was " +
					              	"incompatible with the signature of the invoked method.", GetType().Name), ex);
			}
			return (result == null ? Void : result);
		}

		#endregion
	}
}
