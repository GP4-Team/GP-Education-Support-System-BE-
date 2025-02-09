using ESS.Application.Features.Tenants.Commands;
using FluentValidation;

namespace ESS.Application.Features.Tenants.Validators;
public class AddTenantDomainCommandValidator : AbstractValidator<AddTenantDomainCommand>
{
    public AddTenantDomainCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Domain)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[a-z0-9][a-z0-9-_.]+[a-z0-9]$").WithMessage("Invalid domain format");
    }
}
