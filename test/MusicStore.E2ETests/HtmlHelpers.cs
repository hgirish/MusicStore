using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MusicStore.E2ETests
{
    public class HtmlHelpers
    {
        public static object CanellationToken { get; private set; }

        public static async Task<IHtmlDocument> GetDocumentAsync(HttpResponseMessage response){

            var content = await response.Content.ReadAsStringAsync();
            var document = await BrowsingContext.New()
                .OpenAsync(ResponseFactory, CancellationToken.None);
            return (IHtmlDocument)document;

            void ResponseFactory(VirtualResponse htmlResponse)
            {
                htmlResponse
                    .Address(response.RequestMessage.RequestUri)
                    .Status(response.StatusCode);
                MapHeaders(response.Headers);
                MapHeaders(response.Content.Headers);

                void MapHeaders(HttpHeaders headers)
                {
                    foreach (var header in headers)
                    {
                        foreach (var value in header.Value)
                        {
                            htmlResponse.Header(header.Key, value);
                        }
                    }
                }
            }
        }

        
    }
}
