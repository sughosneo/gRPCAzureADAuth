using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Product.gRPC.API.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public double Price { get; set; }

        public bool isAvailableToPurchase { get; set; }
    }
}
