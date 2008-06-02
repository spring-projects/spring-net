#if NET_2_0

#region License

/*
 * Copyright 2002-2006 the original author or authors.
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
using System.Reflection;
using Common.Logging;
using Spring.Data.Common;

namespace Spring.Data.Support
{
    /// <summary>
    /// Using reflection on VS.NET 2005 a generated typed dataset, apply the
    /// connection/transaction pair associated with the current Spring based
    /// transaction scope.  
    /// </summary>
    /// <remarks>This avoids the limitations of using System.Transaction
    /// based transaction scope for multiple operation on typed datasets withone one
    /// transaction.  Reflection was used rather than partial classes to provide
    /// a general solution that can be written and applied once.
    /// Usage within a DAO method, FindAll() is shown below.  Note: Convenience methods
    /// to simplify calling syntax maybe provided in the future, it is a trade off on type
    /// safety calling the typed adapter defined outside the anonymous method as 
    /// compared to casting inside a "DoInDbAdapter(object dbAdapter) method.
    /// <code>
    /// public PrintGroupMappingDataSet FindAll()
    /// {
    ///
    ///    PrintGroupMappingTableAdapter adapter = new PrintGroupMappingTableAdapter();
    ///    PrintGroupMappingDataSet printGroupMappingDataSet = new PrintGroupMappingDataSet();
    ///        
    ///        
    ///    printGroupMappingDataSet = AdoTemplate.Execute(delegate(IDbCommand command)
    ///                               {
    ///                                   TypedDataSetUtils.ApplyConnectionAndTx(adapter, command);
    ///                                   adapter.Fill(printGroupMappingDataSet.PrintGroupMapping);
    ///                                   return printGroupMappingDataSet;
    ///                               }) 
    ///                               as PrintGroupMappingDataSet;
    ///
    ///    return printGroupMappingDataSet;
    /// }
    /// </code>
    /// See http://www.code-magazine.com/articleprint.aspx?quickid=0605031 for a 
    /// more complete discussion.
    /// 
    /// </remarks>
    public abstract class TypedDataSetUtils
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof (TypedDataSetUtils));


        /// <summary>
        /// Applies the connection and tx to the provided typed dataset.  The connection and transaction
        /// used are taken from the Spring's current transactional context.  See ConnectionUtils.GetConnectionTxPair
        /// for more information
        /// </summary>
        /// <param name="typedDataSetAdapter">The typed data set adapter.</param>
        /// <param name="dbProvider">The db provider.</param>
        public static void ApplyConnectionAndTx(object typedDataSetAdapter, IDbProvider dbProvider)
        {
            ConnectionTxPair connectionTxPairToUse = ConnectionUtils.GetConnectionTxPair(dbProvider);
            ApplyConnectionAndTxToDataAdapter(typedDataSetAdapter, connectionTxPairToUse.Connection, connectionTxPairToUse.Transaction);
        }

        /// <summary>
        /// Applies the connection and tx to the provided typed dataset.
        /// </summary>
        /// <remarks>Generated dataset to not inherit from a common base
        /// class so that we must pass in the generic object type.</remarks>
        /// <param name="typedDataSetAdapter">The typed data set adapter.</param>
        /// <param name="sourceCommand">The IDbCommand managed by Spring and set with
        /// the connection/transaction of the current Spring transaction scope.</param>
        public static void ApplyConnectionAndTx(object typedDataSetAdapter, IDbCommand sourceCommand)
        {                      
            if (sourceCommand != null)
            {
                ApplyConnectionAndTxToDataAdapter(typedDataSetAdapter, sourceCommand.Connection, sourceCommand.Transaction);
            }            
        }
        
        private static IDbCommand[] GetCommandCollection(object typedDataSetAdapter)
        {
            PropertyInfo pi =
                typedDataSetAdapter.GetType().GetProperty("CommandCollection",
                                                          BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo mi = pi.GetGetMethod(true);
            return mi.Invoke(typedDataSetAdapter, null) as IDbCommand[];
        }

        private static IDbDataAdapter GetDataAdapter(object typedDataSetAdapter)
        {
            PropertyInfo api =
                typedDataSetAdapter.GetType().GetProperty("Adapter", 
                                                          BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo ami = api.GetGetMethod(true);
            return ami.Invoke(typedDataSetAdapter, null) as IDbDataAdapter;
        }

        private static void ApplyConnectionAndTxToDataAdapter(object typedDataSetAdapter, IDbConnection connection, IDbTransaction transaction)
        {
            IDbDataAdapter dataAdapter = GetDataAdapter(typedDataSetAdapter);
            if (dataAdapter != null)
            {
                ApplyConnectionAndTxToCommand(dataAdapter.SelectCommand, connection, transaction);


                ApplyConnectionAndTxToCommand(dataAdapter.InsertCommand, connection, transaction);


                ApplyConnectionAndTxToCommand(dataAdapter.UpdateCommand, connection, transaction);


                ApplyConnectionAndTxToCommand(dataAdapter.DeleteCommand, connection, transaction);
            }
            else
            {
                LOG.Warn("Could not extract IDbDataAdapter from TypedDataset.");
            }


            IDbCommand[] commandCollection = GetCommandCollection(typedDataSetAdapter);
            if (commandCollection != null)
            {
                //don't know which one to set, so set them all.
                foreach (IDbCommand dbCommand in commandCollection)
                {
                    dbCommand.Connection = connection;
                    dbCommand.Transaction = transaction;
                }
            }
            else
            {
                LOG.Warn("Could not extract IDbCommand collection from TypedDataset.");
            }
        }


        private static void ApplyConnectionAndTxToCommand(IDbCommand targetDbCommand, IDbConnection connection, IDbTransaction transaction)
        {
            if (targetDbCommand != null)
            {
                targetDbCommand.Connection = connection;
                targetDbCommand.Transaction = transaction;
            }
        }


    }
}
#endif