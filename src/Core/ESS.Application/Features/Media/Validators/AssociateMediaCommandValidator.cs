// src/Core/ESS.Application/Features/Media/Validators/AssociateMediaCommandValidator.cs
using FluentValidation;
using ESS.Application.Features.Media.Commands;

namespace ESS.Application.Features.Media.Validators;

public class AssociateMediaCommandValidator : AbstractValidator<AssociateMediaCommand>
{
    public AssociateMediaCommandValidator()
    {
        RuleFor(x => x.TempGuid)
            .NotEmpty()
            .WithMessage("Temporary GUID is required.");

        RuleFor(x => x.ResourceId)
            .NotEmpty()
            .WithMessage("Resource ID is required.");

        RuleFor(x => x.ResourceType)
            .NotEmpty()
            .WithMessage("Resource type is required.");

        RuleFor(x => x.Collection)
            .NotEmpty()
            .WithMessage("Collection name is required.");
    }
}