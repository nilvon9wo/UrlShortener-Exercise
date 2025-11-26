namespace Com.Example.UrlShortener_Exercise.Tests.UrlShortenerTests;

public class ShortenUrlTests
{
    [Fact]
    public void ShortenUrl_WithNullUrl_ThrowsArgumentNullException()
    {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        UrlShortener urlShortener = new(mockDb.Object);

        // Act & Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => urlShortener.ShortenUrl(null!));
        Assert.Equal("longUrl", exception.ParamName);
    }

    [Fact]
    public void ShortenUrl_WithRelativeUri_ThrowsArgumentException()
    {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        UrlShortener urlShortener = new(mockDb.Object);
        Uri relativeUri = new("/path/to/resource", UriKind.Relative);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => urlShortener.ShortenUrl(relativeUri));
    }

    [Theory]
    [InlineData("ftp://unsupported.com")]
    [InlineData("file:///c:/temp/file.txt")]
    [InlineData("mailto:test@example.com")]
    public void ShortenUrl_WithNonHttpScheme_ThrowsArgumentException(string uriString)
    {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        UrlShortener urlShortener = new(mockDb.Object);
        Uri uri = new(uriString);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => urlShortener.ShortenUrl(uri));
    }

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com")]
    [InlineData("https://example.com/path?query=value")]
    [InlineData("https://subdomain.example.com:8080/path#fragment")]
    public void ShortenUrl_WithValidUrl_ReturnsShortUrlAndSavesMapping(string uriString)
    {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        UrlShortener urlShortener = new(mockDb.Object);
        Uri validUri = new(uriString);

        // Act
        string shortUrl = urlShortener.ShortenUrl(validUri);

        // Assert
        Assert.NotEmpty(shortUrl);
        mockDb.Verify(db => db.SaveUrlMapping(shortUrl, uriString), Times.Once);
    }

    [Fact]
    public void ShortenUrl_WithSameUrlCalledTwice_ReturnsSameShortUrl()
    {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        UrlShortener urlShortener = new(mockDb.Object);
        Uri uri = new("https://example.com");

        // Act
        string shortUrl1 = urlShortener.ShortenUrl(uri);
        string shortUrl2 = urlShortener.ShortenUrl(uri);

        // Assert
        Assert.Equal(shortUrl1, shortUrl2);
    }

    [Fact]
    public void ShortenUrl_WithDifferentUrls_GeneratesDifferentShortUrls()
    {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        UrlShortener urlShortener = new(mockDb.Object);

        // Act
        string shortUrl1 = urlShortener.ShortenUrl(new Uri("https://example1.com"));
        string shortUrl2 = urlShortener.ShortenUrl(new Uri("https://example2.com"));

        // Assert
        Assert.NotEqual(shortUrl1, shortUrl2);
    }
}
