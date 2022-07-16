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

namespace Spring.Data.Common
{
	/// <summary>
    /// A parameter class used by <see cref="DbParametersBuilder"/>
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	public class DbParameter : Spring.Data.Common.IDbParameter
	{
		#region Fields
        private Enum dbType;
	    private ParameterDirection direction;
        private bool isNullable;
        private string name;
	    private string sourceColumn;
	    private DataRowVersion sourceVersion;
        private object val;
	    private byte precision;
	    private byte scale;
	    private int size;
		#endregion

		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="DbParameter"/> class.
        /// </summary>
        /// <remarks>
        /// Sets the SourceVersion to be Default and the ParameterDirection to be Input.
        /// </remarks>
		public DbParameter()
		{
            sourceVersion = DataRowVersion.Default;
            direction = ParameterDirection.Input;
		}

		#endregion

        #region IDbParameter Members

        public Enum DbType
        {
            set
            {
                dbType = value;
            }
        }

        #endregion

	    public IDbParameter Type(Enum dbType)
	    {
	        this.dbType = dbType;
	        return this;
	    }

	    public IDbParameter Type(Enum dbType, int size)
	    {
	        this.dbType = dbType;
	        this.size = size;
	        return this;
	    }

	    public Enum GetDbType()
	    {
	        return dbType;
	    }

	    public IDbParameter Direction(ParameterDirection direction)
	    {
	        this.direction = direction;
	        return this;
	    }

	    public ParameterDirection GetDirection()
	    {
	        return direction;
	    }

	    public IDbParameter IsNullable(bool isNullable)
	    {
	        this.isNullable = isNullable;
	        return this;
	    }

	    public bool GetIsNullable()
	    {
	        return isNullable;
	    }

	    public IDbParameter Name(string name)
	    {
	        this.name = name;
	        return this;
	    }

	    public string GetName()
	    {
	        return name;
	    }

	    public IDbParameter SourceColumn(string sourceColumn)
	    {
	        this.sourceColumn = sourceColumn;
	        return this;
	    }

	    public string GetSourceColumn()
	    {
	        return sourceColumn;
	    }

	    public IDbParameter SourceVersion(DataRowVersion sourceVersion)
	    {
	        this.sourceVersion = sourceVersion;
	        return this;
	    }

	    public DataRowVersion GetSourceVersion()
	    {
	        return sourceVersion;
	    }

	    public IDbParameter Value(object val)
	    {
	        this.val = val;
	        return this;
	    }

	    public object GetValue()
	    {
	       return val;
	    }

	    public IDbParameter Precision(byte precision)
	    {
	        this.precision = precision;
	        return this;
	    }

	    public byte GetPrecision()
	    {
	        return precision;
	    }

	    public IDbParameter Scale(byte scale)
	    {
	        this.scale = scale;
	        return this;
	    }

	    public byte GetScale()
	    {
	        return scale;
	    }

	    public IDbParameter Size(int size)
	    {
	        this.size = size;
	        return this;
	    }

	    public int GetSize()
	    {
	        return size;
	    }
	}
}
