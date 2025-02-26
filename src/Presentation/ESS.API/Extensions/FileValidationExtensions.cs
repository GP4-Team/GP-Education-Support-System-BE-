// src/Presentation/ESS.API/Extensions/FileValidationExtensions.cs
using ESS.API.Middleware;

namespace ESS.API.Extensions;

public static class FileValidationExtensions
{
    public static IApplicationBuilder UseFileValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<FileValidationMiddleware>();
    }
}