namespace Com.Example.UrlShortener_Exercise;

public class UrlShortener(IUrlMapDb urlMapDb)
{
    private readonly IUrlMapDb _urlMapDb = urlMapDb ?? throw new ArgumentNullException(nameof(urlMapDb));

    public string ShortenUrl(Uri longUrl) => throw new NotImplementedException();

    public Uri GetLongUrl(string shortUrl) => throw new NotImplementedException();

}
