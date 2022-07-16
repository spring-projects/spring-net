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

using Spring.Objects;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;
using Spring.Core.IO;

namespace Spring.Context.Support
{
    /// <summary>
    /// Convenient abstract superclass for
    /// <see cref="Spring.Context.IApplicationContext"/> implementations that
    /// draw their configuration from XML documents containing object
    /// definitions as understood by an
    /// <see cref="Spring.Objects.Factory.Xml.XmlObjectDefinitionReader"/>.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Griffin Caprio (.NET)</author>
    public abstract class AbstractXmlApplicationContext : AbstractApplicationContext
    {
        private DefaultListableObjectFactory _objectFactory;

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Context.Support.AbstractXmlApplicationContext"/>
        /// class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an <see langword="abstract"/> class, and as such exposes
        /// no public constructors.
        /// </p>
        /// </remarks>
        protected AbstractXmlApplicationContext()
            : this(null, true, null)
        { }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Context.Support.AbstractXmlApplicationContext"/> class
        /// with the given parent context.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an <see langword="abstract"/> class, and as such exposes
        /// no public constructors.
        /// </p>
        /// </remarks>
        /// <param name="name">The application context name.</param>
        /// <param name="caseSensitive">Flag specifying whether to make this context case sensitive or not.</param>
        /// <param name="parentContext">The parent context.</param>
        protected AbstractXmlApplicationContext(string name, bool caseSensitive,
            IApplicationContext parentContext)
            : base(name, caseSensitive, parentContext)
        { }

        /// <summary>
        /// An array of resource locations, referring to the XML object
        /// definition files that this context is to be built with.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Examples of the format of the various strings that would be
        /// returned by accessing this property can be found in the overview
        /// documentation of with the <see cref="XmlApplicationContext"/>
        /// class.
        /// </p>
        /// </remarks>
        /// <returns>
        /// An array of resource locations, or <see langword="null"/> if none.
        /// </returns>
        protected abstract string[] ConfigurationLocations { get; }


        /// <summary>
        /// An array of resources that this context is to be built with.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Examples of the format of the various strings that would be
        /// returned by accessing this property can be found in the overview
        /// documentation of with the <see cref="XmlApplicationContext"/>
        /// class.
        /// </p>
        /// </remarks>
        /// <returns>
        /// An array of <see cref="Spring.Core.IO.IResource"/>s, or <see langword="null"/> if none.
        /// </returns>
        protected abstract IResource[] ConfigurationResources { get; }


        /// <summary>
        /// Instantiates and populates the underlying
        /// <see cref="Spring.Objects.Factory.IObjectFactory"/> with the object
        /// definitions yielded up by the <see cref="ConfigurationLocations"/>
        /// method.
        /// </summary>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In the case of errors encountered while refreshing the object factory.
        /// </exception>
        /// <exception cref="ApplicationContextException">
        /// In the case of errors encountered reading any of the resources
        /// yielded by the <see cref="ConfigurationLocations"/> method.
        /// </exception>
        /// <seealso cref="Spring.Context.Support.AbstractApplicationContext.RefreshObjectFactory()"/>
        protected override void RefreshObjectFactory()
        {
            // Shut down previous object factory, if any.
            DefaultListableObjectFactory oldObjectFactory = _objectFactory;
            _objectFactory = null;

            if (oldObjectFactory != null)
            {
                oldObjectFactory.Dispose();
            }

            try
            {
                DefaultListableObjectFactory objectFactory = CreateObjectFactory();
                CustomizeObjectFactory(objectFactory);
                LoadObjectDefinitions(objectFactory);

                _objectFactory = objectFactory;

                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug(
                        string.Format(
                            "Refreshed ObjectFactory for application context '{0}'.",
                            Name));
                }

                #endregion
            }
            catch (IOException ex)
            {
                throw new ApplicationContextException(
                    string.Format(
                        "I/O error parsing XML resource for application context '{0}'.",
                        Name), ex);
            }
            catch (UriFormatException ex)
            {
                throw new ApplicationContextException(
                    string.Format(
                        "Error parsing resource locations [{0}] for application context '{1}'.",
                        StringUtils.ArrayToCommaDelimitedString(ConfigurationLocations),
                        Name), ex);
            }
        }


        /// <summary>
        /// Initialize the object definition reader used for loading the object
        /// definitions of this context.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The default implementation of this method is a no-op; i.e. it does
        /// nothing. Can be overridden in subclasses to provide custom
        /// initialization of the supplied
        /// <paramref name="objectDefinitionReader"/>; for example, a derived
        /// class may want to turn off XML validation.
        /// </p>
        /// </remarks>
        /// <param name="objectDefinitionReader">
        /// The object definition reader used by this context.
        /// </param>
        protected virtual void InitObjectDefinitionReader(
            XmlObjectDefinitionReader objectDefinitionReader)
        { }

