using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalPrice { get; set; }
        public string ShippingMethod { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public List<OrderRow> OrderRows { get; set; } = new();
    }

    public class OrderRow
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; } 
    }
}
