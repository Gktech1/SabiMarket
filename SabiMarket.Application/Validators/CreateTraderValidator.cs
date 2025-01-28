using FluentValidation;
using SabiMarket.Application.DTOs.Requests;

namespace SabiMarket.Infrastructure.Validators
{
    public class CreateTraderValidator : AbstractValidator<CreateTraderRequestDto>
    {
        public CreateTraderValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .Length(2, 50).WithMessage("First name must be between 2 and 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .Length(2, 50).WithMessage("Last name must be between 2 and 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email address format");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^[0-9+\s-()]{10,15}$").WithMessage("Invalid phone number format");

            RuleFor(x => x.MarketId)
                .NotEmpty().WithMessage("Market ID is required");

            RuleFor(x => x.CaretakerId)
                .NotEmpty().WithMessage("Caretaker ID is required");

            RuleFor(x => x.BusinessName)
                .NotEmpty().WithMessage("Business name is required")
                .Length(2, 100).WithMessage("Business name must be between 2 and 100 characters");

            RuleFor(x => x.BusinessType)
                .NotEmpty().WithMessage("Business type is required")
                .Must(type => new[] { "Open Space", "Shop" }.Contains(type))
                .WithMessage("Business type must be either 'Open Space' or 'Shop'");
        }
    }

    public class UpdateTraderProfileValidator : AbstractValidator<UpdateTraderProfileDto>
    {
        public UpdateTraderProfileValidator()
        {
            RuleFor(x => x.BusinessName)
                .NotEmpty().WithMessage("Business name is required")
                .Length(2, 100).WithMessage("Business name must be between 2 and 100 characters");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^[0-9+\s-()]{10,15}$").WithMessage("Invalid phone number format");

            When(x => !string.IsNullOrEmpty(x.SectionId), () =>
            {
                RuleFor(x => x.SectionId)
                    .Length(1, 50).WithMessage("Section ID must not exceed 50 characters");
            });

            When(x => !string.IsNullOrEmpty(x.BusinessDescription), () =>
            {
                RuleFor(x => x.BusinessDescription)
                    .Length(10, 500).WithMessage("Business description must be between 10 and 500 characters");
            });
        }
    }

    public class TraderFilterValidator : AbstractValidator<TraderFilterRequestDto>
    {
        public TraderFilterValidator()
        {
            When(x => !string.IsNullOrEmpty(x.MarketId), () =>
            {
                RuleFor(x => x.MarketId)
                    .Length(1, 50).WithMessage("Market ID must not exceed 50 characters");
            });

            When(x => !string.IsNullOrEmpty(x.CaretakerId), () =>
            {
                RuleFor(x => x.CaretakerId)
                    .Length(1, 50).WithMessage("Caretaker ID must not exceed 50 characters");
            });

            When(x => !string.IsNullOrEmpty(x.BusinessType), () =>
            {
                RuleFor(x => x.BusinessType)
                    .Must(type => new[] { "Open Space", "Shop" }.Contains(type))
                    .WithMessage("Business type must be either 'Open Space' or 'Shop'");
            });

            When(x => !string.IsNullOrEmpty(x.SearchTerm), () =>
            {
                RuleFor(x => x.SearchTerm)
                    .Length(2, 50).WithMessage("Search term must be between 2 and 50 characters");
            });
        }
    }
}