#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

#region Imports

using System;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

#endregion

namespace Spring.Objects.Factory.Xml
{
	/// <summary>
	/// Constants defining the structure and values associated with the
	/// Spring.NET XML object definition format.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	public sealed class ObjectDefinitionConstants
	{
		/// <summary>
		/// Value of a boolean attribute that represents
		/// <see langword="true"/>.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Anything else represents <see langword="false"/>.
		/// </p>
		/// </remarks>
		public const string TrueValue = "true";

        /// <summary>
        /// Value of a boolean attribute that represents
        /// <see langword="false"/>.
        /// </summary>
        public const string FalseValue = "false";

		/// <summary>
		/// Signifies that a default value is to be applied.
		/// </summary>
		public const string DefaultValue = "default";

		/// <summary>
		/// Defines an external XML object definition resource.
		/// </summary>
		public const string ImportElement = "import";

		/// <summary>
		/// Specifies the relative path to an external XML object definition
		/// resource.
		/// </summary>
		public const string ImportResourceAttribute = "resource";

		/// <summary>
		/// Defines an alias for an object definition.
		/// </summary>
		public const string AliasElement = "alias";

		/// <summary>
		/// Specifies the alias of an object definition.
		/// </summary>
		public const string AliasAttribute = "alias";

		/// <summary>
		/// Specifies the default lazy initialization mode.
		/// </summary>
		public const string DefaultLazyInitAttribute = "default-lazy-init";

		/// <summary>
		/// Specifies the default dependency checking mode.
		/// </summary>
		public const string DefaultDependencyCheckAttribute
			= "default-dependency-check";

		/// <summary>
		/// Specifies the default autowire mode.
		/// </summary>
		public const string DefaultAutowireAttribute = "default-autowire";

        /// <summary>
        /// Specifies the default autowire candidates.
        /// </summary>
        public const string DefaultAutowireCandidatesAttribute = "default-autowire-candidates";

        /// <summary>
        /// Specifies the default collection merge mode.
        /// </summary>
	    public const string DefaultMergeAttribute = "default-merge";

        /// <summary>
        /// Specifies the default init method.
        /// </summary>
        public const string DefaultInitMethodAttribute = "default-init-method";

        /// <summary>
        /// Specifies the default destroy method.
        /// </summary>
        public const string DefaultDestroyMethodAttribute = "default-destroy-method";

		/// <summary>
		/// Defines a single named object.
		/// </summary>
		public const string ObjectElement = "object";

		/// <summary>
		/// Element containing informative text describing the purpose of the
		/// enclosing element.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Always optional.
		/// </p>
		/// <p>
		/// Used primarily for user documentation of XML object definition
		/// documents.
		/// </p>
		/// </remarks>
		public const string DescriptionElement = "description";

		/// <summary>
		/// Specifies a <see cref="System.Type"/>.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Does not have to be fully assembly qualified, but it is recommended
		/// that the <see cref="System.Type"/> names of one's objects are
		/// specified explicitly.
		/// </p>
		/// </remarks>
		public const string TypeAttribute = "type";

		/// <summary>
		/// The name or alias of the parent object definition that a child
		/// object definition inherits from.
		/// </summary>
		public const string ParentAttribute = "parent";

		/// <summary>
		/// Objects can be identified by an id, to enable reference checking.
		/// </summary>
		/// <remarks>
		/// <p>
		/// There are constraints on a valid XML id: if you want to reference
		/// your object in .NET code using a name that's illegal as an XML id,
		/// use the optional <c>"name"</c> attribute
		/// (<see cref="ObjectDefinitionConstants.NameAttribute"/>).
		/// If neither given, the objects <see cref="System.Type"/> name is
		/// used as id.
		/// </p>
		/// </remarks>
		public const string IdAttribute = "id";

		/// <summary>
		/// Can be used to create one or more aliases illegal in an id.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Multiple aliases can be separated by any number of spaces,
		/// semicolons, or commas
		/// (<see cref="Spring.Objects.Factory.Xml.ObjectDefinitionConstants.ObjectNameDelimiters"/>).
		/// </p>
		/// <p>
		/// Always optional.
		/// </p>
		/// </remarks>
		public const string NameAttribute = "name";

		/// <summary>
		/// Is this object a "singleton" (one shared instance, which will
		/// be returned by all calls to
		/// <see cref="Spring.Objects.Factory.IObjectFactory.GetObject(string)"/> with the id), or a
		/// "prototype" (independent instance resulting from each call to
		/// <see cref="Spring.Objects.Factory.IObjectFactory.GetObject(string)"/>).
		/// </summary>
		/// <remarks>
		/// <p>
		/// Singletons are most commonly used, and are ideal for multi-threaded
		/// service objects.
		/// </p>
		/// </remarks>
		/// <seealso cref="Spring.Objects.Factory.IObjectFactory"/>
		public const string SingletonAttribute = "singleton";

		/// <summary>
		/// Controls object scope. Only applicable to ASP.NET web applications.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Scope can be defined as either application, session or request. It
		/// defines when "singleton" instances are initialized, but has no
		/// effect on prototype definitions.
		/// </p>
		/// </remarks>
		public const string ScopeAttribute = "scope";

		/// <summary>
		/// The names of the objects that this object depends on being
		/// initialized.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The object factory will guarantee that these objects
		/// get initialized before this object definition.
		/// </p>
		/// <note>
		/// Dependencies are normally expressed through object properties or
		/// constructor arguments. This property should just be necessary for
		/// other kinds of dependencies such as statics (*ugh*) or database
		/// preparation on startup.
		/// </note>
		/// </remarks>
		public const string DependsOnAttribute = "depends-on";

		/// <summary>
		/// Optional attribute for the name of the custom initialization method
		/// to invoke after setting object properties.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The method <b>must</b> have no arguments.
		/// </p>
		/// </remarks>
		public const string InitMethodAttribute = "init-method";

		/// <summary>
		/// Optional attribute for the name of the custom destroy method to
		/// invoke on object factory shutdown.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Valid destroy methods have either of the following signatures...
		/// <list type="bullet">
		/// <item><c>void MethodName()</c></item>
		/// <item><c>void MethodName(bool force)</c></item>
		/// </list>
		/// </p>
		/// <note>
		/// Only invoked on singleton objects!
		/// </note>
		/// </remarks>
		public const string DestroyMethodAttribute = "destroy-method";

		/// <summary>
		/// A constructor argument : the constructor-arg tag can have an
		/// optional type attribute, to specify the exact type of the
		/// constructor argument
		/// </summary>
		/// <remarks>
		/// <p>
		/// Only needed  to avoid ambiguities, e.g. in case of 2 single
		/// argument constructors that can both be converted from a
		/// <see cref="System.String"/>.
		/// </p>
		/// </remarks>
		public const string ConstructorArgElement = "constructor-arg";

		/// <summary>
		/// The constructor-arg tag can have an optional index attribute,
		/// to specify the exact index in the constructor argument list.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Only needed to avoid ambiguities, e.g. in case of 2 arguments of
		/// the same type.
		/// </p>
		/// </remarks>
		public const string IndexAttribute = "index";

		/// <summary>
		/// The constructor-arg tag can have an optional named parameter
		/// attribute, to specify a named parameter in the constructor
		/// argument list.
		/// </summary>
		public const string ArgumentNameAttribute = "name";

		/// <summary>
		/// Is this object "abstract", i.e. not meant to be instantiated itself
		/// but rather just serving as parent for concrete child object
		/// definitions?
		/// </summary>
		/// <remarks>
		/// <p>
		/// Default is <see langword="false"/>. Specify <see langword="true"/>
		/// to tell the object factory to not try to instantiate that
		/// particular object in any case.
		/// </p>
		/// </remarks>
		public const string AbstractAttribute = "abstract";

		/// <summary>
		/// A property definition : object definitions can have zero or more
		/// properties.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Spring.NET supports primitives, references to other objects in the
		/// same or related factories, lists, dictionaries, and name value
		/// collections.
		/// </p>
		/// </remarks>
		public const string PropertyElement = "property";
        
        /// <summary>
        /// A qualifier definition used for fine grained autowiring
        /// </summary>
	    public const string QualifierElement = "qualifier";

		/// <summary>
		/// A reference to another managed object or static
		/// <see cref="System.Type"/>.
		/// </summary>
		public const string RefElement = "ref";

		/// <summary>
		/// ID refs must specify a name of the target object.
		/// </summary>
		public const string IdRefElement = "idref";

		/// <summary>
		/// A reference to the name of another managed object in the same
		/// context.
		/// </summary>
		public const string ObjectRefAttribute = "object";

		/// <summary>
		/// A reference to the name of another managed object in the same
		/// context.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Local references, using the "local" attribute, have to use object
		/// ids; they can be checked by a parser, thus should be preferred for
		/// references within the same object factory XML file.
		/// </p>
		/// </remarks>
		public const string LocalRefAttribute = "local";

		/// <summary>
		/// Alternative to type attribute for factory-method usage.
		/// </summary>
		/// <remarks>
		/// <p>
		/// If this is specified, no type attribute should be used. This should
		/// be set to the name of an object in the current or ancestor
		/// factories that contains the relevant factory method. This allows
		/// the factory itself to be configured using Dependency Injection, and
		/// an instance (rather than static) method to be used.
		/// </p>
		/// </remarks>
		public const string FactoryObjectAttribute = "factory-object";

		/// <summary>
		/// Optional attribute specifying the name of a factory method to use
		/// to create this object.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Use constructor-arg elements to specify arguments to the factory
		/// method, if it takes arguments. Autowiring does <b>not</b> apply to
		/// factory methods.
		/// </p>
		/// <p>
		/// If the "type" attribute is present, the factory method will be a
		/// static method on the type specified by the "type" attribute on
		/// this object definition. Often this will be the same type as that
		/// of the constructed object - for example, when the factory method
		/// is used as an alternative to a constructor. However, it may be on
		/// a different type. In that case, the created object will *not* be
		/// of the type specified in the "type" attribute. This is analogous
		/// to <see cref="Spring.Objects.Factory.IFactoryObject"/> behaviour.
		/// </p>
		/// <p>
		/// If the "factory-object" attribute is present, the "type" attribute
		/// is not used, and the factory method will be an instance method on
		/// the object returned from a
		/// <see cref="Spring.Objects.Factory.IObjectFactory.GetObject(string)"/>
		/// call with the specified object name. The factory object may be
		/// defined as a singleton or a prototype.
		/// </p>
		/// <p>
		/// The factory method can have any number of arguments. Use indexed
		/// constructor-arg elements in conjunction with the factory-method
		/// attribute.
		/// </p>
		/// <p>
		/// Setter Injection can be used in conjunction with a factory method.
		/// Method Injection cannot, as the factory method returns an instance,
		/// which will be used when the container creates the object.
		/// </p>
		/// </remarks>
		public const string FactoryMethodAttribute = "factory-method";

		/// <summary>
		/// A list can contain multiple inner object, ref, collection, or
		/// value elements.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Lists are untyped, pending generics support, although references
		/// will be strongly typed.
		/// </p>
		/// <p>
		/// A list can also map to an array type. The necessary conversion is
		/// automatically performed by the
		/// <see cref="Spring.Objects.Factory.Support.AbstractObjectFactory"/>.
		/// </p>
		/// </remarks>
		public const string ListElement = "list";

		/// <summary>
		/// A set can contain multiple inner object, ref, collection, or value
		/// elements.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Sets are untyped, pending generics support, although references
		/// will be strongly typed.
		/// </p>
		/// </remarks>
		public const string SetElement = "set";

		/// <summary>
		/// A Spring.NET map is a mapping from a string key to object (a .NET
		/// <see cref="System.Collections.IDictionary"/>).
		/// </summary>
		/// <remarks>
		/// <p>
		/// Dictionaries may be empty.
		/// </p>
		/// </remarks>
		public const string DictionaryElement = "dictionary";

		/// <summary>
		/// A lookup key (for a dictionary or name / value collection).
		/// </summary>
		public const string KeyAttribute = "key";

		/// <summary>
		/// A lookup key (for a dictionary or name / value collection).
		/// </summary>
		public const string KeyElement = "key";

        /// <summary>
        /// Contains a string representation of a value.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is used by name-value, ctor argument, and property elements.
        /// </p>
        /// </remarks>
        public const string ValueAttribute = "value";
        
        /// <summary>
        /// Contains delimiters that should be used to split delimited string values.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is used by name-value element.
        /// </p>
        /// </remarks>
        public const string DelimitersAttribute = "delimiters";
        
	    /// <summary>
		/// A reference to another objects.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Used as a convenience shortcut on property and constructor-arg
		/// elements to refer to other objects.
		/// </p>
		/// </remarks>
		public const string RefAttribute = "ref";

        /// <summary>
        /// Contains a string representation of an expression.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is used by ctor argument and property elements.
        /// </p>
        /// </remarks>
        public const string ExpressionAttribute = "expression";
        
	    /// <summary>
		/// A map entry can be an inner object, ref, collection, or value.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The name of the property is given by the "key" attribute.
		/// </p>
		/// </remarks>
		public const string EntryElement = "entry";

		/// <summary>
		/// Contains a string representation of a property value.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The property may be a string, or may be converted to the
		/// required <see cref="System.Type"/> using the
		/// <see cref="System.ComponentModel.TypeConverter"/> 
		/// machinery. This makes it possible for application developers to
		/// write custom <see cref="System.ComponentModel.TypeConverter"/>
		/// implementations that can convert strings to objects.
		/// </p>
		/// <note>
		/// This is recommended for simple objects only. Configure more complex
		/// objects by setting properties to references to other objects.
		/// </note>
		/// </remarks>
		public const string ValueElement = "value";

        /// <summary>
        /// Contains a string representation of an expression.
        /// </summary>
        public const string ExpressionElement = "expression";
        
	    /// <summary>
		/// Denotes <see langword="null"/>  value.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Necessary because an empty "value" tag will resolve to an empty
		/// <see cref="System.String"/>, which will not be resolved to
		/// <see langword="null"/> value unless a special
		/// <see cref="System.ComponentModel.TypeConverter"/> does so.
		/// </p>
		/// </remarks>
		public const string NullElement = "null";

		/// <summary>
		/// 'name-values' elements differ from dictionary elements in that
		/// values must be strings.
		/// </summary>
		/// <remarks>
		/// <p>
		/// May be empty.
		/// </p>
		/// </remarks>
		public const string NameValuesElement = "name-values";

		/// <summary>
		/// Element content is the string value of the property.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The "key" attribute is the name of the property.
		/// </p>
		/// </remarks>
		public const string AddElement = "add";

		/// <summary>
		/// The lazy initialization mode for an individual object definition.
		/// </summary>
		public const string LazyInitAttribute = "lazy-init";

		/// <summary>
		/// The dependency checking mode for an individual object definition.
		/// </summary>
		public const string DependencyCheckAttribute = "dependency-check";

		/// <summary>
		/// Defines a subscription to one or more events published by one or
		/// more event sources.
		/// </summary>
		public const string ListenerElement = "listener";

		/// <summary>
		/// The name of an event handling method.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Defaults to <c>On${event}</c>.
		/// <b>Note : this default will probably change before the first 1.0
		/// release.</b>
		/// </p>
		/// </remarks>
		public const string ListenerMethodAttribute = "method";

		/// <summary>
		/// The name of an event.
		/// </summary>
		public const string ListenerEventAttribute = "event";

		/// <summary>
		/// The autowiring mode for an individual object definition.
		/// </summary>
		public const string AutowireAttribute = "autowire";

        /// <summary>
        /// The autowiring mode for an individual object definition.
        /// </summary>
        public const string AutowireCandidateAttribute = "autowire-candidate";

        /// <summary>
        /// Attribute element to farther deifne the qualifier of an object
        /// </summary>
	    public const string AttributeElement = "attribute";

        /// <summary>
        /// The primary object for autwired injection
        /// </summary>
        public const string PrimaryAttribute = "primary";

		/// <summary>
		/// Shortcut alternative to specifying a key element in a
		/// dictionary entry element with <c>&lt;ref object="..."/&gt;</c>.
		/// </summary>
		public const string DictionaryKeyRefShortcutAttribute = "key-ref";

		/// <summary>
		/// Shortcut alternative to specifying a value element in a
		/// dictionary entry element with <c>&lt;ref object="..."/&gt;</c>.
		/// </summary>
		public const string DictionaryValueRefShortcutAttribute = "value-ref";

        /// <summary>
        /// Specify if the collection values should be merged with the parent.
        /// </summary>
	    public const string MergeAttribute = "merge";

        /// <summary>
        /// Defined meta attributes to be used for Autowire objects
        /// </summary>
	    public const string MetaElement = "meta";

		/// <summary>
		/// The string of characters that delimit object names.
		/// </summary>
		public const string ObjectNameDelimiters = ",; ";

		/// <summary>
		/// A lookup method causes the IoC container to override a given method and return
		/// the object with the name given in the attendant <c>object</c> attribute.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is a form of Method Injection.
		/// </p>
		/// <p>
		/// It's particularly useful as an alternative to implementing the
		/// <see cref="Spring.Objects.Factory.IObjectFactoryAware"/> interface,
		/// in order to be able to make
		/// <see cref="Spring.Objects.Factory.IObjectFactory.GetObject(string)"/>
		/// calls for non-singleton instances at runtime. In this case, Method Injection
		/// is a less invasive alternative.
		/// </p>
		/// </remarks>
		public const string LookupMethodElement = "lookup-method";

		/// <summary>
		/// The name of a lookup method. This method <b>must</b> take no arguments.
		/// </summary>
		public const string LookupMethodNameAttribute = "name";

		/// <summary>
		/// The name of the object in the IoC container that the lookup method
		/// must resolve to.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Often this object will be a prototype, in which case the lookup method
		/// will return a distinct instance on every invocation. This is useful
		/// for single-threaded objects.
		/// </p>
		/// </remarks>
		public const string LookupMethodObjectNameAttribute = "object";

		/// <summary>
		/// A replaced method causes the IoC container to override a given method
		/// with an (arbitrary) implementation at runtime.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This (again) is a form of Method Injection.
		/// </p>
		/// </remarks>
		public const string ReplacedMethodElement = "replaced-method";

		/// <summary>
		/// Name of the method whose implementation should be replaced by the
		/// IoC container.
		/// </summary>
		/// <remarks>
		/// <p>
		/// If this method is not overloaded, there's no need to use arg-type 
		/// subelements.
		/// </p>
		/// <p>
		/// If this method is overloaded, <c>arg-type</c> subelements must be
		/// used for all override definitions for the method.
		/// </p>
		/// </remarks>
		public const string ReplacedMethodNameAttribute = "name";

		/// <summary>
		/// The object name of an implementation of the
		/// <see cref="Spring.Objects.Factory.Support.IMethodReplacer"/> interface.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This may be a singleton or prototype. If it's a prototype, a new
		/// instance will be used for each method replacement. Singleton usage
		/// is the norm.
		/// </p>
		/// </remarks>
		public const string ReplacedMethodReplacerNameAttribute = "replacer";

		/// <summary>
		/// Subelement of <c>replaced-method</c> identifying an argument for a
		/// replaced method in the event of method overloading.
		/// </summary>
		/// <seealso cref="Spring.Objects.Factory.Xml.ObjectDefinitionConstants.ReplacedMethodReplacerNameAttribute"/> 
		public const string ReplacedMethodArgumentTypeElement = "arg-type";

		/// <summary>
		/// Specification of the <see cref="System.Type"/> of an overloaded method
		/// argument as a <see cref="System.String"/>.
		/// </summary>
		/// <remarks>
		/// <p>
		/// For convenience, this may be a substring of the FQN. E.g. all the following would match
		/// <see cref="System.String"/>:
		/// </p>
		/// <p>
		/// <list type="bullet">
		/// <item>
		/// <description>System.String</description>
		/// </item>
		/// <item>
		/// <description>string</description>
		/// </item>
		/// <item>
		/// <description>str</description>
		/// </item>
		/// </list>
		/// </p>
		/// </remarks>
		/// <seealso cref="Spring.Objects.Factory.Xml.ObjectDefinitionConstants.ReplacedMethodArgumentTypeElement"/> 
		public const string ReplacedMethodArgumentTypeMatchAttribute = "match";

		/// <summary>
		/// Check everything.
		/// </summary>
		public static readonly string DependencyCheckAllAttributeValue
			= Enum.GetName(typeof (DependencyCheckingMode), DependencyCheckingMode.All);

		/// <summary>
		/// Just check primitive (string, int, etc) values.
		/// </summary>
		public static readonly string DependencyCheckSimpleAttributeValue
			= Enum.GetName(typeof (DependencyCheckingMode), DependencyCheckingMode.Simple);

		/// <summary>
		/// Check object references.
		/// </summary>
		public static readonly string DependencyCheckObjectsAttributeValue
			= Enum.GetName(typeof (DependencyCheckingMode), DependencyCheckingMode.Objects);

		/// <summary>
		/// Autowire by name.
		/// </summary>
		public static readonly string AutowireByNameValue
			= Enum.GetName(typeof (AutoWiringMode), AutoWiringMode.ByName);

		/// <summary>
		/// Autowire by <see cref="System.Type"/>.
		/// </summary>
		public static readonly string AutowireByTypeValue
			= Enum.GetName(typeof (AutoWiringMode), AutoWiringMode.ByType);

		/// <summary>
		/// Autowiring by constructor.
		/// </summary>
		public static readonly string AutowireConstructorValue
			= Enum.GetName(typeof (AutoWiringMode), AutoWiringMode.Constructor);

		/// <summary>
		/// The autowiring strategy is to be determined by introspection
		/// of the object's <see cref="System.Type"/>.
		/// </summary>
		public static readonly string AutowireAutoDetectValue
			= Enum.GetName(typeof (AutoWiringMode), AutoWiringMode.AutoDetect);

		#region Constructor (s) / Destructor

		// CLOVER:OFF

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.Xml.ObjectDefinitionConstants"/>
		/// class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is a utility class, and as such has no publicly visible
		/// constructors.
		/// </p>
		/// </remarks>
		private ObjectDefinitionConstants()
		{
		}

		// CLOVER:ON

		#endregion
	}
}