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
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace Spring.Reflection.Dynamic
{
    /// <summary>
    /// Unit tests for the SafeProperty class. SafeProperty must pass the same tests
    /// as DynamicField plus tests for accessing private members.
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class SafePropertyTests : BasePropertyTests
    {
        protected override IDynamicProperty Create(PropertyInfo property)
        {
            return new SafeProperty(property);
        }

#if NETCOREAPP
        private bool CanGenerateVisualBasic => false;
#else
        private bool CanGenerateVisualBasic => Spring.Util.SystemUtils.MonoRuntime;
#endif

        [Test]
        public void CanGetSetSimpleProperty()
        {
            if (!CanGenerateVisualBasic)
            {
                // TODO (EE): find solution for Mono
                return;
            }
            object o = GetVisualBasicTestObject();
            IDynamicProperty simpleProperty = Create(o.GetType().GetProperty("SimpleProperty"));
            simpleProperty.SetValue(o, "CanGetSimpleText", "args");
            Assert.AreEqual("CanGetSimpleText", ThisLastPropertyValue.GetValue(o));
            Assert.AreEqual("CanGetSimpleText", simpleProperty.GetValue(o));
        }

        [Test]
        public void CanGetSetSimpleIndexer()
        {
            if (!CanGenerateVisualBasic)
            {
                // TODO (EE): find solution for Mono
                return;
            }

            object o = GetVisualBasicTestObject();
            IDynamicProperty simpleProperty = Create(o.GetType().GetProperty("SimpleIndexer"));

            // write
            simpleProperty.SetValue(o, "CanGetSetSimpleIndexer", 2);
            Assert.AreEqual("CanGetSetSimpleIndexer", ThisLastPropertyValue.GetValue(o));
            Assert.AreEqual(2, ThisArg1.GetValue(o));

            // read
            object value = simpleProperty.GetValue(o, 3);
            Assert.AreEqual("CanGetSetSimpleIndexer", value);
            Assert.AreEqual(3, ThisArg1.GetValue(o));
        }

        [Test]
        public void CanGetSetComplexIndexer()
        {
            if (!CanGenerateVisualBasic)
            {
                // TODO (EE): find solution for Mono
                return;
            }
            object o = GetVisualBasicTestObject();
            IDynamicProperty property = Create(o.GetType().GetProperty("ComplexIndexer"));

            // write
            property.SetValue(o, "CanGetSetComplexIndexer", 2, "Arg2");
            Assert.AreEqual("CanGetSetComplexIndexer", ThisLastPropertyValue.GetValue(o));
            Assert.AreEqual(2.0, (double)ThisArg1.GetValue(o));
            Assert.AreEqual("Arg2", ThisArg2.GetValue(o));

            // read
            object value = property.GetValue(o, 3, "Arg3");
            Assert.AreEqual("CanGetSetComplexIndexer", value);
            Assert.AreEqual(3.0, (double)ThisArg1.GetValue(o));
            Assert.AreEqual("Arg3", ThisArg2.GetValue(o));
        }

        [Test]
        public void TestForRestrictiveSetterWithSafeWrapper()
        {
            Something something = new Something();

            IDynamicProperty third = Create(typeof(Something).GetProperty("Third"));
            third.SetValue(something, 456);
            //this should be ok, because both get and set of the "Third" property are public
            Assert.AreEqual(456, third.GetValue(something));

            IDynamicProperty second = Create(typeof(Something).GetProperty("Second"));
            Assert.AreEqual(2, second.GetValue(something));
            //this should not cause MethodAccessException because "second" is created using "CreateSafe"
            second.SetValue(something, 123);

            Assert.AreEqual(123, second.GetValue(something));
        }

        [Test]
        public void TestForRestrictiveGetterWithSafeWrapper()
        {
            Something something = new Something();
            //new SafeProperty()
            IDynamicProperty third = Create(typeof(Something).GetProperty("Third"));
            //this should be ok, because both get and set of the "Third" property are public
            third.SetValue(something, 456);
            Assert.AreEqual(456, third.GetValue(something));

            IDynamicProperty first = Create(typeof(Something).GetProperty("First"));
            first.SetValue(something, 123);
            //this should not cause MethodAccessException, "first" is createtd using "CreateSafe"
            Assert.AreEqual(123, first.GetValue(something));
        }

        #region VB TestClass Code

        private static Type s__visualBasicTestObjectType;
        private static IDynamicField ThisLastPropertyValue;
        private static IDynamicField ThisArg1;
        private static IDynamicField ThisArg2;
        private static IDynamicField ThisOptionalArg;
        private static IDynamicField ThisParamsArg;

        protected static object GetVisualBasicTestObject()
        {
            if (s__visualBasicTestObjectType == null)
            {
                // compile vb test class
                string vbSourceCode = new StreamReader( Assembly.GetExecutingAssembly().GetManifestResourceStream( typeof( BasePropertyTests ), "SafePropertyTests_TestObject.vb" ) ).ReadToEnd();

                CompilerParameters args = new CompilerParameters();
                args.OutputAssembly = "VbTestObject.dll";
                args.GenerateInMemory = true;
                args.GenerateExecutable = false;
                args.IncludeDebugInformation = true;

                CodeDomProvider provider = CodeDomProvider.CreateProvider( "VisualBasic" );
                CompilerResults results = provider.CompileAssemblyFromSource( args, vbSourceCode );

                if (results.Errors.HasErrors)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (CompilerError error in results.Errors)
                    {
                        sb.Append( error.ToString() ).Append( "\n\r" );
                    }
                    throw new TypeLoadException( "failed compiling test class: " + sb );
                }
                s__visualBasicTestObjectType = results.CompiledAssembly.GetType( "VbTestObject" );
                ThisLastPropertyValue = DynamicField.Create( s__visualBasicTestObjectType.GetField("ThisLastPropertyValue") );
                ThisArg1 = DynamicField.Create( s__visualBasicTestObjectType.GetField("ThisArg1") );
                ThisArg2 = DynamicField.Create( s__visualBasicTestObjectType.GetField("ThisArg2") );
                ThisOptionalArg = DynamicField.Create( s__visualBasicTestObjectType.GetField("ThisOptionalArg") );
                ThisParamsArg = DynamicField.Create( s__visualBasicTestObjectType.GetField("ThisParamsArgs") );
            }

            object s__visualBasicTestObject = Activator.CreateInstance(s__visualBasicTestObjectType);

            return s__visualBasicTestObject;
        }

        #endregion
    }

    #region Test Classes

    public class MyClassWithPrivateProperties
    {
        public static object s_staticRef;
        public static int s_staticVt;
        public object _ref;
        public int _vt;

        public MyClassWithPrivateProperties(object @ref, int vt)
        {
            _ref = @ref;
            _vt = vt;
        }

        private static object StaticRef
        {
            get { return s_staticRef; }
            set { s_staticRef = value; }
        }

        private static object StaticRefRo
        {
            get { return s_staticRef; }
        }

        private static object StaticRefWo
        {
            set { s_staticRef = value; }
        }

        private static int StaticVt
        {
            get { return s_staticVt; }
            set { s_staticVt = value; }
        }

        private static int StaticVtRo
        {
            get { return s_staticVt; }
        }

        private static int StaticVtWo
        {
            set { s_staticVt = value; }
        }

        private object Ref
        {
            get { return _ref; }
            set { _ref = value; }
        }

        private object RefRo
        {
            get { return _ref; }
        }

        private object RefWo
        {
            set { _ref = value; }
        }

        private int Vt
        {
            get { return _vt; }
            set { _vt = value; }
        }

        private int VtRo
        {
            get { return _vt; }
        }

        private int VtWo
        {
            set { _vt = value; }
        }
    }


    #endregion
}
