using DeliveryService.DTOs;
using DeliveryService.HttpClients;
using DeliveryService.Models;
using DeliveryService.Repositories;

namespace DeliveryService.Services
{
    public class DeliveryService(
    IDeliveryRepository repo,
    IOrderClient orderClient,
    IProductClient productClient,
    ILogger<DeliveryService> logger) : IDeliveryService
    {
        // ── Valid status transitions ───────────────────────────────────────────
        private static readonly Dictionary<DeliveryStatus, DeliveryStatus[]> ValidTransitions = new()
        {
            [DeliveryStatus.Pending] = [DeliveryStatus.Packed, DeliveryStatus.Failed],
            [DeliveryStatus.Packed] = [DeliveryStatus.Shipped, DeliveryStatus.Failed],
            [DeliveryStatus.Shipped] = [DeliveryStatus.OutForDelivery, DeliveryStatus.Failed],
            [DeliveryStatus.OutForDelivery] = [DeliveryStatus.Delivered, DeliveryStatus.Failed],
            [DeliveryStatus.Delivered] = [],
            [DeliveryStatus.Failed] = [DeliveryStatus.Pending],
        };

        // ── GPS simulation offsets ────────────────────────────────────────────
        private static readonly Dictionary<DeliveryStatus, (double Lat, double Lng)> GpsOffset = new()
        {
            [DeliveryStatus.Pending] = (0.08, 0.08),
            [DeliveryStatus.Packed] = (0.06, 0.06),
            [DeliveryStatus.Shipped] = (0.04, 0.04),
            [DeliveryStatus.OutForDelivery] = (0.01, 0.01),
            [DeliveryStatus.Delivered] = (0, 0),
            [DeliveryStatus.Failed] = (0.02, -0.02),
        };

        // ── City → approximate GPS coordinates for Pending deliveries ─────────
        // Used as fallback when CurrentLatitude/CurrentLongitude is null (new deliveries)
        private static readonly Dictionary<string, (double Lat, double Lng)> CityCoordinates = new(
            StringComparer.OrdinalIgnoreCase)
        {
            ["Bengaluru"] = (12.9716, 77.5946),
            ["Bangalore"] = (12.9716, 77.5946),
            ["Mumbai"] = (19.0760, 72.8777),
            ["Delhi"] = (28.6139, 77.2090),
            ["New Delhi"] = (28.6139, 77.2090),
            ["Hyderabad"] = (17.3850, 78.4867),
            ["Chennai"] = (13.0827, 80.2707),
            ["Kolkata"] = (22.5726, 88.3639),
            ["Pune"] = (18.5204, 73.8567),
            ["Ahmedabad"] = (23.0225, 72.5714),
            ["Jaipur"] = (26.9124, 75.7873),
            ["Lucknow"] = (26.8467, 80.9462),
            ["Noida"] = (28.5355, 77.3910),
            ["Gurgaon"] = (28.4595, 77.0266),
            ["Surat"] = (21.1702, 72.8311),
            ["Indore"] = (22.7196, 75.8577),
            ["Bhopal"] = (23.2599, 77.4126),
            ["Patna"] = (25.5941, 85.1376),
            ["Nagpur"] = (21.1458, 79.0882),
            ["Coimbatore"] = (11.0168, 76.9558),
            ["Kochi"] = (9.9312, 76.2673),
            ["Visakhapatnam"] = (17.6868, 83.2185),
            ["Chandigarh"] = (30.7333, 76.7794),
            ["Bhubaneswar"] = (20.2961, 85.8245),
            ["Vadodara"] = (22.3072, 73.1812),
        };

        // ── Delivery → OrderAPI status mapping ────────────────────────────────
        private static string? ToOrderStatus(DeliveryStatus s) => s switch
        {
            DeliveryStatus.Packed => "Confirmed",
            DeliveryStatus.Shipped => "Shipped",
            DeliveryStatus.OutForDelivery => "Shipped",
            DeliveryStatus.Delivered => "Delivered",
            DeliveryStatus.Failed => "Cancelled",
            _ => null
        };

        // ── Mapping ───────────────────────────────────────────────────────────
        private static DeliveryResponseDto Map(Delivery d) => new(
            d.Id, d.OrderId, d.UserId,
            d.RecipientName,
            $"{d.AddressLine1}{(d.AddressLine2 != null ? ", " + d.AddressLine2 : "")}, {d.City}, {d.State} - {d.Pincode}, {d.Country}",
            d.ContactPhone, d.ContactEmail,
            d.TrackingId, d.Status.ToString(),
            d.DeliveryAgentName, d.DeliveryAgentPhone,
            d.CreatedAt, d.EstimatedDeliveryDate, d.ActualDeliveryDate,
            d.CurrentLatitude, d.CurrentLongitude,
            d.Items.Select(i => new DeliveryItemResponseDto(
                i.ProductId, i.ProductName, i.Quantity, i.UnitPrice,
                i.Subtotal, i.Quantity * i.UnitPrice, i.ImageUrl)).ToList(),
            d.StatusHistory.OrderBy(h => h.Timestamp)
                .Select(h => new StatusHistoryDto(
                    h.Status.ToString(), h.Remarks, h.Location,
                    h.Latitude, h.Longitude, h.Timestamp, h.UpdatedBy)).ToList()
        );

        // ── Resolve coordinates for a delivery ────────────────────────────────
        // Priority: 1) CurrentLatitude/Longitude (GPS from agent)
        //           2) City lookup table (for Pending with no GPS yet)
        //           3) India center as final fallback
        private static (double Lat, double Lng) ResolveCoordinates(Delivery d)
        {
            if (d.CurrentLatitude.HasValue && d.CurrentLongitude.HasValue)
                return (d.CurrentLatitude.Value, d.CurrentLongitude.Value);

            if (CityCoordinates.TryGetValue(d.City?.Trim() ?? "", out var coords))
                return coords;

            // Final fallback: geographic center of India
            return (20.5937, 78.9629);
        }

        // ── CRUD ──────────────────────────────────────────────────────────────

        public async Task<IEnumerable<DeliveryResponseDto>> GetAllAsync() =>
            (await repo.GetAllAsync()).Select(Map);

        public async Task<DeliveryResponseDto?> GetByIdAsync(int id)
        {
            var d = await repo.GetByIdAsync(id);
            return d == null ? null : Map(d);
        }

        public async Task<IEnumerable<DeliveryResponseDto>> GetByUserIdAsync(int userId) =>
            (await repo.GetByUserIdAsync(userId)).Select(Map);

        public async Task<DeliveryResponseDto?> GetByOrderIdAsync(int orderId)
        {
            var d = await repo.GetByOrderIdAsync(orderId);
            return d == null ? null : Map(d);
        }

        public async Task<DeliveryResponseDto> CreateAsync(CreateDeliveryDto dto)
        {
            // Prevent duplicate deliveries for same order
            var existing = await repo.GetByOrderIdAsync(dto.OrderId);
            if (existing != null)
            {
                logger.LogWarning("Delivery already exists for Order {Id}", dto.OrderId);
                return Map(existing);
            }

            var trackingId =
                $"TRK-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

            var delivery = new Delivery
            {
                OrderId = dto.OrderId,
                UserId = dto.UserId,
                RecipientName = dto.RecipientName,
                AddressLine1 = dto.AddressLine1,
                AddressLine2 = dto.AddressLine2,
                City = dto.City,
                State = dto.State,
                Pincode = dto.Pincode,
                Country = dto.Country,
                ContactPhone = dto.ContactPhone,
                ContactEmail = dto.ContactEmail,
                TrackingId = trackingId,
                Status = DeliveryStatus.Pending,
                EstimatedDeliveryDate = DateTime.UtcNow.AddDays(5),
                Items = dto.Items.Select(i => new DeliveryItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Subtotal = i.Subtotal,
                    ImageUrl = i.ImageUrl
                }).ToList(),
                StatusHistory =
                [
                    new DeliveryStatusHistory
                    {
                        Status    = DeliveryStatus.Pending,
                        Remarks   = $"Delivery created for Order #{dto.OrderId}",
                        Location  = "Warehouse",
                        Timestamp = DateTime.UtcNow,
                        UpdatedBy = "System"
                    }
                ]
            };

            var created = await repo.CreateAsync(delivery);
            logger.LogInformation("Delivery {TId} created for Order {OId}",
                trackingId, dto.OrderId);
            return Map(created);
        }

