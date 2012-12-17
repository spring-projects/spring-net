using System;
using System.Collections.Generic;
using System.Text;
using Spring.Data.Common;
using System.Data;

namespace Spring.Spring.Data.Common
{
    /// <summary>
    /// Count the number of calls to "CreateConnection()".
    /// </summary>
    public class CountGetConnDbProvider : DelegatingDbProvider
    {
        /// <summary>
        /// Count of calls to <see cref="CreateConnection"/>
        /// </summary>
        public static Int32 Count = 0;

        /// <summary>
        /// Count. 
        /// </summary>
        /// <returns></returns>
        public override IDbConnection CreateConnection()
        {
            Count++;
            return this.TargetDbProvider.CreateConnection();
        }
    }
}
