#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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


#if NET_2_0

using System;
using System.Data;
using Spring.Data.Generic;

namespace Spring.Data.Objects.Generic
{
    /// <summary>
    /// This is 
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    /// <author>Mark Pollack</author>
    public class VacationRowMapper<T> : IRowMapper<T> where T : Vacation, new()
    {
        public T MapRow(IDataReader reader, int rowNum) 
        {
            T vacation = new T();
            vacation.Id = Decimal.ToInt32(reader.GetDecimal(0));
            vacation.FirstName = reader.GetString(1);
            vacation.LastName = reader.GetString(2);
            vacation.EmployeeId = Decimal.ToInt32(reader.GetDecimal(3));
            vacation.StartDate = reader.GetDateTime(4);
            vacation.EndDate = reader.GetDateTime(5);
            return vacation;
        }
    }
}

#endif