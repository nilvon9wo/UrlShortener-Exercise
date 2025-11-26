using System.Security.Cryptography;
using System.Text;

namespace Com.Example.UrlShortener_Exercise;

public class UrlShortener(IUrlMapDb urlMapDb)
{
    private const string Base62Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const string ShortUrlDomain = "eg.org";

    private static readonly HashSet<string> SupportedSchemes = [
        Uri.UriSchemeHttp,
        Uri.UriSchemeHttps
    ];
    private static readonly string _supportedSchemesDisplay = string.Join(", ", SupportedSchemes);

    private readonly IUrlMapDb _urlMapDb = urlMapDb ?? throw new ArgumentNullException(nameof(urlMapDb));

    public Uri ShortenUrl(Uri longUrl)
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

        string shortCode = GenerateShortCode(longUrl.AbsoluteUri);
        Uri shortUrl = new($"{longUrl.Scheme}://{ShortUrlDomain}/{shortCode}");

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

    private static string GenerateShortCode(string longUrl)
    {
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(longUrl));
        return new string([
            .. hashBytes.Take(6)
                .Select(b => Base62Characters[b % Base62Characters.Length])
            ]);
    }
}
