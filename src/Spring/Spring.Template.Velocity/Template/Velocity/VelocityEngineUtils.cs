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

using System.Collections;
using Common.Logging;
using NVelocity;
using NVelocity.App;
using NVelocity.Exception;

namespace Spring.Template.Velocity{
    /// <summary>
    /// Generalized Utility class for merging velocity templates into a text writer or return the result as a string
    /// </summary>
    /// <author>Erez Mazor</author>
    public class VelocityEngineUtils
    {

        /// <summary>
        /// Shared logger instance.
        /// </summary>
        protected static readonly ILog log = LogManager.GetLogger(typeof(VelocityEngineUtils));

        /// <summary>
        /// Merge the specified Velocity template with the given model and write
        /// the result to the given Writer.
        /// </summary>
        /// <param name="velocityEngine">VelocityEngine to work with</param>
        /// <param name="templateLocation">the location of template, relative to Velocity's resource loader path</param>
        /// <param name="encoding">encoding the encoding of the template file</param>
        /// <param name="model">the Hashtable that contains model names as keys and model objects</param>
        /// <param name="writer">writer the TextWriter to write the result to</param>
        /// <exception cref="VelocityException">thrown if any exception is thrown by the velocity engine</exception>
        public static void MergeTemplate(
            VelocityEngine velocityEngine, string templateLocation, string encoding, Hashtable model, TextWriter writer) {

            try {
                VelocityContext velocityContext = new VelocityContext(model);
                velocityEngine.MergeTemplate(templateLocation, encoding, velocityContext, writer);
            } catch (VelocityException) {
                throw;
            } catch (Exception ex) {
                throw new VelocityException(ex.ToString(), ex);
            }
        }

        /// <summary>
        /// Merge the specified Velocity template with the given model into a string.
        /// </summary>
        /// <param name="velocityEngine">VelocityEngine to work with</param>
        /// <param name="templateLocation">the location of template, relative to Velocity's resource loader path</param>
        /// <param name="encoding">the encoding string to use for the merge</param>
        /// <param name="model">the Hashtable that contains model names as keys and model objects</param>
        /// <returns>the result as string</returns>
        /// <exception cref="VelocityException">thrown if any exception is thrown by the velocity engine</exception>
        public static string MergeTemplateIntoString(
            VelocityEngine velocityEngine, string templateLocation, string encoding, Hashtable model) {

            StringWriter result = new StringWriter();
            MergeTemplate(velocityEngine, templateLocation, encoding, model, result);
            return result.ToString();
        }
    }
}
