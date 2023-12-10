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

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using AopAlliance.Aop;
using NUnit.Framework;
using Spring.Aop;
using Spring.Aop.Framework;
using Spring.Context;
using Spring.Objects.Factory;

#pragma warning disable SYSLIB0050

namespace Spring
{
    /// <summary>
    /// Ensure, that all framework implementations of IAdvice and IAdvisor are serializable
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class AopSerializationTests
    {
        [Test]
        public void AllAopInfrastructureTypesAreSerializable()
        {
            ArrayList brokenTypes = new ArrayList();

            foreach (Type t in GetTypesToTest())
            {
                if (!ExcludeTypeFromTest(t)
                    && IsAopInfrastructureType(t))
                {
                    if (!CheckIsSerializable(t))
                    {
                        brokenTypes.Add(string.Format("{0} or one of its base classes are not marked as serializable\n", t.FullName));
                        continue;
                    }
                    if (t.IsAbstract) continue;

                    // perform a fast ser/deser check
                    try
                    {
                        object o = FormatterServices.GetSafeUninitializedObject(t);
                        o = SerializeAndDeserialize(o);
                    }
                    catch (SerializationException sex)
                    {
                        brokenTypes.Add(string.Format("{0}: {1}\n", t.FullName, sex.Message));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(string.Format("WARN: {0}: {1}\n", t.FullName, ex));
                    }
                }
            }

            Assert.IsEmpty(brokenTypes);
        }

        protected bool CheckIsSerializable(Type t)
        {
            if (t == typeof(object)) return true;

            return (t.IsSerializable && CheckIsSerializable(t.BaseType));
        }

        protected virtual ICollection GetTypesToTest()
        {
            return GetAssemblyToTest().GetTypes();
            //            return new Type[] { typeof(DynamicMethodMatcherPointcutAdvisor) };
        }

        protected virtual Assembly GetAssemblyToTest()
        {
            return typeof(IAdvice).Assembly;
        }

        protected virtual bool ExcludeTypeFromTest(Type t)
        {
            return false //t.IsAbstract 
                || t.IsInterface
                || typeof(IApplicationContextAware).IsAssignableFrom(t)
                || typeof(IObjectFactoryAware).IsAssignableFrom(t)
            ;
        }

        protected virtual bool IsAopInfrastructureType(Type t)
        {
            return typeof(IAdvisedSupportListener).IsAssignableFrom(t)
                   || typeof(ITargetSource).IsAssignableFrom(t)
                   || typeof(IAdvice).IsAssignableFrom(t)
                   || typeof(IAdvisor).IsAssignableFrom(t)
                   || typeof(IMethodMatcher).IsAssignableFrom(t)
                   || typeof(IPointcut).IsAssignableFrom(t);
        }

        private object SerializeAndDeserialize(object s)
        {
            // Serialize the session
            using (Stream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.AssemblyFormat = FormatterAssemblyStyle.Full;
                formatter.TypeFormat = FormatterTypeStyle.TypesAlways;
                formatter.Serialize(stream, s);

                // Deserialize the session
                stream.Position = 0;
                object res = formatter.Deserialize(stream);
                return res;
            }
        }
    }
}
