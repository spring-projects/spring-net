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

using System.Data;

namespace Spring.Data.Support
{
	/// <summary>
	/// A data reader implementation that mapps DBNull values to sensible defaults.
	/// </summary>
	/// <remarks>IDataRecord methods are virtual.</remarks>
	/// <author>Mark Pollack (.NET)</author>
	public class NullMappingDataReader : IDataReaderWrapper
	{
		#region Fields

	    protected IDataReader dataReader;
	    private bool alreadyDisposed = false;

		#endregion

		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="NullMappingDataReader"/> class.
        /// </summary>
		public NullMappingDataReader()
		{

		}

	    /// <summary>
	    /// Initializes a new instance of the <see cref="NullMappingDataReader"/> class.
	    /// </summary>
	    /// <param name="dataReader">The data reader to delegate operations to.</param>
	    public NullMappingDataReader(IDataReader dataReader)
	    {
	        this.dataReader = dataReader;
	    }

		#endregion

	    ~NullMappingDataReader()
	    {
	        Dispose(false);
	    }
		#region Methods
        protected virtual void Dispose(bool isDisposing)
        {
            // Don't dispose more than once
            if (alreadyDisposed)
            {
                return;
            }
            if (isDisposing)
            {
                //free managed resourced
                dataReader.Dispose();
            }
            alreadyDisposed = true;
        }
		#endregion

        #region IDataReaderWrapper Members

        public IDataReader WrappedReader
        {
            get
            {
                return dataReader;
            }
            set
            {
                dataReader = value;
            }
        }

        #endregion

        #region IDataReader Members

        public int RecordsAffected
        {
            get
            {
                return this.dataReader.RecordsAffected;
            }
        }

        public bool IsClosed
        {
            get
            {
                return this.dataReader.IsClosed;
            }
        }

        public bool NextResult()
        {
            return dataReader.NextResult();
        }

        public void Close()
        {
            dataReader.Close();
        }

        public bool Read()
        {
            return dataReader.Read();
        }

        public int Depth
        {
            get
            {
                return dataReader.Depth;
            }
        }

        public DataTable GetSchemaTable()
        {
            return dataReader.GetSchemaTable();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);
        }

        #endregion

        #region IDataRecord Members

        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>
        /// The 32-bit signed integer value of the specified field or 0 if null
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>.</exception>
        public virtual int GetInt32(int i)
        {
            if (dataReader.IsDBNull(i))
            {
                return 0;
            }
            else
            {
                return dataReader.GetInt32(i);
            }
        }


        /// <summary>
        /// Gets the <see cref="Object"/> with the specified name.
        /// Returns null if value equals DBNull.Value
        /// </summary>
        /// <value></value>
	    public virtual object this[string name]
        {
            get
            {
                object objectValue = dataReader[name];
                return DBNull.Value.Equals(objectValue) ? null : objectValue;
            }
        }


        /// <summary>
        /// Gets the <see cref="Object"/> with the specified i.
        /// Returns null if value equals DBNull.Value
        /// </summary>
        /// <value>Return the object or null if the value equals DBNull.Value</value>
	    public virtual object this[int i]
        {
            get
            {

                return dataReader.IsDBNull(i) ? null : dataReader[i];
            }
        }

        /// <summary>
        /// Return the value of the specified field, null if value equals DBNull.Value
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>
        /// The <see cref="T:System.Object"/> which will contain the field value upon return.
        /// Returns null if value equals DBNull.Value
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>.
        /// </exception>
        public virtual object GetValue(int i)
        {
            return dataReader.IsDBNull(i) ? null : dataReader.GetValue(i);
        }

        /// <summary>
        /// Return whether the specified field is set to null.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>
        /// 	<see langword="true"/> if the specified field is set to null, otherwise
        /// <see langword="false"/>.
        /// </returns>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception>
        public virtual bool IsDBNull(int i)
        {
            return dataReader.IsDBNull(i);
        }

        public virtual long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return dataReader.IsDBNull(i) ? 0 : dataReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        public virtual byte GetByte(int i)
        {
            return dataReader.IsDBNull(i) ? (byte)0 : dataReader.GetByte(i);
        }

        public virtual Type GetFieldType(int i)
        {
            return dataReader.GetFieldType(i);
        }

        public virtual decimal GetDecimal(int i)
        {
            return dataReader.IsDBNull(i) ? 0 : dataReader.GetDecimal(i);
        }

        public virtual int GetValues(object[] values)
        {
            return dataReader.GetValues(values);
        }

        public virtual string GetName(int i)
        {
            return dataReader.GetName(i);
        }

        public virtual int FieldCount
        {
            get
            {
                return dataReader.FieldCount;
            }
        }

        public virtual long GetInt64(int i)
        {
            return dataReader.IsDBNull(i) ? 0 : dataReader.GetInt64(i);
        }

        public virtual double GetDouble(int i)
        {
            return dataReader.IsDBNull(i) ? 0 : dataReader.GetDouble(i);
        }

        public virtual bool GetBoolean(int i)
        {
            return dataReader.IsDBNull(i) ? false : dataReader.GetBoolean(i);
        }

        public virtual Guid GetGuid(int i)
        {
             return dataReader.IsDBNull(i) ? Guid.Empty : dataReader.GetGuid(i);
        }

        public virtual DateTime GetDateTime(int i)
        {
             return dataReader.IsDBNull(i) ? DateTime.MinValue : dataReader.GetDateTime(i);
        }

        public virtual int GetOrdinal(string name)
        {
            return dataReader.GetOrdinal(name);
        }

        public virtual string GetDataTypeName(int i)
        {
            return dataReader.GetDataTypeName(i);
        }

        public virtual float GetFloat(int i)
        {
             return dataReader.IsDBNull(i) ? 0 : dataReader.GetFloat(i);
        }

        public virtual IDataReader GetData(int i)
        {
            return dataReader.GetData(i);
        }

        public virtual long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
             return dataReader.IsDBNull(i) ? 0 : dataReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        public virtual string GetString(int i)
        {
            return dataReader.IsDBNull(i) ? string.Empty : dataReader.GetString(i);
        }

        public virtual char GetChar(int i)
        {
            return dataReader.IsDBNull(i) ? char.MinValue : dataReader.GetChar(i);
        }

        public virtual short GetInt16(int i)
        {
            return dataReader.IsDBNull(i) ? (short)0 : dataReader.GetInt16(i);
        }

        #endregion

	}
}
