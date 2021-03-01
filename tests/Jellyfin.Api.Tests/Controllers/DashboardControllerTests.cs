using System.IO;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Api.Models;
using MediaBrowser.Common.Json;
using Xunit;

namespace Jellyfin.Api.Tests.Controllers
{
    public sealed class DashboardControllerTests : IClassFixture<JellyfinApplicationFactory>
    {
        private readonly JellyfinApplicationFactory _factory;
        private readonly JsonSerializerOptions _jsonOpions = JsonDefaults.GetOptions();

        public DashboardControllerTests(JellyfinApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetDashboardConfigurationPage_NonExistingPage_NotFound()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("web/ConfigurationPage?name=ThisPageDoesntExists").ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetDashboardConfigurationPage_ExistingPage_CorrectPage()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/web/ConfigurationPage?name=TestPlugin").ConfigureAwait(false);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(MediaTypeNames.Text.Html, response.Content.Headers.ContentType?.MediaType);
            StreamReader reader = new StreamReader(typeof(TestPlugin).Assembly.GetManifestResourceStream("Jellyfin.Api.Tests.TestPage.html")!);
            Assert.Equal(await response.Content.ReadAsStringAsync(), reader.ReadToEnd());
        }

        [Fact]
        public async Task GetDashboardConfigurationPage_BrokenPage_NotFound()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/web/ConfigurationPage?name=BrokenPage").ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetConfigurationPages_NoParams_AllConfigurationPages()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/web/ConfigurationPages").ConfigureAwait(false);

            Assert.True(response.IsSuccessStatusCode);

            var res = await response.Content.ReadAsStreamAsync();
            _ = await JsonSerializer.DeserializeAsync<ConfigurationPageInfo[]>(res, _jsonOpions);
            // TODO: check content
        }

        [Fact]
        public async Task GetConfigurationPages_True_MainMenuConfigurationPages()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/web/ConfigurationPages?enableInMainMenu=true").ConfigureAwait(false);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(MediaTypeNames.Application.Json, response.Content.Headers.ContentType?.MediaType);
            Assert.Equal(Encoding.UTF8.BodyName, response.Content.Headers.ContentType?.CharSet);

            var res = await response.Content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<ConfigurationPageInfo[]>(res, _jsonOpions);
            Assert.Empty(data);
        }
    }
}