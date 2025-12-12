namespace Com.Example.UrlShortener_Exercise.Tests;

public class UrlShortenerSettingsTests {
    [Fact]
    public void ShortUrlDomain_WithTrailingSlash_AutomaticallyRemovesIt() {
        // Arrange & Act
        UrlShortenerSettings settings = new() {
            ShortUrlDomain = "custom.com/"
        };

        // Assert
        Assert.Equal("custom.com", settings.ShortUrlDomain);
    }

    [Fact]
    public void ShortUrlDomain_WithMultipleTrailingSlashes_RemovesAll() {
        // Arrange & Act
        UrlShortenerSettings settings = new() {
            ShortUrlDomain = "custom.com///"
        };

        // Assert
        Assert.Equal("custom.com", settings.ShortUrlDomain);
    }
}
