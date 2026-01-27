using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Models;
using Microsoft.EntityFrameworkCore;
using Dapper;
using Microsoft.Data.SqlClient;
using Webshop.Data;

namespace Webshop.Services
{
    public class AdminService
    {
        private readonly MyStoreContext _context;
        private readonly string _connectionString;

        public AdminService(MyStoreContext context)
        {
            _context = context;
            _connectionString = context.Database.GetDbConnection().ConnectionString;
        }

        public void ShowAdminMenu()
        {
            bool inAdmin = true;
            while (inAdmin)
            {
                Console.Clear();
                Console.WriteLine("--- ADMIN-PANEL ---");
                Console.WriteLine("1. Hantera Produkter (Lägg till/Ta bort/Ändra)");
                Console.WriteLine("2. Hantera Kategorier");
                Console.WriteLine("3. Kundlista & Orderhistorik");
                Console.WriteLine("4. Se Statistik (SQL/Dapper Queries)");
                Console.WriteLine("0. Gå tillbaka");
                Console.Write("\nVal: ");

                switch (Console.ReadLine())
                {
                    case "1": ManageProducts(); break;
                    case "2": ManageCategories(); break;
                    case "3": ViewCustomers(); break;
                    case "4": ShowStatistics(); break;
                    case "0": inAdmin = false; break;
                }
            }
        }

        private void ManageProducts()
        {
            Console.Clear();
            Console.WriteLine("--- HANTERA PRODUKTER ---");
            Console.WriteLine("1. Lägg till ny produkt");
            Console.WriteLine("2. Uppdatera lagersaldo");
            Console.WriteLine("0. Gå tillbaka");
            Console.Write("\nVal: ");

            string choice = Console.ReadLine() ?? "";
            if (choice == "1")
            {
                Console.Clear();
                Console.WriteLine("--- LÄGG TILL PRODUKT ---");

                // 1. Namn och beskrivning
                Console.Write("Namn: ");
                string name = Console.ReadLine() ?? "";

                Console.Write("Beskrivning: ");
                string desc = Console.ReadLine() ?? "";

                // 2. Pris
                Console.Write("Pris: ");
                decimal price = decimal.Parse(Console.ReadLine() ?? "0");

                // 3. Lagersaldo
                Console.Write("Lagersaldo: ");
                int stock = int.Parse(Console.ReadLine() ?? "0");

                // 4. Kategori
                var categories = _context.Categories.ToList();
                Console.WriteLine("\nVälj kategori:");
                for (int i = 0; i < categories.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {categories[i].Name}");
                }
                int catIndex = int.Parse(Console.ReadLine() ?? "1") - 1;

                // Skapa och spara produkten
                var newProduct = new Product
                {
                    Name = name,
                    Description = desc,
                    Price = price,
                    StockQuantity = stock,
                    CategoryId = categories[catIndex].Id,
                    IsFeatured = false // Kan ändras manuellt i DB eller byggas ut här
                };

                _context.Products.Add(newProduct);
                _context.SaveChanges(); // Sparar till SQL-databasen

                Console.WriteLine("\nProdukten har sparats! Tryck på valfri tangent...");
                Console.ReadKey();
            }
            else if (choice == "2")
            {
                UpdateStock(); // Vi kan bygga denna i nästa del av steget om du vill
            }
        }

        private void UpdateStock()
        {
            Console.Clear();
            Console.WriteLine("--- UPPDATERA LAGERSALDO ---");

            // Hämta alla produkter för att välja vilken som ska uppdateras
            var products = _context.Products.ToList();
            for (int i = 0; i < products.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {products[i].Name} (Nuvarande saldo: {products[i].StockQuantity})");
            }

            Console.Write("\nVälj produkt: ");
            if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= products.Count)
            {
                var product = products[index - 1];

                Console.Write($"Ange nytt lagersaldo för {product.Name}: ");
                if (int.TryParse(Console.ReadLine(), out int newStock))
                {
                    // Uppdatera värdet i objektet
                    product.StockQuantity = newStock;

                    // Spara ändringen till SQL-databasen
                    _context.SaveChanges();

                    Console.WriteLine("Lagersaldot har uppdaterats!");
                }
            }
            Console.WriteLine("\nTryck på valfri tangent för att gå tillbaka...");
            Console.ReadKey();
        }

