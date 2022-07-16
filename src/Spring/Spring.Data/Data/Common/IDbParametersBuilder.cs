namespace Spring.Data.Common
{
	/// <summary>
	/// A builder to create a collection of ADO.NET parameters.
	/// </summary>
	/// <remarks>The chaining of IDbParameter methods does the
	/// building of the parameter details while the builder class
	/// itself keeps track of the collection.</remarks>
	///
	/// <author>Mark Pollack (.NET)</author>
	public interface IDbParametersBuilder
	{
        /// <summary>
        /// Creates a IDbParameter object and adds it to an internal collection for
        /// later retrieval via the GetParameters method.
        /// </summary>
        /// <returns></returns>
        IDbParameter Create();

        /// <summary>
        /// Gets all the parameters created and configured via the Create method.
        /// </summary>
        /// <returns>An instance of IDbParameters</returns>
	    IDbParameters GetParameters();
	}
}
