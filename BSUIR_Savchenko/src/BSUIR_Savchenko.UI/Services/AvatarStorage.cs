using System.Security.Cryptography;
using System.Text;

namespace BSUIR_Savchenko.UI.Services;

public class AvatarStorage(IWebHostEnvironment environment)
{
    private const long MaxBytes = 2 * 1024 * 1024;
    private static readonly string[] Extensions = [".png", ".jpg", ".webp"];

    public string ImageUrl(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return "/Images/avatar.svg";
        var id = FileId(userId);
        var folder = Path.Combine(environment.WebRootPath, "user-avatars");
        foreach (var ext in Extensions)
        {
            if (File.Exists(Path.Combine(folder, id + ext)))
                return "/user-avatars/" + id + ext;
        }
        return "/Images/avatar.svg";
    }

    public async Task SaveAsync(string? userId, IFormFile? file, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidDataException("Идентификатор пользователя не найден.");
        if (file is null || file.Length == 0)
            throw new InvalidDataException("Выберите файл изображения.");
        if (file.Length > MaxBytes)
            throw new InvalidDataException("Изображение должно быть меньше 2 МБ.");

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        if (stream.Length > MaxBytes)
            throw new InvalidDataException("Изображение должно быть меньше 2 МБ.");
        var data = stream.ToArray();
        var extension = DetectExtension(data)
            ?? throw new InvalidDataException("Разрешены только PNG, JPEG и WebP.");

        var folder = Path.Combine(environment.WebRootPath, "user-avatars");
        Directory.CreateDirectory(folder);
        var id = FileId(userId);
        var destination = Path.Combine(folder, id + extension);
        var temporary = Path.Combine(folder, Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            await File.WriteAllBytesAsync(temporary, data, cancellationToken);
            File.Move(temporary, destination, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
        foreach (var oldExtension in Extensions)
        {
            if (oldExtension != extension)
            {
                var old = Path.Combine(folder, id + oldExtension);
                if (File.Exists(old)) File.Delete(old);
            }
        }
    }

    private static string FileId(string userId)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(userId))).ToLowerInvariant();
    }

    private static string? DetectExtension(byte[] bytes)
    {
        if (bytes.Length >= 8 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
            return ".png";
        if (bytes.Length >= 3 && bytes[0] == 255 && bytes[1] == 216 && bytes[2] == 255)
            return ".jpg";
        if (bytes.Length >= 12 && Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF"
            && Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP")
            return ".webp";
        return null;
    }
}
