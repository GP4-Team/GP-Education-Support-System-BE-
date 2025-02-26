
using System.Text.RegularExpressions;

namespace ESS.Domain.ValueObjects.Media;

public class MediaFile
{
    // Parameterless constructor for EF Core
    private MediaFile() { }

    public string FileName { get; private set; } = default!;
    public string FileType { get; private set; } = default!;
    public string MimeType { get; private set; } = default!;
    public long Size { get; private set; }

    private MediaFile(string fileName, string fileType, string mimeType, long size)
    {
        FileName = fileName;
        FileType = fileType;
        MimeType = mimeType;
        Size = size;
    }

    public static MediaFile Create(string fileName, string mimeType, long size)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));

        if (string.IsNullOrWhiteSpace(mimeType))
            throw new ArgumentException("MIME type cannot be empty", nameof(mimeType));

        if (size <= 0)
            throw new ArgumentException("File size must be greater than 0", nameof(size));

        var fileType = Path.GetExtension(fileName).TrimStart('.').ToLower();

        return new MediaFile(fileName, fileType, mimeType, size);
    }
}