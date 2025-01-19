using FluentValidation;
using SabiMarket.Application.DTOs.Requests;

public class UpdateProfileValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(200).WithMessage("Full name cannot exceed 200 characters")
            .Matches(@"^[a-zA-Z]+(?: [a-zA-Z]+)*$")
            .WithMessage("Full name should contain only letters and spaces");

        RuleFor(x => x.EmailAddress)
            .NotEmpty().WithMessage("Email address is required")
            .EmailAddress().WithMessage("Invalid email address format")
            .MaximumLength(256); // IdentityUser email max length

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.LocalGovernmentId)
            .NotEmpty().WithMessage("Local Government is required");

        When(x => !string.IsNullOrEmpty(x.ProfileImageUrl), () =>
        {
            RuleFor(x => x.ProfileImageUrl)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Invalid profile image URL format");
        });
    }
}