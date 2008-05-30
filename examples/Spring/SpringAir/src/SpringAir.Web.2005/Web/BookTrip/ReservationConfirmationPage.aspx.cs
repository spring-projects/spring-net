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
using System.Web.UI.WebControls;

using Spring.Web.UI;
using Spring.Web.UI.Controls;
using SpringAir.Domain;

#endregion

/// <summary>
/// The SpringAir reservation confirmation page.
/// </summary>
/// <author>Aleksandar Seovic</author>
/// <version>$Id: ReservationConfirmationPage.aspx.cs,v 1.2 2006/12/07 04:22:00 aseovic Exp $</version>
public partial class ReservationConfirmationPage : Page
{
    #region Fields

    protected ReservationConfirmation confirmation;

    #endregion

    #region Model Management and Data Binding Methods

    protected override void InitializeModel()
    {
        confirmation = (ReservationConfirmation)Session[Constants.ReservationConfirmationKey];
    }

    protected override void LoadModel(object savedModel)
    {
        confirmation = (ReservationConfirmation)savedModel;
    }

    protected override object SaveModel()
    {
        return confirmation;
    }

    #endregion
    
    #region Page Lifecycle Methods

    protected override void OnInitializeControls(EventArgs e)
    {
        base.OnInitializeControls(e);

        if (!IsPostBack)
        {
            itinerary.DataSource = confirmation.Reservation.Itinerary.Flights;
            itinerary.DataBind();

            Session.Remove(Constants.ReservationConfirmationKey);
            Session.Remove(Constants.SuggestedFlightsKey);
        }
    }

    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);
        confirmationLabel.Text = GetMessage("confirmation", confirmation.ConfirmationNumber);
    }

    #endregion
}