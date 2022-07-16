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

namespace Spring.Web.UI.Controls
{
    /// <summary>
    /// This control should be used instead of standard HTML <c>head</c> tag
    /// in order to render dynamicaly registered head scripts and stylesheets.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class LocalizedImage : System.Web.UI.WebControls.Image
    {
        private string imageName;

        /// <summary>
        /// Tries to determine full URL for the localized image before it's rendered.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.ImageUrl = DetermineLocalizedUrl();
        }

        /// <summary>
        /// Name of the image file.
        /// </summary>
        public string ImageName
        {
            get { return imageName; }
            set { imageName = value; }
        }

        /// <summary>
        /// Gets a reference to the <see cref="T:Spring.Web.UI.Page"/> instance that contains the
        /// server control.
        /// </summary>
        /// <value></value>
        new private Page Page
        {
            get { return base.Page as Page; }
        }

        private string DetermineLocalizedUrl()
        {
            List<string> localeParts = new List<string>(Page.UserCulture.Name.Split('-'));
            while (localeParts.Count > 0 && !FileExists(localeParts))
            {
                localeParts.RemoveAt(localeParts.Count - 1);
            }

            string locale = String.Join("-", localeParts.ToArray());
            return Page.ImagesRoot + (locale.Length > 0 ? "/" + locale : "") + "/" + this.ImageName;
        }

        private bool FileExists(List<string> localeParts)
        {
            string locale = String.Join("-", localeParts.ToArray());
            string url = Page.ImagesRoot + "/" + locale + "/" + this.ImageName;
            return File.Exists(Page.Server.MapPath(url));
        }
    }
}
