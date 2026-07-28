using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Domain.Entities
{
    public class Role
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<User> Users { get; set; }
    }
}
