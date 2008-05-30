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
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Spring.Validation;
using Spring.Web.UI;
using SpringAir.Domain;
using SpringAir.Service;

#endregion

namespace SpringAir.Web
{
	/// <summary>
	/// Displays a list of suggested flights.
	/// </summary>
	/// <author>Rick Evans</author>
	/// <author>Aleksandar Seovic</author>
	/// <author>Marko Dumic</author>
	/// <version>$Id: SuggestedFlights.aspx.cs,v 1.2 2006/12/07 04:22:00 aseovic Exp $</version>
	public class SuggestedFlights : Page
	{
		#region Fields

		private const string ReservationConfirmed = "reservationConfirmed";
		private const int NoFlightSelected = -1;

		protected FlightSuggestions flights;
		private IBookingAgent bookingAgent;

		protected int outboundFlightIndex;
		protected int returnFlightIndex;

		private IValidator flightsValidator;

		#endregion

		#region Controls

		protected Label caption;
		protected Button bookFlights;

		protected Repeater outboundFlightList;
		protected Repeater returnFlightList;

		protected HtmlInputHidden outboundFlight;
		protected Spring.Web.UI.Controls.Content body;
        protected Spring.Web.UI.Controls.ValidationSummary validationSummary;

		protected HtmlInputHidden returnFlight;

		#endregion

		#region Properties

		public IBookingAgent BookingAgent
		{
			set { this.bookingAgent = value; }
		}

		public IValidator FlightsValidator
		{
			set { flightsValidator = value; }
		}

		#endregion

        #region Model Management and Data Binding Methods

        protected override void InitializeModel()
        {
            flights = (FlightSuggestions) Session[Constants.SuggestedFlightsKey];
            outboundFlightIndex = NoFlightSelected;
            returnFlightIndex = NoFlightSelected;
        }

        protected override void LoadModel(object savedModel)
        {
            IDictionary model = (IDictionary) savedModel;
            flights = (FlightSuggestions) model["flights"];
            outboundFlightIndex = (int) model["outboundFlightIndex"];
            returnFlightIndex = (int) model["returnFlightIndex"];
        }

        protected override object SaveModel()
        {
            IDictionary model = new Hashtable();
            model.Add("flights", flights);
            model.Add("outboundFlightIndex", outboundFlightIndex);
            model.Add("returnFlightIndex", returnFlightIndex);
            return model;
        }

        protected override void InitializeDataBindings()
        {
            BindingManager.AddBinding("outboundFlight.Value", "outboundFlightIndex");
            BindingManager.AddBinding("returnFlight.Value", "returnFlightIndex");
        }

        #endregion

	    #region Page Lifecycle Methods

		protected override void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
		}

		protected override void OnInitializeControls(EventArgs e)
		{
			base.OnInitializeControls(e);

			if (!IsPostBack)
			{
				outboundFlightList.DataSource = flights.OutboundFlights;
				outboundFlightList.DataBind();
				if (flights.HasReturnFlights)
				{
					caption.Text = GetMessage("selectFlights");
					returnFlightList.DataSource = flights.ReturnFlights;
					returnFlightList.DataBind();
				}
				else
				{
					caption.Text = GetMessage("selectFlight");
					returnFlightList.Visible = false;
				}
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			caption.Text = flights.HasReturnFlights
				? GetMessage("selectFlights")
				: GetMessage("selectFlight");
		}

		#endregion

		#region Controller Methods

		private void BookFlights(object sender, EventArgs e)
		{
//			if(Validate(this, flightsValidator)) 
//			{
				FlightCollection flightsToBook = GetFlightsToBook();
				Itinerary itinerary = new Itinerary(flightsToBook);
				// TODO: forward to next logical page and get user details...
				ReservationConfirmation confirmation = bookingAgent.Book(
					new Reservation(new Passenger(1, "Aleksandar", "Seovic"), itinerary));
				Session[Constants.ReservationConfirmationKey] = confirmation;
				SetResult(ReservationConfirmed);
//			} 
//			else 
//			{
//				this.outboundFlightIndex = NoFlightSelected;
//				this.returnFlightIndex = NoFlightSelected;
//			}
		}

		private FlightCollection GetFlightsToBook()
		{
			FlightCollection flightsToBook = new FlightCollection();
			Flight outboundFlight = this.flights.GetOutboundFlight(outboundFlightIndex);
			flightsToBook.Add(outboundFlight);
			if (HasReturnFlight)
			{
				Flight returnFlight = this.flights.GetReturnFlight(returnFlightIndex);
				flightsToBook.Add(returnFlight);
			}
			return flightsToBook;
		}

		private bool HasReturnFlight
		{
			get { return this.returnFlightIndex != NoFlightSelected; }
		}

		#endregion

		#region Web Form Designer generated code

		private void InitializeComponent()
		{
            this.bookFlights.Click += new System.EventHandler(this.BookFlights);

        }

		#endregion
	}
}