using Reviews.DTOs;
using FluentValidation;
namespace Reviews.Validators
{
    public class UpdateReviewValidator : AbstractValidator<UpdateReviewDTO>
    {
        public UpdateReviewValidator()
        {
            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5);
            RuleFor(x => x.ReviewText)
                .NotEmpty()
                .MinimumLength(3);
            RuleForEach(x => x.Media).ChildRules(media =>
            {
                media.RuleFor(x => x.Type)
                     .NotEmpty()
                     .Must(x => x == "image" || x == "video");
                media.RuleFor(x => x.Url)
                     .NotEmpty();
            });
        }
    }
}