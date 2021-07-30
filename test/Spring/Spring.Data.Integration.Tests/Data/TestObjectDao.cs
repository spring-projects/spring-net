using System;
using System.Collections;
using System.Data;
using Spring.Data.Common;
using Spring.Data.Core;
using Spring.Objects;

namespace Spring.Data
{
	/// <summary>
	/// AdoTemplate based implementation of ITestObjectDao data access layer.
	/// </summary>
	public class TestObjectDao : AdoDaoSupport, ITestObjectDao
	{

        public void Create(string name, int age)
        {
            AdoTemplate.ExecuteNonQuery(CommandType.Text,
                String.Format("insert into TestObjects(Age, Name) VALUES ({0}, '{1}')", 
                age, name));
        }

        public void Update(TestObject to)
        {
            AdoTemplate.ExecuteNonQuery(CommandType.Text,
                String.Format("UPDATE TestObjects SET Age={0}, Name='{1}' where TestObjectNo = {2}",
                to.Age,
                to.Name,
                to.ObjectNumber));
        }

        public void Delete(string name)
        {
            AdoTemplate.ExecuteNonQuery(CommandType.Text,
                String.Format("delete from TestObjects where Name = '{0}'",
                name));
        }

        public TestObject FindByName(string name)
        {
            IList toList = AdoTemplate.QueryWithRowMapper(CommandType.Text,
                "select TestObjectNo, Age, Name from TestObjects where Name='"+name+"'",
                new TestObjectRowMapper());
            if (toList.Count > 0)
            {
                return (TestObject)toList[0];
            }
            else
            {
                return null;
            }            
        }

        public IList FindAll()
        {
            return AdoTemplate.QueryWithRowMapper(CommandType.Text,
                "select TestObjectNo, Age, Name from TestObjects",
                new TestObjectRowMapper());
        }

        public int GetCount()
        {
            return (int)AdoTemplate.ExecuteScalar(CommandType.Text, "SELECT COUNT(*) FROM TestObjects");
        }

        public int GetCountByDelegate()
        {
            
            return (int)AdoTemplate.Execute(new CommandDelegate(MyDelegate));
        }

	    public int GetCount(int lowerAgeLimit)
	    {
            return (int) AdoTemplate.ExecuteScalar(CommandType.Text, 
                "SELECT COUNT(*) FROM TestObjects where age > @age","age",DbType.Int32, 0, lowerAgeLimit);
	    }

	    public int GetCount(int lowerAgeLimit, string name)
	    {
            IDbParameters dbParameters = AdoTemplate.CreateDbParameters();
            dbParameters.Add("age", DbType.Int32).Value = lowerAgeLimit;
	        dbParameters.Add("name", DbType.String).Value = name;
            return (int)AdoTemplate.ExecuteScalar(CommandType.Text, "SELECT COUNT(*) FROM TestObjects where age > @age and name = @name", dbParameters);
	    
	    }

	    public int GetCountByAltMethod(int lowerAgeLimit)
	    {
            return (int)AdoTemplate.ExecuteScalar(CommandType.Text, 
                "SELECT COUNT(*) FROM TestObjects where age > @age", 
                "age", DbType.Int32, 0, lowerAgeLimit);
	    
	    }

	    public int GetCountByCommandSetter(int lowerAgeLimit)
	    {
            return (int) AdoTemplate.ExecuteScalar(CommandType.Text,
                                                   "SELECT COUNT(*) FROM TestObjects where age > @age",
                                                   new SimpleCommandSetter(lowerAgeLimit));
	    }

	    private class SimpleCommandSetter : ICommandSetter
	    {
            private int lowerLimit;
	        public SimpleCommandSetter(int limit)
	        {
                lowerLimit = limit;	            
	        }

	        public void SetValues(IDbCommand dbCommand)
	        {
	            //wouldn't typically set param values this way....intended to set other IDbCommand props
                dbCommand.CommandTimeout = 999999;
	            
                IDbDataParameter parameter = dbCommand.CreateParameter();
                parameter.ParameterName = "age";
                parameter.DbType = DbType.Int32;
                parameter.Value = lowerLimit;
                dbCommand.Parameters.Add(parameter);
	        }
	    }


	    private Object MyDelegate(IDbCommand command)
        {
            command.CommandType = CommandType.Text;
            command.CommandText = "SELECT COUNT(*) FROM TestObjects";            
            return command.ExecuteScalar();
        }


        private class TestObjectRowMapper : IRowMapper
        {
            #region IRowMapper Members

            public object MapRow(IDataReader dataReader, int rowNum)
            {
                TestObject to = new TestObject();
                to.ObjectNumber = dataReader.GetInt32(0);
                to.Age = dataReader.GetInt32(1);
                to.Name = dataReader.GetString(2);
                return to;                
            }

            #endregion

        }

        public void Cleanup()
        {
            AdoTemplate.ExecuteNonQuery(CommandType.Text, "delete from TestObjects ");
        }
    }
}
