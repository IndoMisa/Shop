﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBP_n4j_v1.DomainModel
{
    class StoreWithPriceAndLink
    {
        public Store store { get; set; }
        public float price { get; set; }
        public String link { get; set; }
    }
}
