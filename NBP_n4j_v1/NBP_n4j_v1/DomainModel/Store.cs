using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBP_n4j_v1.DomainModel
{
    public class Store
    {
        public String name { get; set; }
        public String password { get; set; }
        public String website { get; set; }
        public Store() { }
        public Store(String name, String password, String website)
        {
            this.name = name;
            this.password = password;
            this.website = website;
        }
    }
}
