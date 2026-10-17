using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RauchERP.Modules.Inventory.Contracts;
using System.Net;
using System.Text.Json;

namespace RauchERP.FunctionalTests;
public sealed class ApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    public ApiSmokeTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(b => b.UseSetting("Seed:Enabled", "false"));
        _client = _factory.CreateClient();
    }
    [Fact] public async Task Root_returns_service_metadata() => Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/")).StatusCode);
    [Fact] public async Task Protected_endpoint_requires_authentication() => Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/api/v1/users?page=1&pageSize=20")).StatusCode);
    [Fact] public void Inventory_movement_types_are_serialized_as_names()
    {
        var options = _factory.Services.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;
        Assert.Equal("\"Sale\"", JsonSerializer.Serialize(StockMovementType.Sale, options));
    }
}
