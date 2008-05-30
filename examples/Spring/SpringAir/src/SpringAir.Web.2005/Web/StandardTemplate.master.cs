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

#endregion

/// <summary>
/// SpringAir2 master page.
/// </summary>
/// <author>Aleksandar Seovic</author>
/// <version>$Id: StandardTemplate.master.cs,v 1.2 2007/05/28 19:07:12 markpollack Exp $</version>
public partial class StandardTemplate : MasterPage
{
    private static bool lookupInitialized = false;
    private static bool needsTranslaction = false;

    private void SetLanguage(object sender, CommandEventArgs e)
    {
        string cultureString = e.CommandArgument as string;
        if (cultureString != null)
        {
            UserCulture = new CultureInfo(TranslateCultureIfNecessary(cultureString));
        }
    }

    /// <summary>
    /// Translates the culture if necessary due to changes in Serbian culture for recent
    /// .NET 2.0 releases.
    /// </summary>
    /// <remarks>If 'sr-Latn-CS' is a valid culture, then translate the following;
    /// sr-SP-Latn->sr-Latn-CS and sr-SP-Cyrl->sr-Cyrl-CS.  See 
    /// http://blogs.msdn.com/kierans/archive/2006/08/02/687267.aspx
    /// and http://blogs.msdn.com/shawnste/archive/2006/11/14/problems-compiling-resources-in-net-2-0-apps-after-updates.aspx 
    /// for additional information.</remarks>
    /// <param name="cultureString">The culture string.</param>
    /// <returns>Translated culture string if necessary.</returns>
    private string TranslateCultureIfNecessary(string cultureString)
    {
        if (!lookupInitialized)
        {
            lock(typeof(StandardTemplate))
            {
                lookupInitialized = true;
                //TODO maybe check .NET 2.0 build # instead.
                foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
                {
                    if (ci.Name.Equals("sr-Latn-CS"))
                    {
                        needsTranslaction = true;
                        break;
                    }
                }
            }
        }

        if (needsTranslaction)
        {
            if (cultureString.Equals("sr-SP-Latn"))
            {
                return "sr-Latn-CS";
            }
            else if (cultureString.Equals("sr-SP-Cyrl"))
            {
                return "sr-Cyrl-CS";
            }
        }

        return cultureString;
        

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