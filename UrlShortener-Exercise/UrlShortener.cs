using System.Security.Cryptography;
using System.Text;

namespace Com.Example.UrlShortener_Exercise;

public class UrlShortener(IUrlMapDb urlMapDb, UrlShortenerSettings? settings = null)
{
    private readonly IUrlMapDb _urlMapDb = urlMapDb ?? throw new ArgumentNullException(nameof(urlMapDb));
    private readonly UrlShortenerSettings _settings = settings ?? new UrlShortenerSettings();

    public Uri ShortenUrl(Uri longUrl)
    {
        ValidateLongUrl(longUrl);

        Uri shortUrl = GenerateUniqueShortUrl(longUrl);
        _urlMapDb.SaveUrlMapping(shortUrl.AbsoluteUri, longUrl.AbsoluteUri);

        return shortUrl;
    }

    private void ValidateLongUrl(Uri longUrl)
    {
        ArgumentNullException.ThrowIfNull(longUrl, nameof(longUrl));

        if (!longUrl.IsAbsoluteUri)
        {
            throw new ArgumentException("URL must be an absolute URI.", nameof(longUrl));
        }

        if (!_settings.SupportedSchemes.Contains(longUrl.Scheme))
        {
            throw new ArgumentException($"URL must use one of the supported schemes: {_settings.SupportedSchemesDisplay}.", nameof(longUrl));
        }
    }

    private Uri GenerateUniqueShortUrl(Uri longUrl)
        // Note: Using 8 bytes from SHA256 provides ~218 trillion possible combinations (62^8),
        // significantly reducing collision risk. However, collisions are still theoretically possible.
        // The salt counter provides a fallback mechanism to resolve any collisions that do occur.
        => Enumerable.Range(0, _settings.MaxCollisionAttempts)
            .Select(attempt => GenerateShortUrlWithSalt(longUrl, attempt))
            .FirstOrDefault(shortUrl => IsShortUrlAvailable(shortUrl, longUrl))
            ?? throw new InvalidOperationException(
                $"Unable to generate unique short URL after {_settings.MaxCollisionAttempts} attempts.");

    private Uri GenerateShortUrlWithSalt(Uri longUrl, int salt)
    {
        string input = salt == 0
            ? longUrl.AbsoluteUri
            : $"{longUrl.AbsoluteUri}#{salt}";
        string shortCode = GenerateShortCode(input);
        return new Uri($"{longUrl.Scheme}://{_settings.ShortUrlDomain}/{shortCode}");
    }

    private string GenerateShortCode(string input)
        => new([
            .. SHA256.HashData(Encoding.UTF8.GetBytes(input))
                .Take(_settings.ShortCodeLength)
                .Select(b => _settings.Base62Characters[b % _settings.Base62Characters.Length])
            ]);

    // Note: Deterministic hashing ensures the same long URL always generates the same short URL,
    // providing implicit idempotency without requiring a reverse lookup method in IUrlMapDb.
    // This keeps the interface simple while avoiding unnecessary database queries.
    private bool IsShortUrlAvailable(Uri shortUrl, Uri originalLongUrl)
    {
        string existingLongUrl = _urlMapDb.GetLongUrl(shortUrl.AbsoluteUri);
        return string.IsNullOrEmpty(existingLongUrl)
            || existingLongUrl == originalLongUrl.AbsoluteUri;
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
}
