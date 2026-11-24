using FluentValidation;
using Roshetta.API.Models.DTOs;

namespace Roshetta.API.Models.Validators
{
    public class UpdateDoctorValidator : AbstractValidator<UpdateDoctorDTO>
    {
        public UpdateDoctorValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(256).WithMessage("Email cannot exceed 256 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^01[0125][0-9]{8}$")
                .WithMessage("Enter a valid Egyptian mobile number (e.g. 01012345678).");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required.")
                 .LessThan(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Date of birth must be in the past.");

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("Invalid gender.");

            RuleFor(x => x.SpecializationId)
                .NotEmpty().WithMessage("Specialization is required.");

        }
    }
}
