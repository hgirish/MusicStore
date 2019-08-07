using AngleSharp;
using AngleSharp.Html.Parser;
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

           Assert.Contains("<h4>Album title 6</h4>", content);
            var config = Configuration.Default;

            var context = BrowsingContext.New(config);
            var parser = context.GetService<IHtmlParser>();
            var document = parser.ParseDocument(content);
         //  var document = await HtmlHelpers.GetDocumentAsync(defaultPage);
           
            var elements = document.QuerySelectorAll("li h4");
            Assert.Equal(6, elements.Length);

        }
        
    }
}
