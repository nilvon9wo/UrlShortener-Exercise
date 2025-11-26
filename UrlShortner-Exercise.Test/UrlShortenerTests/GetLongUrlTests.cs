namespace Com.Example.UrlShortener_Exercise.Tests.UrlShortenerTests;

public class GetLongUrlTests
{
    [Fact]
    public void GetLongUrl_WithNullShortUrl_ThrowsArgumentNullException()
    {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        UrlShortener urlShortener = new(mockDb.Object);

        // Act & Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => urlShortener.GetLongUrl(null!));
        Assert.Equal("shortUrl", exception.ParamName);
    }

    [Fact]
    public void GetLongUrl_WithValidShortUrl_ReturnsLongUrl()
    {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        Uri shortUrl = new("https://eg.org/abc123");
        string expectedLongUrl = "https://example.com/";
        mockDb.Setup(db => db.GetLongUrl(shortUrl.AbsoluteUri))
            .Returns(expectedLongUrl);
        UrlShortener urlShortener = new(mockDb.Object);

        // Act
        Uri actualLongUrl = urlShortener.GetLongUrl(shortUrl);

        // Assert
        Assert.Equal(expectedLongUrl, actualLongUrl.AbsoluteUri);
    }

    [Fact]
    public void GetLongUrl_WithNonExistentShortUrl_ThrowsShortUrlNotFoundException()
    {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        Uri shortUrl = new("https://eg.org/nonexistent");
        mockDb.Setup(db => db.GetLongUrl(shortUrl.AbsoluteUri)).Returns(string.Empty);
        UrlShortener urlShortener = new(mockDb.Object);

        // Act & Assert
        ShortUrlNotFoundException exception = Assert.Throws<ShortUrlNotFoundException>(() => urlShortener.GetLongUrl(shortUrl));
        Assert.Equal(shortUrl.AbsoluteUri, exception.ShortUrl);
    }

    [Theory]
    [InlineData("http://example.com/", "http")]
    [InlineData("https://secure.example.com/", "https")]
    public void GetLongUrl_PreservesOriginalScheme(string storedLongUrl, string expectedScheme)
    {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        Uri shortUrl = new("https://eg.org/abc123");
        mockDb.Setup(db => db.GetLongUrl(shortUrl.AbsoluteUri))
            .Returns(storedLongUrl);
        UrlShortener urlShortener = new(mockDb.Object);

        // Act
        Uri actualLongUrl = urlShortener.GetLongUrl(shortUrl);

        // Assert
        Assert.Equal(expectedScheme, actualLongUrl.Scheme);
        Assert.Equal(storedLongUrl, actualLongUrl.AbsoluteUri);
    }
}
