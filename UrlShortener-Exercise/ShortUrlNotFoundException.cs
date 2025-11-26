namespace Com.Example.UrlShortener_Exercise;

public class ShortUrlNotFoundException(
        string shortUrl,
        string? message = null,
        Exception? innerException = null
    )
    : Exception(message ?? $"Short URL '{shortUrl}' was not found.", innerException)
{
    public string ShortUrl { get; } = shortUrl;
}
