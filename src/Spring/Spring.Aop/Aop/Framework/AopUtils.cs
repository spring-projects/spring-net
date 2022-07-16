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

#region Imports

using System.Reflection;

using Spring.Collections;
using Spring.Util;

#endregion

namespace Spring.Aop.Framework
{
    /// <summary>
    /// Utility methods used by the AOP framework.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Not intended to be used directly by applications.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Aleksandar Seovic (.NET)</author>
    public sealed class AopUtils
    {
        // This is a leaky abstraction as we have hardcoded known IAopProxyFactory implementations.
        private const string COMPOSITION_PROXY_TYPE_NAME = "CompositionAopProxy";

        private const string DECORATOR_PROXY_TYPE_NAME = "DecoratorAopProxy";

        private const string INHERITANCE_PROXY_TYPE_NAME = "InheritanceAopProxy";

        /// <summary>
        /// Is the supplied <paramref name="objectType"/> an AOP proxy?
        /// </summary>
        /// <remarks>
        /// Return whether the given type is either a composition-based or a decorator-based proxy type.
        /// </remarks>
        /// <param name="objectType">The type to be checked.</param>
        /// <returns><see langword="true"/> if the supplied <paramref name="objectType"/> is an AOP proxy type.</returns>
        public static bool IsAopProxyType(Type objectType)
        {
            return IsCompositionAopProxyType(objectType) || IsDecoratorAopProxyType(objectType) || IsInheritanceAopProxyType(objectType);
        }

        /// <summary>
        /// Is the supplied <paramref name="instance"/> an AOP proxy?
        /// </summary>
        /// <remarks>
        /// Return whether the given object is either
        /// a composition-based proxy or a decorator-based proxy.
        /// </remarks>
        /// <param name="instance">The instance to be checked.</param>
        /// <returns>
        /// <see langword="true"/> if the supplied <paramref name="instance"/> is
        /// an AOP proxy.
        /// </returns>
        public static bool IsAopProxy(object instance)
        {
            return IsCompositionAopProxy(instance) || IsDecoratorAopProxy(instance) || IsInheritanceAopProxy(instance);
        }

        /// <summary>
        /// Is the supplied <paramref name="instance"/> a composition-based AOP proxy?
        /// </summary>
        /// <param name="instance">The instance to be checked.</param>
        /// <returns>
        /// <see langword="true"/> if the supplied <paramref name="instance"/> is
        /// an composition-based AOP proxy.
        /// </returns>
        public static bool IsCompositionAopProxy(Object instance)
        {
            return ((instance != null) && IsCompositionAopProxyType(instance.GetType()));
        }

        /// <summary>
        /// Is the supplied <paramref name="objectType"/> a composition based AOP proxy type?
        /// </summary>
        /// <remarks>
        /// Return whether the given type is a composition-based proxy type.
        /// </remarks>
        /// <param name="objectType">The type to be checked.</param>
        /// <returns><see langword="true"/> if the supplied <paramref name="objectType"/> is a composition based AOP proxy type.</returns>
        public static bool IsCompositionAopProxyType(Type objectType)
        {
            return ((objectType != null) && objectType.FullName.StartsWith(COMPOSITION_PROXY_TYPE_NAME));
        }

        /// <summary>
        /// Is the supplied <paramref name="instance"/> a decorator-based AOP proxy?
        /// </summary>
        /// <param name="instance">The instance to be checked.</param>
        /// <returns>
        /// <see langword="true"/> if the supplied <paramref name="instance"/> is
        /// an decorator-based AOP proxy.
        /// </returns>
        public static bool IsDecoratorAopProxy(Object instance)
        {
            return ((instance != null) && IsDecoratorAopProxyType(instance.GetType()));
        }

        /// <summary>
        /// Is the supplied <paramref name="objectType"/> a composition based AOP proxy type?
        /// </summary>
        /// <remarks>
        /// Return whether the given type is a composition-based proxy type.
        /// </remarks>
        /// <param name="objectType">The type to be checked.</param>
        /// <returns><see langword="true"/> if the supplied <paramref name="objectType"/> is a composition based AOP proxy type.</returns>
        public static bool IsDecoratorAopProxyType(Type objectType)
        {
            return ((objectType != null) && objectType.FullName.StartsWith(DECORATOR_PROXY_TYPE_NAME));
        }


