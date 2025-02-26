// src/Core/ESS.Application/Features/Media/Validators/UploadTempFileCommandValidator.cs

using FluentValidation;
using ESS.Application.Features.Media.Commands;

namespace ESS.Application.Features.Media.Validators;

public class UploadTempFileCommandValidator : AbstractValidator<UploadTempFileCommand>
{
    public UploadTempFileCommandValidator()
    {
        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage("File stream is required.");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required.")
            .MaximumLength(255)
            .WithMessage("File name must not exceed 255 characters.");

        RuleFor(x => x.MimeType)
            .NotEmpty()
            .WithMessage("MIME type is required.");

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .WithMessage("File size must be greater than 0.");

        RuleFor(x => x.Collection)
            .NotEmpty()
            .WithMessage("Collection name is required.");
    }
}

