#region Licence

/*
 * Copyright © 2002-2005 the original author or authors.
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

#region Imports

using System;
using System.Globalization;
using System.Web.UI.WebControls;
using Spring.Web.UI;
using Spring.Web.UI.Controls;

#endregion

namespace SpringAir.Web
{
    /// <summary>
    /// SpringAir master page.
    /// </summary>
    /// <author>Rick Evans</author>
    public class StandardTemplate : MasterPage
    {
        protected ContentPlaceHolder head;
        protected System.Web.UI.WebControls.Button submit;
        protected Spring.Web.UI.Controls.Head Head1;
        protected Spring.Web.UI.Controls.LocalizedImage logoImage;
        protected System.Web.UI.HtmlControls.HtmlForm form;
        protected ContentPlaceHolder body;

        protected LinkButton english;
        protected LinkButton serbianLatin;
        protected LinkButton serbianCyrillic;

        private void SetLanguage(object sender, CommandEventArgs e)
        {
            UserCulture = new CultureInfo((string) e.CommandArgument);
        }


        #region Web Form Designer generated code

        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
        }

        private void InitializeComponent()
        {
            this.english.Command += new CommandEventHandler(this.SetLanguage);
            this.serbianLatin.Command += new CommandEventHandler(this.SetLanguage);
            this.serbianCyrillic.Command += new CommandEventHandler(this.SetLanguage);
        }

        #endregion

    }
}