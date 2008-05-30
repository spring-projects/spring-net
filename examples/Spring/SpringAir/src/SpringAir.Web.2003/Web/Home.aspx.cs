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

using Common.Logging;
using System;
using Spring.Web.UI;
using Spring.Web.UI.Controls;

#endregion

namespace SpringAir.Web
{
	/// <summary>
	/// The SpringAir home page.
	/// </summary>
	/// <author>Rick Evans</author>
	/// <version>$Id: Home.aspx.cs,v 1.2 2006/11/15 20:30:52 aseovic Exp $</version>
	public class Home : Page
	{
		protected Content body;
		private static readonly ILog logger = LogManager.GetLogger(typeof (Home));

		private void Page_Load(object sender, EventArgs e)
		{
			if (logger.IsDebugEnabled)
			{
				logger.Debug(string.Format(this.CultureResolver.ResolveCulture(),
				                           "The [{0}] page is being loaded...", GetType().Name));
			}
		}

		#region Web Form Designer generated code

		protected override void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
		}

		private void InitializeComponent()
		{
			this.Load += new EventHandler(this.Page_Load);
		}

		#endregion
	}
}