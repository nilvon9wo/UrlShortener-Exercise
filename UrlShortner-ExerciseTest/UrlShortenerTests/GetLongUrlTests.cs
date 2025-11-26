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

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GetLongUrl_WithEmptyShortUrl_ThrowsArgumentException(string invalidShortUrl)
    {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        UrlShortener urlShortener = new(mockDb.Object);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => urlShortener.GetLongUrl(invalidShortUrl));
    }

    [Fact]
    public void GetLongUrl_WithValidShortUrl_ReturnsLongUrl()
    {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        string shortUrl = "abc123";
        string expectedLongUrl = "https://example.com/";
        mockDb.Setup(db => db.GetLongUrl(shortUrl))
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
        string shortUrl = "nonexistent";
        mockDb.Setup(db => db.GetLongUrl(shortUrl)).Returns(string.Empty);
        UrlShortener urlShortener = new(mockDb.Object);

        // Act & Assert
        ShortUrlNotFoundException exception = Assert.Throws<ShortUrlNotFoundException>(() => urlShortener.GetLongUrl(shortUrl));
        Assert.Equal(shortUrl, exception.ShortUrl);
    }
}