        /// <summary>
        /// Is the supplied <paramref name="instance"/> an inheritance based AOP proxy?
        /// </summary>
        /// <param name="instance">The instance to be checked.</param>
        /// <returns>
        /// <see langword="true"/> if the supplied <paramref name="instance"/> is
        /// an inheritacne based AOP proxy.
        /// </returns>
        public static bool IsInheritanceAopProxy(Object instance)
        {
            return instance != null && IsInheritanceAopProxyType(instance.GetType());
        }

        /// <summary>
        /// Is the supplied <paramref name="objectType"/> an inheritance based AOP proxy type?
        /// </summary>
        /// <param name="objectType">The type to be checked.</param>
        /// <returns><see langword="true"/> if the supplied <paramref name="objectType"/> is an inheritance based AOP proxy type.</returns>
        public static bool IsInheritanceAopProxyType(Type objectType)
        {
            return ((objectType != null) && objectType.FullName.StartsWith(INHERITANCE_PROXY_TYPE_NAME));
        }

        /// <summary>
        /// Gets all of the interfaces that the <see cref="System.Type"/> of the
        /// supplied <paramref name="instance"/> implements.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This includes interfaces implemented by any superclasses.
        /// </p>
        /// </remarks>
        /// <param name="instance">
        /// The object to analyse for interfaces.
        /// </param>
        /// <returns>
        /// All of the interfaces that the <see cref="System.Type"/> of the
        /// supplied <paramref name="instance"/> implements; or an empty
        /// array if the supplied <paramref name="instance"/> is
        /// <see langword="null"/>.
        /// </returns>
        public static Type[] GetAllInterfaces(object instance)
        {
            if (instance != null)
            {
                Type type = instance.GetType();
                return GetAllInterfacesFromType(type);
            }
            return Type.EmptyTypes;
        }

        /// <summary>
        /// Gets all of the interfaces that the
        /// supplied <see cref="System.Type"/> implements.
        /// </summary>
        /// <remarks>
        /// This includes interfaces implemented by any superclasses.
        /// </remarks>
        /// <param name="type">
        /// The type to analyse for interfaces.
        /// </param>
        /// <returns>
        /// All of the interfaces that the supplied <see cref="System.Type"/> implements.
        /// </returns>
        public static Type[] GetAllInterfacesFromType(Type type)
        {
            AssertUtils.ArgumentNotNull(type, "type");
            ISet interfaces = new HybridSet();
            do
            {
                Type[] ifcs = type.GetInterfaces();
                foreach (Type ifc in ifcs)
                {
                    interfaces.Add(ifc);
                }
                type = type.BaseType;
            } while (type != null);

            if (interfaces.Count > 0)
            {
                Type[] types = new Type[interfaces.Count];
                interfaces.CopyTo(types, 0);
                return types;
            }
            return Type.EmptyTypes;
        }

