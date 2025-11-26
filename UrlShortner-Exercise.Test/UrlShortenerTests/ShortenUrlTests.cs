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
        mockDb.Setup(db => db.GetLongUrl(It.IsAny<string>())).Returns(string.Empty);
        UrlShortener urlShortener = new(mockDb.Object);
        Uri validUri = new(uriString);

        // Act
        Uri shortUrl = urlShortener.ShortenUrl(validUri);

        // Assert
        Assert.NotNull(shortUrl);
        Assert.Equal(validUri.Scheme, shortUrl.Scheme);
        Assert.Equal("eg.org", shortUrl.Host);
        Assert.NotEmpty(shortUrl.AbsolutePath.TrimStart('/'));
        mockDb.Verify(db => db.SaveUrlMapping(shortUrl.AbsoluteUri, validUri.AbsoluteUri), Times.Once);
    }

    [Fact]
    public void ShortenUrl_WithSameUrlCalledTwice_ReturnsSameShortUrl()
    {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        mockDb.Setup(db => db.GetLongUrl(It.IsAny<string>())).Returns(string.Empty);
        UrlShortener urlShortener = new(mockDb.Object);
        Uri uri = new("https://example.com");

        // Act
        Uri shortUrl1 = urlShortener.ShortenUrl(uri);
        Uri shortUrl2 = urlShortener.ShortenUrl(uri);

        // Assert
        Assert.Equal(shortUrl1, shortUrl2);
    }

    [Fact]
    public void ShortenUrl_WithDifferentUrls_GeneratesDifferentShortUrls()
    {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        mockDb.Setup(db => db.GetLongUrl(It.IsAny<string>())).Returns(string.Empty);
        UrlShortener urlShortener = new(mockDb.Object);

        // Act
        Uri shortUrl1 = urlShortener.ShortenUrl(new Uri("https://example1.com"));
        Uri shortUrl2 = urlShortener.ShortenUrl(new Uri("https://example2.com"));

        // Assert
        Assert.NotEqual(shortUrl1, shortUrl2);
    }

    [Theory]
    [InlineData("http://example.com", "http")]
    [InlineData("https://example.com", "https")]
    public void ShortenUrl_PreservesSchemeFromOriginalUrl(string longUrlString, string expectedScheme)
    {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        mockDb.Setup(db => db.GetLongUrl(It.IsAny<string>())).Returns(string.Empty);
        UrlShortener urlShortener = new(mockDb.Object);
        Uri longUrl = new(longUrlString);

        // Act
        Uri shortUrl = urlShortener.ShortenUrl(longUrl);

        // Assert
        Assert.Equal(expectedScheme, shortUrl.Scheme);
    }

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://secure.example.com")]
    public void ShortenUrl_SavesOriginalSchemeInDatabase(string longUrlString)
    {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        mockDb.Setup(db => db.GetLongUrl(It.IsAny<string>())).Returns(string.Empty);
        UrlShortener urlShortener = new(mockDb.Object);
        Uri longUrl = new(longUrlString);

        // Act
        Uri shortUrl = urlShortener.ShortenUrl(longUrl);

        // Assert
        mockDb.Verify(db => db.SaveUrlMapping(
            It.Is<string>(s => s.StartsWith(longUrl.Scheme)),
            It.Is<string>(saved => saved.StartsWith(longUrl.Scheme))),
            Times.Once);
    }

    [Fact]
    public void ShortenUrl_WhenCollisionOccurs_GeneratesAlternativeShortUrl()
    {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        string existingLongUrl = "https://existing.com/";

        // First call to GetLongUrl returns collision, subsequent calls return empty
        mockDb.SetupSequence(db => db.GetLongUrl(It.IsAny<string>()))
            .Returns(existingLongUrl)
            .Returns(string.Empty);

        UrlShortener urlShortener = new(mockDb.Object);
        Uri newLongUrl = new("https://different.com/");

        // Act
        Uri shortUrl = urlShortener.ShortenUrl(newLongUrl);

        // Assert
        Assert.NotNull(shortUrl);
        mockDb.Verify(db => db.GetLongUrl(It.IsAny<string>()), Times.AtLeast(2));
    }

    [Fact]
    public void ShortenUrl_WhenSameUrlAlreadyExists_DoesNotSaveDuplicate()
    {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        Uri longUrl = new("https://example.com/");

        // First call: generate and save the short URL
        mockDb.Setup(db => db.GetLongUrl(It.IsAny<string>())).Returns(string.Empty);
        UrlShortener urlShortener = new(mockDb.Object);
        Uri firstShortUrl = urlShortener.ShortenUrl(longUrl);

        // Second call: simulate that this short URL already maps to the same long URL
        mockDb.Setup(db => db.GetLongUrl(firstShortUrl.AbsoluteUri))
            .Returns(longUrl.AbsoluteUri);

        // Act
        Uri secondShortUrl = urlShortener.ShortenUrl(longUrl);

        // Assert
        Assert.Equal(firstShortUrl, secondShortUrl);
        mockDb.Verify(db => db.SaveUrlMapping(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
    }
}
