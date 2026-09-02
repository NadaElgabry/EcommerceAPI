using Amazon.S3;
using Amazon.S3.Model;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces.Image;
using EcommerceAPI.Domain.Enums;
using EcommerceAPI.Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

public class ImageService(IAmazonS3 s3Client, IOptions<AwsSettings> awsSettings) : IImageService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private const int MaxDimension = 4096;
    private const int MinDimension = 4;
    private readonly string _bucketName = awsSettings.Value.S3.BucketName;

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
            throw new PayloadTooLargeException($"File exceeds max size of {MaxFileSizeBytes / 1024 / 1024} MB.");

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
        var safeSlug = Path.GetFileNameWithoutExtension(slug); 
        var fileName = $"{safeSlug}{ext}";
        var key = $"{folder}/{fileName}";

        using var outStream = new MemoryStream();
        string contentType;
        if (ext is ".jpg" or ".jpeg")
        {
            await image.SaveAsJpegAsync(outStream, new JpegEncoder { Quality = 85 }, cancellationToken);
            contentType = "image/jpeg";
        }
        else
        {
            await image.SaveAsPngAsync(outStream, new PngEncoder(), cancellationToken);
            contentType = "image/png";
        }
        outStream.Position = 0;

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = outStream,
            ContentType = contentType
        };

        try
        {
            await s3Client.PutObjectAsync(request, cancellationToken);
        }
        catch (AmazonS3Exception ex)
        {
            throw new InvalidOperationException($"S3 upload failed: {ex.Message}", ex);
        }

        return key;
    }

    public async Task DeleteFileAsync(string fileNameWithExtension, ImageOwnerType ownerType, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileNameWithExtension))
            throw new ArgumentNullException(nameof(fileNameWithExtension));

        var folder = ownerType == ImageOwnerType.Category ? "categories" : "products";
        var safeName = Path.GetFileName(fileNameWithExtension); 
        var key = $"{folder}/{safeName}";

        try
        {
            await s3Client.GetObjectMetadataAsync(_bucketName, key, cancellationToken);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new FileNotFoundException("Invalid file path");
        }

        await s3Client.DeleteObjectAsync(_bucketName, key, cancellationToken);
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