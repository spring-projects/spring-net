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

using System.Collections;
using System.Reflection;

using Common.Logging;
using Spring.Util;
using Spring.Core.TypeResolution;
using Spring.Core;

namespace Spring.Transaction.Interceptor
{
	/// <summary>
	/// Simple implementation of the <see cref="Spring.Transaction.Interceptor.ITransactionAttributeSource"/>
	/// interface that allows attributes to be stored per method in a map.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Griffin Caprio (.NET)</author>
	public class MethodMapTransactionAttributeSource : ITransactionAttributeSource
	{
		private IDictionary _methodMap;
		private IDictionary _nameMap;

	    #region Logging Definition

	    private static readonly ILog LOG = LogManager.GetLogger(typeof (MethodMapTransactionAttributeSource));

	    #endregion

	    /// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.Interceptor.MethodMapTransactionAttributeSource"/> class.
		/// </summary>
		public MethodMapTransactionAttributeSource()
		{
			_methodMap = new Hashtable();
			_nameMap = new Hashtable();
		}

		/// <summary>
		/// Set a name/attribute map, consisting of "FQCN.method, AssemblyName" method names
		/// (e.g. "MyNameSpace.MyClass.MyMethod, MyAssembly") and ITransactionAttribute
		/// instances (or Strings to be converted to <see cref="ITransactionAttribute"/> instances).
		/// </summary>
		/// <remarks>
		/// <p>
		/// The key values of the supplied <see cref="System.Collections.IDictionary"/>
		/// must adhere to the following form:<br/>
		/// <code>FQCN.methodName, Assembly</code>.
		/// </p>
		/// <example>
        /// <code>MyNameSpace.MyClass.MyMethod, MyAssembly</code>
		///	</example>
		/// </remarks>
		public IDictionary MethodMap
		{
			set
			{
			    foreach (DictionaryEntry entry in value)
			    {

			        string name = entry.Key as string;
                    ITransactionAttribute attribute = null;
                    //Check if we need to convert from a string to ITransactionAttribute
                    if (entry.Value is ITransactionAttribute)
                    {
                        attribute = entry.Value as ITransactionAttribute;
                    }
                    else
                    {
                        TransactionAttributeEditor editor = new TransactionAttributeEditor();
                        editor.SetAsText(entry.Value.ToString());
                        attribute = editor.Value;
                    }
                    AddTransactionalMethod( name, attribute );

			    }

			}
		}
		/// <summary>
		/// Add an attribute for a transactional method.
		/// </summary>
		/// <remarks>
		/// Method names can end or start with "*" for matching multiple methods.
		/// </remarks>
		/// <param name="name">The class and method name, separated by a dot.</param>
		/// <param name="attribute">The attribute to be associated with the method.</param>
		public void AddTransactionalMethod( string name, ITransactionAttribute attribute )
		{
            AssertUtils.ArgumentNotNull(name, "name");
            int lastCommaIndex = name.LastIndexOf( "," );
            if (lastCommaIndex == -1)
            {
                throw new ArgumentException("'" + name + "'" +
                                            " is not a valid method name, missing AssemblyName.  Format is FQN.MethodName, AssemblyName.");
            }
            string fqnWithMethod = name.Substring(0, lastCommaIndex);

			int lastDotIndex = fqnWithMethod.LastIndexOf( "." );
			if ( lastDotIndex == -1 )
			{
				throw new TransactionUsageException("'" + fqnWithMethod + "' is not a valid method name: format is FQN.methodName");
			}
			string className = fqnWithMethod.Substring(0, lastDotIndex );
            string assemblyName = name.Substring(lastCommaIndex + 1);
			string methodName = fqnWithMethod.Substring(lastDotIndex + 1 );
            string fqnClassName = className + ", " + assemblyName;
			try
			{
				Type type = TypeResolutionUtils.ResolveType(fqnClassName);
				AddTransactionalMethod( type, methodName, attribute );
			} catch ( TypeLoadException exception )
			{
				throw new TransactionUsageException( "Type '" + fqnClassName + "' not found.", exception );
			}
		}

