using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBP_n4j_v1.DomainModel
{
    class Company
    {
        public String name { get; set; }
        public Company() { }
        public Company(String name)
        {
            this.name = name;
        }
    }
}