        private void ShowStatistics()
        {
            Console.Clear();
            Console.WriteLine("--- BUTIKSSTATISTIK (Dapper & SQL) ---");

            using var db = new SqlConnection(_connectionString);

            // 1. Fråga: Topp 5 mest sålda produkter
            string productSql = @"
            SELECT TOP 5 p.Name, SUM(orw.Quantity) as TotalSold
            FROM Products p
            JOIN OrderRows orw ON p.Id = orw.ProductId
            GROUP BY p.Name
            ORDER BY TotalSold DESC";

            // 2. Fråga: Omsättning per stad
            string citySql = @"
            SELECT TOP 5 c.City, SUM(o.TotalPrice) as Revenue
            FROM Customers c
            JOIN Orders o ON c.Id = o.CustomerId
            GROUP BY c.City
            ORDER BY Revenue DESC";

            try
            {
                var productStats = db.Query(productSql).ToList();
                var cityStats = db.Query(citySql).ToList();

                Console.WriteLine("\nMEST SÅLDA PRODUKTER:");
                if (productStats.Any())
                {
                    foreach (var row in productStats)
                    {
                        Console.WriteLine($"- {row.Name}: {row.TotalSold} st");
                    }
                }
                else { Console.WriteLine("Ingen försäljningsdata tillgänglig."); }

                Console.WriteLine("\nOMSÄTTNING PER STAD:");
                if (cityStats.Any())
                {
                    foreach (var row in cityStats)
                    {
                        Console.WriteLine($"- {row.City}: {row.Revenue} kr");
                    }
                }
                else { Console.WriteLine("Ingen geografisk data tillgänglig."); }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kunde inte hämta statistik: {ex.Message}");
            }

            Console.WriteLine("\nTryck på valfri tangent för att gå tillbaka...");
            Console.ReadKey();
        }

        private void ViewCustomers()
        {
            Console.Clear();
            Console.WriteLine("--- KUNDLISTA & ORDERHISTORIK ---");

            // Vi inkluderar både Ordrar och OrderRows (orderrader) för att se detaljer
            var customers = _context.Customers
                .Include(c => c.Orders)
                .ThenInclude(o => o.OrderRows)
                .ThenInclude(or => or.Product)
                .ToList();

            if (!customers.Any())
            {
                Console.WriteLine("Inga kunder registrerade ännu.");
            }

            foreach (var c in customers)
            {
                Console.WriteLine($"\nKUND: {c.Name} ({c.City})"); //
                Console.WriteLine(new string('-', 30));

                if (!c.Orders.Any())
                {
                    Console.WriteLine("  Inga beställningar gjorda.");
                }

                foreach (var o in c.Orders)
                {
                    Console.WriteLine($"  Order #{o.Id} - {o.OrderDate.ToShortDateString()}");
                    Console.WriteLine($"  Totalt: {o.TotalPrice} kr via {o.PaymentMethod}");

                    foreach (var row in o.OrderRows)
                    {
                        Console.WriteLine($"    > {row.Product.Name} ({row.Quantity} st)");
                    }
                }
            }

            Console.WriteLine("\nTryck på valfri tangent för att gå tillbaka...");
            Console.ReadKey();
        }

        private void ManageCategories()
        {
            Console.Clear();
            Console.WriteLine("--- HANTERA KATEGORIER ---");
            Console.WriteLine("1. Lägg till ny kategori");
            Console.WriteLine("2. Lista alla kategorier");
            Console.WriteLine("0. Gå tillbaka");
            Console.Write("\nVal: ");

            string choice = Console.ReadLine() ?? "";
            if (choice == "1")
            {
                Console.Write("Ange namn på den nya kategorin: ");
                string catName = Console.ReadLine() ?? "";

                if (!string.IsNullOrWhiteSpace(catName))
                {
                    var newCategory = new Category { Name = catName };
                    _context.Categories.Add(newCategory);
                    _context.SaveChanges(); // Sparar kategorin till databasen
                    Console.WriteLine($"Kategorin '{catName}' har sparats!");
                }
            }
            else if (choice == "2")
            {
                var categories = _context.Categories.ToList();
                Console.WriteLine("\nBefintliga kategorier:");
                foreach (var c in categories)
                {
                    Console.WriteLine($"- {c.Name}");
                }
            }
            Console.WriteLine("\nTryck på valfri tangent...");
            Console.ReadKey();
        }
    }
}
