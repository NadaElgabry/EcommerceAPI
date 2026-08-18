using EcommerceAPI.Application.Interfaces.Image;
using EcommerceAPI.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

public class ImageService(IWebHostEnvironment environment) : IImageService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private const int MaxDimension = 4096;
    private const int MinDimension = 4;

    private static readonly Dictionary<string, List<byte[]>> Signatures = new()
    {
        [".jpg"] = new() { new byte[] { 0xFF, 0xD8, 0xFF } },
        [".jpeg"] = new() { new byte[] { 0xFF, 0xD8, 0xFF } },
        [".png"] = new() { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
    };

    public async Task<string> SaveFileAsync(IFormFile imageFile, string slug, ImageOwnerType ownerType, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(imageFile);
        if (imageFile.Length == 0)
            throw new ArgumentException("Empty file.");

        if (imageFile.Length > MaxFileSizeBytes)
            throw new ArgumentException($"File exceeds max size of {MaxFileSizeBytes / 1024 / 1024} MB.");

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug is required.");

        var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (!Signatures.ContainsKey(ext))
            throw new ArgumentException($"Only {string.Join(", ", Signatures.Keys)} are allowed.");

        using var ms = new MemoryStream();
        await imageFile.CopyToAsync(ms, cancellationToken);
        ms.Position = 0;

        if (!HasValidSignature(ms, Signatures[ext]))
            throw new ArgumentException("File content does not match its extension.");
        ms.Position = 0;

        try
        {
            var info = await SixLabors.ImageSharp.Image.IdentifyAsync(ms, cancellationToken);
            if (info is null)
                throw new ArgumentException("Unable to identify image format.");
            ms.Position = 0;
        }
        catch (UnknownImageFormatException)
        {
            throw new ArgumentException("File is not a valid, decodable image.");
        }

        using var image = await SixLabors.ImageSharp.Image.LoadAsync(ms, cancellationToken);

        if (image.Width < MinDimension || image.Height < MinDimension)
            throw new ArgumentException("Image dimensions too small.");

        if (image.Width > MaxDimension || image.Height > MaxDimension)
        {
            var ratio = Math.Min((double)MaxDimension / image.Width, (double)MaxDimension / image.Height);
            image.Mutate(x => x.Resize((int)(image.Width * ratio), (int)(image.Height * ratio)));
        }

        var folder = ownerType == ImageOwnerType.Category ? "categories" : "products";
        var uploadsPath = Path.Combine(environment.ContentRootPath, "Uploads", folder);
        Directory.CreateDirectory(uploadsPath);

        var safeSlug = Path.GetFileNameWithoutExtension(slug); // guards against path traversal / accidental extensions
        var fileName = $"{safeSlug}{ext}";
        var fileNameWithPath = Path.Combine(uploadsPath, fileName);

        // Deterministic naming: same slug -> overwrite (this is a re-upload/update of that entity's image)
        using var outStream = new FileStream(fileNameWithPath, FileMode.Create, FileAccess.Write);
        if (ext is ".jpg" or ".jpeg")
            await image.SaveAsJpegAsync(outStream, new JpegEncoder { Quality = 85 }, cancellationToken);
        else
            await image.SaveAsPngAsync(outStream, new PngEncoder(), cancellationToken);

        return Path.Combine(folder, fileName).Replace('\\', '/'); 
    }

    public void DeleteFile(string fileNameWithExtension, ImageOwnerType ownerType)
    {
        if (string.IsNullOrWhiteSpace(fileNameWithExtension))
            throw new ArgumentNullException(nameof(fileNameWithExtension));

        var folder = ownerType == ImageOwnerType.Category ? "categories" : "products";
        var uploadsPath = Path.Combine(environment.ContentRootPath, "Uploads", folder);
        var fullUploadsPath = Path.GetFullPath(uploadsPath);

        var safeName = Path.GetFileName(fileNameWithExtension);
        var fullTargetPath = Path.GetFullPath(Path.Combine(uploadsPath, safeName));

        if (!fullTargetPath.StartsWith(fullUploadsPath, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Invalid file path.");

        if (!File.Exists(fullTargetPath))
            throw new FileNotFoundException("Invalid file path");

        File.Delete(fullTargetPath);
    }

    private static bool HasValidSignature(Stream stream, List<byte[]> validSignatures)
    {
        var maxLen = validSignatures.Max(s => s.Length);
        var header = new byte[maxLen];
        var read = stream.Read(header, 0, maxLen);
        stream.Position = 0;
        if (read < maxLen) return false;
        return validSignatures.Any(sig => header.Take(sig.Length).SequenceEqual(sig));
    }
}