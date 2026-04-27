using DeliveryService.DTOs;
using DeliveryService.Models;
using DeliveryService.Repositories; 

namespace DeliveryService.Services
{
    public class DeliveryServices(
        IDeliveryRepository repo,
        ILogger<DeliveryServices> logger,
        IHttpClientFactory http)          // ← ADD IHttpClientFactory
        : IDeliveryService
    {
        // ── Valid transitions ─────────────────────────────────────────────
        private static readonly Dictionary<DeliveryStatus, DeliveryStatus[]> ValidTransitions = new()
        {
            [DeliveryStatus.Pending] = [DeliveryStatus.Packed, DeliveryStatus.Failed],
            [DeliveryStatus.Packed] = [DeliveryStatus.Shipped, DeliveryStatus.Failed],
            [DeliveryStatus.Shipped] = [DeliveryStatus.OutForDelivery, DeliveryStatus.Failed],
            [DeliveryStatus.OutForDelivery] = [DeliveryStatus.Delivered, DeliveryStatus.Failed],
            [DeliveryStatus.Delivered] = [],
            [DeliveryStatus.Failed] = [DeliveryStatus.Pending],
        };

        // ── GPS simulation ────────────────────────────────────────────────
        private static readonly Dictionary<DeliveryStatus, (double LatOffset, double LngOffset)> GpsOffset = new()
        {
            [DeliveryStatus.Pending] = (0.08, 0.08),
            [DeliveryStatus.Packed] = (0.06, 0.06),
            [DeliveryStatus.Shipped] = (0.04, 0.04),
            [DeliveryStatus.OutForDelivery] = (0.01, 0.01),
            [DeliveryStatus.Delivered] = (0, 0),
            [DeliveryStatus.Failed] = (0.02, -0.02),
        };

        // ── Delivery status → OrderAPI status string ──────────────────────
        // OrderAPI enum: Pending=0, Confirmed=1, Shipped=2, Delivered=3, Cancelled=4
        private static string? MapToOrderStatus(DeliveryStatus status) => status switch
        {
            DeliveryStatus.Packed => "Confirmed",
            DeliveryStatus.Shipped => "Shipped",
            DeliveryStatus.OutForDelivery => "Shipped",   // OrderAPI has no OFD
            DeliveryStatus.Delivered => "Delivered",
            DeliveryStatus.Failed => "Cancelled",
            _ => null         // Pending — no sync
        };

        // ── Mapping ───────────────────────────────────────────────────────
        private static DeliveryResponseDto MapToResponse(Delivery d) => new(
            d.Id, d.OrderId, d.UserId,
            d.RecipientName,
            $"{d.AddressLine1}{(d.AddressLine2 != null ? ", " + d.AddressLine2 : "")}, {d.City}, {d.State} - {d.Pincode}",
            d.ContactPhone, d.ContactEmail,
            d.TrackingId, d.Status.ToString(),
            d.DeliveryAgentName, d.DeliveryAgentPhone,
            d.CreatedAt, d.EstimatedDeliveryDate, d.ActualDeliveryDate,
            d.CurrentLatitude, d.CurrentLongitude,
            d.Items.Select(i => new DeliveryItemResponseDto(
                i.ProductId, i.ProductName, i.Quantity, i.UnitPrice,
                i.Subtotal,                              // ← ADD Subtotal
                i.Quantity * i.UnitPrice,
                i.ImageUrl)).ToList(),
            d.StatusHistory.OrderBy(h => h.Timestamp)
                .Select(h => new StatusHistoryDto(
                    h.Status.ToString(), h.Remarks, h.Location,
                    h.Latitude, h.Longitude, h.Timestamp, h.UpdatedBy)).ToList()
        );

        // ── CRUD ──────────────────────────────────────────────────────────

        public async Task<IEnumerable<DeliveryResponseDto>> GetAllAsync() =>
            (await repo.GetAllAsync()).Select(MapToResponse);

        public async Task<DeliveryResponseDto?> GetByIdAsync(int id)
        {
            var d = await repo.GetByIdAsync(id);
            return d == null ? null : MapToResponse(d);
        }

        public async Task<IEnumerable<DeliveryResponseDto>> GetByUserIdAsync(int userId) =>
            (await repo.GetByUserIdAsync(userId)).Select(MapToResponse);

        public async Task<DeliveryResponseDto> CreateAsync(CreateDeliveryDto dto)
        {
            var trackingId = $"TRK-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

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
                Country = dto.Country,              // ← ADD
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
                    Subtotal = i.Subtotal,             // ← ADD
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
            logger.LogInformation("Delivery {Id} created for Order {OrderId}", created.Id, created.OrderId);
            return MapToResponse(created);
        }

        public async Task<DeliveryResponseDto> UpdateStatusAsync(UpdateDeliveryStatusDto dto)
        {
            var delivery = await repo.GetByIdAsync(dto.DeliveryId)
                ?? throw new KeyNotFoundException($"Delivery {dto.DeliveryId} not found");

            if (!ValidTransitions.TryGetValue(delivery.Status, out var allowed) ||
                !allowed.Contains(dto.NewStatus))
            {
                throw new InvalidOperationException(
                    $"Cannot transition from {delivery.Status} to {dto.NewStatus}. " +
                    $"Allowed: {string.Join(", ", allowed ?? [])}");
            }

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

            // ── Sync back to OrderAPI ─────────────────────────────────────
            await SyncStatusToOrderApiAsync(delivery.OrderId, dto.NewStatus);

            logger.LogInformation("Delivery {Id} status → {Status}", delivery.Id, dto.NewStatus);
            return MapToResponse(delivery);
        }

        public async Task<DeliveryResponseDto> MarkDeliveredAsync(MarkDeliveredDto dto)
        {
            return await UpdateStatusAsync(new UpdateDeliveryStatusDto(
                dto.DeliveryId,
                DeliveryStatus.Delivered,
                "Delivery confirmed by agent",
                UpdatedBy: dto.UpdatedBy
            ));
        }

        public async Task<TrackingResponseDto?> GetTrackingAsync(string trackingId)
        {
            var d = await repo.GetByTrackingIdAsync(trackingId);
            if (d == null) return null;

            var timeline = d.StatusHistory
                .OrderBy(h => h.Timestamp)
                .Select(h => new TrackingPointDto(
                    h.Status.ToString(), h.Location, h.Latitude, h.Longitude,
                    h.Timestamp, h.Remarks,
                    IsCurrent: h.Status == d.Status))
                .ToList();

            return new TrackingResponseDto(
                d.TrackingId, d.RecipientName,
                $"{d.AddressLine1}, {d.City}, {d.State}",
                d.Status.ToString(),
                d.CurrentLatitude, d.CurrentLongitude,
                d.EstimatedDeliveryDate, d.ActualDeliveryDate,
                d.DeliveryAgentName, d.DeliveryAgentPhone,
                timeline
            );
        }

        public async Task<IEnumerable<DeliveryMapPointDto>> GetAllForMapAsync()
        {
            var deliveries = await repo.GetAllAsync();
            return deliveries
                .Where(d => d.CurrentLatitude.HasValue && d.CurrentLongitude.HasValue)
                .Select(d => new DeliveryMapPointDto(
                    d.Id, d.OrderId, d.TrackingId,
                    d.RecipientName,
                    $"{d.AddressLine1}, {d.City}, {d.State}",
                    d.Status.ToString(),
                    d.CurrentLatitude!.Value,
                    d.CurrentLongitude!.Value,
                    d.EstimatedDeliveryDate,
                    d.EstimatedDeliveryDate < DateTime.UtcNow && d.Status != DeliveryStatus.Delivered,
                    d.DeliveryAgentName,
                    d.StatusHistory
                        .Where(h => h.Latitude.HasValue && h.Longitude.HasValue)
                        .OrderBy(h => h.Timestamp)
                        .Select(h => new RoutePointDto(
                            h.Status.ToString(),
                            h.Latitude!.Value, h.Longitude!.Value,
                            h.Timestamp, h.Location))
                        .ToList()
                ));
        }

        // ── Sync delivery status to OrderAPI ──────────────────────────────
        private async Task SyncStatusToOrderApiAsync(int orderId, DeliveryStatus deliveryStatus)
        {
            var orderStatus = MapToOrderStatus(deliveryStatus);
            if (orderStatus == null) return;   // Pending — nothing to sync

            try
            {
                var client = http.CreateClient("OrderService");
                // OrderAPI endpoint: PATCH api/orders/{orderId}/status
                var payload = new UpdateOrderStatusPayload { Status = orderStatus };
                var resp = await client.PatchAsJsonAsync(
                    $"api/orders/{orderId}/status", payload);

                if (!resp.IsSuccessStatusCode)
                    logger.LogWarning(
                        "OrderAPI sync failed for OrderId {Id} → {Status} | HTTP {Code}",
                        orderId, orderStatus, resp.StatusCode);
                else
                    logger.LogInformation(
                        "OrderAPI synced: Order {Id} → {Status}", orderId, orderStatus);
            }
            catch (Exception ex)
            {
                // Never fail delivery update if OrderAPI is down
                logger.LogError(ex, "SyncStatusToOrderApi failed for OrderId {Id}", orderId);
            }
        }
        public async Task<DeliveryResponseDto?> GetByOrderIdAsync(int orderId)
        {
            var d = await repo.GetByOrderIdAsync(orderId);
            return d == null ? null : MapToResponse(d);
        }
    }
}