        /// <summary>
        /// Load the object definitions with the given
        /// <see cref="Spring.Objects.Factory.Xml.XmlObjectDefinitionReader"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The lifecycle of the object factory is handled by
        /// <see cref="Spring.Context.Support.AbstractXmlApplicationContext.RefreshObjectFactory"/>;
        /// therefore this method is just supposed to load and / or register
        /// object definitions.
        /// </p>
        /// </remarks>
        /// <param name="objectDefinitionReader">
        /// The reader containing object definitions.</param>
        /// <exception cref="ObjectsException">
        /// In case of object registration errors.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In the case of errors encountered reading any of the resources
        /// yielded by either the <see cref="ConfigurationLocations"/> or
        /// the <see cref="ConfigurationResources"/> methods.
        /// </exception>
        protected virtual void LoadObjectDefinitions(XmlObjectDefinitionReader objectDefinitionReader)
        {
            string[] locations = ConfigurationLocations;
            if (locations != null)
            {
                objectDefinitionReader.LoadObjectDefinitions(locations);
            }

            IResource[] resources = ConfigurationResources;
            if (resources != null)
            {
                objectDefinitionReader.LoadObjectDefinitions(resources);
            }

        }


        /// <summary>
        /// Loads the object definitions into the given object factory, typically through
        /// delegating to one or more object definition readers.
        /// </summary>
        /// <param name="objectFactory">The object factory to lead object definitions into</param>
        /// <see cref="XmlObjectDefinitionReader"/>
        /// <see cref="PropertiesObjectDefinitionReader"/>
        protected virtual void LoadObjectDefinitions(DefaultListableObjectFactory objectFactory)
        {
            //Create a new XmlObjectDefinitionReader for the given ObjectFactory
            XmlObjectDefinitionReader objectDefinitionReader = CreateXmlObjectDefinitionReader(objectFactory);

            // Configure the bean definition reader with this context's
            // resource loading environment.
            objectDefinitionReader.ResourceLoader = this;

            // Allow a subclass to provide custom initialization of the reader,
            // then proceed with actually loading the object definitions.
            InitObjectDefinitionReader(objectDefinitionReader);
            LoadObjectDefinitions(objectDefinitionReader);
        }

        /// <summary>
        /// Create a new reader instance for importing object definitions into the specified <paramref name="objectFactory"/>.
        /// </summary>
        /// <param name="objectFactory">the <see cref="DefaultListableObjectFactory"/> to be associated with the reader</param>
        /// <returns>a new <see cref="XmlObjectDefinitionReader"/> instance.</returns>
        protected virtual XmlObjectDefinitionReader CreateXmlObjectDefinitionReader(DefaultListableObjectFactory objectFactory)
        {
            return new XmlObjectDefinitionReader(objectFactory);
        }

        /// <summary>
        /// Customizes the internal object factory used by this context.
        /// </summary>
        /// <remarks>Called for each <see cref="AbstractApplicationContext.Refresh()"/> attempt.
        /// <p>
        /// The default implementation is empty.  Can be overriden in subclassses to customize
        /// DefaultListableBeanFatory's standard settings.
        /// </p></remarks>
        /// <param name="objectFactory">The newly created object factory for this context</param>
        protected virtual void CustomizeObjectFactory(DefaultListableObjectFactory objectFactory)
        {
            // noop
        }

        /// <summary>
        /// Create an internal object factory for this context.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Called for each <see cref="AbstractApplicationContext.Refresh()"/> attempt.
        /// This default implementation creates a
        /// <see cref="Spring.Objects.Factory.Support.DefaultListableObjectFactory"/>
        /// with the internal object factory of this context's parent serving
        /// as the parent object factory.  Can be overridden in subclasse,s
        /// for example to customize DefaultListableBeanFactory's settings.
        /// </p>
        /// </remarks>
        /// <returns>The object factory for this context.</returns>
        protected virtual DefaultListableObjectFactory CreateObjectFactory()
        {
            return new DefaultListableObjectFactory(IsCaseSensitive, GetInternalParentObjectFactory());
        }

        /// <summary>
        /// Subclasses must return their internal object factory here.
        /// </summary>
        /// <returns>
        /// The internal object factory for the application context.
        /// </returns>
        /// <seealso cref="Spring.Context.Support.AbstractApplicationContext.ObjectFactory"/>
        public override IConfigurableListableObjectFactory ObjectFactory
        {
            get { return _objectFactory; }
        }

        /// <summary>
        /// Determine whether the given object name is already in use within this context's object factory,
        /// i.e. whether there is a local object or alias registered under this name.
        /// </summary>
        public override bool IsObjectNameInUse(string objectName)
        {
            return _objectFactory.IsObjectNameInUse(objectName);
        }
    }
}
