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

            RuleFor(x => x.Phone).MaximumLength(20);
            RuleFor(x => x.City).MaximumLength(100);
        }
    }
}