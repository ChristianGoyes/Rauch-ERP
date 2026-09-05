using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace RauchERP.FunctionalTests;
public sealed class ApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public ApiSmokeTests(WebApplicationFactory<Program> factory) => _client = factory.WithWebHostBuilder(b => b.UseSetting("Seed:Enabled", "false")).CreateClient();
    [Fact] public async Task Root_returns_service_metadata() => Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/")).StatusCode);
    [Fact] public async Task Protected_endpoint_requires_authentication() => Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/api/v1/users?page=1&pageSize=20")).StatusCode);
}
