using FluentValidation;
using SabiMarket.Application.DTOs;

public class CreateAssistantOfficerRequestDtoValidator : AbstractValidator<CreateAssistantOfficerRequestDto>
{
    public CreateAssistantOfficerRequestDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MinimumLength(2).WithMessage("First name must be at least 2 characters")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters")
            .Matches("^[a-zA-Z]*$").WithMessage("First name can only contain letters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters")
            .Matches("^[a-zA-Z]*$").WithMessage("Last name can only contain letters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^[0-9]{11}$").WithMessage("Phone number must be exactly 11 digits")
            .Must(phone => phone.StartsWith("0")).WithMessage("Phone number must start with 0");

        RuleFor(x => x.Level)
            .NotEmpty().WithMessage("Level is required")
            .MaximumLength(50).WithMessage("Level cannot exceed 50 characters");

        RuleFor(x => x.MarketId)
            .NotEmpty().WithMessage("Market ID is required")
            .MaximumLength(50).WithMessage("Market ID cannot exceed 50 characters");
    }
}