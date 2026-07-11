using backend.DTOs.Employee;
using FluentValidation;

namespace backend.Validators
{
    public class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
    {
        public CreateEmployeeRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty().EmailAddress()
                .MaximumLength(150);

           RuleFor(x => x.Phone)
    .Matches(@"^\d{11}$")
    .WithMessage("Phone must be exactly 11 digits.")
    .When(x => !string.IsNullOrWhiteSpace(x.Phone));

            RuleFor(x => x.City)
                .MaximumLength(100);
        }
    }
}