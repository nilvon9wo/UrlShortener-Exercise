namespace Com.Example.UrlShortener_Exercise.Tests.UrlShortenerTests;

public class GetLongUrlTests {
    [Fact]
    public void GetLongUrl_WithNullShortUrl_ThrowsArgumentNullException() {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        UrlShortener urlShortener = new(mockDb.Object);

        // Act & Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => urlShortener.GetLongUrl(null!));
        Assert.Equal("shortUrl", exception.ParamName);
    }

    [Fact]
    public void GetLongUrl_WithValidShortUrl_ReturnsLongUrl() {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        Uri shortUrl = new("https://eg.org/abc123");
        string expectedLongUrl = "https://example.com/";
        _ = mockDb.Setup(db => db.GetLongUrl(shortUrl.AbsoluteUri))
            .Returns(expectedLongUrl);
        UrlShortener urlShortener = new(mockDb.Object);

        // Act
        Uri actualLongUrl = urlShortener.GetLongUrl(shortUrl);

        // Assert
        Assert.Equal(expectedLongUrl, actualLongUrl.AbsoluteUri);
    }

    [Fact]
    public void GetLongUrl_WithNonExistentShortUrl_ThrowsShortUrlNotFoundException() {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        Uri shortUrl = new("https://eg.org/nonexistent");
        _ = mockDb.Setup(db => db.GetLongUrl(shortUrl.AbsoluteUri)).Returns(string.Empty);
        UrlShortener urlShortener = new(mockDb.Object);

        // Act & Assert
        ShortUrlNotFoundException exception = Assert.Throws<ShortUrlNotFoundException>(() => urlShortener.GetLongUrl(shortUrl));
        Assert.Equal(shortUrl.AbsoluteUri, exception.ShortUrl);
    }

    [Theory]
    [InlineData("http://example.com/", "http")]
    [InlineData("https://secure.example.com/", "https")]
    public void GetLongUrl_PreservesOriginalScheme(string storedLongUrl, string expectedScheme) {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        Uri shortUrl = new("https://eg.org/abc123");
        _ = mockDb.Setup(db => db.GetLongUrl(shortUrl.AbsoluteUri))
            .Returns(storedLongUrl);
        UrlShortener urlShortener = new(mockDb.Object);

        // Act
        Uri actualLongUrl = urlShortener.GetLongUrl(shortUrl);

        // Assert
        Assert.Equal(expectedScheme, actualLongUrl.Scheme);
        Assert.Equal(storedLongUrl, actualLongUrl.AbsoluteUri);
    }

    // ============================================
    // DATABASE CORRUPTION / INVALID DATA TESTS
    // ============================================

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("just some text")]
    [InlineData("http://")]
    [InlineData("://missing-scheme")]
    [InlineData("http:///no-host")]
    public void GetLongUrl_WithInvalidUrlStoredInDatabase_ThrowsInvalidOperationException(string invalidStoredUrl) {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        Uri shortUrl = new("https://eg.org/abc123");
        mockDb.Setup(db => db.GetLongUrl(shortUrl.AbsoluteUri))
            .Returns(invalidStoredUrl);
        UrlShortener urlShortener = new(mockDb.Object);

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => urlShortener.GetLongUrl(shortUrl));
        Assert.Contains("invalid", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetLongUrl_WithRelativeUrlStoredInDatabase_ThrowsInvalidOperationException() {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        Uri shortUrl = new("https://eg.org/abc123");
        string relativeUrl = "/relative/path";
        mockDb.Setup(db => db.GetLongUrl(shortUrl.AbsoluteUri))
            .Returns(relativeUrl);
        UrlShortener urlShortener = new(mockDb.Object);

        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => urlShortener.GetLongUrl(shortUrl));
        Assert.Contains("invalid", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void GetLongUrl_WithWhitespaceStoredInDatabase_ThrowsShortUrlNotFoundException(string whitespace) {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        Uri shortUrl = new("https://eg.org/abc123");
        mockDb.Setup(db => db.GetLongUrl(shortUrl.AbsoluteUri))
            .Returns(whitespace);
        UrlShortener urlShortener = new(mockDb.Object);

        // Act & Assert
        ShortUrlNotFoundException exception = Assert.Throws<ShortUrlNotFoundException>(() => urlShortener.GetLongUrl(shortUrl));
        Assert.Equal(shortUrl.AbsoluteUri, exception.ShortUrl);
    }

    [Theory]
    [InlineData("ftp://unsupported.com/", "ftp")]
    [InlineData("file:///c:/temp/file.txt", "file")]
    [InlineData("mailto:test@example.com", "mailto")]
    [InlineData("custom://valid-but-unusual/", "custom")]
    public void GetLongUrl_WithNonHttpSchemeStoredInDatabase_ReturnsNormalizedUrl(string storedUrl, string expectedScheme) {
        // Arrange
        Mock<IUrlMapDb> mockDb = new();
        Uri shortUrl = new("https://eg.org/abc123");
        mockDb.Setup(db => db.GetLongUrl(shortUrl.AbsoluteUri))
            .Returns(storedUrl);
        UrlShortener urlShortener = new(mockDb.Object);

        // Act
        Uri actualLongUrl = urlShortener.GetLongUrl(shortUrl);

        // Assert
        // Note: GetLongUrl doesn't validate schemes (only ShortenUrl does).
        // This allows retrieval of URLs even if scheme validation rules change.
        // Uri class normalizes URLs (e.g., adds trailing slash to domain-only URLs).
        Assert.Equal(expectedScheme, actualLongUrl.Scheme);
        Assert.Equal(storedUrl, actualLongUrl.AbsoluteUri);
    }
}
