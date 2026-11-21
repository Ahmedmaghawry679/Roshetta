using FluentValidation;
using Roshetta.API.Models.DTOs;

namespace Roshetta.API.Models.Validators
{
    public sealed class LoginValidator: AbstractValidator<LoginDTO>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(256).WithMessage("Email cannot exceed 256 characters.");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }   
}
