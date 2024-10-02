using System;
using System.Collections.Generic;

namespace CMCCyberSecurity.Models
{
    public partial class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; } = null!;
    }
}
