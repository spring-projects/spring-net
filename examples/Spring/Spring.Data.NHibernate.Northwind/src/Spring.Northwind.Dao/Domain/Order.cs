#region License

/*
 * Copyright 2002-2007 the original author or authors.
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

using System;
using System.Collections;

namespace Spring.Northwind.Domain
{
    public class Order
    {
        #region Fields

        protected int id;
        protected DateTime? orderDate;
        protected DateTime? requiredDate;
        protected DateTime? shippedDate;
        protected decimal freight;
        protected string shipName;
        protected string shipAddress;
        protected string shipCity;
        protected string shipRegion;
        protected string shipPostalCode;
        protected string shipCountry;
        protected Customer customer;

        //protected Employee _employee;
        //protected Shipper _shipVia;
        protected IList orderDetails;

        #endregion

        #region Constructor (s)

        public Order()
        {
        }

        public Order(DateTime orderDate, DateTime requiredDate, DateTime shippedDate, decimal freight, string shipName,
                     string shipAddress, string shipCity, string shipRegion, string shipPostalCode, string shipCountry,
                     Customer customer)
        {
            this.orderDate = orderDate;
            this.requiredDate = requiredDate;
            this.shippedDate = shippedDate;
            this.freight = freight;
            this.shipName = shipName;
            this.shipAddress = shipAddress;
            this.shipCity = shipCity;
            this.shipRegion = shipRegion;
            this.shipPostalCode = shipPostalCode;
            this.shipCountry = shipCountry;
            this.customer = customer;
        }

        #endregion


        #region Properties

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public DateTime? OrderDate
        {
            get { return orderDate; }
            set { orderDate = value; }
        }

        public DateTime? RequiredDate
        {
            get { return requiredDate; }
            set { requiredDate = value; }
        }

        public DateTime? ShippedDate
        {
            get { return shippedDate; }
            set { shippedDate = value; }
        }

        public decimal Freight
        {
            get { return freight; }
            set { freight = value; }
        }

        public string ShipName
        {
            get { return shipName; }
            set { shipName = value; }
        }

        public string ShipAddress
        {
            get { return shipAddress; }
            set { shipAddress = value; }
        }

        public string ShipCity
        {
            get { return shipCity; }
            set { shipCity = value; }
        }

        public string ShipRegion
        {
            get { return shipRegion; }
            set { shipRegion = value; }
        }

        public string ShipPostalCode
        {
            get { return shipPostalCode; }
            set { shipPostalCode = value; }
        }

        public string ShipCountry
        {
            get { return shipCountry; }
            set { shipCountry = value; }
        }

        public Customer Customer
        {
            get { return customer; }
            set { customer = value; }
        }


        public IList OrderDetails
        {
            get
            {
                if(orderDetails == null)
                {
                    orderDetails = new ArrayList();
                }
                return orderDetails;
            }
            set { orderDetails = value; }
        }

        #endregion
    }
}