#region Imports

using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Spring.Objects;

#endregion

namespace Spring.Data
{
    public class NativeAdoTestObjectDao : ITestObjectDao
    {
        private String connectionString;

        public string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }


        public IList ExplicitTx()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        using (SqlCommand command = new SqlCommand("sql"))
                        {
                            command.Connection = connection;
                            command.Transaction = transaction;

                            /*
                             *   dbParameters.Add("savings", DbType.Decimal).Value = b.Savings.AsDecimal();
  dbParameters.Add("accountId", DbType.Int).Value = account.EntityId;
  dbParameters.Add("name", DbType.String).Value = b.Name;
                             */
                            command.Parameters.Add("@savings", SqlDbType.Decimal).Value = 1;// = b.Savings.AsDecimal();
                            command.Parameters.Add("@accountId", SqlDbType.Int).Value = 2;
                            command.Parameters.Add("@name", SqlDbType.NVarChar, 40).Value = 3;
                            command.ExecuteNonQuery();
                            //foreach (Benificiary b in Account.Beneficiaries)
                            //{
                                
                            //}

                            transaction.Commit();
                        }
                    }
                }
            } catch (Exception)
            {
                //log exception and rethrow
            }
            return null;
        }

        public IList FindAllPeople()
        {
            IList results = new ArrayList();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = "select Name, Age from ...";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //process the result set - populate Person object, add to array
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //throw application exception
            }
            return results;
        }

        public void Create2(string name, int age)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                String sql =
                    String.Format("insert into TestObjects(Age, Name) " +
                                  "VALUES ({0}, '{1}')",
                                  age, name);

                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Create(string name, int age)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlParameter p1 = new SqlParameter("name", SqlDbType.NVarChar);
                p1.Value = "asdf";

                /*
                string strSql = "insert into TestObjects(Age,Name) values (@Age,@Name)";
                
                
                SqlCommand comm = connection.CreateCommand();
                comm.CommandText = strSql;
                SqlParameter p1 = new SqlParameter();
                p1.ParameterName = "@Age";
                p1.Value = age;
                
                SqlParameter p2 = new SqlParameter();
                p2.ParameterName = "@Name";
                p2.Value = name;
                
                comm.Parameters.Add(p1);
                comm.Parameters.Add(p2);
                
                */

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(
                    "insert into TestObjects(Age, Name) VALUES ({0}, '{1}')",
                    age, name);

                SqlCommand comm = connection.CreateCommand();
                comm.CommandText = sb.ToString();


                connection.Open();
                comm.ExecuteNonQuery();
            }
        }

        public void Update(TestObject to)
        {
            // TODO:  Add NativeAdoTestObjectDao.Update implementation
        }

        public void Delete(string name)
        {
            // TODO:  Add NativeAdoTestObjectDao.Delete implementation
        }

        public TestObject FindByName(string name)
        {
            // TODO:  Add NativeAdoTestObjectDao.FindByName implementation
            return null;
        }

        public System.Collections.IList FindAll()
        {
            // TODO:  Add NativeAdoTestObjectDao.FindAll implementation
            return null;
        }

        public int GetCount()
        {
            // TODO:  Add NativeAdoTestObjectDao.GetCount implementation
            return 0;
        }

        public int GetCountByDelegate()
        {
            // TODO:  Add NativeAdoTestObjectDao.GetCountByDelegate implementation
            return 0;
        }

        public int GetCount(int lowerAgeLimit)
        {
            throw new NotImplementedException();
        }

        public int GetCount(int lowerAgeLimit, string name)
        {
            throw new NotImplementedException();
        }

        public int GetCountByAltMethod(int lowerAgeLimit)
        {
            throw new NotImplementedException();
        }

        public int GetCountByCommandSetter(int lowerAgeLimit)
        {
            throw new NotImplementedException();
        }

        public void Cleanup()
        {
            throw new NotImplementedException();
        }
    }
}