using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MusicStore.E2ETests.HomePage
{
    public class HomePageTests : 
        IClassFixture<CustomWebApplicationFactory<MusicStore.Startup>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Startup> _factory;


        public HomePageTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(
                new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        }

        [Fact]
        public async Task DefaultUrl_ReturnsHomePage()
        {
            var defaultPage = await _client.GetAsync("/");
            //var content = await HtmlHelpers.GetDocumentAsync(defaultPage);

            var content = await defaultPage.Content.ReadAsStringAsync();

            Assert.Contains("Welcome", content);

        }
        
    }
}
