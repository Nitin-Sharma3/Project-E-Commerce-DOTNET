using FluentValidation;
using Reviews.DTOs;

namespace Reviews.Validators
{
    public class CreateReviewValidator : AbstractValidator<CreateReviewDTO>
    {
        public CreateReviewValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0);
            RuleFor(x => x.OrderId)
                .GreaterThan(0);
            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5);
            RuleFor(x => x.ReviewText)
                .NotEmpty()
                .MinimumLength(3);
            RuleForEach(x => x.Media).ChildRules(media =>
            {
                media.RuleFor(x => x.Type)
                     .NotEmpty()
                     .Must(x => x == "image" || x == "video")
                     .WithMessage("Type must be 'image' or 'video'");
                media.RuleFor(x => x.Url)
                     .NotEmpty();
            });
        }
    }
}