/*
 * Copyright © 2002-2026 the original author or authors.
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

namespace Spring.Expressions;

/// <summary>
/// Holds the extension methods and extension properties registered against a variables
/// dictionary. A single registry instance is stored in the dictionary under the reserved
/// <see cref="Expression.ReservedVariableNames.ExpressionExtensions"/> key, so registrations
/// are scoped to the dictionary they were made against.
/// </summary>
/// <remarks>
/// Lookups are lock-free: both tables are immutable once published and replaced wholesale
/// (copy-on-write) on registration, so concurrent evaluation never observes a table under
/// mutation. The <see cref="hasInterfaceRegistrations"/> flag is written before the table
/// that contains the interface registration is published, so any reader that sees the new
/// table also performs the interface walk.
/// </remarks>
[Serializable]
internal sealed class ExpressionExtensionRegistry
{
    [Serializable]
    private readonly struct ExtensionKey : IEquatable<ExtensionKey>
    {
        private readonly Type type;
        private readonly string name;

        public ExtensionKey(Type type, string name)
        {
            this.type = type;
            this.name = name;
        }

        public bool Equals(ExtensionKey other)
        {
            return type == other.type && string.Equals(name, other.name, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return obj is ExtensionKey && Equals((ExtensionKey) obj);
        }

        public override int GetHashCode()
        {
            return unchecked((type.GetHashCode() * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(name));
        }
    }

    private static readonly object registrationLock = new object();

    private readonly object syncRoot = new object();

    private volatile Dictionary<ExtensionKey, ExpressionExtension> methods = new Dictionary<ExtensionKey, ExpressionExtension>();
    private volatile Dictionary<ExtensionKey, ExpressionExtension> properties = new Dictionary<ExtensionKey, ExpressionExtension>();
    private volatile bool hasInterfaceRegistrations;

    /// <summary>
    /// Registers an extension in the registry stored in <paramref name="variables"/>,
    /// creating the registry on first use. A later registration for the same type and
    /// name replaces the earlier one.
    /// </summary>
    public static void Register(IDictionary<string, object> variables, ExpressionExtension extension, bool isProperty)
    {
        ExpressionExtensionRegistry registry;
        lock (registrationLock)
        {
            object existing;
            variables.TryGetValue(Expression.ReservedVariableNames.ExpressionExtensions, out existing);
            registry = existing as ExpressionExtensionRegistry;
            if (registry == null)
            {
                registry = new ExpressionExtensionRegistry();
                variables[Expression.ReservedVariableNames.ExpressionExtensions] = registry;
            }
        }

        registry.Add(extension, isProperty);
    }

    /// <summary>
    /// Finds the extension method registered for the runtime type of <paramref name="context"/>
    /// under <paramref name="name"/>, or <c>null</c>.
    /// </summary>
    public static ExpressionExtension FindMethod(IDictionary<string, object> variables, object context, string name)
    {
        return Find(variables, context, name, false);
    }

    /// <summary>
    /// Finds the extension property registered for the runtime type of <paramref name="context"/>
    /// under <paramref name="name"/>, or <c>null</c>.
    /// </summary>
    public static ExpressionExtension FindProperty(IDictionary<string, object> variables, object context, string name)
    {
        return Find(variables, context, name, true);
    }

    private static ExpressionExtension Find(IDictionary<string, object> variables, object context, string name, bool isProperty)
    {
        if (context == null || variables == null || variables.Count == 0)
        {
            return null;
        }

        object value;
        variables.TryGetValue(Expression.ReservedVariableNames.ExpressionExtensions, out value);
        ExpressionExtensionRegistry registry = value as ExpressionExtensionRegistry;
        if (registry == null)
        {
            return null;
        }

        return registry.Find(context.GetType(), name, isProperty);
    }

    private ExpressionExtension Find(Type contextType, string name, bool isProperty)
    {
        Dictionary<ExtensionKey, ExpressionExtension> table = isProperty ? properties : methods;
        if (table.Count == 0)
        {
            return null;
        }

        ExpressionExtension extension;
        for (Type type = contextType; type != null; type = type.BaseType)
        {
            if (table.TryGetValue(new ExtensionKey(type, name), out extension))
            {
                return extension;
            }
        }

        if (hasInterfaceRegistrations)
        {
            Type[] interfaces = contextType.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                if (table.TryGetValue(new ExtensionKey(interfaces[i], name), out extension))
                {
                    return extension;
                }
            }
        }

        return null;
    }

    private void Add(ExpressionExtension extension, bool isProperty)
    {
        lock (syncRoot)
        {
            if (extension.TargetType.IsInterface)
            {
                hasInterfaceRegistrations = true;
            }

            Dictionary<ExtensionKey, ExpressionExtension> copy =
                new Dictionary<ExtensionKey, ExpressionExtension>(isProperty ? properties : methods);
            copy[new ExtensionKey(extension.TargetType, extension.Name)] = extension;

            if (isProperty)
            {
                properties = copy;
            }
            else
            {
                methods = copy;
            }
        }
    }
}
