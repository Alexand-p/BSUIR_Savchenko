using Microsoft.AspNetCore.Http;

namespace BSUIR_Savchenko.API.Services;

public class ImageStorage(IWebHostEnvironment environment)
{
    private static readonly Dictionary<string, string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".webp"] = "image/webp"
    };

    public async Task<(string Path, string Mime)?> SaveAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;
        if (file.Length > 4 * 1024 * 1024)
            throw new InvalidDataException("Максимальный размер изображения — 4 МБ.");

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedTypes.TryGetValue(extension, out var mime) ||
            !string.Equals(mime, file.ContentType, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Разрешены только PNG, JPEG и WebP.");

        var folder = Path.Combine(environment.WebRootPath, "images");
        Directory.CreateDirectory(folder);
        var name = $"upload_{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var destination = Path.Combine(folder, name);
        await using var output = new FileStream(destination, FileMode.CreateNew);
        await file.CopyToAsync(output);
        return ($"/images/{name}", mime);
    }

    public void Delete(string? image)
    {
        var name = Path.GetFileName(image ?? string.Empty);
        if (!name.StartsWith("upload_", StringComparison.Ordinal) || name.Contains("..")) return;
        var path = Path.Combine(environment.WebRootPath, "images", name);
        if (File.Exists(path)) File.Delete(path);
    }
}
