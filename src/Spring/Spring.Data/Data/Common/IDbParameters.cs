using System.Data;

namespace Spring.Data.Common
{
    public interface IDbParameters
    {
        //TODO implement IDataParameterCollection....?

        //TODO should add methods return IDbDataParameter?

#if !MONO // EE: mono/win32 doesn't accept comments on indexer?!?
        /// <summary>
        /// Gets the underlying standard ADO.NET <see cref="IDbDataParameter"/> for the specified parameter name.
        /// </summary>
#endif
        IDataParameter this[string parameterName] { get; set; }

#if !MONO // EE: mono/win32 doesn't accept comments on indexer?!?
        /// <summary>
        /// Gets the underlying standard ADO.NET <see cref="IDbDataParameter"/> for the specified index.
        /// </summary>
#endif
        IDataParameter this[int index] { get; set; }

        int Count
        {
            get;
        }

        /// <summary>
        /// Determines whether this collection contains the specified parameter name.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns>
        /// 	<c>true</c> if contains the specified parameter name; otherwise, <c>false</c>.
        /// </returns>
        bool Contains(string parameterName);

        /// <summary>
        /// Returns the underlying standard ADO.NET parameters collection.
        /// </summary>
        IDataParameterCollection DataParameterCollection { get; }

        /// <summary>
        /// Add an instance of <see cref="IDbDataParameter"/>.
        /// </summary>
        /// <param name="dbParameter"></param>
        void AddParameter(IDataParameter dbParameter);

        /// <summary>
        /// Add a parameter specifying the all the individual properties of a
        /// IDbDataParameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameterType"></param>
        /// <param name="size"></param>
        /// <param name="direction"></param>
        /// <param name="isNullable"></param>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <param name="sourceColumn"></param>
        /// <param name="sourceVersion"></param>
        /// <param name="parameterValue"></param>
        /// <returns>The newly created parameter</returns>
        IDbDataParameter AddParameter(string name, Enum parameterType, int size, ParameterDirection direction, bool isNullable,
                          byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object parameterValue);

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameterType"></param>
        /// <param name="direction"></param>
        /// <param name="isNullable"></param>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <param name="sourceColumn"></param>
        /// <param name="sourceVersion"></param>
        /// <param name="parameterValue"></param>
        /// <returns>The newly created parameter</returns>
        IDbDataParameter AddParameter(string name, Enum parameterType, ParameterDirection direction, bool isNullable, byte precision,
                          byte scale, string sourceColumn, DataRowVersion sourceVersion, object parameterValue);


        /// <summary>
        /// Adds a value to the parameter collection
        /// </summary>
        /// <remarks>This method can not be used with stored procedures for
        /// providers that require named parameters.  However, it is of
        /// great convenience for other cases.</remarks>
        /// <param name="parameterValue">The value of the parameter.</param>
        /// <returns>Index of added parameter.</returns>
        int Add(object parameterValue);

        /// <summary>
        /// Adds a range of vlaues to the parameter collection.
        /// </summary>
        /// <remarks>This method can not be used with stored procedures for
        /// providers that require named parameters.  However, it is of
        /// great convenience for other cases.</remarks>
        /// <param name="values"></param>
        void AddRange(Array values);

        IDbDataParameter AddWithValue(string name, object parameterValue);

        IDbDataParameter Add(string name, Enum parameterType);

        IDbDataParameter Add(string name, Enum parameterType, int size);

        IDbDataParameter Add(string name, Enum parameterType, int size, string sourceColumn);

        IDbDataParameter AddOut(string name, Enum parameterType);

        IDbDataParameter AddOut(string name, Enum parameterType, int size);

        IDbDataParameter AddInOut(string name, Enum parameterType);

        IDbDataParameter AddInOut(string name, Enum parameterType, int size);

        IDbDataParameter AddReturn(string name, Enum parameterType);

        IDbDataParameter AddReturn(string name, Enum parameterType, int size);

        object GetValue(string name);

        void SetValue(string name, object parameterValue);

    }
}
