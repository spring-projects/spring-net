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

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Implementation of <see cref="IVariableSource"/> that
    /// resolves variable name against command line arguments.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class CommandLineArgsVariableSource : IVariableSource
    {
        private const string DEFAULT_ARG_PREFIX = "/";
        private const string DEFAULT_VALUE_SEPARATOR = ":";

        private string argumentPrefix = DEFAULT_ARG_PREFIX;
        private string valueSeparator = DEFAULT_VALUE_SEPARATOR;

        private string[] commandLineArgs;
        protected IDictionary<string, string> arguments;

        private object objectMonitor = new object();

        /// <summary>
        /// Default constructor. 
        /// Initializes command line arguments from the environment.
        /// </summary>
        public CommandLineArgsVariableSource()
        {
            this.commandLineArgs = Environment.GetCommandLineArgs();
        }

        /// <summary>
        /// Constructor that allows arguments to be passed externally.
        /// Useful for testing.
        /// </summary>
        public CommandLineArgsVariableSource(string[] commandLineArgs)
        {
            this.commandLineArgs = commandLineArgs;
        }

        /// <summary>
        /// Gets or sets a prefix that should be used to 
        /// identify arguments to extract values from.
        /// </summary>
        /// <value>
        /// A prefix that should be used to identify arguments 
        /// to extract values from. Defaults to slash ("/").
        /// </value>
        public string ArgumentPrefix
        {
            get { return argumentPrefix; }
            set { argumentPrefix = value; }
        }

        /// <summary>
        /// Gets or sets a character that should be used to
        /// separate argument name from its value.
        /// </summary>
        /// <value>
        /// A character that should be used to separate argument 
        /// name from its value. Defaults to colon (":").
        /// </value>
        public string ValueSeparator
        {
            get { return valueSeparator; }
            set { valueSeparator = value; }
        }

        /// <summary>
        /// Before requesting a variable resolution, a client should
        /// ask, whether the source can resolve a particular variable name.
        /// </summary>
        /// <param name="name">the name of the variable to resolve</param>
        /// <returns><c>true</c> if the variable can be resolved, <c>false</c> otherwise</returns>
        public bool CanResolveVariable(string name)
        {
            lock (objectMonitor)
            {
                if (arguments == null)
                {
                    InitArguments();
                }
                return arguments.ContainsKey(name);
            }
        }

        /// <summary>
        /// Resolves variable value for the specified variable name.
        /// </summary>
        /// <param name="name">
        /// The name of the variable to resolve.
        /// </param>
        /// <returns>
        /// The variable value if able to resolve, <c>null</c> otherwise.
        /// </returns>
        public string ResolveVariable(string name)
        {
            lock (objectMonitor)
            {
                if (arguments == null)
                {
                    InitArguments();
                }
                string retValue;
                arguments.TryGetValue(name, out retValue);
                return retValue;
            }
        }

        /// <summary>
        /// Initializes command line arguments dictionary.
        /// </summary>
        protected virtual void InitArguments()
        {
            this.arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string arg in commandLineArgs)
            {
                int separatorIndex = arg.IndexOf(valueSeparator);
                if (arg.StartsWith(argumentPrefix) && separatorIndex > argumentPrefix.Length)
                {
                    string argName = arg.Substring(argumentPrefix.Length, separatorIndex - argumentPrefix.Length);
                    string argValue = arg.Substring(separatorIndex + valueSeparator.Length);
                    this.arguments[argName] = argValue;
                }
            }
        }
    }
}
