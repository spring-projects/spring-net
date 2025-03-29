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

using System.Data;
using System.Web.UI.WebControls;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.TestSupport;
using System.Collections;

namespace Spring.Util;

[TestFixture]
public class WebDIPerformanceTests
{
    private DataView CreateDataSource()
    {
        DataTable dt = new DataTable();
        DataRow dr;

        dt.Columns.Add(new DataColumn("IntegerValue", typeof(Int32)));
        dt.Columns.Add(new DataColumn("StringValue", typeof(string)));
        dt.Columns.Add(new DataColumn("CurrencyValue", typeof(double)));

        for (int i = 0; i < 100; i++)
        {
            dr = dt.NewRow();

            dr[0] = i;
            dr[1] = "Item " + i;
            dr[2] = 1.23 * (i + 1);

            dt.Rows.Add(dr);
        }

        DataView dv = new DataView(dt);
        return dv;
    }

    [Test, Explicit]
    public void RunTestWithoutDI()
    {
        DataView dv = CreateDataSource();

        int runs = 1000;

        StopWatch watch = new StopWatch();
        using (watch.Start("Duration: {0}"))
        {
            for (int i = 0; i < runs; i++)
            {
                DataGrid grid = new DataGrid();
                grid.DataSource = dv;
                grid.DataBind();
            }
        }
    }

    [Test, Explicit]
    public void RunTestWithDI()
    {
//            LogManager.Adapter = new Common.Logging.Simple.TraceLoggerFactoryAdapter();

        DataView dv = CreateDataSource();
        using (TestWebContext wctx = new TestWebContext("/testpath", "testpage.aspx"))
        {
            IApplicationContext ctx = new WebApplicationContext();

            int runs = 1000;

            StopWatch watch = new StopWatch();
            using (watch.Start("Duration: {0}"))
            {
                for (int i = 0; i < runs; i++)
                {
                    DataGrid grid = new DataGrid();
                    Spring.Web.Support.WebDependencyInjectionUtils.InjectDependenciesRecursive(ctx, grid);
                    grid.DataSource = dv;
                    grid.DataBind();
                }
            }

            using (watch.Start("Duration: {0}"))
            {
                for (int i = 0; i < runs; i++)
                {
                    DataGrid grid = new DataGrid();
                    grid.DataSource = dv;
                    grid.DataBind();
                    Spring.Web.Support.WebDependencyInjectionUtils.InjectDependenciesRecursive(ctx, grid);
                }
            }
        }
    }

    [Test, Explicit]
    public void TestHashtableVsDictionary()
    {
        int runs = 10000000;
        Hashtable ht = new Hashtable();
        Dictionary<object, object> dict = new Dictionary<object, object>();

        for (int i = 0; i < 100000; i++)
        {
            ht.Add(i, new object());
            dict.Add(new object(), new object());
        }

        StopWatch watch = new StopWatch();
        using (watch.Start("Duration Hashtable: {0}"))
        {
            for (int i = 0; i < runs; i++)
            {
                ht.ContainsKey(i);
            }
        }

        using (watch.Start("Duration Dictionary: {0}"))
        {
            for (int i = 0; i < runs; i++)
            {
                dict.ContainsKey(i);
            }
        }
    }
}
