using Webshop.Models;
using Microsoft.EntityFrameworkCore;
using Webshop.Services;
using Webshop.Data;

namespace Webshop
{
    class Program
    {
        // Varukorgen lagras här under hela körningen
        static List<Product> cart = new List<Product>();

        static void Main(string[] args)
        {
            using var context = new MyStoreContext();
            var cart = new List<Product>();

            var storeService = new StoreService(context, cart);
            var cartService = new CartService(cart, context); 
            var adminService = new AdminService(context); 

            SeedData(context);

            bool isRunning = true;
            while (isRunning)
            {
                Console.Clear();
                Console.WriteLine("# KAMERAWEBBSHOPPEN #");
                ShowFeaturedProducts(context);

                Console.WriteLine("\n1. Shoppen\n2. Varukorg\n3. Admin\n0. Avsluta");
                string input = Console.ReadLine() ?? "";

                switch (input)
                {
                    case "1":
                        storeService.ShowShop();
                        break;
                    case "2":
                        cartService.ShowCart();
                        break;
                    case "3":
                        adminService.ShowAdminMenu();
                        break;
                    case "0":
                        isRunning = false;
                        break;
                }
            }
        }

        

        static void ShowFeaturedProducts(MyStoreContext context)
        {
            var featured = context.Products.Where(p => p.IsFeatured).Take(3).ToList();

            Console.WriteLine("--- UTVALDA ERBJUDANDEN ---");
            foreach (var p in featured)
            {
                Console.WriteLine($"* {p.Name} - {p.Price} kr");
            }
        }

        static void SeedData(MyStoreContext context)
        {
            if (!context.Categories.Any())
            {
                var kat1 = new Category { Name = "Systemkameror" };
                var kat2 = new Category { Name = "Objektiv" };
                context.Categories.AddRange(kat1, kat2);

                context.Products.Add(new Product { Name = "Canon R5", Price = 45000, IsFeatured = true, Category = kat1 });
                context.Products.Add(new Product { Name = "Sony A7 IV", Price = 28000, IsFeatured = true, Category = kat1 });
                context.Products.Add(new Product { Name = "Nikon Z6 II", Price = 22000, IsFeatured = true, Category = kat1 });

                context.SaveChanges();
            }
        }

        static void ShowShop(MyStoreContext context)
        {
            Console.Clear();
            Console.WriteLine("Välkommen till Shoppen! (Här kommer vi lista kategorier snart)");
            Console.WriteLine("\nTryck på valfri tangent för att gå tillbaka...");
            Console.ReadKey();
        }

        static void ShowCart()
        {
            Console.Clear();
            Console.WriteLine("Här är din varukorg! (Här kommer vi visa dina köp snart)");
            Console.WriteLine("\nTryck på valfri tangent för att gå tillbaka...");
            Console.ReadKey();
        }

        static void ShowAdmin(MyStoreContext context)
        {
            Console.Clear();
            Console.WriteLine("Admin-panel (Här kommer du kunna ändra produkter)");
            Console.WriteLine("\nTryck på valfri tangent för att gå tillbaka...");
            Console.ReadKey();
        }
    }
}

