﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBP_n4j_v1.DomainModel
{
    class ProductWithStoresAndPrices
    {
        public Product product { get; set; }
        public List<StoreWithPriceAndLink> storesWithPricesAndLinks { get; set; }
    }
}
