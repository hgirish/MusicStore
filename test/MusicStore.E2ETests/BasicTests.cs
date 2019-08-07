using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace MusicStore.E2ETests
{
    public class BasicTests
        :IClassFixture<WebApplicationFactory<MusicStore.Startup>>
    {
        private readonly WebApplicationFactory<MusicStore.Startup> _factory;

        public BasicTests(WebApplicationFactory<MusicStore.Startup> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Home")]
        [InlineData("/Home/Index")]
        [InlineData("/Home/Error")]
        [InlineData("/Home/StatusCodePage")]
        [InlineData("/Home/Privacy")]
        [InlineData("/Home/AccessDenied")]
        public async Task Get_EndpointsReturnsSuccessAndCorrectContentType(string url)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            Assert.Equal("text/html; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task Get_SecureEndEndPointsRequiresAutheticatedUser()
        {
            var client = _factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                }
                );

            var response = await client.GetAsync("/manage");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.StartsWith("http://localhost/Identity/Account/Login",
                response.Headers.Location.OriginalString);
        }
    }
}
