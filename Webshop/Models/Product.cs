using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Models;

namespace Webshop.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(0, 1000000)]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        // För "Utvalda produkter" på startsidan
        public bool IsFeatured { get; set; }

        // Relationer
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
