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

using System;
using System.Diagnostics;
using System.EnterpriseServices;
using System.IO;
using System.Reflection;

/*
The code below is only for reverse engineering generated code in reflector
 */
namespace Spring.EnterpriseServices
{
    internal interface ITestInterface
    {
        void Method();
    }

    internal delegate object GetObjectHandler(ServicedComponent component, string targetName);


    internal class ExportedServicedComponentSample : ServicedComponent, ITestInterface
    {
        protected static GetObjectHandler getObject;

        static ExportedServicedComponentSample()
        {
            try
            {
                // in case we're dealing w/ unsigned assemblies, that is the way to go...
                string path = Path.Combine( new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "Spring.Services.dll");
                Assembly a = Assembly.LoadFrom(path);
                if (a == null)
                {
                    a = Assembly.Load("Spring.Services, Version=1.2.0.20001, Culture=neutral, PublicKeyToken=null");
                }
                Type t = a.GetType("Spring.EnterpriseServices.ServicedComponentHelper", true);
                getObject = (GetObjectHandler) Delegate.CreateDelegate(typeof(GetObjectHandler), t.GetMethod("GetObject"));
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex);
                throw;
            }
        }

        public void Method()
        {
            ITestInterface targetInstance = (ITestInterface) getObject(null, "name");
            targetInstance.Method();
        }
    }
}
