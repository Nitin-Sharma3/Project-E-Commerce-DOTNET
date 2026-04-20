using DeliveryService.DTOs;
using DeliveryService.Models;
using DeliveryService.Repositories;

namespace DeliveryService.Services
{

    public class DeliveryServices(IDeliveryRepository repo, ILogger<DeliveryServices> logger)
        : IDeliveryService
    {
        // ── Valid transition map ──────────────────────────────────────────────────
        private static readonly Dictionary<DeliveryStatus, DeliveryStatus[]> ValidTransitions = new()
        {
            [DeliveryStatus.Pending] = [DeliveryStatus.Packed, DeliveryStatus.Failed],
            [DeliveryStatus.Packed] = [DeliveryStatus.Shipped, DeliveryStatus.Failed],
            [DeliveryStatus.Shipped] = [DeliveryStatus.OutForDelivery, DeliveryStatus.Failed],
            [DeliveryStatus.OutForDelivery] = [DeliveryStatus.Delivered, DeliveryStatus.Failed],
            [DeliveryStatus.Delivered] = [],   // terminal
            [DeliveryStatus.Failed] = [DeliveryStatus.Pending],  // allow retry
        };

        // ── GPS simulation: city coords ──────────────────────────────────────────
        private static readonly Dictionary<DeliveryStatus, (double LatOffset, double LngOffset)> GpsOffset = new()
        {
            [DeliveryStatus.Pending] = (0.08, 0.08),
            [DeliveryStatus.Packed] = (0.06, 0.06),
            [DeliveryStatus.Shipped] = (0.04, 0.04),
            [DeliveryStatus.OutForDelivery] = (0.01, 0.01),
            [DeliveryStatus.Delivered] = (0, 0),
            [DeliveryStatus.Failed] = (0.02, -0.02),
        };

        // ── Mapping helpers ───────────────────────────────────────────────────────
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
                i.Quantity * i.UnitPrice, i.ImageUrl)).ToList(),
            d.StatusHistory.OrderBy(h => h.Timestamp)
                .Select(h => new StatusHistoryDto(
                    h.Status.ToString(), h.Remarks, h.Location,
                    h.Latitude, h.Longitude, h.Timestamp, h.UpdatedBy)).ToList()
        );

        // ── CRUD ──────────────────────────────────────────────────────────────────

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
                    ImageUrl = i.ImageUrl
                }).ToList(),
                StatusHistory =
                [
                    new DeliveryStatusHistory
                {
                    Status = DeliveryStatus.Pending,
                    Remarks = "Delivery created, awaiting packing",
                    Location = "Warehouse",
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

            // Validate transition
            if (!ValidTransitions.TryGetValue(delivery.Status, out var allowed) ||
                !allowed.Contains(dto.NewStatus))
            {
                throw new InvalidOperationException(
                    $"Cannot transition from {delivery.Status} to {dto.NewStatus}. " +
                    $"Allowed: {string.Join(", ", allowed ?? [])}");
            }

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
                d.TrackingId,
                d.RecipientName,
                $"{d.AddressLine1}, {d.City}, {d.State}",
                d.Status.ToString(),
                d.CurrentLatitude,
                d.CurrentLongitude,
                d.EstimatedDeliveryDate,
                d.ActualDeliveryDate,
                d.DeliveryAgentName,
                d.DeliveryAgentPhone,
                timeline
            );
        }
    }
}
