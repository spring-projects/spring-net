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

namespace Spring.Northwind.Domain
{
    public class Product
    {
        protected int _id;
        protected string _productName;
        protected string _quantityPerUnit;
        protected decimal _unitPrice;
        protected short _unitsInStock;
        protected short _unitsOnOrder;
        protected short _reorderLevel;
        protected bool _discontinued;
        //protected Category _category;
        //protected Supplier _supplier;
        protected OrderDetail _orderDetail;
    }
}
