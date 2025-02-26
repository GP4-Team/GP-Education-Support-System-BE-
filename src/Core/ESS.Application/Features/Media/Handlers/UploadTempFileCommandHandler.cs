using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ESS.Application.Common.Models;
using ESS.Application.Common.Interfaces;
using ESS.Application.Features.Media.Commands;
using ESS.Application.Features.Media.DTOs;
using ESS.Domain.ValueObjects.Media;
using ESS.Domain.Entities.Media;

namespace ESS.Application.Features.Media.Handlers;

public class UploadTempFileCommandHandler
    : IRequestHandler<UploadTempFileCommand, Result<UploadedFileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMediaStorageService _storageService;
    private readonly ITenantService _tenantService;

    public UploadTempFileCommandHandler(
        IApplicationDbContext context,
        IMediaStorageService storageService,
        ITenantService tenantService)
    {
        _context = context;
        _storageService = storageService;
        _tenantService = tenantService;
    }

    public async Task<Result<UploadedFileDto>> Handle(
        UploadTempFileCommand request,
        CancellationToken cancellationToken)
    {
        var tempGuid = Guid.NewGuid();
        var tenantId = request.TenantId;

        // Validate tenant exists
        var tenant = await _tenantService.GetTenantByIdentifierAsync(tenantId);
        if (tenant == null)
            return Result.Failure<UploadedFileDto>("Tenant not found");

        if (!tenant.IsActive)
            return Result.Failure<UploadedFileDto>("Tenant is inactive");

        // Get media collection
        var collection = await _context.MediaCollections
            .FirstOrDefaultAsync(x => x.Name == request.Collection && x.TenantId == tenantId, cancellationToken);

        if (collection == null)
            return Result.Failure<UploadedFileDto>("Media collection not found.");

        if (!collection.IsFileTypeAllowed(Path.GetExtension(request.FileName)))
            return Result.Failure<UploadedFileDto>("File type not allowed.");

        if (!collection.IsFileSizeAllowed(request.FileSize))
            return Result.Failure<UploadedFileDto>("File size exceeds limit.");

        try
        {
            // Upload to S3 first
            var filePath = await _storageService.UploadTempFileAsync(
                request.FileStream,
                request.FileName,
                tempGuid,
                cancellationToken);

            // Create media entity
            var mediaFile = MediaFile.Create(
                request.FileName,
                request.MimeType,
                request.FileSize);

            var media = Domain.Entities.Media.Media.CreateTemporary(
                tenantId,
                mediaFile,
                "system", // TODO: Get current user
                tempGuid,
                collection);

            _context.Media.Add(media);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(new UploadedFileDto
            {
                TempGuid = tempGuid,
                FileName = request.FileName,
                FilePath = filePath
            });
        }
        catch (Exception ex)
        {
            return Result.Failure<UploadedFileDto>($"Error processing file upload: {ex.Message}");
        }
    }
}