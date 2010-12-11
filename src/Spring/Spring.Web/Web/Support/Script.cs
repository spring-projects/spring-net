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

using Spring.Util;

namespace Spring.Web.Support
{
    /// <summary>
    /// Base class that describes client side script block or file.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Classes that extend this class are used by Spring.Web client-side
    /// scripting support.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    internal abstract class Script
    {
        internal static readonly MimeMediaType DefaultType = MimeMediaType.Text.Javascript;

        private string language;
        private MimeMediaType type;

        /// <summary>
        /// Initialize a new Script object of the specified language
        /// </summary>
        /// <param name="language">Script language.</param>
        public Script(string language)
        {
            this.language = language;
        }

        /// <summary>
        /// Initialize a new script object of the specified type
        /// </summary>
        /// <param name="type">a <see cref="MimeMediaType"/></param>
        public Script(MimeMediaType type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Gets or sets script language.
        /// </summary>
        public string Language
        {
            get { return language; }
            set { language = value; }
        }

        /// <summary>
        /// Gets or sets script mime type
        /// </summary>
        public MimeMediaType Type
        {
            get { return type; }
            set { AssertUtils.ArgumentNotNull(value, "Type"); type = value; }
        }
    }

    /// <summary>
    /// Class that describes client side script block.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Script blocks are used to insert script code directly into the page,
    /// without references to an external file.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    internal class ScriptBlock : Script
    {
        private string script;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="language">Script language.</param>
        /// <param name="script">The script text.</param>
        public ScriptBlock(string language, string script) : base(language)
        {
            this.script = script;
        }

        /// <summary>
        /// Initialize a new script block instance.
        /// </summary>
        /// <param name="type">the script language's <see cref="MimeMediaType"/></param>
        /// <param name="script">the script body</param>
        public ScriptBlock(MimeMediaType type, string script) : base(type)
        {
            this.script = script;
        }

        /// <summary>
        /// Gets or sets the script text.
        /// </summary>
        /// <value>The script text.</value>
        public string Script
        {
            get { return script; }
            set { script = value; }
        }
    }

    /// <summary>
    /// Class that describes client side script file.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Script file references script code in the external file.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    internal class ScriptFile : Script
    {
        private string fileName;

        /// <summary>
        /// Initialize a new <see cref="ScriptFile"/> instance.
        /// </summary>
        /// <param name="language">Script language.</param>
        /// <param name="fileName">The name of the script file.</param>
        public ScriptFile(string language, string fileName) : base(language)
        {
            this.fileName = fileName;
        }

        /// <summary>
        /// Initialize a new <see cref="ScriptFile"/> instance.
        /// </summary>
        /// <param name="type">the script language's <see cref="MimeMediaType"/></param>
        /// <param name="fileName">the (virtual) path to the script</param>
        public ScriptFile(MimeMediaType type, string fileName)
            : base(type)
        {
            this.fileName = fileName;
        }

        /// <summary>
        /// Gets or sets the name of the script file.
        /// </summary>
        /// <value>The name of the script file.</value>
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }
    }

    /// <summary>
    /// Class that describes client side script file.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Script file references script code in the external file.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    internal class ScriptEvent : ScriptBlock
    {
        private string element;
        private string eventName;

        /// <summary>
        /// Initialize a new <see cref="ScriptEvent"/> instance.
        /// </summary>
        /// <param name="language">Script language.</param>
        /// <param name="element">Element ID of the event source.</param>
        /// <param name="eventName">Name of the event to handle.</param>
        /// <param name="script">Script text.</param>
        public ScriptEvent(string language, string element, string eventName, string script) : base(language, script)
        {
            this.element = element;
            this.eventName = eventName;
        }

        /// <summary>
        /// Initialize a new <see cref="ScriptEvent"/> instance.
        /// </summary>
        /// <param name="type">the script language's <see cref="MimeMediaType"/></param>
        /// <param name="element">Element ID of the event source.</param>
        /// <param name="eventName">Name of the event to handle.</param>
        /// <param name="script">Script text.</param>
        public ScriptEvent(MimeMediaType type, string element, string eventName, string script)
            : base(type, script)
        {
            this.element = element;
            this.eventName = eventName;
        }

        /// <summary>
        /// Gets or sets the element ID.
        /// </summary>
        /// <value>The element ID.</value>
        public string Element
        {
            get { return element; }
            set { element = value; }
        }

        /// <summary>
        /// Gets or sets the name of the event.
        /// </summary>
        /// <value>The name of the event.</value>
        public string EventName
        {
            get { return eventName; }
            set { eventName = value; }
        }
    }

}