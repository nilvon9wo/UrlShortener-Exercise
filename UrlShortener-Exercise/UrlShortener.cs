using System.Security.Cryptography;
using System.Text;

namespace Com.Example.UrlShortener_Exercise;

public class UrlShortener(IUrlMapDb urlMapDb)
{
    private readonly IUrlMapDb _urlMapDb = urlMapDb ?? throw new ArgumentNullException(nameof(urlMapDb));

    private const string Base62Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const string ShortUrlDomain = "eg.org";
    private const int MaxCollisionAttempts = 100;

    private static readonly HashSet<string> SupportedSchemes = [
        Uri.UriSchemeHttp,
        Uri.UriSchemeHttps
    ];
    private static readonly string _supportedSchemesDisplay = string.Join(", ", SupportedSchemes);

    public Uri ShortenUrl(Uri longUrl)
    {
        ValidateLongUrl(longUrl);

        Uri shortUrl = GenerateUniqueShortUrl(longUrl);
        _urlMapDb.SaveUrlMapping(shortUrl.AbsoluteUri, longUrl.AbsoluteUri);

        return shortUrl;
    }

    public Uri GetLongUrl(Uri shortUrl)
    {
        ArgumentNullException.ThrowIfNull(shortUrl, nameof(shortUrl));

        string longUrl = _urlMapDb.GetLongUrl(shortUrl.AbsoluteUri);

        if (string.IsNullOrEmpty(longUrl))
        {
            throw new ShortUrlNotFoundException(shortUrl.AbsoluteUri);
        }

        return new Uri(longUrl);
    }

    private static void ValidateLongUrl(Uri longUrl)
    {
        ArgumentNullException.ThrowIfNull(longUrl, nameof(longUrl));

        if (!longUrl.IsAbsoluteUri)
        {
            throw new ArgumentException("URL must be an absolute URI.", nameof(longUrl));
        }

        if (!SupportedSchemes.Contains(longUrl.Scheme))
        {
            throw new ArgumentException($"URL must use one of the supported schemes: {_supportedSchemesDisplay}.", nameof(longUrl));
        }
    }

    private Uri GenerateUniqueShortUrl(Uri longUrl)
        // Note: Using 8 bytes from SHA256 provides ~218 trillion possible combinations (62^8),
        // significantly reducing collision risk. However, collisions are still theoretically possible.
        // The salt counter provides a fallback mechanism to resolve any collisions that do occur.
        => Enumerable.Range(0, MaxCollisionAttempts)
            .Select(attempt => GenerateShortUrlWithSalt(longUrl, attempt))
            .FirstOrDefault(shortUrl => IsShortUrlAvailable(shortUrl, longUrl))
            ?? throw new InvalidOperationException(
                $"Unable to generate unique short URL after {MaxCollisionAttempts} attempts.");

    private static Uri GenerateShortUrlWithSalt(Uri longUrl, int salt)
    {
        string input = salt == 0
            ? longUrl.AbsoluteUri
            : $"{longUrl.AbsoluteUri}#{salt}";
        string shortCode = GenerateShortCode(input);
        return new Uri($"{longUrl.Scheme}://{ShortUrlDomain}/{shortCode}");
    }

    private static string GenerateShortCode(string input)
        => new([
            .. SHA256.HashData(Encoding.UTF8.GetBytes(input))
                .Take(8)
                .Select(b => Base62Characters[b % Base62Characters.Length])
            ]);

    private bool IsShortUrlAvailable(Uri shortUrl, Uri originalLongUrl)
    {
        string existingLongUrl = _urlMapDb.GetLongUrl(shortUrl.AbsoluteUri);
        return string.IsNullOrEmpty(existingLongUrl)
            || existingLongUrl == originalLongUrl.AbsoluteUri;
    }
}
