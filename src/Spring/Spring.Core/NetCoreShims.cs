using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace System
{
#if NETCORE
    internal static class TypeExtensions
    {
        public static Reflection.TypeInfo GetType(this object obj)
        {
            return Reflection.IntrospectionExtensions.GetTypeInfo(obj.GetType());
        }
    }
#endif


#if !BINARY_SERIALIZATION
    internal class SerializableAttribute : Attribute
    {
    }

    internal class NonSerializedAttribute : Attribute
    {
    }
#endif

#if NETCORE
    public class ApplicationException : Exception
    {
        public ApplicationException()
        {
        }

        public ApplicationException(string message) : base(message)
        {
        }

        public ApplicationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    internal class SystemException : Exception
    {
        public SystemException()
        {
        }

        public SystemException(string message) : base(message)
        {
        }

        public SystemException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public interface ICloneable
    {
        object Clone();
    }

    internal interface ISerializable
    {
    }

    internal interface IDeserializationCallback
    {
    }

    namespace Configuration.Internal
    {
        public interface IConfigErrorInfo
        {
        }

        public interface IInternalConfigSystem
        {
        }
    }

    namespace Configuration
    {
        /// <summary>Handles the access to certain configuration sections.</summary>
        /// <filterpriority>2</filterpriority>
        public interface IConfigurationSectionHandler
        {
            /// <summary>Creates a configuration section handler.</summary>
            /// <returns>The created section handler object.</returns>
            /// <param name="parent">Parent object.</param>
            /// <param name="configContext">Configuration context object.</param>
            /// <param name="section">Section XML node.</param>
            /// <filterpriority>2</filterpriority>
            object Create(object parent, object configContext, XmlNode section);
        }

        internal class ConfigurationErrorsException : Exception
        {
            public ConfigurationErrorsException()
            {
            }

            public ConfigurationErrorsException(string message) : base(message)
            {
            }

            public ConfigurationErrorsException(string message, Exception innerException)
                : base(message, innerException)
            {
            }

            public ConfigurationErrorsException(string message, Exception innerException, XmlNode node)
                : this(message, innerException)
            {
            }

            public ConfigurationErrorsException(string message, Exception innerException, string fileName, int line)
                : this(message, innerException)
            {
            }
        }
    }
#endif

#if !REMOTING
    namespace Runtime.Remoting
    {
        internal static class RemotingServices
        {
            [CompilerServices.MethodImpl(CompilerServices.MethodImplOptions.AggressiveInlining)]
            internal static bool IsTransparentProxy(object o)
            {
                return false;
            }
        }
    }
#endif
}