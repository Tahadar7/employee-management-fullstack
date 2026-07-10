using FluentValidation;
using backend.DTOs.Auth;
using System.Text.RegularExpressions;

namespace backend.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
    .NotEmpty().WithMessage("Email is required.")
    .EmailAddress().WithMessage("Invalid email format.")
    .Must(email => Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
    .WithMessage("Email contains invalid characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
        }
    }
}