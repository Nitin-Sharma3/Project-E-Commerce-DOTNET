namespace Consumer.MVC.ViewModel
{
   
        public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Tagline { get; set; }
            public decimal Price { get; set; }
            public decimal? OriginalPrice { get; set; }
            public string ImageUrl { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string? Badge { get; set; }
            public double Rating { get; set; } = 4.8;
            public int ReviewCount { get; set; } = 128;
        }

        public class Testimonial
        {
            public string Author { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public string Text { get; set; } = string.Empty;
            public int Rating { get; set; }
        }

        public class HomeViewModel
        {
            public Product HeroProduct { get; set; } = new();
            public List<Product> FeaturedLaptopAccessories { get; set; } = new();
            public List<Product> FeaturedMobileAccessories { get; set; } = new();
            public List<Testimonial> Testimonials { get; set; } = new();
        }
    }

