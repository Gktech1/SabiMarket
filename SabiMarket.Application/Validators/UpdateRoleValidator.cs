using FluentValidation;
using SabiMarket.Application.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.Validators
{
    // Validators/UpdateRoleValidator.cs
    public class UpdateRoleValidator : AbstractValidator<UpdateRoleRequestDto>
    {
        public UpdateRoleValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required")
                .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters")
                .Matches("^[a-zA-Z0-9\\s-]+$").WithMessage("Role name can only contain letters, numbers, spaces and hyphens");

            RuleFor(x => x.Permissions)
                .NotNull().WithMessage("Permissions cannot be null")
                .Must(permissions => permissions.All(p => RolePermissionConstants.AllPermissions.Contains(p)))
                .WithMessage($"Permissions must be from the allowed list: {string.Join(", ", RolePermissionConstants.AllPermissions)}");
        }
    }
}