		/// <summary>
		/// Add an attribute for a transactional method.
		/// </summary>
		/// <remarks>
		/// Method names can end or start with "*" for matching multiple methods.
		/// </remarks>
		/// <param name="type">The target interface or class.</param>
		/// <param name="mappedName">The mapped method name.</param>
		/// <param name="transactionAttribute">
		/// The attribute to be associated with the method.
		/// </param>
		public void AddTransactionalMethod(
			Type type, string mappedName, ITransactionAttribute transactionAttribute )
		{
            // TODO address method overloading? At present this will
            // simply match all methods that have the given name.
			string name = type.Name + "." + mappedName;
			MethodInfo[] methods = type.GetMethods();
			IList matchingMethods = new ArrayList();
			for ( int i = 0; i < methods.Length; i++ )
			{
				if ( methods[i].Name.Equals( mappedName ) || IsMatch(methods[i].Name, mappedName ))
				{
					matchingMethods.Add( methods[i] );
				}
			}
			if ( matchingMethods.Count == 0 )
			{
				throw new TransactionUsageException("Couldn't find method '" + mappedName + "' on Type [" + type.Name + "]");
			}
            // register all matching methods
			foreach ( MethodInfo currentMethod in matchingMethods )
			{
				string regularMethodName = (string)_nameMap[currentMethod];
				if ( regularMethodName == null || ( !regularMethodName.Equals(name) && regularMethodName.Length <= name.Length))
				{
                    // No already registered method name, or more specific
                    // method name specification now -> (re-)register method.
                    if (LOG.IsDebugEnabled && regularMethodName != null)
                    {
                        LOG.Debug("Replacing attribute for transactional method [" + currentMethod + "]: current name '" +
                            name + "' is more specific than '" + regularMethodName + "'");
                    }
					_nameMap.Add( currentMethod, name );
					AddTransactionalMethod( currentMethod, transactionAttribute );
				}
                else
				{
                    if (LOG.IsDebugEnabled && regularMethodName != null)
                    {
                        LOG.Debug("Keeping attribute for transactional method [" + currentMethod + "]: current name '" +
                            name + "' is not more specific than '" + regularMethodName + "'");
                    }
				}
			}
		}

		/// <summary>
		/// Add an attribute for a transactional method.
		/// </summary>
		/// <param name="method">The transactional method.</param>
		/// <param name="transactionAttribute">
		/// The attribute to be associated with the method.
		/// </param>
		public void AddTransactionalMethod( MethodInfo method, ITransactionAttribute transactionAttribute )
		{
			_methodMap.Add( method, transactionAttribute );
		}

		/// <summary>
		/// Does the supplied <paramref name="methodName"/> match the supplied <paramref name="mappedName"/>?
		/// </summary>
		/// <remarks>
		/// The default implementation checks for "xxx*", "*xxx" and "*xxx*" matches,
	    /// as well as direct equality.  This behaviour can (of course) be overridden in
	    /// derived classes.
		/// </remarks>
		/// <param name="methodName">The method name of the class.</param>
		/// <param name="mappedName">The name in the descriptor.</param>
		/// <returns><b>True</b> if the names match.</returns>
		protected virtual bool IsMatch( string methodName, string mappedName )
		{
            return PatternMatchUtils.SimpleMatch(mappedName, methodName);
		}

		#region ITransactionAttributeSource Members
		/// <summary>
		/// Return the <see cref="Spring.Transaction.Interceptor.ITransactionAttribute"/> for this
		/// method.
		/// </summary>
		/// <param name="method">The method to check.</param>
		/// <param name="targetType">
		/// The target <see cref="System.Type"/>. May be null, in which case the declaring
		/// class of the supplied <paramref name="method"/> must be used.
		/// </param>
		/// <returns>
		/// A <see cref="Spring.Transaction.Interceptor.ITransactionAttribute"/> or
		/// null if the method is non-transactional.
		/// </returns>
		public ITransactionAttribute ReturnTransactionAttribute(MethodInfo method, Type targetType)
		{
            //Might have registered MethodInfo objects whose declaring type is the interface, so 'downcast'
            //to the most specific method which is typically what is passed in as the first method argument.
            foreach (DictionaryEntry dictionaryEntry in _methodMap)
            {
                MethodInfo currentMethod = (MethodInfo)dictionaryEntry.Key;

                MethodInfo specificMethod;
                if (targetType == null)
                {
                    specificMethod = currentMethod;
                }
                else
                {
                    ParameterInfo[] parameters = currentMethod.GetParameters();

                    ComposedCriteria searchCriteria = new ComposedCriteria();
                    searchCriteria.Add(new MethodNameMatchCriteria(currentMethod.Name));
                    searchCriteria.Add(new MethodParametersCountCriteria(parameters.Length));
                    searchCriteria.Add(new MethodGenericArgumentsCountCriteria(currentMethod.GetGenericArguments().Length));
                    searchCriteria.Add(new MethodParametersCriteria(ReflectionUtils.GetParameterTypes(parameters)));

                    MemberInfo[] matchingMethods = targetType.FindMembers(
                        MemberTypes.Method,
                        BindingFlags.Instance | BindingFlags.Public,
                        new MemberFilter(new CriteriaMemberFilter().FilterMemberByCriteria),
                        searchCriteria);

                    if (matchingMethods != null && matchingMethods.Length == 1)
                    {
                        specificMethod = matchingMethods[0] as MethodInfo;
                    }
                    else
                    {
                        specificMethod = currentMethod;
                    }
                }

                if (method == specificMethod)
                {
                    return (ITransactionAttribute)dictionaryEntry.Value;
                }
            }
            return (ITransactionAttribute)_methodMap[method];
		}
		#endregion


	}
}