        // ── Called by OrderAPI after order placed ─────────────────────────────
        public async Task<DeliveryResponseDto> CreateFromOrderAsync(int orderId, int userId)
        {
            // Idempotency check
            var existing = await repo.GetByOrderIdAsync(orderId);
            if (existing != null)
            {
                logger.LogInformation("Delivery already exists for Order {Id}", orderId);
                return Map(existing);
            }

            // Fetch order from OrderAPI: GET api/users/{userId}/orders/{orderId}
            var order = await orderClient.GetOrderAsync(userId, orderId)
                ?? throw new KeyNotFoundException(
                    $"Order {orderId} not found in OrderAPI for User {userId}");

            // Enrich items with ProductAPI image (best-effort)
            var items = new List<DeliveryItemDto>();
            foreach (var item in order.Items)
            {
                var product = await productClient.GetProductAsync(item.ProductId);
                items.Add(new DeliveryItemDto(
                    item.ProductId,
                    item.ProductName,
                    item.Quantity,
                    item.UnitPrice,
                    item.Subtotal,
                    product?.ImageUrl1   // ImageUrl1 from ProductAPI
                ));
            }

            var dto = new CreateDeliveryDto(
                order.Id,
                order.UserId,
                order.ShippingAddress.FullName,
                order.ShippingAddress.AddressLine1,
                order.ShippingAddress.AddressLine2,
                order.ShippingAddress.City,
                order.ShippingAddress.State,
                order.ShippingAddress.PostalCode,
                order.ShippingAddress.Country,
                order.ShippingAddress.Phone,
                null,
                items
            );

            return await CreateAsync(dto);
        }

