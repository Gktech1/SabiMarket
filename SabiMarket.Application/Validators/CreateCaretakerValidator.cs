using FluentValidation;
using SabiMarket.Application.DTOs.Requests;

public class CreateCaretakerValidator : AbstractValidator<CaretakerForCreationRequestDto>
{
    public CreateCaretakerValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full Name is required.")
            .MaximumLength(100).WithMessage("Full Name cannot exceed 100 characters.");

        RuleFor(x => x.PhoneNumber)
             .NotEmpty().WithMessage("Phone Number is required.")
             .Matches(@"^\d{11}$").WithMessage("Phone Number must be exactly 11 digits.");


        RuleFor(x => x.EmailAddress)
            .NotEmpty().WithMessage("Email Address is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.MarketId)
            .NotEmpty().WithMessage("Market ID is required.")
            .MaximumLength(50).WithMessage("Market ID cannot exceed 50 characters.");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required.")
            .Must(g => g.Equals("Male", StringComparison.OrdinalIgnoreCase) || g.Equals("Female", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Gender must be either 'Male' or 'Female'.");

        RuleFor(x => x.PhotoUrl)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.PhotoUrl))
            .WithMessage("Invalid Photo URL format.");

        RuleFor(x => x.LocalGovernmentId)
            .MaximumLength(50).WithMessage("Local Government ID cannot exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.LocalGovernmentId));
    }
}
