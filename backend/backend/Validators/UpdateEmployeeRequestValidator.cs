using backend.DTOs.Employee;
using FluentValidation;

namespace backend.Validators
{
    public class UpdateEmployeeRequestValidator : AbstractValidator<UpdateEmployeeRequest>
    {
        public UpdateEmployeeRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty().EmailAddress().MaximumLength(150);

            RuleFor(x => x.Phone)
    .Matches(@"^\d{11}$")
    .WithMessage("Phone must be exactly 11 digits.")
    .When(x => !string.IsNullOrWhiteSpace(x.Phone));
        }
    }
}