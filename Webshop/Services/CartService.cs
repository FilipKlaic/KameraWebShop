using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Webshop.Data;
using Webshop.Models;

namespace Webshop.Services
{
    public class CartService
    {
        private List<Product> _cart;
        private readonly MyStoreContext _context;

        public CartService(List<Product> cart, MyStoreContext context)
        {
            _cart = cart;
            _context = context;
        }

        public void ShowCart()
        {
            bool inCart = true;
            while (inCart)
            {
                Console.Clear();
                Console.WriteLine("--- DIN VARUKORG ---");

                if (!_cart.Any())
                {
                    Console.WriteLine("Varukorgen är tom.");
                    Console.WriteLine("\nTryck på valfri tangent för att gå tillbaka...");
                    Console.ReadKey();
                    return;
                }

                // Gruppera produkter för att visa antal snyggt
                var groupedCart = _cart.GroupBy(p => p.Id).ToList();
                decimal totalSum = 0;

                for (int i = 0; i < groupedCart.Count; i++)
                {
                    var item = groupedCart[i].First();
                    int count = groupedCart[i].Count();
                    decimal lineTotal = item.Price * count;
                    totalSum += lineTotal;

                    Console.WriteLine($"{i + 1}. {item.Name} | {count} st | {item.Price} kr/st | Totalt: {lineTotal} kr");
                }

                Console.WriteLine($"\nTOTALTSUMMA: {totalSum} kr"); //
                Console.WriteLine("\n1. Ta bort en produkt");
                Console.WriteLine("2. Gå till kassan (Betala)");
                Console.WriteLine("0. Gå tillbaka");
                Console.Write("\nVal: ");

                string choice = Console.ReadLine() ?? "";
                if (choice == "1") RemoveProduct(groupedCart);
                else if (choice == "2") Checkout(totalSum); // Vi bygger Checkout i nästa steg!
                else if (choice == "0") inCart = false;
            }
        }

        private void RemoveProduct(List<IGrouping<int, Product>> groupedCart)
        {
            Console.Write("Vilken produkt vill du ta bort? (Ange siffra): ");
            if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= groupedCart.Count)
            {
                var productIdToRemove = groupedCart[index - 1].Key;
                // Vi tar bort EN av produkterna från den globala listan
                var itemToRemove = _cart.First(p => p.Id == productIdToRemove);
                _cart.Remove(itemToRemove);

                Console.WriteLine("Produkten har tagits bort.");
                Console.ReadKey();
            }
        }

        private void Checkout(decimal productSum)
        {
            Console.Clear();
            Console.WriteLine("--- KASSA ---");

            // 1. FRAKTALTERNATIV
            Console.WriteLine("Välj fraktsätt:");
            Console.WriteLine("1. Standard (49 kr)");
            Console.WriteLine("2. Express (149 kr)");
            string shippingChoice = Console.ReadLine() ?? "1";
            decimal shippingPrice = shippingChoice == "2" ? 149 : 49;

            // 2. KUNDUPPGIFTER
            Console.Write("\nAnge ditt namn: ");
            string name = Console.ReadLine() ?? "Okänd";
            Console.Write("Ange din stad: ");
            string city = Console.ReadLine() ?? "Okänd";
            Console.Write("Ange din adress: ");
            string address = Console.ReadLine() ?? "Okänd";

            // 3. SAMMANSTÄLLNING & MOMS
            decimal totalWithShipping = productSum + shippingPrice;
            decimal vat = totalWithShipping * 0.20m; // Beräknar 25% moms av priset (eller 20% av totalen)

            Console.Clear();
            Console.WriteLine("--- DIN BESTÄLLNING ---");
            Console.WriteLine($"Kund: {name}");
            Console.WriteLine($"Adress: {address}, {city}");
            Console.WriteLine(new string('-', 20));
            Console.WriteLine($"Produkter: {productSum} kr");
            Console.WriteLine($"Frakt: {shippingPrice} kr");
            Console.WriteLine($"Moms (ingår): {vat:F2} kr");
            Console.WriteLine($"TOTALT ATT BETALA: {totalWithShipping} kr");

            Console.WriteLine("\nTryck på ENTER för att genomföra köpet...");
            Console.ReadLine();

            // 4. SPARA TILL DATABASEN
            SaveOrderToDb(name, city, address, totalWithShipping, shippingChoice == "2" ? "Express" : "Standard");

            Console.WriteLine("\nTack för ditt köp! En orderbekräftelse har skapats.");
            _cart.Clear(); // Tömmer varukorgen efter köp
            Console.ReadKey();
        }

        private void SaveOrderToDb(string name, string city, string address, decimal total, string shippingMethod)
        {
            // Hitta eller skapa kunden
            var customer = _context.Customers.FirstOrDefault(c => c.Name == name)
                           ?? new Customer { Name = name, City = city, StreetAddress = address };

            // Skapa själva ordern
            var order = new Order
            {
                Customer = customer,
                OrderDate = DateTime.Now,
                TotalPrice = total,
                ShippingMethod = shippingMethod,
                PaymentMethod = "Kort"
            };

            // Skapa rader för varje produkt och uppdatera lager
            var groupedItems = _cart.GroupBy(p => p.Id);
            foreach (var group in groupedItems)
            {
                var product = _context.Products.Find(group.Key);
                int quantity = group.Count();

                if (product != null)
                {
                    order.OrderRows.Add(new OrderRow
                    {
                        Product = product,
                        Quantity = quantity,
                        PriceAtPurchase = product.Price
                    });

                    // Minska lagersaldot
                    product.StockQuantity -= quantity;
                }
            }

            _context.Orders.Add(order);
            _context.SaveChanges(); // Skriver allt till SQL i en enda transaktion
        }
    }
}
