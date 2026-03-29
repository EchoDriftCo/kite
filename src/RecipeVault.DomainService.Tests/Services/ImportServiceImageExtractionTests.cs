using Xunit;
using Shouldly;
using RecipeVault.DomainService;

namespace RecipeVault.DomainService.Tests.Services {
    public class ImportServiceImageExtractionTests {

        [Fact]
        public void ExtractFallbackImageUrl_WithOgImage_ReturnsOgImageUrl() {
            var html = @"
                <html>
                <head>
                    <meta property=""og:image"" content=""https://example.com/recipe-photo.jpg"">
                </head>
                <body></body>
                </html>";

            var result = ImportService.ExtractFallbackImageUrl(html, "https://example.com/recipes/1");
            result.ShouldBe("https://example.com/recipe-photo.jpg");
        }

        [Fact]
        public void ExtractFallbackImageUrl_WithOgImageReversedAttributes_ReturnsOgImageUrl() {
            // Some sites put content before property
            var html = @"
                <html>
                <head>
                    <meta content=""https://example.com/photo.jpg"" property=""og:image"">
                </head>
                <body></body>
                </html>";

            var result = ImportService.ExtractFallbackImageUrl(html, "https://example.com/recipes/1");
            result.ShouldBe("https://example.com/photo.jpg");
        }

        [Fact]
        public void ExtractFallbackImageUrl_WithTwitterImage_ReturnsTwitterImageUrl() {
            var html = @"
                <html>
                <head>
                    <meta name=""twitter:image"" content=""https://example.com/twitter-photo.jpg"">
                </head>
                <body></body>
                </html>";

            var result = ImportService.ExtractFallbackImageUrl(html, "https://example.com/recipes/1");
            result.ShouldBe("https://example.com/twitter-photo.jpg");
        }

        [Fact]
        public void ExtractFallbackImageUrl_WithOgAndTwitter_PrefersOgImage() {
            var html = @"
                <html>
                <head>
                    <meta property=""og:image"" content=""https://example.com/og-photo.jpg"">
                    <meta name=""twitter:image"" content=""https://example.com/twitter-photo.jpg"">
                </head>
                <body></body>
                </html>";

            var result = ImportService.ExtractFallbackImageUrl(html, "https://example.com/recipes/1");
            result.ShouldBe("https://example.com/og-photo.jpg");
        }

        [Fact]
        public void ExtractFallbackImageUrl_WithNoMetaTags_FallsBackToFirstImg() {
            var html = @"
                <html>
                <body>
                    <img src=""https://example.com/recipe-header.jpg"" alt=""Recipe"">
                    <img src=""https://example.com/other.jpg"" alt=""Other"">
                </body>
                </html>";

            var result = ImportService.ExtractFallbackImageUrl(html, "https://example.com/recipes/1");
            result.ShouldBe("https://example.com/recipe-header.jpg");
        }

        [Fact]
        public void ExtractFallbackImageUrl_SkipsDataUris() {
            var html = @"
                <html>
                <body>
                    <img src=""data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP"">
                    <img src=""https://example.com/real-image.jpg"">
                </body>
                </html>";

            var result = ImportService.ExtractFallbackImageUrl(html, "https://example.com/recipes/1");
            result.ShouldBe("https://example.com/real-image.jpg");
        }

        [Fact]
        public void ExtractFallbackImageUrl_SkipsTrackingPixels() {
            var html = @"
                <html>
                <body>
                    <img src=""https://example.com/1x1.gif"">
                    <img src=""https://example.com/pixel.png"">
                    <img src=""https://example.com/spacer.gif"">
                    <img src=""https://example.com/actual-photo.jpg"">
                </body>
                </html>";

            var result = ImportService.ExtractFallbackImageUrl(html, "https://example.com/recipes/1");
            result.ShouldBe("https://example.com/actual-photo.jpg");
        }

        [Fact]
        public void ExtractFallbackImageUrl_SkipsSvgAndIcoFiles() {
            var html = @"
                <html>
                <body>
                    <img src=""https://example.com/logo.svg"">
                    <img src=""https://example.com/favicon.ico"">
                    <img src=""https://example.com/recipe.jpg"">
                </body>
                </html>";

            var result = ImportService.ExtractFallbackImageUrl(html, "https://example.com/recipes/1");
            result.ShouldBe("https://example.com/recipe.jpg");
        }

        [Fact]
        public void ExtractFallbackImageUrl_ResolvesRelativeUrls() {
            var html = @"
                <html>
                <head>
                    <meta property=""og:image"" content=""/images/recipe-photo.jpg"">
                </head>
                <body></body>
                </html>";

            var result = ImportService.ExtractFallbackImageUrl(html, "https://example.com/recipes/1");
            result.ShouldBe("https://example.com/images/recipe-photo.jpg");
        }

        [Fact]
        public void ExtractFallbackImageUrl_ResolvesProtocolRelativeUrls() {
            var html = @"
                <html>
                <head>
                    <meta property=""og:image"" content=""//cdn.example.com/photo.jpg"">
                </head>
                <body></body>
                </html>";

            var result = ImportService.ExtractFallbackImageUrl(html, "https://example.com/recipes/1");
            result.ShouldBe("https://cdn.example.com/photo.jpg");
        }

        [Fact]
        public void ExtractFallbackImageUrl_WithEmptyHtml_ReturnsNull() {
            var result = ImportService.ExtractFallbackImageUrl("", "https://example.com");
            result.ShouldBeNull();
        }

        [Fact]
        public void ExtractFallbackImageUrl_WithNullHtml_ReturnsNull() {
            var result = ImportService.ExtractFallbackImageUrl(null, "https://example.com");
            result.ShouldBeNull();
        }

        [Fact]
        public void ExtractFallbackImageUrl_WithNoImages_ReturnsNull() {
            var html = @"
                <html>
                <head><title>No Images Here</title></head>
                <body><p>Just text.</p></body>
                </html>";

            var result = ImportService.ExtractFallbackImageUrl(html, "https://example.com/recipes/1");
            result.ShouldBeNull();
        }

        [Fact]
        public void ExtractFallbackImageUrl_ResolvesRelativeImgSrc() {
            var html = @"
                <html>
                <body>
                    <img src=""/uploads/recipes/photo.jpg"" alt=""Recipe"">
                </body>
                </html>";

            var result = ImportService.ExtractFallbackImageUrl(html, "https://cooking.example.com/recipes/pasta");
            result.ShouldBe("https://cooking.example.com/uploads/recipes/photo.jpg");
        }
    }
}
