#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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
using System.Reflection;
using AopAlliance.Intercept;
using Common.Logging;
using Spring.Objects.Factory.Config;
using Spring.Context.Attributes;

namespace Spring.Context.Advice
{
    /// <summary>
    /// Intercepts calls to methods within the configuration object
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <author>Erich Eichinger</author>
    public class SpringObjectMethodInterceptor : IMethodInterceptor
    {
        #region Logging Definition

        private static readonly ILog LOG = LogManager.GetLogger(typeof(SpringObjectMethodInterceptor));

        #endregion

        private readonly IConfigurableListableObjectFactory _configurableListableObjectFactory;
        

        public SpringObjectMethodInterceptor(IConfigurableListableObjectFactory configurableListableObjectFactory)
        {
            _configurableListableObjectFactory = configurableListableObjectFactory;
            
        }

        #region IMethodInterceptor Members

        public object Invoke(IMethodInvocation invocation)
        {
            MethodInfo m = invocation.Method;
            if (m.Name.StartsWith("set_") || m.Name.StartsWith("get_"))
                return invocation.Proceed();

            string name = m.Name;

            object[] attribs = m.GetCustomAttributes(typeof(DefinitionAttribute), true);
            if (attribs.Length > 0)
            {

            }


            if (IsCurrentlyInCreation(name))
            {
                if (LOG.IsDebugEnabled)
                {
                    LOG.Debug(name + " currently in creation, created one.");
                }
                return invocation.Proceed();                
            }
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug(name + " not in creation, asked the application context for one");
            }

            return _configurableListableObjectFactory.GetObject(name);
        }

        private bool IsCurrentlyInCreation(string name)
        {
            return _configurableListableObjectFactory.IsCurrentlyInCreation(name);
        }

        #endregion
    }
}