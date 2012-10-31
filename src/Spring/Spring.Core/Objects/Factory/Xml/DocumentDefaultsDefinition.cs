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

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// Simple class that holds the defaults specified at the <code>&lt;objects&gt;</code> 
    /// level in a standard Spring XML object definition document:
    /// <code>default-lazy-init</code>, <code>default-autowire</code>, etc.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class DocumentDefaultsDefinition
    {
        private string autowire;
        private string dependencyCheck;
        private string lazyInit;
        private string merge;
        private string initMethod;
        private string destroyMethod;
        private string autowireCandidates;

        /// <summary>
        /// Gets or sets the autowire setting for the document that's currently parsed.
        /// </summary>
        /// <value>The autowire.</value>
        public string Autowire
        {
            get { return autowire; }
            set { autowire = value; }
        }

        /// <summary>
        /// Gets or sets the dependency-check setting for the document that's currently parsed
        /// </summary>
        /// <value>The dependency check.</value>
        public string DependencyCheck
        {
            get { return dependencyCheck; }
            set { dependencyCheck = value; }
        }

        /// <summary>
        /// Gets or sets the lazy-init flag for the document that's currently parsed.
        /// </summary>
        /// <value>The lazy init.</value>
        public string LazyInit
        {
            get { return lazyInit; }
            set { lazyInit = value; }
        }

        /// <summary>
        /// Gets or sets the merge setting for the document that's currently parsed.
        /// </summary>
        /// <value>The merge.</value>
        public string Merge
        {
            get { return merge; }
            set { merge = value; }
        }

        /// <summary>
        /// Get or sets the init method for the document that's currently parsed.
        /// </summary>
        /// <value>The init method</value>
        public string InitMethod
        {
            get { return initMethod; }
            set { initMethod = value; }
        }

        /// <summary>
        /// Gets or sets the destroy method for the document that's currently parsed.
        /// </summary>
        /// <value>The destroy methood</value>
        public string DestroyMethod
        {
            get { return destroyMethod; }
            set { destroyMethod = value; }
        }

        /// <summary>
        /// Gets or sets autowire candidates for the document that's currently parsed
        /// </summary>
        /// <value>The Autowire Candidates</value>
        public string AutowireCandidates 
        { 
            get { return autowireCandidates; }
            set { autowireCandidates = value; }
        }
    }
}
