using FluentValidation;

namespace Application.Auth.Commands.RefreshJwt;

public sealed class RefreshJwtCommandValidator : AbstractValidator<RefreshJwtCommand>
{
    public RefreshJwtCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}