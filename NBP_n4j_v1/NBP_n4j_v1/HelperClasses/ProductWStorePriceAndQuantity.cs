using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBP_n4j_v1.DomainModel;
namespace NBP_n4j_v1.HelperClasses
{
    class ProductWStorePriceAndQuantity
    {
        public Product product { get; set; }
        public Store store { get; set; }
        public float price { get; set; }
        public int quantity { get; set; }
    }
}
