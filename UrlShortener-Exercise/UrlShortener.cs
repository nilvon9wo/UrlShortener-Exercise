using System.Security.Cryptography;
using System.Text;

namespace Com.Example.UrlShortener_Exercise;

public class UrlShortener(IUrlMapDb urlMapDb)
{
    private readonly IUrlMapDb _urlMapDb = urlMapDb ?? throw new ArgumentNullException(nameof(urlMapDb));

    public string ShortenUrl(Uri longUrl)
    {
        ArgumentNullException.ThrowIfNull(longUrl, nameof(longUrl));

        if (!longUrl.IsAbsoluteUri)
        {
            throw new ArgumentException("URL must be an absolute URI.", nameof(longUrl));
        }

        if (longUrl.Scheme != Uri.UriSchemeHttp && longUrl.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException("URL must use HTTP or HTTPS scheme.", nameof(longUrl));
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

        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        StringBuilder shortUrl = new(6);

        for (int i = 0; i < 6; i++)
        {
            shortUrl.Append(chars[hashBytes[i] % chars.Length]);
        }

        return shortUrl.ToString();
    }
}
