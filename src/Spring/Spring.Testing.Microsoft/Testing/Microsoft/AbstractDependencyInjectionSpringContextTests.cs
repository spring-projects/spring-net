#region License

/*
/// Copyright 2002-2010 the original author or authors.
///
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///      http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
 */

#endregion

using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Spring.Context;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

namespace Spring.Testing.Microsoft
{
    /// <summary>
    /// Convenient superclass for tests depending on a Spring context.
    /// The test instance itself is populated by Dependency Injection.
    /// </summary>
    ///
    /// <remarks>
    /// <p>Really for integration testing, not unit testing.
    /// You should <i>not</i> normally use the Spring container
    /// for unit tests: simply populate your objects in plain NUnit tests!</p>
    ///
    /// <p>This supports two modes of populating the test:
    /// <ul>
    /// <li>Via Property Dependency Injection. Simply express dependencies on objects
    /// in the test fixture, and they will be satisfied by autowiring by type.</li>
    /// <li>Via Field Injection. Declare protected variables of the required type
    /// which match named beans in the context. This is autowire by name,
    /// rather than type. This approach is based on an approach originated by
    /// Ara Abrahmian. Property Dependency Injection is the default: set the
    /// "populateProtectedVariables" property to true in the constructor to switch
    /// on Field Injection.</li>
    /// </ul></p>
    ///
    /// <p>This class will normally cache contexts based on a <i>context key</i>:
    /// normally the config locations String array describing the Spring resource
    /// descriptors making up the context. Unless the <code>SetDirty()</code> method
    /// is called by a test, the context will not be reloaded, even across different
    /// subclasses of this test. This is particularly beneficial if your context is
    /// slow to construct, for example if you are using Hibernate and the time taken
    /// to load the mappings is an issue.</p>
    ///
    /// <p>If you don't want this behavior, you can override the <code>ContextKey</code>
    /// property, most likely to return the test class. In conjunction with this you would
    /// probably override the <code>GetContext</code> method, which by default loads
    /// the locations specified by the <code>ConfigLocations</code> property.</p>
    ///
    /// <p><b>WARNING:</b> When doing integration tests from within VS.NET, only use
    /// assembly resource URLs. Else, you may see misleading failures when changing
    /// context locations.</p>
    /// </remarks>
    ///
    /// <author>Rod Johnson</author>
    /// <author>Rob Harrop</author>
    /// <author>Rick Evans</author>
    /// <author>Aleksandar Seovic (.NET)</author>
    public abstract class AbstractDependencyInjectionSpringContextTests : AbstractSpringContextTests
    {
        private bool populateProtectedVariables = false;
        private AutoWiringMode autowireMode = AutoWiringMode.ByType;
        private bool dependencyCheck = true;

        /// <summary>
        /// Application context this test will run against.
        /// </summary>
        protected IConfigurableApplicationContext applicationContext;

        /// <summary>
        /// Holds names of the fields that should be used for field injection.
        /// </summary>
        protected string[] managedVariableNames;
        private int loadCount = 0;

        /// <summary>
        /// Default constructor for AbstractDependencyInjectionSpringContextTests.
        /// </summary>
        public AbstractDependencyInjectionSpringContextTests()
        {}

        /// <summary>
        /// Gets or sets a flag specifying whether to populate protected
        /// variables of this test case.
        /// </summary>
        /// <value>
        /// A flag specifying whether to populate protected variables of this test case.
        /// Default is <b>false</b>.
        /// </value>
        public bool PopulateProtectedVariables
        {
            get { return populateProtectedVariables; }
            set { populateProtectedVariables = value; }
        }

        /// <summary>
        /// Gets or sets the autowire mode for test properties set by Dependency Injection.
        /// </summary>
        /// <value>
        /// The autowire mode for test properties set by Dependency Injection.
        /// The default is <see cref="AutoWiringMode.ByType"/>.
        /// </value>
        public AutoWiringMode AutowireMode
        {
            get { return autowireMode; }
            set { autowireMode = value; }
        }

        /// <summary>
        /// Gets or sets a flag specifying whether or not dependency checking
        /// should be performed for test properties set by Dependency Injection.
        /// </summary>
        /// <value>
        /// <p>A flag specifying whether or not dependency checking
        /// should be performed for test properties set by Dependency Injection.</p>
        /// <p>The default is <b>true</b>, meaning that tests cannot be run
        /// unless all properties are populated.</p>
        /// </value>
        public bool DependencyCheck
        {
            get { return dependencyCheck; }
            set { dependencyCheck = value; }
        }

        /// <summary>
        /// Gets the current number of context load attempts.
        /// </summary>
        public int LoadCount
        {
            get { return loadCount; }
        }

        /// <summary>
        /// Called to say that the "applicationContext" instance variable is dirty and
        /// should be reloaded. We need to do this if a test has modified the context
        /// (for example, by replacing an object definition).
        /// </summary>
        public void SetDirty()
        {
            SetDirty(ConfigLocations);
        }

