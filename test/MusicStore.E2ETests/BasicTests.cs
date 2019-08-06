using Microsoft.AspNetCore.Mvc.Testing;
using System;
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
      //  [InlineData("/")]
        //[InlineData("/Index")]
        //[InlineData("/About")]
        [InlineData("/Privacy")]
       // [InlineData("/Contact")]
        public async Task Get_EndpointsReturnsSuccessAndCorrectContentType(string url)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            Assert.Equal("text/html; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }
    }
}
