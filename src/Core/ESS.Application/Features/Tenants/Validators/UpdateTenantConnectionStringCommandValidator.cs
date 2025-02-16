using FluentValidation;
using Npgsql;
using ESS.Application.Features.Tenants.Commands;

namespace ESS.Application.Features.Tenants.Validators;

public class UpdateTenantConnectionStringCommandValidator : AbstractValidator<UpdateTenantConnectionStringCommand>
{
    public UpdateTenantConnectionStringCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .MaximumLength(500)
            .Must(BeValidConnectionString)
            .WithMessage("Invalid connection string format");
    }

    private bool BeValidConnectionString(string connectionString)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            return !string.IsNullOrEmpty(builder.Database) &&
                   !string.IsNullOrEmpty(builder.Host) &&
                   !string.IsNullOrEmpty(builder.Username);
        }
        catch
        {
            return false;
        }
    }
}