using FluentValidation;

namespace Application.Users.Commands.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.TargetUserId)
            .NotEmpty();

        RuleFor(x => x.RequestingUserId)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.")
            .Matches(@"[!@#$%^&*()_+\-{}[\]:;""'`~<>,.?/\\|]")
            .WithMessage("Password must contain a special character.");

        When(x => !x.IsAdmin && x.RequestingUserId == x.TargetUserId, () =>
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty();
        });
    }
}