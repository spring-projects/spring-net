#if NH_2_1XX
using System;

namespace Spring.Data.NHibernate.Bytecode
{
    public class Product
    {
        public virtual Guid Id { get; set; }
        public virtual string Description { get; set; }
        public virtual decimal Price { get; set; }
    }
}
#endif