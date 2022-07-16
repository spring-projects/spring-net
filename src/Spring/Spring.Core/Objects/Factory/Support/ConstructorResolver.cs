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

using System.Reflection;
using Common.Logging;
using Spring.Collections;
using Spring.Core;
using Spring.Core.TypeConversion;
using Spring.Core.TypeResolution;
using Spring.Objects.Factory.Config;
using Spring.Util;

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Helper class for resolving constructors and factory methods.
    /// Performs constructor resolution through argument matching.
    /// </summary>
    /// <remarks>
    /// Operates on a <see cref="AbstractObjectFactory"/> and an <see cref="IInstantiationStrategy"/>.
    /// Used by <see cref="AbstractAutowireCapableObjectFactory"/>.
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack</author>
    public class ConstructorResolver
    {
        private readonly ILog log = LogManager.GetLogger(typeof(ConstructorResolver));

        private readonly AbstractObjectFactory objectFactory;

        private readonly IAutowireCapableObjectFactory autowireFactory;

        private readonly IInstantiationStrategy instantiationStrategy;
        private readonly ObjectDefinitionValueResolver valueResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstructorResolver"/> class for the given factory
        /// and instantiation strategy.
        /// </summary>
        /// <param name="objectFactory">The object factory to work with.</param>
        /// <param name="autowireFactory">The object factory as IAutowireCapableObjectFactory.</param>
        /// <param name="instantiationStrategy">The instantiation strategy for creating objects.</param>
        /// <param name="valueResolver">the resolver to resolve property value placeholders if any</param>
        public ConstructorResolver(AbstractObjectFactory objectFactory, IAutowireCapableObjectFactory autowireFactory,
                                   IInstantiationStrategy instantiationStrategy, ObjectDefinitionValueResolver valueResolver)
        {
            this.objectFactory = objectFactory;
            this.autowireFactory = autowireFactory;
            this.instantiationStrategy = instantiationStrategy;
            this.valueResolver = valueResolver;
        }

        /// <summary>
        /// "autowire constructor" (with constructor arguments by type) behavior.
        /// Also applied if explicit constructor argument values are specified,
        /// matching all remaining arguments with objects from the object factory.
        /// </summary>
        /// <para>
        /// This corresponds to constructor injection: In this mode, a Spring
        /// object factory is able to host components that expect constructor-based
        /// dependency resolution.
        /// </para>
        /// <param name="objectName">Name of the object.</param>
        /// <param name="rod">The merged object definition for the object.</param>
        /// <param name="chosenCtors">The chosen chosen candidate constructors (or <code>null</code> if none).</param>
        /// <param name="explicitArgs">The explicit argument values passed in programmatically via the getBean method,
        /// or <code>null</code> if none (-> use constructor argument values from object definition)</param>
        /// <returns>An IObjectWrapper for the new instance</returns>
        public IObjectWrapper AutowireConstructor(string objectName, RootObjectDefinition rod,
                                                  ConstructorInfo[] chosenCtors, object[] explicitArgs)
        {

            ObjectWrapper wrapper = new ObjectWrapper();

            ConstructorInstantiationInfo constructorInstantiationInfo = GetConstructorInstantiationInfo(
                objectName, rod, chosenCtors, explicitArgs);

            wrapper.WrappedInstance = instantiationStrategy.Instantiate(rod, objectName, objectFactory,
                    constructorInstantiationInfo.ConstructorInfo, constructorInstantiationInfo.ArgInstances);

            if (log.IsDebugEnabled)
            {
                log.Debug(
                    $"Object '{objectName}' instantiated via constructor [{constructorInstantiationInfo.ConstructorInfo}].");
            }

            return wrapper;
        }


        /// <summary>
        /// Gets the constructor instantiation info given the object definition.
        /// </summary>
        /// <param name="objectName">Name of the object.</param>
        /// <param name="rod">The RootObjectDefinition</param>
        /// <param name="chosenConstructors">The explicitly chosen ctors.</param>
        /// <param name="explicitArgs">The explicit chose ctor args.</param>
        /// <returns>A ConstructorInstantiationInfo containg the specified constructor in the RootObjectDefinition or
        /// one based on type matching.</returns>
        public ConstructorInstantiationInfo GetConstructorInstantiationInfo(
            string objectName,
            RootObjectDefinition rod,
            ConstructorInfo[] chosenConstructors,
            object[] explicitArgs)
        {
            object[] argsToUse = null;

            if (explicitArgs != null)
            {
                argsToUse = explicitArgs;
            }
            else
            {
                //TODO performance optmization on cached ctors.
            }

            // Need to resolve the constructor.
            bool autowiring = chosenConstructors != null || rod.ResolvedAutowireMode == AutoWiringMode.Constructor;
            ConstructorArgumentValues resolvedValues = null;
            ObjectWrapper wrapper = null;
            ConstructorInfo constructorToUse = null;

            int minNrOfArgs;
            if (explicitArgs != null)
            {
                minNrOfArgs = explicitArgs.Length;
            }
            else
            {
                ConstructorArgumentValues cargs = rod.ConstructorArgumentValues;
                resolvedValues = new ConstructorArgumentValues();
                wrapper = new ObjectWrapper();
                minNrOfArgs = ResolveConstructorArguments(objectName, rod, wrapper, cargs, resolvedValues);
            }
            // Take specified constructors, if any.
            ConstructorInfo[] candidates = chosenConstructors ?? AutowireUtils.GetConstructors(rod, 0);
            AutowireUtils.SortConstructors(candidates);
            int minTypeDiffWeight = int.MaxValue;

            for (int i = 0; i < candidates.Length; i++)
            {
                ConstructorInfo candidate = candidates[i];
                var parameters = candidate.GetParameters();
                if (constructorToUse != null && argsToUse.Length > parameters.Length)
                {
                    // already found greedy constructor that can be satisfied, so
                    // don't look any further, there are only less greedy constructors left...
                    break;
                }
                if (parameters.Length < minNrOfArgs)
                {
                    throw new ObjectCreationException(rod.ResourceDescription, objectName,
                        $"'{minNrOfArgs}' constructor arguments specified but no matching constructor found " +
                        $"in object '{objectName}' (hint: specify argument indexes, names, or " +
                        "types to avoid ambiguities).");
                }

                Type[] paramTypes = ReflectionUtils.GetParameterTypes(parameters);
                ArgumentsHolder args;
                if (resolvedValues != null)
                {
                    // Try to resolve arguments for current constructor

                    //need to check for null as indicator of no ctor arg match instead of using exceptions for flow
                    //control as in the Java implementation
                    args = CreateArgumentArray(
                        objectName,
                        rod,
                        resolvedValues,
                        wrapper,
                        paramTypes,
                        candidate,
                        autowiring,
                        out var unsatisfiedDependencyExceptionData);

                    if (args == null)
                    {
                        if (i == candidates.Length - 1 && constructorToUse == null)
                        {
                            throw new UnsatisfiedDependencyException(rod.ResourceDescription,
                                            objectName,
                                            unsatisfiedDependencyExceptionData.ParameterIndex,
                                            unsatisfiedDependencyExceptionData.ParameterType,
                                            unsatisfiedDependencyExceptionData.ErrorMessage);
                        }
                        // try next constructor...
                        continue;
                    }
                }
                else
                {
                    // Explicit arguments given -> arguments length must match exactly
                    if (parameters.Length != explicitArgs.Length)
                    {
                        continue;
                    }
                    args = ArgumentsHolder.Create(explicitArgs);
                }
                int typeDiffWeight = args.GetTypeDifferenceWeight(paramTypes);
                // Choose this constructor if it represents the closest match.
                if (typeDiffWeight < minTypeDiffWeight)
                {
                    constructorToUse = candidate;
                    argsToUse = args.arguments;
                    minTypeDiffWeight = typeDiffWeight;
                }
            }

            if (constructorToUse == null)
            {
                throw new ObjectCreationException(rod.ResourceDescription, objectName, "Could not resolve matching constructor.");
            }

            return new ConstructorInstantiationInfo(constructorToUse, argsToUse);
        }

        /// <summary>
        /// Instantiate an object instance using a named factory method.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The method may be static, if the <paramref name="definition"/>
        /// parameter specifies a class, rather than a
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/> instance, or an
        /// instance variable on a factory object itself configured using Dependency
        /// Injection.
        /// </p>
        /// <p>
        /// Implementation requires iterating over the static or instance methods
        /// with the name specified in the supplied <paramref name="definition"/>
        /// (the method may be overloaded) and trying to match with the parameters.
        /// We don't have the types attached to constructor args, so trial and error
        /// is the only way to go here.
        /// </p>
        /// </remarks>
        /// <param name="name">
        /// The name associated with the supplied <paramref name="definition"/>.
        /// </param>
        /// <param name="definition">
        /// The definition describing the instance that is to be instantiated.
        /// </param>
        /// <param name="arguments">
        /// Any arguments to the factory method that is to be invoked.
        /// </param>
        /// <returns>
        /// The result of the factory method invocation (the instance).
        /// </returns>
        public virtual IObjectWrapper InstantiateUsingFactoryMethod(string name, RootObjectDefinition definition, object[] arguments)
        {
            ObjectWrapper wrapper = new ObjectWrapper();
            Type factoryClass = null;
            bool isStatic = true;


            ConstructorArgumentValues cargs = definition.ConstructorArgumentValues;
            ConstructorArgumentValues resolvedValues = new ConstructorArgumentValues();
            int expectedArgCount = 0;

            // we don't have arguments passed in programmatically, so we need to resolve the
            // arguments specified in the constructor arguments held in the object definition...
            if (arguments == null || arguments.Length == 0)
            {
                expectedArgCount = cargs.ArgumentCount;
                ResolveConstructorArguments(name, definition, wrapper, cargs, resolvedValues);
            }
            else
            {
                // if we have constructor args, don't need to resolve them...
                expectedArgCount = arguments.Length;
            }


            if (StringUtils.HasText(definition.FactoryObjectName))
            {
                // it's an instance method on the factory object's class...
                factoryClass = objectFactory.GetObject(definition.FactoryObjectName).GetType();
                isStatic = false;
            }
            else
            {
                // it's a static factory method on the object class...
                factoryClass = definition.ObjectType;
            }

            GenericArgumentsHolder genericArgsInfo = new GenericArgumentsHolder(definition.FactoryMethodName);
            IList<MethodInfo> factoryMethodCandidates = FindMethods(genericArgsInfo.GenericMethodName, expectedArgCount, isStatic, factoryClass);

            bool autowiring = (definition.AutowireMode == AutoWiringMode.Constructor);

            // try all matching methods to see if they match the constructor arguments...
            for (int i = 0; i < factoryMethodCandidates.Count; i++)
            {
                MethodInfo factoryMethodCandidate = factoryMethodCandidates[i];
                if (genericArgsInfo.ContainsGenericArguments)
                {
                    string[] unresolvedGenericArgs = genericArgsInfo.GetGenericArguments();
                    if (factoryMethodCandidate.GetGenericArguments().Length != unresolvedGenericArgs.Length)
                        continue;

                    Type[] paramTypes = new Type[unresolvedGenericArgs.Length];
                    for (int j = 0; j < unresolvedGenericArgs.Length; j++)
                    {
                        paramTypes[j] = TypeResolutionUtils.ResolveType(unresolvedGenericArgs[j]);
                    }
                    factoryMethodCandidate = factoryMethodCandidate.MakeGenericMethod(paramTypes);
                }
                if (arguments == null || arguments.Length == 0)
                {
                    Type[] paramTypes = ReflectionUtils.GetParameterTypes(factoryMethodCandidate.GetParameters());
                    // try to create the required arguments...
                    UnsatisfiedDependencyExceptionData unsatisfiedDependencyExceptionData = null;
                    ArgumentsHolder args = CreateArgumentArray(name, definition, resolvedValues, wrapper, paramTypes,
                                                               factoryMethodCandidate, autowiring, out unsatisfiedDependencyExceptionData);
                    if (args == null)
                    {
                        arguments = null;
                        // if we failed to match this method, keep
                        // trying new overloaded factory methods...
                        continue;
                    }
                    else
                    {
                        arguments = args.arguments;
                    }
                }

                // if we get here, we found a usable candidate factory method - check, if arguments match
                //arguments = (arguments.Length == 0 ? null : arguments);
                if (ReflectionUtils.GetMethodByArgumentValues(new MethodInfo[] { factoryMethodCandidate }, arguments) == null)
                {
                    continue;
                }

                object objectInstance = instantiationStrategy.Instantiate(definition, name, objectFactory, factoryMethodCandidate, arguments);
                wrapper.WrappedInstance = objectInstance;

                if (log.IsDebugEnabled)
                {
                    log.Debug($"Object '{name}' instantiated via factory method [{factoryMethodCandidate}].");
                }

                return wrapper;
            }



            // if we get here, we didn't match any method...
            throw new ObjectDefinitionStoreException(
                $"Cannot find matching factory method '{definition.FactoryMethodName} on Type [{factoryClass}].");
        }

        /// <summary>
        /// Create an array of arguments to invoke a constructor or static factory method,
        /// given the resolved constructor arguments values.
        /// </summary>
        /// <remarks>When return value is null the out parameter UnsatisfiedDependencyExceptionData will contain
        /// information for use in throwing a UnsatisfiedDependencyException by the caller.  This avoids using
        /// exceptions for flow control as in the original implementation.</remarks>
        private ArgumentsHolder CreateArgumentArray(
            string objectName,
            RootObjectDefinition rod,
            ConstructorArgumentValues resolvedValues,
            ObjectWrapper wrapper,
            Type[] paramTypes,
            MethodBase methodOrCtorInfo,
            bool autowiring,
            out UnsatisfiedDependencyExceptionData unsatisfiedDependencyExceptionData)
        {
            string GetMethodType()
            {
                return methodOrCtorInfo is ConstructorInfo ? "constructor" : "factory method";
            }

            unsatisfiedDependencyExceptionData = null;
            if (paramTypes.Length == 0)
            {
                return ArgumentsHolder.Empty;
            }

            ArgumentsHolder args = new ArgumentsHolder(paramTypes.Length);
            var usedValueHolders = new HybridSet();
            List<string> autowiredObjectNames = null;

            ParameterInfo[] argTypes =  methodOrCtorInfo.GetParameters();

            for (int paramIndex = 0; paramIndex < paramTypes.Length; paramIndex++)
            {
                Type paramType = paramTypes[paramIndex];

                string parameterName = argTypes[paramIndex].Name;
                // If we couldn't find a direct match and are not supposed to autowire,
                // let's try the next generic, untyped argument value as fallback:
                // it could match after type conversion (for example, String -> int).
                ConstructorArgumentValues.ValueHolder valueHolder = null;
                if (resolvedValues.GetNamedArgumentValue(parameterName) != null)
                {
                    valueHolder = resolvedValues.GetArgumentValue(parameterName, paramType, usedValueHolders);
                }
                else
                {
                    valueHolder = resolvedValues.GetArgumentValue(paramIndex, paramType, usedValueHolders);
                }

                if (valueHolder == null && !autowiring)
                {
                    valueHolder = resolvedValues.GetGenericArgumentValue(null, usedValueHolders);
                }
                if (valueHolder != null)
                {
                    // We found a potential match - let's give it a try.
                    // Do not consider the same value definition multiple times!
                    usedValueHolders.Add(valueHolder);
                    args.rawArguments[paramIndex] = valueHolder.Value;
                    try
                    {
                        object originalValue = valueHolder.Value;
                        object convertedValue = TypeConversionUtils.ConvertValueIfNecessary(paramType, originalValue, null);
                        args.arguments[paramIndex] = convertedValue;

                        //?
                        args.preparedArguments[paramIndex] = convertedValue;
                    }
                    catch (TypeMismatchException ex)
                    {
                        // to avoid using exceptions for flow control, this is not a cost in Java as stack trace is lazily created.
                        string errorMessage = $"Could not convert {GetMethodType()} argument value [{valueHolder.Value}] to required type [{paramType}] : {ex.Message}";
                        unsatisfiedDependencyExceptionData = new UnsatisfiedDependencyExceptionData(paramIndex, paramType, errorMessage);
                        return null;
                    }
                }
                else
                {
                    // No explicit match found: we're either supposed to autowire or
                    // have to fail creating an argument array for the given constructor.
                    if (!autowiring)
                    {
                        string errorMessage = $"Ambiguous {GetMethodType()} argument types - " +
                                              $"Did you specify the correct object references as {GetMethodType()} arguments?";
                        unsatisfiedDependencyExceptionData = new UnsatisfiedDependencyExceptionData(paramIndex, paramType, errorMessage);

                        return null;
                    }
                    try
                    {
                        MethodParameter param = MethodParameter.ForMethodOrConstructor(methodOrCtorInfo, paramIndex);
                        autowiredObjectNames = new List<string>();
                        object autowiredArgument = ResolveAutoWiredArgument(param, objectName, autowiredObjectNames);
                        args.rawArguments[paramIndex] = autowiredArgument;
                        args.arguments[paramIndex] = autowiredArgument;
                        args.preparedArguments[paramIndex] = new AutowiredArgumentMarker();
                    }
                    catch (ObjectsException ex)
                    {
                        unsatisfiedDependencyExceptionData = new UnsatisfiedDependencyExceptionData(paramIndex, paramType, ex.Message);

                        return null;
                    }
                }
            }

            if (log.IsDebugEnabled && autowiredObjectNames != null)
            {
                for (var i = 0; i < autowiredObjectNames.Count; i++)
                {
                    string autowiredObjectName = autowiredObjectNames[i];
                    log.Debug(
                        $"Autowiring by type from object name '{objectName}' via {GetMethodType()} to object named '{autowiredObjectName}'");
                }
            }

            return args;
        }

        private class AutowiredArgumentMarker
        {
        }

        private object ResolveAutoWiredArgument(
            MethodParameter methodParameter,
            string objectName,
            List<string> autowiredObjectNames)
        {
            return autowireFactory.ResolveDependency(
                new DependencyDescriptor(methodParameter, true),
                objectName,
                autowiredObjectNames);
        }

        /// <summary>
        /// Resolves the <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues"/>
        /// of the supplied <paramref name="definition"/>.
        /// </summary>
        /// <param name="objectName">The name of the object that is being resolved by this factory.</param>
        /// <param name="definition">The rod.</param>
        /// <param name="wrapper">The wrapper.</param>
        /// <param name="cargs">The cargs.</param>
        /// <param name="resolvedValues">Where the resolved constructor arguments will be placed.</param>
        /// <returns>
        /// The minimum number of arguments that any constructor for the supplied
        /// <paramref name="definition"/> must have.
        /// </returns>
        /// <remarks>
        /// 	<p>
        /// 'Resolve' can be taken to mean that all of the <paramref name="definition"/>s
        /// constructor arguments is resolved into a concrete object that can be plugged
        /// into one of the <paramref name="definition"/>s constructors. Runtime object
        /// references to other objects in this (or a parent) factory are resolved,
        /// type conversion is performed, etc.
        /// </p>
        /// 	<p>
        /// These resolved values are plugged into the supplied
        /// <paramref name="resolvedValues"/> object, because we wouldn't want to touch
        /// the <paramref name="definition"/>s constructor arguments in case it (or any of
        /// its constructor arguments) is a prototype object definition.
        /// </p>
        /// 	<p>
        /// This method is also used for handling invocations of static factory methods.
        /// </p>
        /// </remarks>
        private int ResolveConstructorArguments(string objectName, RootObjectDefinition definition, ObjectWrapper wrapper,
                                                ConstructorArgumentValues cargs,
                                                ConstructorArgumentValues resolvedValues)
        {
//            ObjectDefinitionValueResolver valueResolver = new ObjectDefinitionValueResolver(objectFactory);
            int minNrOfArgs = cargs.ArgumentCount;

            if (cargs._indexedArgumentValues != null)
            {
                foreach (var entry in cargs._indexedArgumentValues)
                {
                    int index = entry.Key;
                    if (index < 0)
                    {
                        throw new ObjectCreationException(
                            definition.ResourceDescription,
                            objectName,
                            $"Invalid constructor argument index: {index}");
                    }

                    if (index > minNrOfArgs)
                    {
                        minNrOfArgs = index + 1;
                    }

                    ConstructorArgumentValues.ValueHolder valueHolder = entry.Value;
                    string argName = "constructor argument with index " + index;
                    object resolvedValue = valueResolver.ResolveValueIfNecessary(objectName, definition, argName, valueHolder.Value);
                    resolvedValues.AddIndexedArgumentValue(index, resolvedValue,
                        StringUtils.HasText(valueHolder.Type)
                            ? TypeResolutionUtils.ResolveType(valueHolder.Type).AssemblyQualifiedName
                            : null);
                }
            }

            if (definition.ConstructorArgumentValues._genericArgumentValues != null)
            {
                const string argName = "constructor argument";
                for (var i = 0; i < definition.ConstructorArgumentValues._genericArgumentValues.Count; i++)
                {
                    var valueHolder = definition.ConstructorArgumentValues._genericArgumentValues[i];
                    object resolvedValue = valueResolver.ResolveValueIfNecessary(objectName, definition, argName, valueHolder.Value);

                    resolvedValues.AddGenericArgumentValue(
                        resolvedValue,
                        StringUtils.HasText(valueHolder.Type)
                            ? TypeResolutionUtils.ResolveType(valueHolder.Type).AssemblyQualifiedName
                            : null);
                }
            }

            if (definition.ConstructorArgumentValues._namedArgumentValues != null)
            {
                foreach (var entry in definition.ConstructorArgumentValues._namedArgumentValues)
                {
                    string argumentName = entry.Key;
                    string syntheticArgumentName = "constructor argument with name " + argumentName;
                    ConstructorArgumentValues.ValueHolder valueHolder = entry.Value;
                    object resolvedValue = valueResolver.ResolveValueIfNecessary(objectName, definition, syntheticArgumentName, valueHolder.Value);
                    resolvedValues.AddNamedArgumentValue(argumentName, resolvedValue);
                }
            }

            return minNrOfArgs;
        }

        /// <summary>
        /// Returns an array of all of those
        /// <see cref="System.Reflection.MethodInfo">methods</see> exposed on the
        /// <paramref name="searchType"/> that match the supplied criteria.
        /// </summary>
        /// <param name="methodName">
        /// Methods that have this name (can be in the form of a regular expression).
        /// </param>
        /// <param name="expectedArgumentCount">
        /// Methods that have exactly this many arguments.
        /// </param>
        /// <param name="isStatic">
        /// Methods that are static / instance.
        /// </param>
        /// <param name="searchType">
        /// The <see cref="System.Type"/> on which the methods (if any) are to be found.
        /// </param>
        /// <returns>
        /// An array of all of those
        /// <see cref="System.Reflection.MethodInfo">methods</see> exposed on the
        /// <paramref name="searchType"/> that match the supplied criteria.
        /// </returns>
        private static IList<MethodInfo> FindMethods(string methodName, int expectedArgumentCount, bool isStatic, Type searchType)
        {
            ComposedCriteria methodCriteria = new ComposedCriteria();
            methodCriteria.Add(new MethodNameMatchCriteria(methodName));
            methodCriteria.Add(new MethodParametersCountCriteria(expectedArgumentCount));
            BindingFlags methodFlags = BindingFlags.Public | BindingFlags.IgnoreCase | (isStatic ? BindingFlags.Static : BindingFlags.Instance);
            MemberInfo[] methods = searchType.FindMembers(MemberTypes.Method, methodFlags, new CriteriaMemberFilter().FilterMemberByCriteria, methodCriteria);
            return methods.Cast<MethodInfo>().ToArray();
        }

        private class ArgumentsHolder
        {
            internal static readonly ArgumentsHolder Empty = new ArgumentsHolder(0);

            public readonly object[] rawArguments;
            public readonly object[] arguments;
            public readonly object[] preparedArguments;

            public ArgumentsHolder(int size)
            {
                rawArguments = size == 0 ? ObjectUtils.EmptyObjects : new object[size];
                arguments = size == 0 ? ObjectUtils.EmptyObjects : new object[size];
                preparedArguments = size == 0 ? ObjectUtils.EmptyObjects : new object[size];
            }

            private ArgumentsHolder(object[] args)
            {
                rawArguments = args;
                arguments = args;
                preparedArguments = args;
            }

            public static ArgumentsHolder Create(object[] args)
            {
                if (args.Length == 0)
                {
                    return Empty;
                }
                return new ArgumentsHolder(args);
            }

            public static ArgumentsHolder Create(int size)
            {
                if (size == 0)
                {
                    return Empty;
                }
                return new ArgumentsHolder(size);
            }

            public int GetTypeDifferenceWeight(Type[] paramTypes)
            {
                // If valid arguments found, determine type difference weight.
                // Try type difference weight on both the converted arguments and
                // the raw arguments. If the raw weight is better, use it.
                // Decrease raw weight by 1024 to prefer it over equal converted weight.
                int typeDiffWeight = AutowireUtils.GetTypeDifferenceWeight(paramTypes, arguments);
                int rawTypeDiffWeight = AutowireUtils.GetTypeDifferenceWeight(paramTypes, rawArguments) - 1024;
                return (rawTypeDiffWeight < typeDiffWeight ? rawTypeDiffWeight : typeDiffWeight);
            }
        }
    }
}

