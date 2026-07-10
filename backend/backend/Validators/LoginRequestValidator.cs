using FluentValidation;
using backend.DTOs.Auth;
using System.Text.RegularExpressions;

namespace backend.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
    .NotEmpty().WithMessage("Email is required.")
    .EmailAddress().WithMessage("Invalid email format.")
    .Must(email => Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
    .WithMessage("Email contains invalid characters.");

            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }
}