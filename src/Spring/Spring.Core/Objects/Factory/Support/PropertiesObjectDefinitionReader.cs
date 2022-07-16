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

using System.Collections;
using System.Collections.Specialized;
using System.Resources;

using Spring.Core.IO;
using Spring.Objects.Factory.Config;
using Spring.Util;

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Object definition reader for a simple properties format.
    /// </summary>
    /// <remarks>
    /// Provides object definition registration methods for
    /// <see cref="System.Collections.IDictionary"/> and
    /// <see cref="System.Resources.ResourceSet"/> instances. Typically applied to a
    /// <see cref="Spring.Objects.Factory.Support.DefaultListableObjectFactory"/>.
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Simon White (.NET)</author>
    public class PropertiesObjectDefinitionReader : AbstractObjectDefinitionReader
    {
        /// <summary>
        /// Value of a T/F attribute that represents true.
        /// Anything else represents false. Case seNsItive.
        /// </summary>
        public const string TrueValue = "true";

        /// <summary>
        /// Separator between object name and property name.
        /// </summary>
        public const string Separator = ".";

        /// <summary>
        /// Prefix for the class property of a root object definition.
        /// </summary>
        public const string ClassKey = "class";

        /// <summary>
        /// Special string added to distinguish if the object will be
        /// a singleton.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Default is true.
        /// </p>
        /// </remarks>
        /// <example>
        /// <p>
        /// owner.(singleton)=true
        /// </p>
        /// </example>
        public const string SingletonKey = "(singleton)";

        /// <summary>
        /// Special string added to distinguish if the object will be
        /// lazily initialised.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Default is false.
        /// </p>
        /// </remarks>
        /// <example>
        /// <p>
        /// owner.(lazy-init)=true
        /// </p>
        /// </example>
        public const string LazyInitKey = "(lazy-init)";

        /// <summary>
        /// Reserved "property" to indicate the parent of a child object definition.
        /// </summary>
        public const string ParentKey = "parent";

        /// <summary>
        /// Property suffix for references to other objects in the current
        /// <see cref="Spring.Objects.Factory.IObjectFactory"/>: e.g.
        /// owner.dog(ref)=fido.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Whether this is a reference to a singleton or a prototype
        /// will depend on the definition of the target object.
        /// </p>
        /// </remarks>
        public const string RefSuffix = "(ref)";

        /// <summary>
        /// Prefix before values referencing other objects.
        /// </summary>
        public const string RefPrefix = "*";

        private string _defaultParentObject = string.Empty;

        private IObjectDefinitionFactory _objectDefinitionFactory = new DefaultObjectDefinitionFactory();

        /// <summary>
        /// Name of default parent object
        /// </summary>
        public string DefaultParentObject
        {
            get { return _defaultParentObject; }
            set { this._defaultParentObject = value; }
        }

        /// <summary>
        /// Gets or sets object definition factory to use.
        /// </summary>
        public IObjectDefinitionFactory ObjectDefinitionFactory
        {
            get { return _objectDefinitionFactory; }
            set { _objectDefinitionFactory = value; }
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.PropertiesObjectDefinitionReader"/>
        /// class.
        /// </summary>
        /// <param name="registry">
        /// The <see cref="Spring.Objects.Factory.Support.IObjectDefinitionRegistry"/>
        /// instance that this reader works on.
        /// </param>
        public PropertiesObjectDefinitionReader(IObjectDefinitionRegistry registry)
            : base(registry)
        {}

        /// <summary>
        /// Load object definitions from the supplied <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">
        /// The resource for the object definitions that are to be loaded.
        /// </param>
        /// <returns>
        /// The number of object definitions that were loaded.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In the case of loading or parsing errors.
        /// </exception>
        public override int LoadObjectDefinitions(IResource resource)
        {
            return LoadObjectDefinitions(resource, string.Empty);
        }

        /// <summary>
        /// Load object definitions from the specified properties file.
        /// </summary>
        /// <param name="resource">
        /// The resource descriptor for the properties file.
        /// </param>
        /// <param name="prefix">
        /// The match or filter for object definition names, e.g. 'objects.'
        /// </param>
        /// <exception cref="Spring.Objects.ObjectsException">in case of loading or parsing errors</exception>
        /// <returns>the number of object definitions found</returns>
        public int LoadObjectDefinitions(IResource resource, string prefix)
        {
            Properties props = new Properties();
            try
            {
                Stream str = resource.InputStream;
                try
                {
                    props.Load(str);
                }
                finally
                {
                    str.Close();
                }
                return RegisterObjectDefinitions(props, prefix, resource.Description);
            }
            catch (IOException ex)
            {
                throw new ObjectDefinitionStoreException("IOException parsing properties from " + resource, ex);
            }
        }

        /// <summary>
        /// Register object definitions contained in a
        /// <see cref="System.Resources.ResourceSet"/>, using all property keys (i.e.
        /// not filtering by prefix).
        /// </summary>
        /// <param name="rs">
        /// The <see cref="System.Resources.ResourceSet"/> containing object definitions.
        /// </param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of loading or parsing errors.
        /// </exception>
        /// <returns>The number of object definitions registered.</returns>
        public int RegisterObjectDefinitions(ResourceSet rs)
        {
            return RegisterObjectDefinitions(rs, string.Empty);
        }

        /// <summary>
        /// Register object definitions contained in a
        /// <see cref="System.Resources.ResourceSet"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Similar syntax as for an <see cref="System.Collections.IDictionary"/>.
        /// This method is useful to enable standard .NET internationalization support.
        /// </p>
        /// </remarks>
        /// <param name="rs">
        /// The <see cref="System.Resources.ResourceSet"/> containing object definitions.
        /// </param>
        /// <param name="prefix">
        /// The match or filter for object definition names, e.g. 'objects.'
        /// </param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of loading or parsing errors.
        /// </exception>
        /// <returns>The number of object definitions registered.</returns>
        public int RegisterObjectDefinitions(ResourceSet rs, string prefix)
        {
            // Simply create a map and call overloaded method
            IDictionary id = new Hashtable();
            foreach (DictionaryEntry de in rs)
            {
                id.Add(de.Key, de.Value);
            }
            return RegisterObjectDefinitions(id, prefix);
        }

        /// <summary>
        /// Register object definitions contained in an
        /// <see cref="System.Collections.IDictionary"/>, using all property keys
        /// (i.e. not filtering by prefix).
        /// </summary>
        /// <param name="id">
        /// The <see cref="System.Collections.IDictionary"/> containing object definitions.
        /// </param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of loading or parsing errors.
        /// </exception>
        /// <returns>The number of object definitions registered.</returns>
        public int RegisterObjectDefinitions(IDictionary id)
        {
            return RegisterObjectDefinitions(id, string.Empty);
        }

        /// <summary>
        /// Registers object definitions contained in an <see cref="System.Collections.Specialized.NameValueCollection"/>
        /// using all property keys ( i.e. not filtering by prefix )
        /// </summary>
        /// <param name="nameValueCollection">The <see cref="System.Collections.Specialized.NameValueCollection"/> containing
        /// object definitions.
        /// </param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of loading or parsing errors.
        /// </exception>
        /// <returns>The number of object definitions registered.</returns>
        public int RegisterObjectDefinitions(NameValueCollection nameValueCollection)
        {
            IDictionary id = new Hashtable();
            foreach (DictionaryEntry de in nameValueCollection)
            {
                id.Add(de.Key, de.Value);
            }

            return RegisterObjectDefinitions(id);
        }

        /// <summary>
        /// Register object definitions contained in a
        /// <see cref="System.Collections.IDictionary"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Ignores ineligible properties.
        /// </p>
        /// </remarks>
        /// <param name="id">IDictionary name -> property (String or Object). Property values
        /// will be strings if coming from a Properties file etc. Property names
        /// (keys) must be strings. Type keys must be strings.
        /// </param>
        /// <param name="prefix">
        /// The match or filter within the keys in the map: e.g. 'objects.'
        /// </param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of loading or parsing errors.
        /// </exception>
        /// <returns>The number of object definitions found.</returns>
        public int RegisterObjectDefinitions(IDictionary id, string prefix)
        {
            return RegisterObjectDefinitions(id, prefix, "(no description)");
        }

        /// <summary>
        /// Register object definitions contained in a
        /// <see cref="System.Collections.IDictionary"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Ignores ineligible properties.
        /// </p>
        /// </remarks>
        /// <param name="id">IDictionary name -> property (String or Object). Property values
        /// will be strings if coming from a Properties file etc. Property names
        /// (keys) must be strings. Type keys must be strings.
        /// </param>
        /// <param name="prefix">
        /// The match or filter within the keys in the map: e.g. 'objects.'
        /// </param>
        /// <param name="resourceDescription">
        /// The description of the resource that the
        /// <see cref="System.Collections.IDictionary"/> came from (for logging purposes).
        /// </param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of loading or parsing errors.
        /// </exception>
        /// <returns>The number of object definitions found.</returns>
        public int RegisterObjectDefinitions(
            IDictionary id, string prefix, string resourceDescription)
        {
            if (prefix == null)
            {
                prefix = string.Empty;
            }
            int objectCount = 0;
            foreach (string key in id.Keys)
            {
                if (key.StartsWith(prefix))
                {
                    // Key is of form prefix<name>.property
                    string nameAndProperty = key.Substring(prefix.Length);
                    int sepIndx = nameAndProperty.IndexOf(Separator);
                    if (sepIndx != -1)
                    {
                        string name = nameAndProperty.Substring(0, sepIndx);

                        #region Instrumentation

                        if (log.IsDebugEnabled)
                        {
                            log.Debug("Found object name '" + name + "'");
                        }

                        #endregion

                        if (!Registry.ContainsObjectDefinition(name))
                        {
                            ++objectCount;
                        }
                        RegisterObjectDefinition(name, id, prefix + name, resourceDescription);
                    }
                    else
                    {
                        // Ignore it: it wasn't a valid object name and property,
                        // although it did start with the required prefix

                        #region Instrumentation

                        if (log.IsDebugEnabled)
                        {
                            log.Debug("Invalid object name and property [" + nameAndProperty + "]");
                        }

                        #endregion
                    }
                } // if the key started with the prefix we're looking for
            } // while there are more keys
            return objectCount;
        }

        /// <summary>
        /// Get all property values, given a prefix (which will be stripped)
        /// and add the object they define to the factory with the given name
        /// </summary>
        /// <param name="name">The name of the object to define.</param>
        /// <param name="id">
        /// The <see cref="System.Collections.IDictionary"/> containing string pairs.
        /// </param>
        /// <param name="prefix">The prefix of each entry, which will be stripped.</param>
        /// <param name="resourceDescription">
        /// The description of the resource that the
        /// <see cref="System.Collections.IDictionary"/> came from (for logging purposes).
        /// </param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of loading or parsing errors.
        /// </exception>
        protected void RegisterObjectDefinition(
            string name, IDictionary id, string prefix, string resourceDescription)
        {
            string typeName = null;
            string parent = null;
            bool singleton = true;
            bool lazyInit = false;

            MutablePropertyValues pvs = new MutablePropertyValues();
            foreach (string key in id.Keys)
            {
                if (key.StartsWith(prefix + Separator))
                {
                    string property = key.Substring(prefix.Length + Separator.Length);
                    if (property.Equals(ClassKey))
                    {
                        typeName = (string) id[key];
                    }
                    else if (property.Equals(SingletonKey))
                    {
                        string val = (string) id[key];
                        singleton = (val == null) || val.Equals(TrueValue);
                    }
                    else if (property.Equals(LazyInitKey))
                    {
                        string val = (string) id[key];
                        lazyInit = val.Equals(TrueValue);
                    }
                    else if (property.Equals(ParentKey))
                    {
                        parent = (string) id[key];
                    }
                    else if (property.EndsWith(RefSuffix))
                    {
                        // This isn't a real property, but a reference to another prototype
                        // Extract property name: property is of form dog(ref)
                        property = property.Substring(0, property.Length - RefSuffix.Length);
                        string reference = (String) id[key];

                        // It doesn't matter if the referenced object hasn't yet been registered:
                        // this will ensure that the reference is resolved at runtime
                        // Default is not to use singleton
                        object val = new RuntimeObjectReference(reference);
                        pvs.Add(new PropertyValue(property, val));
                    }
                    else
                    {
                        // normal object property
                        object val = id[key];
                        if (val is String)
                        {
                            string strVal = (string) val;
                            // if it starts with a reference prefix...
                            if (strVal.StartsWith(RefPrefix))
                            {
                                // expand reference
                                string targetName = strVal.Substring(1);
                                if (targetName.StartsWith(RefPrefix))
                                {
                                    // escaped prefix -> use plain value
                                    val = targetName;
                                }
                                else
                                {
                                    val = new RuntimeObjectReference(targetName);
                                }
                            }
                        }
                        pvs.Add(new PropertyValue(property, val));
                    }
                }
            }
            if (log.IsDebugEnabled)
            {
                log.Debug(pvs.ToString());
            }
            if (parent == null)
            {
                log.Debug(this.DefaultParentObject);
                parent = this.DefaultParentObject;
            }
            if (typeName == null && parent == null)
            {
                throw new ObjectDefinitionStoreException(resourceDescription, name,
                                                         "Either 'type' or 'parent' is required");
            }
            try
            {
                IConfigurableObjectDefinition objectDefinition = ObjectDefinitionFactory.CreateObjectDefinition(typeName, parent, Domain);
                objectDefinition.PropertyValues = pvs;
                objectDefinition.IsSingleton = singleton;
                objectDefinition.IsLazyInit = lazyInit;
                Registry.RegisterObjectDefinition(name, objectDefinition);
            }
            catch (Exception ex)
            {
                throw new ObjectDefinitionStoreException(
                    resourceDescription, name, "Unable to load type [" + typeName + "]", ex);
            }
        }
    }
}
