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

#region Imports

using Spring.Context;

#endregion

namespace Spring.Web.Support
{
	/// <summary>
	/// "Contract"-interface of the Web-DI infrastructure.
	/// </summary>
	/// <remarks>
	/// This interface supports Spring's DI infrastructure and normally doesn't need to be implemented<br/>
	/// <br/>
	/// Any Page, Control or ControlCollection implementing this interface guarantees to 
	/// call <see cref="WebDependencyInjectionUtils.InjectDependenciesRecursive"/> on any control being added 
	/// before it is actually added to the child-collection.
	/// </remarks>
	/// <example>
	/// <p>The following example shows, how to make a Control support the DI-infrastructure:</p>
	/// <code language="c#">
	/// class MyControl : Control, ISupportsWebDependencyInjection
	/// {
	///		private IApplicationContext _defaultApplicationContext;
	/// 
	///		public IApplicationContext DefaultApplicationContext
	///		{
	///			get { return _defaultApplicationContext; }
	///			set { _defaultApplicationContext = value; }
	///		}
	/// 
	///		override protected AddedControl( Control control, int index )
	///		{
	///			WebUtils.InjectDependenciesRecursive( _defaultApplicationContext, control );
	///			base.AddedControl( control, index );
	///		}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// <p>The following example shows, how to make a ControlCollection support the DI-infrastructure:</p>
	/// <p>Note, that you MUST implement the single-argument constructor <c>ControlCollection( Control owner )</c>!</p>
	/// <code language="c#">
	/// class MyControlCollection : ControlCollection, ISupportsWebDependencyInjection
	/// {
	///		private IApplicationContext _defaultApplicationContext;
	/// 
	///		public MyControlCollection( Control owner ) : base( owner )
	///		{}
	/// 
	///		public IApplicationContext DefaultApplicationContext
	///		{
	///			get { return _defaultApplicationContext; }
	///			set { _defaultApplicationContext = value; }
	///		}
	/// 
	///		override public Add( Control child )
	///		{
	///			WebUtils.InjectDependenciesRecursive( _defaultApplicationContext, child );
	///			base.Add( child );
	///		}
	/// 
	///		override public AddAt( int index, Control child )
	///		{
	///			WebUtils.InjectDependenciesRecursive( _defaultApplicationContext, child );
	///			base.AddAt( index, child );
	///		}
	/// }
	/// </code>
	/// </example>
	/// <author>Erich Eichinger</author>
	public interface ISupportsWebDependencyInjection
	{
		/// <summary>
		/// Holds the default <see cref="IApplicationContext"/> instance to be used during injecting a control-tree.
		/// </summary>
		IApplicationContext DefaultApplicationContext { get; set; }
	}
}