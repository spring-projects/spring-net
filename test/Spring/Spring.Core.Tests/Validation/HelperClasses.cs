#region License

/*
 * Copyright 2002-2004 the original author or authors.
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
using System.Collections;

using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Validation.Actions;

namespace Spring.Validation
{
    /// <summary>
    /// Helper classes for validation tests.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <version>$Id: HelperClasses.cs,v 1.2 2006/04/09 07:24:52 markpollack Exp $</version>
    public class TrueValidator : BaseValidator
    {
        public TrueValidator()
        {}

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

    public class FalseValidator : BaseValidator
    {
        public FalseValidator()
        {
            this.Actions.Add(new ErrorMessageAction("error", "errors"));
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
        private IDictionary objects = new Hashtable();

        public int ObjectDefinitionCount
        {
            get { return this.objects.Count; }
        }

        public string[] GetObjectDefinitionNames()
        {
            return (string[]) new ArrayList(this.objects.Keys).ToArray(typeof(string));
        }

        public IObjectDefinition[] GetObjectDefinitions()
        {
            return (IObjectDefinition[]) new ArrayList(this.objects.Values).ToArray(typeof(IObjectDefinition));
        }

        public bool ContainsObjectDefinition(string name)
        {
            return objects.Contains(name);
        }

        public IObjectDefinition GetObjectDefinition(string name)
        {
            return (IObjectDefinition) objects[name];
        }

        public void RegisterObjectDefinition(string name, IObjectDefinition definition)
        {
            this.objects[name] = definition;
        }

        public string[] GetAliases(string name)
        {
            throw new NotImplementedException();
        }

        public void RegisterAlias(string name, string theAlias)
        {
            throw new NotImplementedException();
        }
    }

}