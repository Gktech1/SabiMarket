using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Application.Validators
{
    public class UpdateProfileValidator : AbstractValidator<UpdateProfileDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateProfileValidator(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MinimumLength(2).WithMessage("Full name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Full name can only contain letters and spaces")
                .Must(name => name.Trim().Split(' ').Length >= 2)
                .WithMessage("Please provide both first and last name");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Please provide a valid email address")
                .MustAsync(async (model, email, cancellation) =>
                {
                    if (string.IsNullOrEmpty(email)) return false;
                    var user = await _userManager.FindByEmailAsync(email);
                    return user == null;
                })
                .WithMessage("This email is already in use");
        }
    }
}