        public async Task<DeliveryResponseDto> UpdateStatusAsync(UpdateDeliveryStatusDto dto)
        {
            var delivery = await repo.GetByIdAsync(dto.DeliveryId)
                ?? throw new KeyNotFoundException($"Delivery {dto.DeliveryId} not found");

            // Validate transition
            if (!ValidTransitions.TryGetValue(delivery.Status, out var allowed) ||
                !allowed.Contains(dto.NewStatus))
                throw new InvalidOperationException(
                    $"Cannot transition from {delivery.Status} to {dto.NewStatus}. " +
                    $"Allowed: [{string.Join(", ", allowed ?? [])}]");

            // Simulate GPS movement toward destination
            var (latOff, lngOff) = GpsOffset[dto.NewStatus];
            delivery.CurrentLatitude = (dto.Latitude ?? delivery.CurrentLatitude ?? 0) - latOff;
            delivery.CurrentLongitude = (dto.Longitude ?? delivery.CurrentLongitude ?? 0) - lngOff;
            delivery.Status = dto.NewStatus;

            if (dto.NewStatus == DeliveryStatus.Delivered)
            {
                delivery.ActualDeliveryDate = DateTime.UtcNow;
                delivery.CurrentLatitude = dto.Latitude ?? delivery.CurrentLatitude;
                delivery.CurrentLongitude = dto.Longitude ?? delivery.CurrentLongitude;
            }

            await repo.UpdateAsync(delivery);
            await repo.AddStatusHistoryAsync(new DeliveryStatusHistory
            {
                DeliveryId = delivery.Id,
                Status = dto.NewStatus,
                Remarks = dto.Remarks,
                Location = dto.Location,
                Latitude = delivery.CurrentLatitude,
                Longitude = delivery.CurrentLongitude,
                Timestamp = DateTime.UtcNow,
                UpdatedBy = dto.UpdatedBy
            });

            // Sync to OrderAPI (non-blocking, best-effort)
            var orderStatus = ToOrderStatus(dto.NewStatus);
            if (orderStatus != null)
            {
                var synced = await orderClient.UpdateOrderStatusAsync(
                    delivery.OrderId, orderStatus);
                if (!synced)
                    logger.LogWarning("OrderAPI sync failed for Order {Id}", delivery.OrderId);
            }

            logger.LogInformation("Delivery {Id}: {Old} → {New}",
                delivery.Id, delivery.Status, dto.NewStatus);
            return Map(delivery);
        }

