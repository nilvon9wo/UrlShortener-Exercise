namespace Com.Example.UrlShortener_Exercise;

public record UrlShortenerSettings
{
    private string _shortUrlDomain = "eg.org";

    public string Base62Characters { get; init; } = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public string ShortUrlDomain
    {
        get => _shortUrlDomain;
        init => _shortUrlDomain = value?.TrimEnd('/')
            ?? throw new ArgumentNullException(nameof(value));
    }

    public int ShortCodeLength { get; init; } = 8;
    public int MaxCollisionAttempts { get; init; } = 100;

    public IReadOnlySet<string> SupportedSchemes { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Uri.UriSchemeHttp,
        Uri.UriSchemeHttps
    };

    public string SupportedSchemesDisplay => string.Join(", ", SupportedSchemes);
}
