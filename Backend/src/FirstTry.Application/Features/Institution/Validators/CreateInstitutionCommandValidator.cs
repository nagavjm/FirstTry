using FirstTry.Application.Features.Institution.Commands;
using FluentValidation;

namespace FirstTry.Application.Features.Institution.Validators;

public class CreateInstitutionCommandValidator : AbstractValidator<CreateInstitutionCommand>
{
    public CreateInstitutionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Institution name is required")
            .MaximumLength(200).WithMessage("Institution name cannot exceed 200 characters");

        RuleFor(x => x.UniversalId)
            .NotEmpty().WithMessage("Universal ID is required")
            .MaximumLength(100).WithMessage("Universal ID cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Institution email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters");

        RuleFor(x => x.Users)
            .NotEmpty().WithMessage("At least one user is required when creating an institution")
            .Must(users => users.Count >= 1).WithMessage("At least one user must be provided");

        RuleForEach(x => x.Users).ChildRules(user =>
        {
            user.RuleFor(u => u.Email)
                .NotEmpty().WithMessage("User email is required")
                .EmailAddress().WithMessage("Invalid user email format");

            user.RuleFor(u => u.Password)
                .NotEmpty().WithMessage("User password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number");

            user.RuleFor(u => u.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

            user.RuleFor(u => u.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");
        });

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Website)
            .MaximumLength(500).WithMessage("Website cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Website));
    }
}

