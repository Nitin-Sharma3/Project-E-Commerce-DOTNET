using DeliveryService.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace DeliveryService.Data
{
    public class DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : DbContext(options)
    {
        public DbSet<Delivery> Deliveries => Set<Delivery>();
        public DbSet<DeliveryItem> DeliveryItems => Set<DeliveryItem>();
        public DbSet<DeliveryStatusHistory> StatusHistories => Set<DeliveryStatusHistory>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Delivery>().HasIndex(d => d.TrackingId).IsUnique();
            b.Entity<Delivery>().HasIndex(d => d.OrderId).IsUnique();
            b.Entity<Delivery>().Property(d => d.Status).HasConversion<string>();
            b.Entity<DeliveryStatusHistory>().Property(d => d.Status).HasConversion<string>();

            // ── Seed Deliveries ──────────────────────────────────────
            var now = new DateTime(2025, 4, 1, 10, 0, 0, DateTimeKind.Utc);

            b.Entity<Delivery>().HasData(
                new Delivery
                {
                    Id = 1,
                    OrderId = 101,
                    UserId = 1,
                    RecipientName = "Aarav Sharma",
                    AddressLine1 = "42, Green Park Colony",
                    City = "New Delhi",
                    State = "Delhi",
                    Pincode = "110016",
                    ContactPhone = "9810012345",
                    ContactEmail = "aarav@email.com",
                    TrackingId = "TRK-2025-001",
                    Status = DeliveryStatus.Delivered,
                    CreatedAt = now.AddDays(-7),
                    EstimatedDeliveryDate = now.AddDays(-4),
                    ActualDeliveryDate = now.AddDays(-5),
                    DeliveryAgentName = "Ravi Kumar",
                    DeliveryAgentPhone = "9911223344",
                    CurrentLatitude = 28.5494,
                    CurrentLongitude = 77.2001
                },
                new Delivery
                {
                    Id = 2,
                    OrderId = 102,
                    UserId = 2,
                    RecipientName = "Priya Mehta",
                    AddressLine1 = "B-12, Koregaon Park",
                    City = "Pune",
                    State = "Maharashtra",
                    Pincode = "411001",
                    ContactPhone = "9823345678",
                    ContactEmail = "priya@email.com",
                    TrackingId = "TRK-2025-002",
                    Status = DeliveryStatus.OutForDelivery,
                    CreatedAt = now.AddDays(-2),
                    EstimatedDeliveryDate = now.AddDays(1),
                    DeliveryAgentName = "Suresh Patil",
                    DeliveryAgentPhone = "9900112233",
                    CurrentLatitude = 18.5362,
                    CurrentLongitude = 73.8941
                },
                new Delivery
                {
                    Id = 3,
                    OrderId = 103,
                    UserId = 3,
                    RecipientName = "Rohan Verma",
                    AddressLine1 = "C-99, Salt Lake Sector V",
                    City = "Kolkata",
                    State = "West Bengal",
                    Pincode = "700091",
                    ContactPhone = "9734456789",
                    TrackingId = "TRK-2025-003",
                    Status = DeliveryStatus.Shipped,
                    CreatedAt = now.AddDays(-3),
                    EstimatedDeliveryDate = now.AddDays(2),
                    DeliveryAgentName = "Amit Das",
                    DeliveryAgentPhone = "9800334455",
                    CurrentLatitude = 22.5726,
                    CurrentLongitude = 88.3639
                },
                new Delivery
                {
                    Id = 4,
                    OrderId = 104,
                    UserId = 4,
                    RecipientName = "Sneha Nair",
                    AddressLine1 = "15, Jayanagar 4th Block",
                    City = "Bengaluru",
                    State = "Karnataka",
                    Pincode = "560011",
                    ContactPhone = "9845567890",
                    TrackingId = "TRK-2025-004",
                    Status = DeliveryStatus.Packed,
                    CreatedAt = now.AddDays(-1),
                    EstimatedDeliveryDate = now.AddDays(3),
                    CurrentLatitude = 12.9250,
                    CurrentLongitude = 77.5938
                },
                new Delivery
                {
                    Id = 5,
                    OrderId = 105,
                    UserId = 5,
                    RecipientName = "Karan Joshi",
                    AddressLine1 = "Shop No. 7, Navrangpura",
                    City = "Ahmedabad",
                    State = "Gujarat",
                    Pincode = "380009",
                    ContactPhone = "9712678901",
                    TrackingId = "TRK-2025-005",
                    Status = DeliveryStatus.Pending,
                    CreatedAt = now,
                    EstimatedDeliveryDate = now.AddDays(5),
                    CurrentLatitude = 23.0225,
                    CurrentLongitude = 72.5714
                },
                new Delivery
                {
                    Id = 6,
                    OrderId = 106,
                    UserId = 6,
                    RecipientName = "Divya Reddy",
                    AddressLine1 = "Flat 3B, Banjara Hills Road 12",
                    City = "Hyderabad",
                    State = "Telangana",
                    Pincode = "500034",
                    ContactPhone = "9676789012",
                    TrackingId = "TRK-2025-006",
                    Status = DeliveryStatus.Failed,
                    CreatedAt = now.AddDays(-4),
                    EstimatedDeliveryDate = now.AddDays(-2),
                    DeliveryAgentName = "Mohammed Ali",
                    DeliveryAgentPhone = "9100223344",
                    CurrentLatitude = 17.4126,
                    CurrentLongitude = 78.4071
                },
                new Delivery
                {
                    Id = 7,
                    OrderId = 107,
                    UserId = 7,
                    RecipientName = "Manish Patel",
                    AddressLine1 = "221B, Satellite Road",
                    City = "Surat",
                    State = "Gujarat",
                    Pincode = "395007",
                    ContactPhone = "9601890123",
                    TrackingId = "TRK-2025-007",
                    Status = DeliveryStatus.Shipped,
                    CreatedAt = now.AddDays(-2),
                    EstimatedDeliveryDate = now.AddDays(1),
                    DeliveryAgentName = "Vijay Shah",
                    DeliveryAgentPhone = "9512334455",
                    CurrentLatitude = 21.1702,
                    CurrentLongitude = 72.8311
                }
            );

            // ── Seed Items ────────────────────────────────────────────
            b.Entity<DeliveryItem>().HasData(
                new DeliveryItem { Id = 1, DeliveryId = 1, ProductId = 10, ProductName = "Samsung Galaxy S24", Quantity = 1, UnitPrice = 79999 },
                new DeliveryItem { Id = 2, DeliveryId = 1, ProductId = 11, ProductName = "Phone Case", Quantity = 2, UnitPrice = 499 },
                new DeliveryItem { Id = 3, DeliveryId = 2, ProductId = 20, ProductName = "Nike Air Max 270", Quantity = 1, UnitPrice = 12495 },
                new DeliveryItem { Id = 4, DeliveryId = 3, ProductId = 30, ProductName = "Dell XPS 15 Laptop", Quantity = 1, UnitPrice = 125000 },
                new DeliveryItem { Id = 5, DeliveryId = 3, ProductId = 31, ProductName = "USB-C Hub", Quantity = 1, UnitPrice = 2499 },
                new DeliveryItem { Id = 6, DeliveryId = 4, ProductId = 40, ProductName = "Sony WH-1000XM5 Headphones", Quantity = 1, UnitPrice = 29990 },
                new DeliveryItem { Id = 7, DeliveryId = 5, ProductId = 50, ProductName = "Levi's 511 Slim Jeans", Quantity = 2, UnitPrice = 3499 },
                new DeliveryItem { Id = 8, DeliveryId = 5, ProductId = 51, ProductName = "Formal White Shirt", Quantity = 3, UnitPrice = 1299 },
                new DeliveryItem { Id = 9, DeliveryId = 6, ProductId = 60, ProductName = "boAt Airdopes 141", Quantity = 1, UnitPrice = 1299 },
                new DeliveryItem { Id = 10, DeliveryId = 7, ProductId = 70, ProductName = "Instant Pot Duo 7-in-1", Quantity = 1, UnitPrice = 8999 },
                new DeliveryItem { Id = 11, DeliveryId = 7, ProductId = 71, ProductName = "Glass Meal Prep Containers (Set of 5)", Quantity = 1, UnitPrice = 1599 }
            );

            // ── Seed Status History ───────────────────────────────────
            b.Entity<DeliveryStatusHistory>().HasData(
                new DeliveryStatusHistory { Id = 1, DeliveryId = 1, Status = DeliveryStatus.Pending, Remarks = "Order received", Location = "Warehouse, Delhi", Latitude = 28.6292, Longitude = 77.2182, Timestamp = now.AddDays(-7), UpdatedBy = "System" },
                new DeliveryStatusHistory { Id = 2, DeliveryId = 1, Status = DeliveryStatus.Packed, Remarks = "Order packed and ready", Location = "Warehouse, Delhi", Latitude = 28.6292, Longitude = 77.2182, Timestamp = now.AddDays(-6), UpdatedBy = "Packing Team" },
                new DeliveryStatusHistory { Id = 3, DeliveryId = 1, Status = DeliveryStatus.Shipped, Remarks = "Dispatched via BlueDart", Location = "Delhi Hub", Latitude = 28.6139, Longitude = 77.2090, Timestamp = now.AddDays(-6).AddHours(4), UpdatedBy = "Dispatch Team" },
                new DeliveryStatusHistory { Id = 4, DeliveryId = 1, Status = DeliveryStatus.OutForDelivery, Remarks = "Out for delivery", Location = "Green Park, Delhi", Latitude = 28.5510, Longitude = 77.2000, Timestamp = now.AddDays(-5).AddHours(8), UpdatedBy = "Ravi Kumar" },
                new DeliveryStatusHistory { Id = 5, DeliveryId = 1, Status = DeliveryStatus.Delivered, Remarks = "Delivered and signed", Location = "42, Green Park Colony, Delhi", Latitude = 28.5494, Longitude = 77.2001, Timestamp = now.AddDays(-5).AddHours(11), UpdatedBy = "Ravi Kumar" },

                new DeliveryStatusHistory { Id = 6, DeliveryId = 2, Status = DeliveryStatus.Pending, Remarks = "Order received", Location = "Mumbai Warehouse", Latitude = 19.0760, Longitude = 72.8777, Timestamp = now.AddDays(-2), UpdatedBy = "System" },
                new DeliveryStatusHistory { Id = 7, DeliveryId = 2, Status = DeliveryStatus.Packed, Remarks = "Packed", Location = "Mumbai Warehouse", Latitude = 19.0760, Longitude = 72.8777, Timestamp = now.AddDays(-1), UpdatedBy = "Packing Team" },
                new DeliveryStatusHistory { Id = 8, DeliveryId = 2, Status = DeliveryStatus.Shipped, Remarks = "En route to Pune", Location = "Mumbai-Pune Expressway", Latitude = 18.9200, Longitude = 73.1200, Timestamp = now.AddDays(-1).AddHours(6), UpdatedBy = "Driver" },
                new DeliveryStatusHistory { Id = 9, DeliveryId = 2, Status = DeliveryStatus.OutForDelivery, Remarks = "Out for delivery in Pune", Location = "Koregaon Park, Pune", Latitude = 18.5362, Longitude = 73.8941, Timestamp = now.AddHours(8), UpdatedBy = "Suresh Patil" },

                new DeliveryStatusHistory { Id = 10, DeliveryId = 3, Status = DeliveryStatus.Pending, Remarks = "Order received", Location = "Kolkata Hub", Latitude = 22.5726, Longitude = 88.3639, Timestamp = now.AddDays(-3), UpdatedBy = "System" },
                new DeliveryStatusHistory { Id = 11, DeliveryId = 3, Status = DeliveryStatus.Packed, Remarks = "Ready for dispatch", Location = "Kolkata Hub", Latitude = 22.5726, Longitude = 88.3639, Timestamp = now.AddDays(-2), UpdatedBy = "Packing Team" },
                new DeliveryStatusHistory { Id = 12, DeliveryId = 3, Status = DeliveryStatus.Shipped, Remarks = "In transit", Location = "Kolkata Hub", Latitude = 22.5726, Longitude = 88.3639, Timestamp = now.AddDays(-2).AddHours(3), UpdatedBy = "Amit Das" },

                new DeliveryStatusHistory { Id = 13, DeliveryId = 6, Status = DeliveryStatus.Pending, Remarks = "Order received", Location = "Hyderabad Hub", Latitude = 17.4126, Longitude = 78.4071, Timestamp = now.AddDays(-4), UpdatedBy = "System" },
                new DeliveryStatusHistory { Id = 14, DeliveryId = 6, Status = DeliveryStatus.Packed, Remarks = "Packed", Location = "Hyderabad Hub", Latitude = 17.4126, Longitude = 78.4071, Timestamp = now.AddDays(-3), UpdatedBy = "Packing Team" },
                new DeliveryStatusHistory { Id = 15, DeliveryId = 6, Status = DeliveryStatus.Shipped, Remarks = "Out for delivery", Location = "Banjara Hills, Hyderabad", Latitude = 17.4126, Longitude = 78.4071, Timestamp = now.AddDays(-2), UpdatedBy = "Mohammed Ali" },
                new DeliveryStatusHistory { Id = 16, DeliveryId = 6, Status = DeliveryStatus.Failed, Remarks = "Recipient not available, will retry tomorrow", Location = "Banjara Hills, Hyderabad", Latitude = 17.4126, Longitude = 78.4071, Timestamp = now.AddDays(-2).AddHours(5), UpdatedBy = "Mohammed Ali" }
            );
        }
    }
}
