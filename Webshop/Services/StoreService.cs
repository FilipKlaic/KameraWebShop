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
    public class StoreService
    {
        private readonly MyStoreContext _context;
        private List<Product> _cart;

        public StoreService(MyStoreContext context, List<Product> cart)
        {
            _context = context;
            _cart = cart;
        }
        public void ShowShop()
        {
            bool inShop = true;
            while (inShop)
            {
                Console.Clear();
                Console.WriteLine("--- KAMERASHOPPEN ---");
                Console.WriteLine("1. Visa alla kategorier");
                Console.WriteLine("2. Sök efter produkt (Fritext)");
                Console.WriteLine("0. Gå tillbaka till huvudmenyn");
                Console.Write("\nVal: ");

                string choice = Console.ReadLine() ?? "";
                switch (choice)
                {
                    case "1": ListCategories(); break;
                    case "2": SearchProducts(); break;
                    case "0": inShop = false; break;
                }
            }
        }
        private void ListCategories()
        {
            // Vi hämtar kategorier direkt från DB så att "Tillbehör" syns
            var categories = _context.Categories.ToList();

            Console.Clear();
            Console.WriteLine("--- VÄLJ KATEGORI ---");

            for (int i = 0; i < categories.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {categories[i].Name}");
            }

            Console.WriteLine("0. Gå tillbaka");
            Console.Write("\nVal: ");

            if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= categories.Count)
            {
                ShowProductsInCategory(categories[index - 1].Id);
            }
        }
        private void SearchProducts()
        {
            Console.Clear();
            Console.Write("Ange sökord (Fritextsök): "); // Krav: Fritextsöka
            string search = Console.ReadLine() ?? "";

            var results = _context.Products
                .Where(p => p.Name.Contains(search) || p.Description.Contains(search))
                .ToList();

            DisplayProductList(results);
        }
        private void ShowProductsInCategory(int categoryId)
        {
            var products = _context.Products.Where(p => p.CategoryId == categoryId).ToList();
            DisplayProductList(products);
        }
        private void DisplayProductList(List<Product> products)
        {
            Console.Clear();
            if (!products.Any())
            {
                Console.WriteLine("Inga produkter hittades.");
                Console.ReadKey();
                return;
            }

            for (int i = 0; i < products.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {products[i].Name} - {products[i].Price} kr");
            }

            Console.WriteLine("\nVälj en siffra för detaljer/köp, eller 0 för att gå tillbaka.");
            if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= products.Count)
            {
                ProductDetails(products[index - 1]);
            }
        }
        private void ProductDetails(Product product)
        {
            Console.Clear();
            // Krav: Varje produkt ska kunna väljas för mer info
            Console.WriteLine($"--- {product.Name} ---");
            Console.WriteLine($"Beskrivning: {product.Description}");
            Console.WriteLine($"Pris: {product.Price} kr"); // Krav: Visa pris

            Console.WriteLine("\n1. Köp (Lägg i kundkorg)"); // Krav: Val för köp
            Console.WriteLine("0. Gå tillbaka");

            if (Console.ReadLine() == "1")
            {
                _cart.Add(product);
                Console.WriteLine($"{product.Name} tillagd!");
                Console.ReadKey();
            }
        }
    }
}
