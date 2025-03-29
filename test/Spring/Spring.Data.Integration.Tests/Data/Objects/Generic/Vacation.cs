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

namespace Spring.Data.Objects.Generic;

public class Vacation
{
    private int id;
    private string firstName;
    private string lastName;
    private int employeeId;
    private DateTime startDate;
    private DateTime endDate;

    public int Id
    {
        get { return id; }
        set { id = value; }
    }

    public string FirstName
    {
        get { return firstName; }
        set { firstName = value; }
    }

    public string LastName
    {
        get { return lastName; }
        set { lastName = value; }
    }

    public int EmployeeId
    {
        get { return employeeId; }
        set { employeeId = value; }
    }

    public DateTime StartDate
    {
        get { return startDate; }
        set { startDate = value; }
    }

    public DateTime EndDate
    {
        get { return endDate; }
        set { endDate = value; }
    }

    public override bool Equals(object obj)
    {
        if (this == obj) return true;
        Vacation vacation = obj as Vacation;
        if (vacation == null) return false;
        return employeeId == vacation.employeeId && Equals(startDate, vacation.startDate);
    }

    public override int GetHashCode()
    {
        return employeeId + 29 * startDate.GetHashCode();
    }
}