        public async Task<DeliveryResponseDto> MarkDeliveredAsync(MarkDeliveredDto dto) =>
            await UpdateStatusAsync(new UpdateDeliveryStatusDto(
                dto.DeliveryId, DeliveryStatus.Delivered,
                "Delivery confirmed by agent", UpdatedBy: dto.UpdatedBy));

        public async Task<DeliveryResponseDto> AssignAgentAsync(AssignAgentDto dto)
        {
            var delivery = await repo.GetByIdAsync(dto.DeliveryId)
                ?? throw new KeyNotFoundException($"Delivery {dto.DeliveryId} not found");

            delivery.DeliveryAgentName = dto.AgentName;
            delivery.DeliveryAgentPhone = dto.AgentPhone;
            await repo.UpdateAsync(delivery);

            logger.LogInformation("Agent {Name} assigned to Delivery {Id}",
                dto.AgentName, dto.DeliveryId);
            return Map(delivery);
        }

        public async Task<TrackingResponseDto?> GetTrackingAsync(string trackingId)
        {
            var d = await repo.GetByTrackingIdAsync(trackingId);
            if (d == null) return null;

            return new TrackingResponseDto(
                d.TrackingId, d.RecipientName,
                $"{d.AddressLine1}, {d.City}, {d.State}",
                d.Status.ToString(),
                d.CurrentLatitude, d.CurrentLongitude,
                d.EstimatedDeliveryDate, d.ActualDeliveryDate,
                d.DeliveryAgentName, d.DeliveryAgentPhone,
                d.StatusHistory.OrderBy(h => h.Timestamp)
                    .Select(h => new TrackingPointDto(
                        h.Status.ToString(), h.Location,
                        h.Latitude, h.Longitude,
                        h.Timestamp, h.Remarks,
                        h.Status == d.Status))
                    .ToList()
            );
        }

        public async Task<IEnumerable<DeliveryMapPointDto>> GetAllForMapAsync()
        {
            var deliveries = await repo.GetAllAsync();

            // ✅ FIX: Removed the filter `.Where(d => d.CurrentLatitude.HasValue && ...)`
            // Previously this filtered out ALL Pending deliveries (which have no GPS yet).
            // Now we include ALL deliveries and resolve coordinates via ResolveCoordinates():
            //   - GPS coordinates if available (Shipped, OutForDelivery, Delivered)
            //   - City-based lookup for Pending/Packed with no GPS yet
            //   - India center as final fallback

            return deliveries
                .Select(d =>
                {
                    var (lat, lng) = ResolveCoordinates(d);
                    return new DeliveryMapPointDto(
                        d.Id, d.OrderId, d.TrackingId,
                        d.RecipientName,
                        $"{d.AddressLine1}, {d.City}, {d.State}",
                        d.Status.ToString(),
                        lat,
                        lng,
                        d.EstimatedDeliveryDate,
                        d.EstimatedDeliveryDate < DateTime.UtcNow
                            && d.Status != DeliveryStatus.Delivered,
                        d.DeliveryAgentName,
                        d.StatusHistory
                            .Where(h => h.Latitude.HasValue && h.Longitude.HasValue)
                            .OrderBy(h => h.Timestamp)
                            .Select(h => new RoutePointDto(
                                h.Status.ToString(),
                                h.Latitude!.Value, h.Longitude!.Value,
                                h.Timestamp, h.Location))
                            .ToList()
                    );
                });
        }
    }
}