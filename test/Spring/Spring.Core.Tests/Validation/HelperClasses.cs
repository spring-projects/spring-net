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
using System.Collections.Generic;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Validation.Actions;

namespace Spring.Validation
{
    public abstract class BaseTestValidator : BaseSimpleValidator
    {
        private bool _wasCalled;

        public bool WasCalled => _wasCalled;

        public override bool Validate(object validationContext, IDictionary<string, object> contextParams,
            IValidationErrors errors)
        {
            _wasCalled = true;
            return base.Validate(validationContext, contextParams, errors);
        }
    }

    /// <summary>
    /// Helper classes for validation tests.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class TrueValidator : BaseTestValidator
    {
        public TrueValidator()
        {
        }

        /// <summary>
        /// Validates test object.
        /// </summary>
        /// <param name="objectToValidate">Object to validate.</param>
        /// <returns><c>True</c> if specified object is valid, <c>False</c> otherwise.</returns>
        protected override bool Validate(object objectToValidate)
        {
            return true;
        }
    }

    public class FalseValidator : BaseTestValidator
    {
        public FalseValidator()
        {
            Actions.Add(new ErrorMessageAction("error", "errors"));
        }

        /// <summary>
        /// Validates test object.
        /// </summary>
        /// <param name="objectToValidate">Object to validate.</param>
        /// <returns><c>True</c> if specified object is valid, <c>False</c> otherwise.</returns>
        protected override bool Validate(object objectToValidate)
        {
            return false;
        }
    }

    public sealed class MockObjectDefinitionRegistry : IObjectDefinitionRegistry
    {
        private readonly IDictionary<string, IObjectDefinition> objects = new Dictionary<string, IObjectDefinition>();

        public int ObjectDefinitionCount => objects.Count;

        public IReadOnlyList<string> GetObjectDefinitionNames()
        {
            return new List<string>(objects.Keys);
        }

        public IReadOnlyList<string> GetObjectDefinitionNames(bool includeAncestor)
        {
            return new List<string>(objects.Keys);
        }

        public IList<IObjectDefinition> GetObjectDefinitions()
        {
            return new List<IObjectDefinition>(objects.Values);
        }

        public bool ContainsObjectDefinition(string name)
        {
            return objects.ContainsKey(name);
        }

        public IObjectDefinition GetObjectDefinition(string name)
        {
            objects.TryGetValue(name, out var definition);
            return definition;
        }

        public void RegisterObjectDefinition(string name, IObjectDefinition definition)
        {
            objects[name] = definition;
        }

        public IReadOnlyList<string> GetAliases(string name)
        {
            throw new NotImplementedException();
        }

        public void RegisterAlias(string name, string theAlias)
        {
            throw new NotImplementedException();
        }

        public bool IsObjectNameInUse(string objectName)
        {
            return objects[objectName] != null;
        }
    }
}