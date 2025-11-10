using FluentValidation;
using UserCreation.Business.Dto;

namespace UserCreation.Business.Validators
{
    public class UserCertationDtoValidator : AbstractValidator<UserCertationDto>
    {
        public UserCertationDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MinimumLength(2).WithMessage("Name must be at least 2 characters long.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.")
                .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.");
        }
    }
}