        /// <summary>
        /// Test setup method.
        /// </summary>
        [TestInitialize]
        public virtual void TestInitialize()
        {
            this.applicationContext = GetContext(ContextKey);
            InjectDependencies();
            try
            {
                OnTestInitialize();
            }
            catch (Exception ex)
            {
                logger.Error("Setup error", ex);
                throw;
            }
        }

        /// <summary>
        /// Inject dependencies into 'this' instance (that is, this test instance).
        /// </summary>
        /// <remarks>
        /// <p>The default implementation populates protected variables if the
        /// <see cref="PopulateProtectedVariables"/> property is set, else
        /// uses autowiring if autowiring is switched on (which it is by default).</p>
        /// <p>You can certainly override this method if you want to totally control
        /// how dependencies are injected into 'this' instance.</p>
        /// </remarks>
        protected virtual void InjectDependencies()
        {
            if (PopulateProtectedVariables)
            {
                if (this.managedVariableNames == null)
                {
                    InitManagedVariableNames();
                }
                InjectProtectedVariables();
            }
            else if (AutowireMode != AutoWiringMode.No)
            {
                IConfigurableListableObjectFactory factory = this.applicationContext.ObjectFactory;
                ((AbstractObjectFactory) factory).IgnoreDependencyType(typeof(AutoWiringMode));
                factory.AutowireObjectProperties(this, AutowireMode, DependencyCheck);
            }
        }

        /// <summary>
        /// Gets a key for this context. Usually based on config locations, but
        /// a subclass overriding buildContext() might want to return its class.
        /// </summary>
        protected virtual object ContextKey
        {
            get { return ConfigLocations; }
        }

        /// <summary>
        /// Loads application context from the specified resource locations.
        /// </summary>
        /// <param name="locations">Resources to load object definitions from.</param>
        protected override IConfigurableApplicationContext LoadContextLocations(string[] locations)
        {
            ++this.loadCount;
            return base.LoadContextLocations(locations);
        }

        /// <summary>
        /// Retrieves the names of the fields that should be used for field injection.
        /// </summary>
        protected virtual void InitManagedVariableNames()
        {
            List<string> managedVarNames = new List<string>();
            Type type = GetType();

            do
            {
                FieldInfo[] fields =
                    type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance);
                if (logger.IsDebugEnabled)
                {
                    logger.Debug("Found " + fields.Length + " fields on " + type);
                }

                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo field = fields[i];
                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug("Candidate field: " + field);
                    }
                    if (IsProtectedInstanceField(field))
                    {
                        object oldValue = field.GetValue(this);
                        if (oldValue == null)
                        {
                            managedVarNames.Add(field.Name);
                            if (logger.IsDebugEnabled)
                            {
                                logger.Debug("Added managed variable '" + field.Name + "'");
                            }
                        }
                        else
                        {
                            if (logger.IsDebugEnabled)
                            {
                                logger.Debug("Rejected managed variable '" + field.Name + "'");
                            }
                        }
                    }
                }
                type = type.BaseType;
            } while (type != typeof (AbstractDependencyInjectionSpringContextTests));

            this.managedVariableNames = managedVarNames.ToArray();
        }

        private static bool IsProtectedInstanceField(FieldInfo field)
        {
            return field.IsFamily;
        }

        /// <summary>
        /// Injects protected fields using Field Injection.
        /// </summary>
        protected virtual void InjectProtectedVariables()
        {
            for (int i = 0; i < this.managedVariableNames.Length; i++)
            {
                string fieldName = this.managedVariableNames[i];
                Object obj = null;
                try
                {
                    FieldInfo field = GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null)
                    {
                        BeforeProtectedVariableInjection(field);
                        obj = this.applicationContext.GetObject(fieldName, field.FieldType);
                        field.SetValue(this, obj);
                        if (logger.IsDebugEnabled)
                        {
                            logger.Debug("Populated field: " + field);
                        }
                    }
                    else
                    {
                        if (logger.IsWarnEnabled)
                        {
                            logger.Warn("No field with name '" + fieldName + "'");
                        }
                    }
                }
                catch (NoSuchObjectDefinitionException)
                {
                    if (logger.IsWarnEnabled)
                    {
                        logger.Warn("No object definition with name '" + fieldName + "'");
                    }
                }
            }
        }

        /// <summary>
        /// Called right before a field is being injected
        /// </summary>
        protected virtual void BeforeProtectedVariableInjection(FieldInfo fieldInfo)
        {

        }

        /// <summary>
        /// Subclasses can override this method in order to
        /// add custom test setup logic.
        /// </summary>
        protected virtual void OnTestInitialize()
        {}

        /// <summary>
        /// Test teardown method.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            try
            {
                OnTestCleanup();
            }
            catch (Exception ex)
            {
                logger.Error("OnTearDown error", ex);
            }
        }

        /// <summary>
        /// Subclasses can override this method in order to
        /// add custom test teardown logic.
        /// </summary>
        protected virtual void OnTestCleanup()
        {}


        /// <summary>
        /// Subclasses must implement this property to return the locations of their
        /// config files. A plain path will be treated as a file system location.
        /// </summary>
        /// <value>An array of config locations</value>
        protected abstract string[] ConfigLocations { get; }
    }
}
