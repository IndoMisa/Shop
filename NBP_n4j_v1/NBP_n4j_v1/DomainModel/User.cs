using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBP_n4j_v1.DomainModel
{
    public class User
    {
        public String name { get; set; }
        public String email { get; set; }
        public String password { get; set; }
        public User() { }
        public User(String name, String email, String password)
        {
            this.name = name;
            this.email = email;
            this.password = password;
        }
    }
}
