

using System.Data;

namespace Spring.Data
{
	/// <summary>
	/// Callback to process each row of data in an AdoOperation's Query method.
	/// </summary>
	/// <remarks>
	/// Implementations of this interface perform the actual work of extracting
	/// results. In contrast to a IResultSetExtractor, a IRowCallback object is typically
	/// stateful.  It keeps the result state within the object, to be available
	/// for later inspection.
	/// </remarks>	
	/// 
	public interface IRowCallback
	{
        /// <summary>
        /// Implementations must implement this method to process each row of data
        /// in the data reader.
        /// </summary>
        /// <remarks>
        /// This method should not advance the cursor by calling Read()
        /// on IDataReader but only extract the current values.  The
        /// caller does not need to care about closing the reader, command, connection, or
        /// about handling transactions:  this will all be handled by 
        /// Spring's AdoTemplate
        /// </remarks>
        /// <param name="reader">An active IDataReader instance</param>
        /// <returns>The result object</returns>
        void ProcessRow(IDataReader reader);	  

	}
}
