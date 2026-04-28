using FluentValidation;
using OrderAPI.DTOs;
namespace OrderAPI.Validators
{
    public class PlaceOrderDtoValidator : AbstractValidator<PlaceOrderDto>
    {
        public PlaceOrderDtoValidator()
        {
            RuleFor(x => x.AddressId)
                .GreaterThan(0)
                .WithMessage("A valid AddressId is required.");
        }
    }

    // ── UpdateOrderStatusDto ──────────────────────────────────
    public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
    {
        private static readonly string[] ValidStatuses =
            { "Confirmed", "Shipped", "Delivered", "Cancelled" };

        public UpdateOrderStatusDtoValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty()
                .WithMessage("Status is required.")
                .Must(s => ValidStatuses.Contains(s, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.");
        }
    }

}
