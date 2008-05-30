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

namespace Spring.Northwind.Domain
{
    public class OrderDetail
    {
        protected double unitPrice;
        protected int quantity;
        protected double discount;
        protected Order order;
        protected int productId;
        //protected Product product;

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            OrderDetail orderDetail = obj as OrderDetail;
            if (orderDetail == null) return false;
            return Equals(order.Id, orderDetail.order.Id) && Equals(productId, orderDetail.productId);
        }

        public override int GetHashCode()
        {
            return order.Id.GetHashCode() + 29*productId.GetHashCode();
        }

        public double UnitPrice
        {
            get { return this.unitPrice; }
            set { this.unitPrice = value; }
        }

        public int Quantity
        {
            get { return this.quantity; }
            set { this.quantity = value; }
        }

        public double Discount
        {
            get { return this.discount; }
            set { this.discount = value; }
        }

        public Order Order
        {
            get { return this.order; }
            set { this.order = value; }
        }

        public int ProductId
        {
            get { return this.productId; }
            set { this.productId = value; }
        }
    }
}
