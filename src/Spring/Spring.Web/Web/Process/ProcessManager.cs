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

using System.Collections;

namespace Spring.Web.Process
{
    /// <summary>
    /// Singleton that keeps track of all active process instances.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class ProcessManager
    {
        private static readonly ProcessManager instance = new ProcessManager();

        private IDictionary processInstances = new Hashtable();

        /// <summary>
        /// Creates singleton instance.
        /// </summary>
        private ProcessManager()
        {}

        /// <summary>
        /// Registers process instance.
        /// </summary>
        /// <param name="process">Process instance to register.</param>
        public static void RegisterProcess(IProcess process)
        {
            instance.processInstances.Add(process.Id, process);
        }

        /// <summary>
        /// Returns process with the specified ID.
        /// </summary>
        /// <param name="id">Process ID to use for lookup.</param>
        /// <returns>Process with the specified ID, or <c>null</c> if process with that ID is not registered.</returns>
        public static IProcess GetProcess(string id)
        {
            return (IProcess) instance.processInstances[id];
        }

        /// <summary>
        /// Unregisters process with the specified ID.
        /// </summary>
        /// <param name="id">ID of the process to unregister.</param>
        public static void UnregisterProcess(string id)
        {
            instance.processInstances.Remove(id);
        }
    }
}