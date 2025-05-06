using Fizzler.Systems.HtmlAgilityPack;
using FluentAssertions;
using HtmlAgilityPack;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CRUDTests
{
    public class PersonsControllerIntegrationTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _httpClient;

        public PersonsControllerIntegrationTest(CustomWebApplicationFactory customWebApplicationFactory)
        {
            _httpClient = customWebApplicationFactory.CreateClient();
        }

        #region Index
        [Fact]
        public async Task Index_ShouldReturnView()
        {
            // Act
            HttpResponseMessage response = await _httpClient.GetAsync("/Persons/Index");

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            //Odczytuje treść odpowiedzi HTTP (HTML) jako string.
            string responseBody = await response.Content.ReadAsStringAsync();
            HtmlDocument html = new HtmlDocument();
            //Ładuje HTML do parsera z pobranego stringa.
            html.LoadHtml(responseBody);
            // Pobiera główny węzeł(DocumentNode) – korzeń drzewa HTML.
            var docuemnt = html.DocumentNode;

            docuemnt.QuerySelectorAll("table.persons").Should().NotBeEmpty(); //tabela z klasa persons
        }
        #endregion
    }
}
