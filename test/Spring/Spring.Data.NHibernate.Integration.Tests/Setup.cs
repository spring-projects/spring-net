#region License

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

#endregion

using NUnit.Framework;
using Spring.Core.IO;
using Spring.Data.Common;
using Spring.Data.Core;
using Spring.Testing.Ado;

namespace Spring
{
    /// <summary>
    /// </summary>
    /// <author>Erich Eichinger</author>
    [SetUpFixture]
    public class Setup
    {
        private const string DBConnection = "Data Source=SPRINGQA;Database=$DATABASE$;Trusted_Connection=False;User ID=springqa;Password=springqa";
        private readonly IResourceLoader resourceLoader = new ConfigurableResourceLoader();

        private IResource GetResource(object instance, string name)
        {
            string resourcePath = TestResourceLoader.GetAssemblyResourceUri(instance, name);
            return resourceLoader.GetResource(resourcePath);
        }

        [SetUp]
        public void InstallMSSQLDatabase()
        {
            IDbProvider dbProvider = DbProviderFactory.GetDbProvider("SqlServer-2.0");
            AdoTemplate ado = new AdoTemplate(dbProvider);

            // (re-)create database(s)
            dbProvider.ConnectionString = DBConnection.Replace("$DATABASE$", "master");
            SimpleAdoTestUtils.ExecuteSqlScript(ado, GetResource(this, "RecreateDatabases.sql"));

            // create tables
            dbProvider.ConnectionString = DBConnection.Replace("$DATABASE$", "Spring");
            SimpleAdoTestUtils.ExecuteSqlScript(ado, GetResource(this, "Data.NHibernate/creditdebit.sql"));
            SimpleAdoTestUtils.ExecuteSqlScript(ado, GetResource(this, "Data.NHibernate/testObject.sql"));
        }
    }
}