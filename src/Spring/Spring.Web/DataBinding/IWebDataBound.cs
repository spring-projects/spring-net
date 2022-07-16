using Spring.Context;
using Spring.Globalization;

namespace Spring.DataBinding
{
	/// <summary>
	/// A page or control must implement this interface to support spring's databinding infrastructure.
	/// </summary>
	/// <remarks>
	/// <seealso cref="IDataBound"/>
	/// <seealso cref="IBindingContainer"/>
	/// <seealso cref="Spring.Web.UI.Controls.DataBindingPanel"/>
	/// </remarks>
	public interface IWebDataBound : IDataBound
	{
		/// <summary>
		/// Return the UniqueID of this page or control.
		/// </summary>
		string UniqueID { get; }
		/// <summary>
		/// Return the <see cref="IApplicationContext"/> where <see cref="IFormatter"/>
		/// instances should be optained from.
		/// </summary>
		IApplicationContext ApplicationContext { get; }
		/// <summary>
		/// This event is raised to initialize bindings for <see cref="IDataBound.BindingManager" />.
		/// </summary>
		event EventHandler DataBindingsInitialized;
	}
}
