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
using System.Web.UI;
using Spring.Util;
using Spring.Web.Support;

namespace Spring.Web.UI.Controls
{
    /// <summary>
    /// This control should be used instead of standard HTML <c>head</c> tag
    /// in order to render dynamicaly registered head scripts and stylesheets.
    /// </summary>
    /// <remarks>
    /// If you need to use ASP.NETs built-in &lt;head&gt; tag, you should nest Spring's <see cref="Head"/> within ASP.NET's:
    /// <example>
    /// &lt;html&gt;
    ///   &lt;head&gt;
    ///     &lt;title&gt;Some Title&lt;/title&gt;
    ///     &lt;spring:head&gt;
    ///         &lt;-- will render styleblocks etc. here --&gt;
    ///     &lt;/spring:head&gt;
    ///   &lt;/head&gt;
    /// &lt;/html&gt;
    /// </example>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class Head : Control
    {
        private string _defaultStyleType = "text/css";

        /// <summary>
        /// Gets or sets the default mimetype for the &lt;style&gt; element's 'type' attribute
        /// </summary>
        /// <remarks>
        /// Defaults to "text/css"
        /// </remarks>
        public string DefaultStyleType
        {
            get { return _defaultStyleType; }
            set { _defaultStyleType = value; }
        }

        /// <summary>
        /// Gets a reference to the <see cref="T:Spring.Web.UI.Page"/> instance that contains the
        /// server control.
        /// </summary>
        /// <value></value>
        private new Page Page
        {
            get { return base.Page as Page; }
        }

        /// <summary>
        /// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to
        /// be rendered on
        /// the client.
        /// </summary>
        /// <param name="writer">The <see langword="HtmlTextWriter"/> object that receives the server control content.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            bool hasIntrinsicHead = (this.Page.Header != null);

            // don't render begin/end element if we are nested within an ASP.NET <head> control
            if (!hasIntrinsicHead)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Head);
            }

            RenderChildren(writer);
            RenderStyleBlocks(writer);
            RenderStyleFiles(writer);
            RenderHeadScripts(writer);

            if (!hasIntrinsicHead)
            {
                writer.RenderEndTag();
            }
        }

        private void RenderStyleBlocks(HtmlTextWriter writer)
        {
            if (Page.Styles.Count > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Type, _defaultStyleType);
                writer.RenderBeginTag(HtmlTextWriterTag.Style);

                foreach (DictionaryEntry style in Page.Styles)
                {
                    writer.WriteLine(style.Key + " { " + style.Value + " }");
                }
                writer.RenderEndTag();
            }
        }

        private void RenderStyleFiles(HtmlTextWriter writer)
        {
            foreach (DictionaryEntry file in Page.StyleFiles)
            {
                writer.AddAttribute("rel", "stylesheet");
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                writer.AddAttribute(HtmlTextWriterAttribute.Href, WebUtils.CreateAbsolutePath(Page.CssRoot, (string)file.Value));

                writer.RenderBeginTag(HtmlTextWriterTag.Link);
                writer.RenderEndTag();
            }
        }

        private void RenderHeadScripts(HtmlTextWriter writer)
        {
            foreach (DictionaryEntry scriptEntry in Page.HeadScripts)
            {
                object script = scriptEntry.Value;
                if (script is ScriptEvent)
                {
                    RenderScriptEvent(writer, script as ScriptEvent);
                }
                else if (script is ScriptFile)
                {
                    RenderScriptFile(writer, script as ScriptFile);
                }
                else if (script is ScriptBlock)
                {
                    RenderScriptBlock(writer, script as ScriptBlock);
                }
            }
        }

        private void RenderScriptBlock(HtmlTextWriter writer, ScriptBlock script)
        {
            RenderCommonScriptAttributes(writer, script);
            writer.RenderBeginTag(HtmlTextWriterTag.Script);
            writer.WriteLine(script.Script);
            writer.RenderEndTag();
        }

        private void RenderScriptFile(HtmlTextWriter writer, ScriptFile script)
        {
            RenderCommonScriptAttributes(writer, script);
            writer.AddAttribute(HtmlTextWriterAttribute.Src, WebUtils.CreateAbsolutePath(Page.ScriptsRoot, script.FileName));

            writer.RenderBeginTag(HtmlTextWriterTag.Script);
            writer.RenderEndTag();
        }

        private void RenderScriptEvent(HtmlTextWriter writer, ScriptEvent script)
        {
            RenderCommonScriptAttributes(writer, script);
            writer.AddAttribute(HtmlTextWriterAttribute.For, script.Element);
            writer.AddAttribute("event", script.EventName);

            writer.RenderBeginTag(HtmlTextWriterTag.Script);
            writer.WriteLine(script.Script);
            writer.RenderEndTag();
        }

        private void RenderCommonScriptAttributes(HtmlTextWriter writer, Script script)
        {
            if (StringUtils.HasLength(script.Language))
            {
                writer.AddAttribute("language", script.Language);
            }
            writer.AddAttribute("type", script.Type.ToString());
        }
    }
}