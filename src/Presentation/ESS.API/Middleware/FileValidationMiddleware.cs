// src/Presentation/ESS.API/Middleware/FileValidationMiddleware.cs
using ESS.Infrastructure.Media.Configuration;
using Microsoft.Extensions.Options;

namespace ESS.API.Middleware;

public class FileValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly MediaValidationSettings _settings;
    private readonly ILogger<FileValidationMiddleware> _logger;

    public FileValidationMiddleware(
        RequestDelegate next,
        IOptions<MediaValidationSettings> settings,
        ILogger<FileValidationMiddleware> logger)
    {
        _next = next;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!IsFileUploadRequest(context))
        {
            await _next(context);
            return;
        }

        try
        {
            var form = await context.Request.ReadFormAsync();
            var file = form.Files.FirstOrDefault();

            if (file != null)
            {
                if (file.Length > _settings.MaxFileSize)
                {
                    context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = $"File size exceeds maximum limit of {_settings.MaxFileSize / 1024 / 1024}MB"
                    });
                    return;
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_settings.AllowedFileTypes.Contains(extension))
                {
                    context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = $"File type {extension} is not allowed"
                    });
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file upload");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Error processing file upload"
            });
            return;
        }

        await _next(context);
    }

    private bool IsFileUploadRequest(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/api/media/upload") &&
               context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase);
    }
}