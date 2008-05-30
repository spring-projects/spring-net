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
using System.Collections;
using System.Web.UI.WebControls;

using Spring.Validation;
using Spring.Web.UI;
using Spring.Web.UI.Controls;

using SpringAir.Data;
using SpringAir.Domain;
using SpringAir.Service;

using Calendar = Spring.Web.UI.Controls.Calendar;
using ValidationSummary = Spring.Web.UI.Controls.ValidationSummary;

#endregion

/// <summary>
/// Handles the 'type in your trip details and we'll see what we can find
/// for you' use case.
/// </summary>
/// <author>Rick Evans</author>
/// <author>Aleksandar Seovic</author>
/// <author>Marko Dumic</author>
/// <version>$Id: TripForm.aspx.cs,v 1.2 2006/12/07 04:22:01 aseovic Exp $</version>
public partial class TripForm : Page
{
    #region Fields

    private const string DisplaySuggestedFlights = "displaySuggestedFlights";

    private IBookingAgent bookingAgent;
    private IAirportDao airportDao;
    private Trip trip;
    private IValidator tripValidator;
    
    #endregion

    #region Properties

    /// <summary>
    /// The service interface that will handle the business logic of
    /// booking trips and suggesting itineraries.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Is injected into this page by the Spring IoC container. You might
    /// want to look at the definition of this page in the ~/Config/Web.xml
    /// file to confirm exactly which implementation of the
    /// <see cref="SpringAir.Service.IBookingAgent"/> is being injected,
    /// and to see how this is wired together.
    /// </p>
    /// </remarks>
    public IBookingAgent BookingAgent
    {
        set { bookingAgent = value; }
    }

    /// <summary>
    /// This DAO object is used to retrieve a list of airports in order
    /// to populate airport drop-down lists.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Is injected into this page by the Spring IoC container. You might
    /// want to look at the definition of this page in the ~/Config/Web.xml
    /// file to confirm exactly which implementation of the
    /// <see cref="IAirportDao"/> is being injected,
    /// and to see how this is wired together.
    /// </p>
    /// </remarks>
    public IAirportDao AirportDao
    {
        set { airportDao = value; }
    }

    /// <summary>
    /// The user's desired <see cref="SpringAir.Domain.Trip"/>.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The properties of this domain object are populated from the
    /// values present in the controls on this page, according to the
    /// data binding definitions.
    /// </p>
    /// </remarks>
    public Trip Trip
    {
        get { return trip; }
        set { trip = value; }
    }

    /// <summary>
    /// Sets the trip validator.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Unsurprisingly, this <see cref="Spring.Validation.IValidator"/>
    /// instance will be used to validate the associated
    /// <see cref="SpringAir.Web.TripForm.Trip"/> on a form submission. 
    /// </p>
    /// </remarks>
    /// <value>The trip validator.</value>
    public IValidator TripValidator
    {
        set { tripValidator = value; }
    }

    #endregion

    #region Model Management and Data Binding Methods

    protected override void InitializeModel()
    {
        trip = new Trip();
        trip.Mode = TripMode.RoundTrip;
        trip.StartingFrom.Date = DateTime.Today;
        trip.ReturningFrom.Date = DateTime.Today.AddDays(1);
    }

    protected override void LoadModel(object savedModel)
    {
        trip = (Trip)savedModel;
    }

    protected override object SaveModel()
    {
        return trip;
    }

    protected override void InitializeDataBindings()
    {
        BindingManager.AddBinding("tripMode.Value", "Trip.Mode");
        BindingManager.AddBinding("leavingFromAirportCode.SelectedValue", "Trip.StartingFrom.AirportCode");
        BindingManager.AddBinding("goingToAirportCode.SelectedValue", "Trip.ReturningFrom.AirportCode");
        BindingManager.AddBinding("leavingFromDate.SelectedDate", "Trip.StartingFrom.Date");
        BindingManager.AddBinding("returningOnDate.SelectedDate", "Trip.ReturningFrom.Date");
    }

    #endregion

    #region Page Lifecycle Methods

    protected override void OnInitializeControls(EventArgs e)
    {
        if (!IsPostBack)
        {
            BindAirportDropdowns();
        }
    }

    protected override void OnUserCultureChanged(EventArgs e)
    {
        BindAirportDropdowns();
    }

    private void BindAirportDropdowns()
    {
        ArrayList airportList = new ArrayList();
        airportList.Add(new Airport(0, string.Empty, string.Empty, "-- " + GetMessage("selectAirport") + " --"));
        airportList.AddRange(airportDao.GetAllAirports());

        leavingFromAirportCode.DataSource = airportList;
        leavingFromAirportCode.DataTextField = "Description";
        leavingFromAirportCode.DataValueField = "Code";
        leavingFromAirportCode.DataBind();

        goingToAirportCode.DataSource = airportList;
        goingToAirportCode.DataTextField = "Description";
        goingToAirportCode.DataValueField = "Code";
        goingToAirportCode.DataBind();
    }

    #endregion

    #region Controller Methods

    protected void SearchForFlights(object sender, EventArgs e)
    {
        if (Validate(trip, tripValidator))
        {
            FlightSuggestions suggestions = this.bookingAgent.SuggestFlights(Trip);
            if (suggestions.HasOutboundFlights)
            {
                Session[Constants.SuggestedFlightsKey] = suggestions;
                SetResult(DisplaySuggestedFlights);
            }
        }
    }

    #endregion
}