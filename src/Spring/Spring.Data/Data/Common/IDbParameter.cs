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
	/// A parameter interface used by <see cref="IDbParametersBuilder"/>
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	public interface IDbParameter
	{

	    IDbParameter Type(Enum dbType);

	    IDbParameter Type(Enum dbType, int size);

	    Enum GetDbType();

	    IDbParameter Direction(ParameterDirection direction);

	    ParameterDirection GetDirection();

	    IDbParameter IsNullable(bool isNullable);

	    bool GetIsNullable();

	    IDbParameter Name(string name);

	    string GetName();

	    IDbParameter SourceColumn(string sourceColumn);

	    string GetSourceColumn();

	    IDbParameter SourceVersion(DataRowVersion sourceVersion);

	    DataRowVersion GetSourceVersion();


	    IDbParameter Value(object val);

	    object GetValue();

	    IDbParameter Precision(byte precision);

	    byte GetPrecision();


	    IDbParameter Scale(byte scale);

	    byte GetScale();


	    IDbParameter Size(int size);

	    int GetSize();

	}
}
