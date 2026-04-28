using AutoMapper;
using OrderAPI.DTOs;
using OrderAPI.Models;

namespace OrderAPI.Mapping
{
    
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            // ── ShippingAddress → SnapshotAddressDto ─────────────
            CreateMap<ShippingAddress, SnapshotAddressDto>();

            // ── AddressDto (external) → ShippingAddress ──────────
            // Used when snapshotting the address from AddressAPI into an Order
            CreateMap<AddressDto, ShippingAddress>();

            // ── OrderItem → OrderItemResponseDto ─────────────────
            // Subtotal is a computed property on the model, mapped directly
            CreateMap<OrderItem, OrderItemResponseDto>()
                .ForMember(dest => dest.Subtotal,
                           opt => opt.MapFrom(src => src.Subtotal));

            // ── Order → OrderResponseDto ──────────────────────────
            CreateMap<Order, OrderResponseDto>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Items,
                           opt => opt.MapFrom(src => src.OrderItems))
                .ForMember(dest => dest.ShippingAddress,
                           opt => opt.MapFrom(src => src.ShippingAddress));

            // ── Order → OrderSummaryDto ───────────────────────────
            // TotalItems is a calculated field — summed in AfterMap
            CreateMap<Order, OrderSummaryDto>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.TotalItems,
                           opt => opt.MapFrom(src => src.OrderItems.Sum(oi => oi.Quantity)));

            // ── CartItemDto → OrderItem ───────────────────────────
            // Used during PlaceOrder to snapshot cart items
            CreateMap<CartItemDto, OrderItem>()
                .ForMember(dest => dest.ProductName,
                           opt => opt.MapFrom(src => $"Product #{src.ProductId}"))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrderId, opt => opt.Ignore())
                .ForMember(dest => dest.Order, opt => opt.Ignore());
        }
    }
}
