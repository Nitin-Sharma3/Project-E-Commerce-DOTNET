using Consumer.MVC.Models;
using Consumer.MVC.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Consumer.MVC.Controllers
{    public class HomeController : Controller
        {
            public IActionResult Index()
            {
                var model = new HomeViewModel
                {
                    HeroProduct = new Product
                    {
                        Id = 1,
                        Name = "MagSafe Pro Hub",
                        Tagline = "Seven ports. Zero compromise.",
                        Price = 129.99m,
                        OriginalPrice = 159.99m,
                        ImageUrl = "https://images.unsplash.com/photo-1625895197185-efcec01cffe0?w=600&q=80",
                        Category = "Laptop",
                        Badge = "Bestseller"
                    },
                    FeaturedLaptopAccessories = new List<Product>
                {
                    new() { Id = 2, Name = "ErgoFlow Keyboard", Price = 89.99m, ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=400&q=80", Category = "Laptop", Badge = "New" },
                    new() { Id = 3, Name = "UltraSlim Sleeve", Price = 39.99m, ImageUrl = "https://images.unsplash.com/photo-1548036328-c9fa89d128fa?w=400&q=80", Category = "Laptop" },
                    new() { Id = 4, Name = "Nano USB-C Hub", Price = 59.99m, ImageUrl = "https://images.unsplash.com/photo-1625895197185-efcec01cffe0?w=400&q=80", Category = "Laptop" },
                    new() { Id = 5, Name = "Wireless Charging Pad", Price = 44.99m, ImageUrl = "https://images.unsplash.com/photo-1586495777744-4e6232bf7264?w=400&q=80", Category = "Laptop", Badge = "Sale" }
                },
                    FeaturedMobileAccessories = new List<Product>
                {
                    new() { Id = 6, Name = "ArmorShield Case", Price = 34.99m, ImageUrl = "https://images.unsplash.com/photo-1601972599748-bf60983b2a07?w=400&q=80", Category = "Mobile", Badge = "Top Rated" },
                    new() { Id = 7, Name = "GlassGuard Pro", Price = 19.99m, ImageUrl = "https://images.unsplash.com/photo-1601784551446-20c9e07cdbdb?w=400&q=80", Category = "Mobile" },
                    new() { Id = 8, Name = "PocketGrip Stand", Price = 14.99m, ImageUrl = "https://images.unsplash.com/photo-1585338107529-13afc5f02586?w=400&q=80", Category = "Mobile" },
                    new() { Id = 9, Name = "FastCharge Cable", Price = 24.99m, ImageUrl = "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=400&q=80", Category = "Mobile", Badge = "New" }
                },
                    Testimonials = new List<Testimonial>
                {
                    new() { Author = "Arjun S.", Role = "Product Designer", Text = "The hub changed my entire workstation setup. Build quality is exceptional.", Rating = 5 },
                    new() { Author = "Priya M.", Role = "Software Engineer", Text = "Finally an accessories brand that gets aesthetics and function right.", Rating = 5 },
                    new() { Author = "Rohan K.", Role = "Content Creator", Text = "Fast delivery, premium packaging, and the products speak for themselves.", Rating = 5 }
                }
                };

                return View(model);
            }
        }
    }