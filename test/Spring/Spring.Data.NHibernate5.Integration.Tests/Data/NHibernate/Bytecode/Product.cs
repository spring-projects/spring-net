using System;

namespace Spring.Data.NHibernate.Bytecode
{
    public class Product
    {
        private Guid _id;
        private string _description;
        private decimal _price;

        public virtual Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public virtual string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public virtual decimal Price
        {
            get { return _price; }
            set { _price = value; }
        }
    }
}
