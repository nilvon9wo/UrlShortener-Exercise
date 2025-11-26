using System.Security.Cryptography;
using System.Text;

namespace Com.Example.UrlShortener_Exercise;

public class UrlShortener(IUrlMapDb urlMapDb)
{
    private readonly IUrlMapDb _urlMapDb = urlMapDb ?? throw new ArgumentNullException(nameof(urlMapDb));

    private const string Base62Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private static readonly HashSet<string> SupportedSchemes = [
        Uri.UriSchemeHttp,
        Uri.UriSchemeHttps
    ];
    private static readonly string supportedSchemesDisplay = string.Join(", ", SupportedSchemes);

    public string ShortenUrl(Uri longUrl)
    {
        ArgumentNullException.ThrowIfNull(longUrl, nameof(longUrl));

        if (!longUrl.IsAbsoluteUri)
        {
            throw new ArgumentException("URL must be an absolute URI.", nameof(longUrl));
        }

        if (!SupportedSchemes.Contains(longUrl.Scheme))
        {
            throw new ArgumentException($"URL must use one of the supported schemes: {supportedSchemesDisplay}.", nameof(longUrl));
        }

        string shortUrl = GenerateShortUrl(longUrl.AbsoluteUri);
        _urlMapDb.SaveUrlMapping(shortUrl, longUrl.AbsoluteUri);

        return shortUrl;
    }

    public Uri GetLongUrl(string shortUrl)
    {
        ArgumentNullException.ThrowIfNull(shortUrl, nameof(shortUrl));

        if (string.IsNullOrWhiteSpace(shortUrl))
        {
            throw new ArgumentException("Short URL cannot be empty or whitespace.", nameof(shortUrl));
        }

        string longUrl = _urlMapDb.GetLongUrl(shortUrl);

        if (string.IsNullOrEmpty(longUrl))
        {
            throw new ShortUrlNotFoundException(shortUrl);
        }

        return new Uri(longUrl);
    }

    private static string GenerateShortUrl(string longUrl)
    {
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(longUrl));
        return new string([
            .. hashBytes.Take(6)
                .Select(b => Base62Characters[b % Base62Characters.Length])
            ]);
    }
}