        /// <summary>
        /// Can the supplied <paramref name="pointcut"/> apply at all on the
        /// supplied <paramref name="targetType"/>?
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an important test as it can be used to optimize out a
        /// pointcut for a class.
        /// </p>
        /// <p>
        /// Invoking this method with a <paramref name="targetType"/> that is
        /// an interface type will always yield a <see langword="false"/>
        /// return value.
        /// </p>
        /// </remarks>
        /// <param name="pointcut">The pointcut being tested.</param>
        /// <param name="targetType">The class being tested.</param>
        /// <param name="proxyInterfaces">
        /// The interfaces being proxied. If <see langword="null"/>, all
        /// methods on a class may be proxied.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the pointcut can apply on any method.
        /// </returns>
        public static bool CanApply(IPointcut pointcut, Type targetType, Type[] proxyInterfaces)
        {
            return CanApply(pointcut, targetType, proxyInterfaces, false);
        }
        /// <summary>
        /// Can the supplied <paramref name="pointcut"/> apply at all on the
        /// supplied <paramref name="targetType"/>?
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an important test as it can be used to optimize out a
        /// pointcut for a class.
        /// </p>
        /// <p>
        /// Invoking this method with a <paramref name="targetType"/> that is
        /// an interface type will always yield a <see langword="false"/>
        /// return value.
        /// </p>
        /// </remarks>
        /// <param name="pointcut">The pointcut being tested.</param>
        /// <param name="targetType">The class being tested.</param>
        /// <param name="proxyInterfaces">
        /// The interfaces being proxied. If <see langword="null"/>, all
        /// methods on a class may be proxied.
        /// </param>
        /// <param name="hasIntroductions">whether or not the advisor chain for the target object includes any introductions.</param>
        /// <returns>
        /// <see langword="true"/> if the pointcut can apply on any method.
        /// </returns>
        public static bool CanApply(IPointcut pointcut, Type targetType, Type[] proxyInterfaces, bool hasIntroductions)
        {
            if (!pointcut.TypeFilter.Matches(targetType))
            {
                return false;
            }

            // It may apply to the class
            // Check whether it can apply on any method
            // Checks public methods, including inherited methods
            MethodInfo[] methods = targetType.GetMethods();
            for (int i = 0; i < methods.Length; ++i)
            {
                MethodInfo m = methods[i];
                // If we're looking only at interfaces and this method
                // isn't on any of them, skip it
                if (proxyInterfaces != null
                    && !ReflectionUtils.MethodIsOnOneOfTheseInterfaces(m, proxyInterfaces))
                {
                    continue;
                }
                if (pointcut.MethodMatcher.Matches(m, targetType))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Can the supplied <paramref name="advisor"/> apply at all on the
        /// supplied <paramref name="targetType"/>?
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an important test as it can be used to optimize out an
        /// advisor for a class.
        /// </p>
        /// </remarks>
        /// <param name="advisor">The advisor to check.</param>
        /// <param name="targetType">The class being tested.</param>
        /// <param name="proxyInterfaces">
        /// The interfaces being proxied. If <see langword="null"/>, all
        /// methods on a class may be proxied.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the advisor can apply on any method.
        /// </returns>
        public static bool CanApply(IAdvisor advisor, Type targetType, Type[] proxyInterfaces)
        {
            return CanApply(advisor, targetType, proxyInterfaces, false);
        }

        /// <summary>
        /// Can the supplied <paramref name="advisor"/> apply at all on the
        /// supplied <paramref name="targetType"/>?
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an important test as it can be used to optimize out an
        /// advisor for a class.
        /// </p>
        /// </remarks>
        /// <param name="advisor">The advisor to check.</param>
        /// <param name="targetType">The class being tested.</param>
        /// <param name="proxyInterfaces">
        /// The interfaces being proxied. If <see langword="null"/>, all
        /// methods on a class may be proxied.
        /// </param>
        /// <param name="hasIntroductions">whether or not the advisor chain for the target object includes any introductions.</param>
        /// <returns>
        /// <see langword="true"/> if the advisor can apply on any method.
        /// </returns>
        public static bool CanApply(IAdvisor advisor, Type targetType, Type[] proxyInterfaces, bool hasIntroductions)
        {
            if (advisor is IIntroductionAdvisor)
            {
                return ((IIntroductionAdvisor)advisor).TypeFilter.Matches(targetType);
            }
            else if (advisor is IPointcutAdvisor)
            {
                IPointcutAdvisor pca = (IPointcutAdvisor)advisor;
                return CanApply(pca.Pointcut, targetType, proxyInterfaces, hasIntroductions);
            }
            // no pointcut specified so assume it applies...
            return true;
        }

        #region Constructor (s) / Destructor

        // CLOVER:OFF

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="AopUtils"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is a utility class, and as such has no publicly
        /// visible constructors.
        /// </p>
        /// </remarks>
        private AopUtils()
        {
        }

        // CLOVER:ON

        #endregion

        /// <summary>
        /// Gets the type of the target.
        /// </summary>
        /// <param name="candidate">The candidate.</param>
        /// <returns></returns>
        public static Type GetTargetType(object candidate)
        {
            AssertUtils.ArgumentNotNull(candidate, "candidate", "Candidate object must not be null");
            if (candidate is ITargetSource)
            {
                return ((ITargetSource)candidate).TargetType;
            }
            if (candidate is IAdvised)
            {
                return ((IAdvised)candidate).TargetSource.TargetType;
            }
            if (IsDecoratorAopProxy(candidate))
            {
                return candidate.GetType().BaseType;
            }
            return candidate.GetType();
        }

    }
}
