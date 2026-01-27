using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Webshop.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Relation: En kategori har många produkter
        public List<Product> Products { get; set; } = new();
    }
}