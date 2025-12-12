namespace Com.Example.UrlShortener_Exercise;

public record UrlShortenerSettings {
    public string Base62Characters { get; init; }
        = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public string ShortUrlDomain {
        get;
        init => field = value
                        ?.TrimEnd('/')
                        ?? throw new ArgumentNullException(nameof(value));
    } = "eg.org";

    public int ShortCodeLength { get; init; } = 8;
    public int MaxCollisionAttempts { get; init; } = 100;

    public IReadOnlySet<string> SupportedSchemes { get; init; }
        = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Uri.UriSchemeHttp,
                Uri.UriSchemeHttps
            };

    public string SupportedSchemesDisplay
        => string.Join(", ", SupportedSchemes);
}
